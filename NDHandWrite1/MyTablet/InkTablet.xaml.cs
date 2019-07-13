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
    using System.Windows.Ink;

    using MyTablet.Systems;

    /// <summary>
    /// Interaction logic for InkTablet.xaml
    /// </summary>
    public partial class InkTablet : UserControl, INotifyPropertyChanged
    {
        public delegate void OnTextChangedHandler(string value);

        public event OnTextChangedHandler OnTextChanged;

        public InkTablet()
        {
            InitializeComponent();

            this.theInkCanvas.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(this.theInkCanvas_StrokeCollected);
        }

        ~InkTablet() {
            this.theInkCanvas.StrokeCollected -= new InkCanvasStrokeCollectedEventHandler(this.theInkCanvas_StrokeCollected);
        }

        public string OutputText
        {
            get { return (string)GetValue(OutputTextProperty); }
            set { SetValue(OutputTextProperty, value); }
        }

        public static readonly DependencyProperty OutputTextProperty =
            DependencyProperty.Register("OutputText", typeof(string), typeof(InkTablet), new FrameworkPropertyMetadata { CoerceValueCallback = OnValueChange });

        private static object OnValueChange(DependencyObject d, object baseValue)
        {
            var control = d as InkTablet;
            control?.NoticeChanged(baseValue + "");
            return baseValue;
        }

        public void NoticeChanged(string value)
        {
            OnTextChanged?.Invoke(value);
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
            DrawingBrush db = this.FindResource("dPen") as DrawingBrush;
            Rectangle exampleRec = new Rectangle();
            exampleRec.Width = 28;
            exampleRec.Height = 32;
            exampleRec.Fill = db;
            this.PenCursor = CursorHelper.CreateCursor(exampleRec, 5, 5);
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

        private static Stroke GetCombinedStore(StrokeCollection strokes)
        {
            var points = new StylusPointCollection();
            foreach (var stroke in strokes)
            {
                points.Add(stroke.StylusPoints);
            }
            return new Stroke(points);
        }

        private void theInkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var stroke = GetCombinedStore(this.theInkCanvas.Strokes);

            InkAnalyzer theInkAnalyzer = new InkAnalyzer();
            //theInkAnalyzer.AddStrokes(this.theInkCanvas.Strokes, 0x0804);
            //theInkAnalyzer.SetStrokesType(this.theInkCanvas.Strokes, StrokeType.Writing);

            theInkAnalyzer.AddStroke(stroke, 0x0804);
            theInkAnalyzer.SetStrokeType(stroke, StrokeType.Writing);
            AnalysisStatus status = theInkAnalyzer.Analyze();

            if (status.Successful)
            {
                List<StrokeWord> slist = new List<StrokeWord>();
                //textBox1.Text = theInkAnalyzer.GetRecognizedString();
                for (int i = 0; i < theInkAnalyzer.GetAlternates().Count; i++)
                {
                    StrokeWord sw = new StrokeWord();
                    sw.StrokeName = theInkAnalyzer.GetAlternates()[i].RecognizedString;
                    slist.Add(sw);
                }

                this.StrokeWordList = new List<StrokeWord>();
                this.StrokeWordList = slist;
            }
            else
            {
                MessageBox.Show("识别失败");
            }
        }

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
            OutputText = btn.Tag.ToString();
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
    }

    public class StrokeWord
    {
        public string StrokeName { get; set; }
    }
}