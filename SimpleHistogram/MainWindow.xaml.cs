using System;
using System.Collections.Generic;
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
                new Item(){ XValue = 0.04 },
                new Item(){ XValue = 0.04 },
                new Item(){ XValue = 0.05 },
                new Item(){ XValue = 0.05 },
                new Item(){ XValue = 0.05 },
                new Item(){ XValue = 0.05 },
                new Item(){ XValue = 0.05 },
                new Item(){ XValue = 0.05 },
                new Item(){ XValue = 0.06 },
                new Item(){ XValue = 0.06 },
                new Item(){ XValue = 0.06 },
                new Item(){ XValue = 0.06 },
                new Item(){ XValue = 0.06 },
                new Item(){ XValue = 0.06 },
                new Item(){ XValue = 0.06 },
                new Item(){ XValue = 0.06 },
                new Item(){ XValue = 0.06 },
                new Item(){ XValue = 0.07 },
                new Item(){ XValue = 0.07 },
                new Item(){ XValue = 0.07 },
                new Item(){ XValue = 0.07 },
                new Item(){ XValue = 0.08 },
                new Item(){ XValue = 0.08 },
                new Item(){ XValue = 0.08 },
            };

            InitializeComponent();
        }

        private void WpfSimpleHistogram_BarClicked(object sender, RoutedEventArgs e)
        {
            var histogram = sender as WpfSimpleHistogram.View.WpfSimpleHistogram;
            var items = histogram.ClickeddItems;

            MessageBox.Show("This bar contains " + items.Count() + " items", "Bar clicked");
        }
    }

    public class Item : IHistogramItem
    {
        public double XValue { get; set; }
    }
}
