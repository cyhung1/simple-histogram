using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfSimpleHistogram
{
    /// <summary>
    /// Interaction logic for CustomLegend.xaml
    /// </summary>
    public partial class CustomLegend : UserControl, IChartLegend
    {
        private List<SeriesViewModel> _series;
        public List<SeriesViewModel> Series
        {
            get { return _series; }
            set
            {
                _series = value == null ? null : value.Where(s => s.Title != "Series").ToList();
                OnPropertyChanged("Series");
            }
        }

        public CustomLegend()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
