using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpfSimpleHistogram.Interface;

namespace WpfSimpleHistogram.Model
{
    public class HistogramVM : INotifyPropertyChanged
    {
        string _xLabel;
        public string XLabel
        {
            get { return _xLabel; }
            set
            {
                _xLabel = value;
                RaisePropertyChanged("XLabel");
            }
        }

        string _yLabel;
        public string YLabel
        {
            get { return _yLabel; }
            set
            {
                _yLabel = value;
                RaisePropertyChanged("YLabel");
            }
        }

        IEnumerable<IHistogramItem> _itemsSource;
        public IEnumerable<IHistogramItem> ItemsSource
        {
            set
            {
                _itemsSource = value;
                DrawGraph(_itemsSource, (Decimal)BinSize);
            }
        }

        double? _binSize = null;
        public double? BinSize
        {
            set
            {
                _binSize = value <= 0 ? null : value;
                DrawGraph(_itemsSource, (Decimal)BinSize);
            }
            private get
            {
                if (_binSize == null)
                {
                    return GetProperBinSize();
                }
                return _binSize == 0 ? 1.0 : _binSize;
            }
        }

        int _xLabelDecimals;
        public int XLabelDecimals
        {
            get { return _xLabelDecimals; }
            set
            {
                _xLabelDecimals = value;
                DrawGraph(_itemsSource, (Decimal)BinSize);
                RaisePropertyChanged("XLabelDecimals");
            }
        }

        bool _showCurve = true;
        public bool ShowCurve
        {
            get { return _showCurve; }
            set
            {
                _showCurve = value;
                ChangeCurveVis();
            }
        }

        public Func<double, string> Formatter { get; set; }

        public SeriesCollection SeriesCollection { get; private set; }
        public string[] Labels { get; private set; }
        public string[] CurveAxisLabels { get; private set; }

        public HistogramVM()
        {
            Formatter = v => v.ToString("0");
            SeriesCollection = new SeriesCollection();
        }

        public string GetFormatStr(int dNum)
        {
            return "0" + (dNum > 0 ? "." + new string('0', dNum) : "");
        }

        double GetProperBinSize()
        {
            if (_itemsSource == null || _itemsSource.Count() < 2) return 1;
            var binNum = Math.Sqrt(_itemsSource.Count());

            var min = _itemsSource.Select(i => i.XValue).Min();
            var max = _itemsSource.Select(i => i.XValue).Max();

            var cands = new Decimal[] { 1, 2, 5 };
            for (Decimal d = 0.01m; d <= 100000m; d *= 10)
            {
                foreach (var c in cands)
                {
                    var ret = (double)c * (double)d;
                    if ((max - min) / ret <= binNum) return ret;
                }
            }

            return 10000000;
        }

        List<Bin> _binItems;
        List<string> _categories;
        void DrawGraph(IEnumerable<IHistogramItem> items, Decimal binSize)
        {
            if (items == null) return;
            _binItems = GetBinItems(items, binSize);
            _categories = items.Select(i => i.Category).OrderBy(c => c).Distinct().ToList();

            SeriesCollection.Clear();

            DrawHistogram(_binItems, _categories);
            DrawBellCurve(_binItems);

            RaisePropertyChanged("SeriesCollection");
            RaisePropertyChanged("Labels");
            RaisePropertyChanged("CurveAxisLabels");
            ChangeCurveVis();
        }

        List<Bin> GetBinItems(IEnumerable<IHistogramItem> items, Decimal binSize)
        {
            var ret = new List<Bin>();
            if (items.Count() == 0) return ret;

            if (binSize <= 0) binSize = 1.0M;
            if ((items.Select(i => i.XValue).Max() - items.Select(i => i.XValue).Min()) / (double)binSize > 1000.0)
            {
                binSize = (Decimal)((items.Select(i => i.XValue).Max() - items.Select(i => i.XValue).Min()) / 1000.0);
            }

            var sortedItems = items.OrderBy(i => i.XValue).ToList();

            var val = GetStartVal((Decimal)sortedItems[0].XValue, binSize);
            ret.Add(new Bin(this, val, val + binSize));

            for (int i = 0; i < sortedItems.Count(); i++)
            {
                while ((Decimal)sortedItems[i].XValue >= ret.Last().Right)
                {
                    val += binSize;
                    ret.Add(new Bin(this, val, val + binSize));
                }
                ret.Last().Items.Add(sortedItems[i]);
            }

            return ret;
        }

        Decimal GetStartVal(Decimal minVal, Decimal binSize)
        {
            if (minVal == 0) return minVal;

            if (minVal > 0)
            {
                Decimal ret_pos = 0;
                while (ret_pos <= minVal) ret_pos += binSize;
                return ret_pos - binSize;
            }
            else
            {
                Decimal ret_neg = 0;
                while (ret_neg >= minVal) ret_neg -= binSize;
                return ret_neg;
            }
        }

        // [Left, Right)
        class Bin
        {
            public string Title
            {
                get
                {
                    return Left.ToString(_model.GetFormatStr(_model.XLabelDecimals)) + "-" + Right.ToString(_model.GetFormatStr(_model.XLabelDecimals));
                }
            }
            public Decimal Left;
            public Decimal Right;
            public List<IHistogramItem> Items;

            HistogramVM _model;

            public Bin(HistogramVM model, Decimal left, Decimal right)
            {
                _model = model;
                Left = left;
                Right = right;
                Items = new List<IHistogramItem>();
            }
        }

