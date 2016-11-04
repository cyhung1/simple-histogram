using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfSimpleHistogram.Interface;
using WpfSimpleHistogram.Model;
using System;
using System.Windows.Media;

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

        public event RoutedEventHandler BarClicked
        {
            add { AddHandler(BarClickedEvent, value); }
            remove { RemoveHandler(BarClickedEvent, value); }
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
            (view.DataContext as HistogramViewModel).XLabel = view.XLabel;
            (view.DataContext as HistogramViewModel).YLabel = view.YLabel;
        }

        static void ItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as Histogram;
            (view.DataContext as HistogramViewModel).ItemsSource = view.ItemsSource;
        }

        static void BinSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as Histogram;
            (view.DataContext as HistogramViewModel).BinSize = view.BinSize;
        }

        static void ShowCurveChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as Histogram;
            (view.DataContext as HistogramViewModel).BellCurveVisibility = view.ShowCurve ? Visibility.Visible : Visibility.Hidden;

            //check curve visibility: sometimes binding does not work...
            var needUpdate = false;
            foreach (var s in view.chart.Series)
            {
                var ls = s.GetType() == typeof(LineSeries) ? s as LineSeries : null;
                needUpdate |= ls != null && view.ShowCurve != ls.IsVisible;
            }
            if (needUpdate) view.chart.Update();
        }

        public Histogram()
        {
            this.DataContext = new HistogramViewModel();
            InitializeComponent();
        }

        void CartesianChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            if (chartPoint.SeriesView == null || (chartPoint.SeriesView.GetType() != typeof(ColumnSeries) && chartPoint.SeriesView.GetType() != typeof(StackedColumnSeries))) return;

            var tgtCategory = chartPoint.SeriesView.GetType() == typeof(StackedColumnSeries) ? (chartPoint.SeriesView as StackedColumnSeries).Title : null;
            ClickedItems = (this.DataContext as HistogramViewModel).GetClickedItems(chartPoint.X, tgtCategory);

            var eventArgs = new RoutedEventArgs(Histogram.BarClickedEvent);
            RaiseEvent(eventArgs);
        }

        /// <summary>
        /// return number of items for each bin
        /// </summary>
        /// <returns>bin.Left, bin.Right, number of items</returns>
        public List<Tuple<Decimal, Decimal, int>> GetStatistic()
        {
            return (this.DataContext as HistogramViewModel).GetStatistic();
        }

        public List<Tuple<string, Brush>> GetLegendInfo()
        {
            return (this.DataContext as HistogramViewModel).GetLegendInfo();
        }
    }
}
