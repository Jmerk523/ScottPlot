﻿using ScottPlot.DataStructures;
using System;

namespace ScottPlot.MinMaxSearchStrategies
{
    public class SegmentedTreeMinMaxSearchStrategy<T> : IMinMaxSearchStrategy<T> where T : struct, IComparable<T>
    {
        private SegmentedTree<T> segmentedTree;

        public bool TreesReady => segmentedTree.TreesReady;
        public SegmentedTreeMinMaxSearchStrategy()
        {
            segmentedTree = new SegmentedTree<T>();
        }

        public SegmentedTreeMinMaxSearchStrategy(in PlotData<T> data) : this()
        {
            SourceArray = data;
        }

        public PlotData<T> SourceArray
        {
            get => segmentedTree.SourceArray;
            set => segmentedTree.SourceArray = value;
        }

        public void MinMaxRangeQuery(int l, int r, out double lowestValue, out double highestValue)
        {
            segmentedTree.MinMaxRangeQuery(l, r, out lowestValue, out highestValue);
        }

        public double SourceElement(int index)
        {
            return Convert.ToDouble(SourceArray[index]);
        }

        public void updateElement(int index, T newValue)
        {
            segmentedTree.updateElement(index, newValue);
        }

        public void updateRange(int from, int to, PlotData<T> newData, int fromData = 0)
        {
            segmentedTree.updateRange(from, to, newData, fromData);
        }
    }
}
