using System;
using System.Drawing;

namespace ScottPlot
{
    public static class Validate
    {
        private static string ValidLabel(string label) =>
            string.IsNullOrWhiteSpace(label) ? "[unknown variable]" : label;

        /// <summary>
        /// Throw an exception if the value is NaN or infinity
        /// </summary>
        public static void AssertIsReal(string label, double value)
        {
            label = ValidLabel(label);

            if (double.IsNaN(value))
                throw new InvalidOperationException($"{label} is NaN");

            if (double.IsInfinity(value))
                throw new InvalidOperationException($"{label} is infinity");
        }

        /// <summary>
        /// Throw an exception if the array is null or contains NaN or infinity
        /// </summary>
        public static void AssertAllReal(string label, in PlotData<double> values)
        {
            label = ValidLabel(label);

            for (int i = 0; i < values.Length; i++)
                if (double.IsNaN(values[i]) || double.IsInfinity(values[i]))
                    throw new InvalidOperationException($"{label} index {i} is invalid ({values[i]})");
        }

        /// <summary>
        /// Throw an exception if the array is null or contains NaN or infinity
        /// </summary>
        public static void AssertAllReal(string label, in PlotData<float> values)
        {
            label = ValidLabel(label);

            for (int i = 0; i < values.Length; i++)
                if (float.IsNaN(values[i]) || float.IsInfinity(values[i]))
                    throw new InvalidOperationException($"{label} index {i} is invalid ({values[i]})");
        }

        /// <summary>
        /// Throw an exception if the array is null or contains NaN or infinity
        /// </summary>
        public static void AssertAllReal<T>(string label, in PlotData<T> values)
        {
            if (values.TryCast<double>(out var doubleValues))
                AssertAllReal(label, doubleValues);
            else if (values.TryCast<float>(out var floatValues))
                AssertAllReal(label, floatValues);
            else
                throw new InvalidOperationException("values must be float[] or double[]");
        }

        /// <summary>
        /// Throw an exception if one elemnt is equal to or less than the previous element
        /// </summary>
        public static void AssertAscending(string label, in PlotData<double> values)
        {
            label = ValidLabel(label);

            for (int i = 0; i < values.Length - 1; i++)
                if (values[i] >= values[i + 1])
                    throw new InvalidOperationException($"{label} must be ascending values (index {i} >= {i + 1}");
        }

        /// <summary>
        /// Throw an exception if one elemnt is equal to or less than the previous element
        /// </summary>
        public static void AssertAscending<T>(string label, in PlotData<T> values)
        {
            label = ValidLabel(label);

            for (int i = 0; i < values.Length - 1; i++)
                if (Convert.ToDouble(values[i]) >= Convert.ToDouble(values[i + 1]))
                    throw new InvalidOperationException($"{label} must be ascending values (index {i} >= {i + 1}");
        }

        /// <summary>
        /// Throw an exception if the array does not contain at least one element
        /// </summary>
        public static void AssertHasElements(string label, in PlotData<double> values)
        {
            label = ValidLabel(label);

            if (values.Length == 0)
                throw new InvalidOperationException($"{label} must contain at least one element");
        }

        /// <summary>
        /// Throw an exception if the array does not contain at least one element
        /// </summary>
        public static void AssertHasElements<T>(string label, in PlotData<T> values)
        {
            label = ValidLabel(label);

            if (values.Length == 0)
                throw new InvalidOperationException($"{label} must contain at least one element");
        }

        /// <summary>
        /// Throw an exception if the array does not contain at least one element
        /// </summary>
        public static void AssertHasElements(string label, Color[] values)
        {
            label = ValidLabel(label);

            if (values is null)
                throw new InvalidOperationException($"{label} must not be null");

            if (values.Length == 0)
                throw new InvalidOperationException($"{label} must contain at least one element");
        }

        /// <summary>
        /// Throw an exception if the array does not contain at least one element
        /// </summary>
        public static void AssertHasElements(string label, string[] values)
        {
            label = ValidLabel(label);

            if (values is null)
                throw new InvalidOperationException($"{label} must not be null");

            if (values.Length == 0)
                throw new InvalidOperationException($"{label} must contain at least one element");
        }

        /// <summary>
        /// Throw an exception if non-null arrays have different lengths
        /// </summary>
        public static void AssertEqualLength(string label,
            in PlotData<double> a, in PlotData<double> b = default, in PlotData<double> c = default,
            in PlotData<double> d = default, in PlotData<double> e = default, in PlotData<double> f = default)
        {
            label = ValidLabel(label);

            if (!IsEqualLength(a, b, c, d, e, f))
                throw new InvalidOperationException($"{label} must all have same length");
        }

        /// <summary>
        /// Throw an exception if non-null arrays have different lengths
        /// </summary>
        public static void AssertEqualLength<T1, T2>(string label, in PlotData<T1> a, in PlotData<T2> b)
        {
            label = ValidLabel(label);

            if (a.Length != b.Length)
                throw new InvalidOperationException($"{label} must all have same length");
        }

        /// <summary>
        /// Returns true if all non-null arguments have equal length
        /// </summary>
        public static bool IsEqualLength(in PlotData<double> a, in PlotData<double> b = default, in PlotData<double> c = default,
                                         in PlotData<double> d = default, in PlotData<double> e = default, in PlotData<double> f = default)
        {
            if (a.IsEmpty)
                throw new InvalidOperationException("First data set must contain data");
            if (!b.IsEmpty && b.Length != a.Length) return false;
            if (!c.IsEmpty && c.Length != a.Length) return false;
            if (!d.IsEmpty && d.Length != a.Length) return false;
            if (!e.IsEmpty && e.Length != a.Length) return false;
            if (!f.IsEmpty && f.Length != a.Length) return false;
            return true;
        }

        /// <summary>
        /// Throws an exception if the string is null, empty, or only contains whitespace
        /// </summary>
        public static void AssertHasText(string label, string value)
        {
            label = ValidLabel(label);

            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"{label} must contain text");
        }
    }
}
