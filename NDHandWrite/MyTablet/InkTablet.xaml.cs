/*

* ==============================================================================
*
* Filename: MyTablet
* Description: 
*
* Version: 1.0
* Created: 2018.11.02
* Compiler: Visual Studio 2015
*
* Author: zyj
* Company: nined
*
* ==============================================================================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyTablet
{
    using System.ComponentModel;


    using MyTablet.Systems;
    using System.Threading;
    using Microsoft.Ink;
    using System.IO;
    using NDDomain;
    using System.Drawing;
    using System.Windows.Interop;

    //using System.Windows.Ink;

    /// <summary>
    /// Interaction logic for InkTablet.xaml
    /// </summary>
    public partial class InkTablet : UserControl, INotifyPropertyChanged
    {
        //public delegate void OnTextChangedHandler(string value);

        //public event OnTextChangedHandler OnTextChanged;

        public InkTablet()
        {
            InitializeComponent();

            this.theInkCanvas.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(this.theInkCanvas_StrokeCollected);

            MessageActions.ShowHandWriteHandler = MA_ShowHandWrite;

            
        }

        static InkTablet() {
            MyCommandProperty =//外部绑定命令
           DependencyProperty.Register("MyCommand", typeof(ICommand), typeof(InkTablet), new PropertyMetadata(null));
        }

        public static readonly DependencyProperty MyCommandProperty;

        public ICommand MyCommand
        {
            //get
            //{
            //    return this._myCommand ?? (this._myCommand = new DelegateCommand(this.OnMyClick));
            //}

            get { return (ICommand)GetValue(MyCommandProperty); }
            set { SetValue(MyCommandProperty, value); }
        }

        private void MA_ShowHandWrite(bool obj)
        {
            if(true == obj)
            {
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }

        public  BitmapSource GetBitmapImage_back()
        {
            BitmapSource returnSource; try
            {                //直接获取资源                
                Bitmap bmap = MyTablet.Properties.Resources.ktv_views_inputdevice_handBack;                //转换格式                
                returnSource = Imaging.CreateBitmapSourceFromHBitmap(bmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bmap.Dispose();
            }
            catch{
                returnSource = null;
            }
            return returnSource;
        }

        private void handwrite_Loaded(object sender, RoutedEventArgs e)
        {
            backImg.Source = GetBitmapImage_back();
        }


        private void theInkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            theInkCanvas.Strokes.Save(ms);//canvas为InkCanvas
            Microsoft.Ink.Ink ink = new Microsoft.Ink.Ink();
            ink.Load(ms.ToArray());
            string[] resultArray = null;//存放识别的结果
            using (Microsoft.Ink.RecognizerContext myRecoContext = new Microsoft.Ink.RecognizerContext())
            {
                Microsoft.Ink.RecognitionStatus status;//识别的结果状态，可用于判断是否识别成功
                Microsoft.Ink.RecognitionResult recoResult;
                myRecoContext.Strokes = ink.Strokes;
                try
                {
                    recoResult = myRecoContext.Recognize(out status);

                    if (status == RecognitionStatus.NoError)
                    {
                        List<StrokeWord> slist = new List<StrokeWord>();
                        Microsoft.Ink.RecognitionAlternates als = recoResult.GetAlternatesFromSelection();
                        int resultCount = als.Count;
                        resultArray = new string[resultCount];
                        for (int i = 0; i < resultCount; i++)
                        {
                            StrokeWord sw = new StrokeWord();
                            sw.StrokeName = als[i].ToString();
                            slist.Add(sw);
                            //resultArray[i] = als[i].ToString();//多余
                        }

                        this.StrokeWordList = new List<StrokeWord>();
                        this.StrokeWordList = slist;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }

        public string KeyText
        {
            get { return (string)GetValue(KeyTextProperty); }
            set { SetValue(KeyTextProperty, value); }
        }

        public static readonly DependencyProperty KeyTextProperty =
            DependencyProperty.Register("KeyText", typeof(string), typeof(InkTablet), new FrameworkPropertyMetadata { CoerceValueCallback = OnValueChange });

        private static object OnValueChange(DependencyObject d, object baseValue)
        {
            var control = d as InkTablet;
            control?.NoticeChanged(baseValue + "");
            return baseValue;
        }

        public void NoticeChanged(string value)
        {
            //OnTextChanged?.Invoke(value);
        }

        private Cursor _penCursor;

        public Cursor PenCursor
        {
            get
            {
                return this._penCursor;
            }
            set
            {
                if (value != this._penCursor)
                {
                    this._penCursor = value;
                    this.NotifyPropertyChanged("PenCursor");
                }
            }
        }

        public void SetCursor()
        {
            //DrawingBrush db = this.FindResource("dPen") as DrawingBrush;
            //Rectangle exampleRec = new Rectangle();
            //exampleRec.Width = 28;
            //exampleRec.Height = 32;
            //exampleRec.Fill = db;
            //this.PenCursor = CursorHelper.CreateCursor(exampleRec, 5, 5);
        }

        private Visibility _isTabletVisible;

        public Visibility IsTabletVisible
        {
            get
            {
                return this._isTabletVisible;
            }
            set
            {
                if (value != this._isTabletVisible)
                {
                    this._isTabletVisible = value;
                    this.NotifyPropertyChanged("IsTabletVisible");
                }
            }
        }

        private ICommand _newCommand;

        public ICommand NewCommand
        {
            get
            {
                return this._newCommand ?? (this._newCommand = new DelegateCommand(this.OnNewClick));
            }
        }

        


        private void OnMyClick()
        {

            this.Visibility = Visibility.Collapsed;
            MessageActions.ShowKeyboard(true);

        }

        private void OnNewClick()
        {
            this.theInkCanvas.Strokes.Clear();
            this.StrokeWordList = new List<StrokeWord>();
        }

        private IList<StrokeWord> _strokeWordList;

        public IList<StrokeWord> StrokeWordList
        {
            get
            {
                return this._strokeWordList;
            }
            set
            {
                if (value != this._strokeWordList)
                {
                    this._strokeWordList = value;
                    this.NotifyPropertyChanged("StrokeWordList");
                }
            }
        }

        ///// <summary>
        ///// 识别
        ///// </summary>
        ///// <param name="strokes">笔迹集合</param>
        ///// <returns>候选词数组</returns>
        //public string[] Recognize(StrokeCollection strokes)
        //{
        //    if (strokes == null || strokes.Count == 0)
        //        return Constants.EmptyAlternates;

        //    var stroke = GetCombinedStore(strokes);

        //    var analyzer = new InkAnalyzer();
        //    analyzer.AddStroke(stroke, Constants.ChsLanguageId);
        //    analyzer.SetStrokeType(stroke, StrokeType.Writing);

        //    var status = analyzer.Analyze();
        //    if (status.Successful)
        //    {
        //        return analyzer.GetAlternates()
        //            .OfType<AnalysisAlternate>()
        //            .Select(x => x.RecognizedString)
        //            .ToArray();
        //    }

        //    analyzer.Dispose();

        //    return Constants.EmptyAlternates;
        //}

        #region 废弃

        //private static System.Windows.Ink.Stroke GetCombinedStore(StrokeCollection strokes)
        //{
        //    var points = new StylusPointCollection();
        //    foreach (var stroke in strokes)
        //    {
        //        points.Add(stroke.StylusPoints);
        //    }
        //    return new System.Windows.Ink.Stroke(points);
        //}

        //private void theInkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        //{
        //    var stroke = GetCombinedStore(this.theInkCanvas.Strokes);

        //    //theInkAnalyzer.AddStrokes(this.theInkCanvas.Strokes, 0x0804);
        //    //theInkAnalyzer.SetStrokesType(this.theInkCanvas.Strokes, StrokeType.Writing);

        //    System.Windows.Ink.InkAnalyzer theInkAnalyzer = new InkAnalyzer();
        //    theInkAnalyzer.AddStroke(stroke, 0x0804);
        //    theInkAnalyzer.SetStrokeType(stroke, StrokeType.Writing);
        //    AnalysisStatus status = theInkAnalyzer.Analyze();

        //    if (status.Successful)
        //    {
        //        List<StrokeWord> slist = new List<StrokeWord>();
        //        //textBox1.Text = theInkAnalyzer.GetRecognizedString();
        //        for (int i = 0; i < theInkAnalyzer.GetAlternates().Count; i++)
        //        {
        //            StrokeWord sw = new StrokeWord();
        //            sw.StrokeName = theInkAnalyzer.GetAlternates()[i].RecognizedString;
        //            slist.Add(sw);
        //        }

        //        this.StrokeWordList = new List<StrokeWord>();
        //        this.StrokeWordList = slist;
        //    }
        //    else
        //    {
        //        MessageBox.Show("识别失败");
        //    }
        //}

        #endregion



        private string _nameStr;

        public string NameStr
        {
            get
            {
                return this._nameStr;
            }
            set
            {
                if (value != this.NameStr)
                {
                    this._nameStr = value;
                    this.NotifyPropertyChanged("NameStr");
                }
            }
        }

        private ICommand _itemCommand;

        public ICommand ItemCommand
        {
            get
            {
                return this._itemCommand ?? (this._itemCommand = new DelegateCommand<Button>(this.OnItemClick));
            }
        }

        private void OnItemClick(Button btn)
        {
            KeyText += btn.Tag.ToString();
            this.NameStr += btn.Tag.ToString();
            this.OnNewClick();
        }

        private ICommand _sureCommand;

        public ICommand SureCommand
        {
            get
            {
                return this._sureCommand ?? (this._sureCommand = new DelegateCommand(this.OnSureClick));
            }
        }

        private void OnSureClick()
        {
            this.IsTabletVisible = Visibility.Hidden;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }


        private void delete_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(KeyText))
            {
                KeyText = KeyText.Substring(0, KeyText.Length - 1);
            }
        }

        
    }

    public class StrokeWord
    {
        public string StrokeName { get; set; }
    }
}