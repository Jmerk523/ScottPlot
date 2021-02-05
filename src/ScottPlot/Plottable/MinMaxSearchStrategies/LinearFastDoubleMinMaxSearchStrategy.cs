using System;

namespace ScottPlot.MinMaxSearchStrategies
{
    public class LinearFastDoubleMinMaxSearchStrategy<T> : LinearMinMaxSearchStrategy<T> where T : struct, IComparable<T>
    {
        private PlotData<double> sourceArrayDouble;

        public override PlotData<T> SourceArray
        {
            get => base.SourceArray;
            set
            {
                value.TryCast(out sourceArrayDouble);
                base.SourceArray = value;
            }
        }

        public override void MinMaxRangeQuery(int l, int r, out double lowestValue, out double highestValue)
        {
            if (sourceArrayDouble.Length > 0)
            {
                lowestValue = sourceArrayDouble[l];
                highestValue = sourceArrayDouble[l];
                for (int i = l; i <= r; i++)
                {
                    if (sourceArrayDouble[i] < lowestValue)
                        lowestValue = sourceArrayDouble[i];
                    if (sourceArrayDouble[i] > highestValue)
                        highestValue = sourceArrayDouble[i];
                }
                return;
            }
            else
            {
                base.MinMaxRangeQuery(l, r, out lowestValue, out highestValue);
            }
        }

        public override double SourceElement(int index)
        {
            if (sourceArrayDouble.Length > 0)
                return sourceArrayDouble[index];
            return Convert.ToDouble(SourceArray[index]);
        }
    }
}
