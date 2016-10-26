using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfSimpleHistogram.Interface;
using WpfSimpleHistogram.Model;

namespace WpfSimpleHistogram.View
{
    /// <summary>
    /// Interaction logic for WpfSimpleHistogram.xaml
    /// </summary>
    public partial class WpfSimpleHistogram : UserControl
    {
        public static readonly DependencyProperty YLabelProperty = DependencyProperty.Register("YLabel", typeof(string), 
            typeof(WpfSimpleHistogram), new FrameworkPropertyMetadata("Frequency", new PropertyChangedCallback(LabelChanged)));

        public static readonly DependencyProperty XLabelProperty = DependencyProperty.Register("XLabel", typeof(string),
            typeof(WpfSimpleHistogram), new FrameworkPropertyMetadata("Measures", new PropertyChangedCallback(LabelChanged)));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IHistogramItem>),
            typeof(WpfSimpleHistogram), new FrameworkPropertyMetadata(new List<IHistogramItem>(), new PropertyChangedCallback(ItemsSourceChanged)));

        public static readonly DependencyProperty BinSizeProperty = DependencyProperty.Register("BinSize", typeof(double),
            typeof(WpfSimpleHistogram), new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(BinSizeChanged)));

        public static readonly RoutedEvent BarClickedEvent = EventManager.RegisterRoutedEvent("BarClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WpfSimpleHistogram));

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

        public IEnumerable<IHistogramItem> ClickeddItems { get; private set; }

        public double BinSize
        {
            get { return (double)GetValue(BinSizeProperty); }
            set { SetValue(BinSizeProperty, value); }
        }
        
        static void LabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as WpfSimpleHistogram;
            (view.DataContext as WpfSimpleHistogramViewModel).XLabel = view.XLabel;
            (view.DataContext as WpfSimpleHistogramViewModel).YLabel = view.YLabel;
        }

        static void ItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as WpfSimpleHistogram;
            (view.DataContext as WpfSimpleHistogramViewModel).ItemsSource = view.ItemsSource;
        }

        static void BinSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = obj as WpfSimpleHistogram;
            (view.DataContext as WpfSimpleHistogramViewModel).BinSize = view.BinSize;
        }

        public WpfSimpleHistogram()
        {
            this.DataContext = new WpfSimpleHistogramViewModel();
            InitializeComponent();
        }

        void CartesianChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            var series = chartPoint.SeriesView == null ? null : chartPoint.SeriesView as ColumnSeries;
            if (series == null) return;
            ClickeddItems = (this.DataContext as WpfSimpleHistogramViewModel).GetClickedItems(chartPoint.X);

            var eventArgs = new RoutedEventArgs(WpfSimpleHistogram.BarClickedEvent);
            RaiseEvent(eventArgs);
        }
    }
}
