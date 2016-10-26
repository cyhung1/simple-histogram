using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using WpfSimpleHistogram.Interface;

namespace WpfSimpleHistogram.Model
{
    public class WpfSimpleHistogramViewModel : INotifyPropertyChanged
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

        double _binSize = -1;
        public double BinSize
        {
            set
            {
                _binSize = value;
                DrawGraph(_itemsSource, (Decimal)BinSize);
            }
            private get
            {
                if (_binSize == -1)
                {
                    var binNum = (int)Math.Sqrt(_itemsSource.Count());
                    _binSize = binNum == 0 ? 1.0 : (_itemsSource.Select(i => i.XValue).Max() - _itemsSource.Select(i => i.XValue).Min()) / (double)binNum;
                }
                return _binSize == 0 ? 1.0 : _binSize;
            }
        }

        public Func<double, string> Formatter { get; set; }

        public SeriesCollection SeriesCollection { get; private set; }
        public string[] Labels { get; private set; }
        public string[] CurveAxisLabels { get; private set; }

        public WpfSimpleHistogramViewModel()
        {
            Formatter = value => value.ToString("0.0");
            SeriesCollection = new SeriesCollection();
        }

        List<Bin> _binItems;
        void DrawGraph(IEnumerable<IHistogramItem> items, Decimal binSize)
        {
            if (items == null) return;
            _binItems = GetBinItems(items, binSize);

            SeriesCollection.Clear();

            DrawHistogram(_binItems);
            DrawBellCurve(_binItems);

            RaisePropertyChanged("SeriesCollection");
            RaisePropertyChanged("Labels");
            RaisePropertyChanged("CurveAxisLabels");
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
            var val = (Decimal)sortedItems[0].XValue;
            ret.Add(new Bin(val, val + binSize));

            for (int i = 0; i < sortedItems.Count(); i++)
            {
                if ((Decimal)sortedItems[i].XValue >= ret.Last().Right)
                {
                    val += binSize;
                    ret.Add(new Bin(val, val + binSize));
                }
                ret.Last().Items.Add(sortedItems[i]);
            }

            return ret;
        }

        // [Left, Right)
        class Bin
        {
            public string Title
            {
                get
                {
                    return Left.ToString("0.00") + "-";
                }
            }

            public Decimal Left;
            public Decimal Right;
            public List<IHistogramItem> Items;

            public Bin(Decimal left, Decimal right)
            {
                Left = left;
                Right = right;
                Items = new List<IHistogramItem>();
            }
        }

        void DrawHistogram(List<Bin> bins)
        {
            Labels = bins.Select(s => s.Title).ToArray();

            SeriesCollection.Add(new ColumnSeries()
            {
                Title = "",
                Values = new ChartValues<Int32>(new List<Int32>(bins.Select(b => b.Items.Count()))),
                ScalesXAt = 0,
                Cursor = Cursors.Hand,
            });
        }

        void DrawBellCurve(List<Bin> bins)
        {
            if (bins.Count() == 0) return;

            var bLabels = GetBellLabels(bins);
            CurveAxisLabels = bLabels.Select(b => b.ToString()).ToArray();

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

            SeriesCollection.Add(new LineSeries()
            {
                Values = new ChartValues<double>(bLabels.Select(v => GetGaussian(v, a, u, stdDev, (double)(bins[0].Right - bins[0].Left), (double)allValues.Count()))),
                ScalesXAt = 1,
                LineSmoothness = 1, //smooth
                PointGeometry = null,
                Fill = Brushes.Transparent,
            });
        }

        double GetGaussian(double x, double a, double u, double stdDev, double binSize, double numOfRec)
        {
            return a * Math.Exp((-Math.Pow(x - u, 2.0)) / (2.0 * stdDev * stdDev)) * binSize * numOfRec;
        }

        List<double> GetBellLabels(List<Bin> bins)
        {
            const int INTERNAL_POINT_NUM = 10;

            var ret = new List<double>();
            if (bins.Count() <= 0) return ret;

            var gap = ((double)bins[0].Right - (double)bins[0].Left) / (double)INTERNAL_POINT_NUM;
            ret.Add((double)bins[0].Left - gap);

            foreach (var b in bins)
            {
                ret.Add((double)b.Left);
                for (int i = 0; i < INTERNAL_POINT_NUM - 1; i++)
                {
                    ret.Add((double)b.Left + (double)(i + 1) * gap);
                }
            }

            ret.Add((double)bins.Last().Right);
            ret.Add((double)bins.Last().Right + gap);

            return ret;
        }

        public IEnumerable<IHistogramItem> GetClickedItems(double xIdx)
        {
            return _binItems == null || (int)xIdx < 0 || (int)xIdx >= _binItems.Count() ? null : _binItems[(int)xIdx].Items;
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
