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
using WpfSimpleHistogram.Interface;

namespace SimpleHistogram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<IHistogramItem> Items { get; set; }
        public MainWindow()
        {
            this.DataContext = this;

            Items = new List<IHistogramItem>()
            {
                new Item(){ XValue = 0.041 },
                new Item(){ XValue = 0.045 },
                new Item(){ XValue = 0.050 },
                new Item(){ XValue = 0.053 },
                new Item(){ XValue = 0.053 },
                new Item(){ XValue = 0.054 },
                new Item(){ XValue = 0.054 },
                new Item(){ XValue = 0.056 },
                new Item(){ XValue = 0.067 },
                new Item(){ XValue = 0.068 },
                new Item(){ XValue = 0.062 },
                new Item(){ XValue = 0.064 },
                new Item(){ XValue = 0.065 },
                new Item(){ XValue = 0.065 },
                new Item(){ XValue = 0.068 },
                new Item(){ XValue = 0.068 },
                new Item(){ XValue = 0.068 },
                new Item(){ XValue = 0.070 },
                new Item(){ XValue = 0.072 },
                new Item(){ XValue = 0.075 },
                new Item(){ XValue = 0.079 },
                new Item(){ XValue = 0.083 },
                new Item(){ XValue = 0.086 },
                new Item(){ XValue = 0.089 },
                new Item(){ XValue = 0.089 },
                new Item(){ XValue = 0.101 },
            };

            for (int i = 0; i < Items.Count(); i++) (Items[i] as Item).ItemId = i + 1;
                
            InitializeComponent();
        }

        private void Histogram_BarClicked(object sender, RoutedEventArgs e)
        {
            var histogram = sender as WpfSimpleHistogram.Histogram;
            var items = histogram.ClickeddItems;

            MessageBox.Show("This bar holds: Item Id " + items.Select(i => ((Item)i).ItemId.ToString()).Aggregate((a, b) => { return a + ", " + b; }), "Bar clicked");
        }
    }

    public class Item : IHistogramItem
    {
        public int ItemId { get; set; }
        public double XValue { get; set; }
    }

    public class StrToDoubleConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = (value as ComboBoxItem).Content.ToString();
            var d = -1.0;
            if(double.TryParse(s, out d))
                return d;
            return -1;  //auto
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
