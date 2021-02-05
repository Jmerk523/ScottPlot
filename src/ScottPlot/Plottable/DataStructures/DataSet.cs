using System;
using System.Collections.Generic;
using System.Text;

namespace ScottPlot
{
    /// <summary>
    /// Represents a series of data values with a common name. Values from several DataSets can be grouped (by value index).
    /// </summary>
    public class DataSet
    {
        public string label;
        public PlotData<double> values;
        public PlotData<double> errors;

        public DataSet(string label, in PlotData<double> values, in PlotData<double> errors = default)
        {
            this.values = values;
            this.label = label;
            this.errors = errors;

            if (errors.Length > 0 && errors.Length != values.Length)
                throw new ArgumentException("values and errors must have identical length");
        }
    }
}
