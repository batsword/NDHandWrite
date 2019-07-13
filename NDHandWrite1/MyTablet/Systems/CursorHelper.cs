namespace MyTablet.Systems
{
    using System;
    using System.Windows.Interop;

    using Microsoft.Win32.SafeHandles;

    using System.Runtime.InteropServices;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.IO;
    using System.Windows;

    public class CursorHelper
    {
        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(
            ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr hIcon,
            ref IconInfo pIconInfo);


        private static Cursor InternalCreateCursor(System.Drawing.Bitmap bmp,
            int xHotSpot, int yHotSpot)
        {
            IconInfo tmp = new IconInfo();
            GetIconInfo(bmp.GetHicon(), ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;

            IntPtr ptr = CreateIconIndirect(ref tmp);
            SafeFileHandle handle = new SafeFileHandle(ptr, true);
            return CursorInteropHelper.Create(handle);
        }

        public static Cursor CreateCursor(UIElement element, int xHotSpot,
            int yHotSpot)
        {
            element.Measure(new Size(double.PositiveInfinity,
              double.PositiveInfinity));
            element.Arrange(new Rect(0, 0, element.DesiredSize.Width,
              element.DesiredSize.Height));

            RenderTargetBitmap rtb =
              new RenderTargetBitmap((int)element.DesiredSize.Width,
                (int)element.DesiredSize.Height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(element);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);

            ms.Close();
            ms.Dispose();

            Cursor cur = InternalCreateCursor(bmp, xHotSpot, yHotSpot);

            bmp.Dispose();

            return cur;
        }
    }
}