        void DrawHistogram(List<Bin> bins, IEnumerable<string> categories)
        {
            Labels = bins.Select(s => s.Title).ToArray();

            if (categories.Count() <= 1)
            {
                SeriesCollection.Add(new ColumnSeries()
                {
                    Title = categories.FirstOrDefault(),
                    Values = new ChartValues<Int32>(new List<Int32>(bins.Select(b => b.Items.Count()))),
                    ScalesXAt = 0,
                    Cursor = Cursors.Hand,
                    DataLabels = true,
                    ColumnPadding = 0.5,
                    MaxColumnWidth = 1000,
                });
            }
            else
            {
                foreach (var c in categories)
                {
                    SeriesCollection.Add(new StackedColumnSeries()
                    {
                        Title = c,
                        Values = new ChartValues<Int32>(new List<Int32>(bins.Select(b => b.Items.Count(i => i.Category == c)))),
                        ScalesXAt = 0,
                        Cursor = Cursors.Hand,
                        DataLabels = true,
                        ColumnPadding = 0.5,
                        MaxColumnWidth = 1000
                    });
                }
            }
        }

        private List<double> _bellCurvePoints = new List<double>();
        public IEnumerable<double> BellCurvePoints
        {
            get { return _bellCurvePoints.Select(s => s); }
        }

        void DrawBellCurve(List<Bin> bins)
        {
            if (bins.Count() == 0) return;

            var bLabels = GetBellLabels(bins);
            CurveAxisLabels = bLabels.Select(b => b.ToString(GetFormatStr(XLabelDecimals))).ToArray();

            var allValues = new List<double>();
            foreach (var bin in bins)
            {
                foreach (var item in bin.Items) allValues.Add((double)item.XValue);
            }
            if (allValues.Count() == 0) return;

            var u = allValues.Average();
            var variance = allValues.Sum(v => Math.Pow(v - u, 2)) / (double)allValues.Count();
            var stdDev = Math.Sqrt(variance);
            var a = 1.0 / (stdDev * Math.Sqrt(2.0 * Math.PI));

            var binRange = (double)(bins[0].Right - bins[0].Left);
            var binCount = (double)allValues.Count;
            _bellCurvePoints = bLabels.Select(v => GetGaussian(v, a, u, stdDev, binRange, binCount)).ToList();

            var lineSeries = new LineSeries()
            {
                Values = new ChartValues<double>(_bellCurvePoints),
                Title = "",
                ScalesXAt = 1,
                LineSmoothness = 1, //smooth
                PointGeometry = null,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                LabelPoint = o => ((double)o.Instance).ToString(GetFormatStr(XLabelDecimals)),
            };
            SeriesCollection.Add(lineSeries);
        }

        void ChangeCurveVis()
        {
            // cannot use Visibility dependency prop on LineSeries that causes weired label issue...
            var line = SeriesCollection.FirstOrDefault(s => s.GetType() == typeof(LineSeries)) as LineSeries;
            if (line == null) return;
            line.Stroke = ShowCurve ? Brushes.Gray : Brushes.Transparent;
        }

        double GetGaussian(double x, double a, double u, double stdDev, double binSize, double numOfRec)
        {
            var ret = a * Math.Exp((-Math.Pow(x - u, 2.0)) / (2.0 * stdDev * stdDev)) * binSize * numOfRec;
            return Math.Max(0, ret);
        }

        List<double> GetBellLabels(List<Bin> bins)
        {
            int INTERNAL_POINT_NUM = 10;

            if (bins.Count() < 2) INTERNAL_POINT_NUM = 500;
            if (bins.Count() < 3) INTERNAL_POINT_NUM = 200;
            if (bins.Count() < 5) INTERNAL_POINT_NUM = 100;
            if (bins.Count() < 10) INTERNAL_POINT_NUM = 50;
            if (bins.Count() > 100) INTERNAL_POINT_NUM = 4;
            if (bins.Count() > 500) INTERNAL_POINT_NUM = 2;

            var ret = new List<double>();
            if (bins.Count() <= 0) return ret;

            var gap = (bins[0].Right - bins[0].Left) / (decimal)INTERNAL_POINT_NUM;

            foreach (var b in bins)
            {
                ret.Add((double)b.Left);
                for (int i = 0; i < INTERNAL_POINT_NUM - 1; i++)
                {
                    ret.Add((double)(b.Left + (decimal)(i + 1) * gap));
                }
            }

            ret.Add((double)bins.Last().Right);

            return ret;
        }

        public IEnumerable<IHistogramItem> GetClickedItems(double xIdx, string category)
        {
            var items = _binItems == null || (int)xIdx < 0 || (int)xIdx >= _binItems.Count() ? null : _binItems[(int)xIdx].Items;
            if (items == null) return null;
            if (category != null)
            {
                items = items.Where(i => i.Category == category).ToList();
            }
            return items;
        }

        //category name => bin infos (left, right, count)
        public Dictionary<string, List<Tuple<Decimal, Decimal, int>>> GetStatistic()
        {
            var ret = new Dictionary<string, List<Tuple<Decimal, Decimal, int>>>();
            if (_binItems == null) return null;

            foreach (Series series in SeriesCollection.Where(s => s.GetType() != typeof(LineSeries)))
            {
                var category = series.Title;
                var items = _binItems.Select(b => new Tuple<Decimal, Decimal, int>(b.Left, b.Right, b.Items.Count(i => category == null ? true : i.Category == category))).ToList();
                ret.Add(category == null ? "" : category, items);
            }

            return ret;
        }

        public List<Tuple<string, Brush>> GetLegendInfo()
        {
            var ret = new List<Tuple<string, Brush>>();
            foreach (var s in SeriesCollection.Where(s => s.GetType() != typeof(LineSeries)))
            {
                var series = s as Series;
                ret.Add(new Tuple<string, Brush>(series.Title, series.Fill));
            }

            return ret;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
