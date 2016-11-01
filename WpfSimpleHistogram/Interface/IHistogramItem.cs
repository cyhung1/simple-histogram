using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfSimpleHistogram.Interface
{
    public interface IHistogramItem
    {
        double XValue { get; set; }
        string Category { get; set; }
    }
}
