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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        List<IHistogramItem> _items;
        public List<IHistogramItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public MainWindow()
        {
            this.DataContext = this;
                
            InitializeComponent();

            singleCategory.IsChecked = true;
        }

        List<IHistogramItem> GetDummyData(bool singleCategory)
        {
            var ret = new List<IHistogramItem>()
            {
                new Item(){ XValue = 0.041, Category = "London" },
                new Item(){ XValue = 0.045, Category = "London" },
                new Item(){ XValue = 0.050, Category = "London" },
                new Item(){ XValue = 0.053, Category = "London" },
                new Item(){ XValue = 0.053, Category = "London" },
                new Item(){ XValue = 0.054, Category = "London" },
                new Item(){ XValue = 0.054, Category = "London" },
                new Item(){ XValue = 0.056, Category = "London" },
                new Item(){ XValue = 0.067, Category = "London" },
                new Item(){ XValue = 0.068, Category = "London" },
                new Item(){ XValue = 0.062, Category = "London" },
                new Item(){ XValue = 0.064, Category = "London" },
                new Item(){ XValue = 0.065, Category = "London" },
                new Item(){ XValue = 0.065, Category = "London" },
                new Item(){ XValue = 0.068, Category = "London" },
                new Item(){ XValue = 0.068, Category = "London" },
                new Item(){ XValue = 0.068, Category = "London" },
                new Item(){ XValue = 0.070, Category = "London" },
                new Item(){ XValue = 0.072, Category = "London" },
                new Item(){ XValue = 0.075, Category = "London" },
                new Item(){ XValue = 0.079, Category = "London" },
                new Item(){ XValue = 0.083, Category = "London" },
                new Item(){ XValue = 0.086, Category = "London" },
                new Item(){ XValue = 0.089, Category = "London" },
                new Item(){ XValue = 0.089, Category = "London" },
                new Item(){ XValue = 0.101, Category = "London" },                
                new Item(){ XValue = 0.030, Category = "New York" },
                new Item(){ XValue = 0.025, Category = "New York" },
                new Item(){ XValue = 0.045, Category = "New York" },
                new Item(){ XValue = 0.051, Category = "New York" },
                new Item(){ XValue = 0.050, Category = "New York" },
                new Item(){ XValue = 0.059, Category = "New York" },
                new Item(){ XValue = 0.056, Category = "New York" },
                new Item(){ XValue = 0.060, Category = "New York" },
                new Item(){ XValue = 0.071, Category = "New York" },
            };

            for (int i = 0; i < ret.Count(); i++) (ret[i] as Item).ItemId = i + 1;
            ret = singleCategory ? ret.Where(i => i.Category == "London").ToList() : ret;
            return ret;
        }
        
        private void singleCategory_Checked(object sender, RoutedEventArgs e)
        {
            Items = GetDummyData(singleCategory.IsChecked == true);
        }

        private void Histogram_BarClicked(object sender, RoutedEventArgs e)
        {
            var histogram = sender as WpfSimpleHistogram.Histogram;
            var items = histogram.ClickedItems;

            MessageBox.Show("This bar holds: Item Id " + items.Select(i => ((Item)i).ItemId.ToString()).Aggregate((a, b) => { return a + ", " + b; }), "Bar clicked");
        }

        private void ShowLegendButton_Click(object sender, RoutedEventArgs e)
        {
            var items = histogram.GetLegendInfo();
            var st1 = new StackPanel() { Orientation = Orientation.Vertical };

            foreach (var item in items)
            {
                var st2 = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(5, 0, 5, 0) };
                st2.Children.Add(new Rectangle() { Fill = item.Item2, Width = 20, Height = 20 });
                st2.Children.Add(new TextBlock() { Text = item.Item1, Margin = new Thickness(5, 0, 5, 0) });
                st1.Children.Add(st2);
            }
            var win = new Window() { Width = 100, Height = 100, Content = st1, Title = "Legend Information", WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner };
            win.ShowDialog();
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

    public class Item : IHistogramItem
    {
        public int ItemId { get; set; }
        public double XValue { get; set; }
        public string Category { get; set; }
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
