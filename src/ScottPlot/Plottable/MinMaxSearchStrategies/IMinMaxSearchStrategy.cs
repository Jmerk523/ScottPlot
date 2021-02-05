namespace ScottPlot.MinMaxSearchStrategies
{
    public interface IMinMaxSearchStrategy<T>
    {
        PlotData<T> SourceArray { get; set; }
        void MinMaxRangeQuery(int l, int r, out double lowestValue, out double highestValue);
        void updateElement(int index, T newValue);
        void updateRange(int from, int to, PlotData<T> newData, int fromData = 0);
        double SourceElement(int index);
    }
}
