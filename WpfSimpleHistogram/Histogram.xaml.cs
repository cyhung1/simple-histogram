using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfSimpleHistogram.Interface;
using WpfSimpleHistogram.Model;
using System;
using System.Windows.Media;
using LiveCharts.Wpf.Charts.Base;
using System.Timers;
using System.Windows.Shapes;
using System.Linq;

namespace WpfSimpleHistogram
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : UserControl
    {
        public static readonly DependencyProperty YLabelProperty = DependencyProperty.Register("YLabel", typeof(string), 
            typeof(Histogram), new FrameworkPropertyMetadata("Frequency", new PropertyChangedCallback(LabelChanged)));

        public static readonly DependencyProperty XLabelProperty = DependencyProperty.Register("XLabel", typeof(string),
            typeof(Histogram), new FrameworkPropertyMetadata("Measures", new PropertyChangedCallback(LabelChanged)));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IHistogramItem>),
            typeof(Histogram), new FrameworkPropertyMetadata(new List<IHistogramItem>(), new PropertyChangedCallback(ItemsSourceChanged)));

        public static readonly DependencyProperty BinSizeProperty = DependencyProperty.Register("BinSize", typeof(double?),
            typeof(Histogram), new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(BinSizeChanged)));

        public static readonly DependencyProperty ShowCurveProperty = DependencyProperty.Register("ShowCurve", typeof(bool),
            typeof(Histogram), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(ShowCurveChanged)));

        public static readonly RoutedEvent BarClickedEvent = EventManager.RegisterRoutedEvent("BarClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Histogram));

        public static readonly RoutedEvent UpdaterTickEvent = EventManager.RegisterRoutedEvent("UpdaterTick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Histogram));

        public event RoutedEventHandler BarClicked
        {
            add { AddHandler(BarClickedEvent, value); }
            remove { RemoveHandler(BarClickedEvent, value); }
        }

        public event RoutedEventHandler UpdaterTick
        {
            add { AddHandler(UpdaterTickEvent, value); }
            remove { RemoveHandler(UpdaterTickEvent, value); }
        }
        
        public string YLabel
        {
            get { return (string)GetValue(YLabelProperty); }
            set { SetValue(YLabelProperty, value); }
        }

        public string XLabel
        {
            get { return (string)GetValue(XLabelProperty); }
            set { SetValue(XLabelProperty, value); }
        }

        public IEnumerable<IHistogramItem> ItemsSource
        {
            get { return (IEnumerable<IHistogramItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public IEnumerable<IHistogramItem> ClickedItems { get; private set; }

        public double? BinSize
        {
            get { return (double?)GetValue(BinSizeProperty); }
            set { SetValue(BinSizeProperty, value); }
        }

        public bool ShowCurve
        {
            get { return (bool)GetValue(ShowCurveProperty); }
            set { SetValue(ShowCurveProperty, value); }
        }

        static void LabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as Histogram;
            (view.DataContext as HistogramVM).XLabel = view.XLabel;
            (view.DataContext as HistogramVM).YLabel = view.YLabel;
        }

        static void ItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as Histogram;
            (view.DataContext as HistogramVM).ItemsSource = view.ItemsSource;
        }

        static void BinSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as Histogram;
            (view.DataContext as HistogramVM).BinSize = view.BinSize;
        }

        static void ShowCurveChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as Histogram;
            (view.DataContext as HistogramVM).BellCurveVisibility = view.ShowCurve ? Visibility.Visible : Visibility.Hidden;

            //check curve visibility: sometimes binding does not work...
            var needUpdate = false;
            foreach (var s in view.chart.Series)
            {
                var ls = s.GetType() == typeof(LineSeries) ? s as LineSeries : null;
                needUpdate |= ls != null && view.ShowCurve != ls.IsVisible;
            }
            if (needUpdate) view.chart.Update();
        }

        Timer _updateTimer = new Timer() { Interval = 50 };

        public Histogram()
        {
            this.DataContext = new HistogramVM();
            InitializeComponent();

            Chart.Colors = new List<Color>
            {
                Color.FromArgb(0x80, 0x06, 0x97, 0xFB),
                Color.FromArgb(0x80, 0xd8, 0x3a, 0x3a),
                Color.FromArgb(0x80, 0x22, 0xbd, 0x3f),
                Color.FromArgb(0x80, 0xf1, 0xc9, 0x13),
                Color.FromArgb(0x80, 0x75, 0x5a, 0x9c),
                Color.FromArgb(0x80, 0x00, 0xdd, 0xdd),
                Color.FromArgb(0x80, 0xa0, 0x52, 0x2d),
                Color.FromArgb(0x80, 0xb2, 0xc0, 0xb2),
            };

            _updateTimer.Elapsed += updateTimer_Elapsed;
            _updateTimer.Start();
        }

        void chart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            if (chartPoint.SeriesView == null || (chartPoint.SeriesView.GetType() != typeof(ColumnSeries) && chartPoint.SeriesView.GetType() != typeof(StackedColumnSeries))) return;

            var tgtCategory = chartPoint.SeriesView.GetType() == typeof(StackedColumnSeries) ? (chartPoint.SeriesView as StackedColumnSeries).Title : null;
            ClickedItems = (this.DataContext as HistogramVM).GetClickedItems(chartPoint.X, tgtCategory);

            var eventArgs = new RoutedEventArgs(Histogram.BarClickedEvent);
            RaiseEvent(eventArgs);
        }

        void chart_UpdaterTick()
        {
            var eventArgs = new RoutedEventArgs(Histogram.UpdaterTickEvent);
            RaiseEvent(eventArgs);
        }

        /// <summary>
        /// return number of items for each category / bin
        /// </summary>
        /// <returns>category name => bin infos (left, right, count)</returns>
        public Dictionary<string, List<Tuple<Decimal, Decimal, int>>> GetStatistic()
        {
            return (this.DataContext as HistogramVM).GetStatistic();
        }

        public List<Tuple<string, Brush>> GetLegendInfo()
        {
            return (this.DataContext as HistogramVM).GetLegendInfo();
        }

        void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                DrawAxisLines();
            });
        }

        Point _zeroPoint = new Point(0, 0);

        #region Axis Lines

        Line _xAxisLine;
        Line _yAxisLine;

        void DrawAxisLines()
        {
            var canvas = chart.GetCanvas() as Canvas;
            var originPoint = GetChartEdgePoint(ChartEdge.BottomLeft);
            var trPoint = GetChartEdgePoint(ChartEdge.TopRight);

            if (_xAxisLine == null)
            {
                _xAxisLine = CreateLine("XAxis", true);
                canvas.Children.Add(_xAxisLine);
            }
            _xAxisLine.X1 = originPoint.X;
            _xAxisLine.Y1 = originPoint.Y;
            _xAxisLine.X2 = trPoint.X;
            _xAxisLine.Y2 = originPoint.Y;

            if (_yAxisLine == null)
            {
                _yAxisLine = CreateLine("YAxis", true);
                canvas.Children.Add(_yAxisLine);
            }
            _yAxisLine.X1 = originPoint.X;
            _yAxisLine.Y1 = originPoint.Y;
            _yAxisLine.X2 = originPoint.X;
            _yAxisLine.Y2 = trPoint.Y;
        }

        #endregion

        Line CreateLine(string tag = null, bool isThinLine = false)
        {
            var ret = new Line() { StrokeThickness = isThinLine ? 1 : 2, Stroke = Brushes.Black };
            if (tag != null) ret.Tag = tag;
            return ret;
        }

        IEnumerable<object> GetChildrenFromCanvas(Type t)
        {
            var canvas = chart.GetCanvas() as Canvas;
            foreach (var c in canvas.Children)
            {
                if (c.GetType() == t) yield return c;
            }
        }

        enum ChartEdge { BottomLeft, TopLeft, BottomRight, TopRight }
        Point GetChartEdgePoint(ChartEdge edge)
        {
            var lines = GetChildrenFromCanvas(typeof(Line)).Select(c => (Line)c).Where(c => c.Tag == null);

            switch (edge)
            {
                case ChartEdge.BottomLeft:
                    return new Point(lines.Select(l => Math.Min(l.X1, l.X2)).Min(), lines.Select(l => Math.Max(l.Y1, l.Y2)).Max());
                case ChartEdge.TopLeft:
                    return new Point(lines.Select(l => Math.Min(l.X1, l.X2)).Min(), lines.Select(l => Math.Min(l.Y1, l.Y2)).Min());
                case ChartEdge.BottomRight:
                    return new Point(lines.Select(l => Math.Max(l.X1, l.X2)).Max(), lines.Select(l => Math.Max(l.Y1, l.Y2)).Max());
                default:
                    return new Point(lines.Select(l => Math.Max(l.X1, l.X2)).Max(), lines.Select(l => Math.Min(l.Y1, l.Y2)).Min());
            }
        }
    }
}
