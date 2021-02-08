using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScottPlot
{
    public static class DataExtensions
    {
        public static int BinarySearch<T>(this in PlotData<T> data, T item)
            where T : IComparable<T>
        {
            return data.Data.Span.BinarySearch(item);
        }

        public static int BinarySearch<T>(this in PlotData<T> data, int start, T item)
            where T : IComparable<T>
        {
            return data.Data.Span.Slice(start).BinarySearch(item) + start;
        }

        public static int BinarySearch<T>(this in PlotData<T> data, int start, int length, T item)
            where T : IComparable<T>
        {
            return data.Data.Span.Slice(start, length).BinarySearch(item) + start;
        }
    }

    // Copied from System.Reactive
    internal sealed class AnonymousDisposable : IDisposable
    {
        private volatile Action _dispose;

        public AnonymousDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _dispose, null)?.Invoke();
        }
    }

    internal sealed class AnonymousDisposable<T> : IDisposable
    {
        private volatile Action<T> _dispose;
        private T _state;

        public AnonymousDisposable(T state, Action<T> dispose)
        {
            _dispose = dispose;
            _state = state;
        }

        public void Dispose()
        {
            var disposal = Interlocked.Exchange(ref _dispose, null);
            if (disposal != null)
            {
                disposal(_state);
                _state = default;
            }
        }
    }

    public readonly struct PlotData<T> : IReadOnlyCollection<T>
    {
        public static IDisposable FromArrayPool(int count, out PlotData<T> data, bool clearWhenDisposed = true)
        {
            var array = ArrayPool<T>.Shared.Rent(count);
            data = array.AsMemory().Slice(0, count);
            return new AnonymousDisposable<T[]>(array, a => ArrayPool<T>.Shared.Return(a, clearWhenDisposed));
        }

        private readonly ReadOnlyMemory<T> memory;

        public bool IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public ReadOnlyMemory<T> Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => memory;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => memory.Length;
        }

        int IReadOnlyCollection<T>.Count => memory.Length;

        public bool IsEmpty => memory.Length == 0;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => memory.Span[index];
            // For fastest (and ideally inlined) writes, prefer to make one call to TryGetWritable
            // and then do the rest of the work on the Memory<T> result
            readonly set
            {
                if (TryGetWritable(out var writable))
                {
                    writable.Span[index] = value;
                }
                else
                {
                    throw new InvalidOperationException("Plot data is read-only");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PlotData(in ReadOnlyMemory<T> memory, bool readOnly)
        {
            this.memory = memory;
            IsReadOnly = readOnly;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlotData(in ReadOnlyMemory<T> memory) : this(memory, true)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlotData(in Memory<T> memory) : this(memory, false)
        {
        }

        public PlotData(T[] data, bool readOnly = false)
            : this(data?.AsMemory() ?? throw new ArgumentNullException(nameof(data)), readOnly)
        {
        }

        public PlotData(ArraySegment<T> segment, bool readOnly = false)
            : this(segment.AsMemory(), readOnly)
        {
        }

        public PlotData(int length) : this(new T[length])
        {
        }

        public PlotData<T> AsReadOnly()
        {
            return new PlotData<T>(memory, true);
        }

        public PlotData2<T> As2D(int stride)
        {
            if (TryGetWritable(out var writable))
            {
                return new PlotData2<T>(writable, stride);
            }
            else
            {
                return new PlotData2<T>(memory, stride);
            }
        }

        public bool TryGetWritable(out Memory<T> memory)
        {
            if (!IsReadOnly)
            {
                memory = MemoryMarshal.AsMemory(this.memory);
                return true;
            }
            memory = default;
            return false;
        }

        public PlotData<U> Cast<U>()
        {
            if (TryCast(out PlotData<U> data))
            {
                return data;
            }
            throw new InvalidCastException($"Cannot cast {typeof(T).FullName} data to {typeof(U).FullName} data");
        }

        public bool TryCast<U>(out PlotData<U> data)
        {
            if (this is PlotData<U> uData)
            {
                data = uData;
                return true;
            }
            data = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlotData<T> Slice(int start)
        {
            return new PlotData<T>(memory.Slice(start), IsReadOnly);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlotData<T> Slice(int start, int length)
        {
            return new PlotData<T>(memory.Slice(start, length), IsReadOnly);
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PlotData<T>(in ReadOnlyMemory<T> memory)
        {
            return new PlotData<T>(memory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PlotData<T>(in Memory<T> memory)
        {
            return new PlotData<T>(memory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PlotData<T>(T[] array)
        {
            if (array == null)
            {
                // Just return an empty set of plot data, which allows passing parameter 'null' without exception
                return default;
            }
            return new PlotData<T>(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>(in PlotData<T> data)
        {
            return data.memory;
        }

        public static explicit operator Memory<T>(in PlotData<T> data)
        {
            if (data.TryGetWritable(out var memory))
            {
                return memory;
            }
            throw new InvalidCastException("Memory is read-only");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(in PlotData<T> data)
        {
            return data.memory.Span;
        }

        public static explicit operator Span<T>(in PlotData<T> data)
        {
            if (data.TryGetWritable(out var memory))
            {
                return memory.Span;
            }
            throw new InvalidCastException("Memory is read-only");
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly ReadOnlyMemory<T> memory;

            private int state;

            public T Current
            {
                get
                {
                    if (state == 0)
                    {
                        throw new InvalidOperationException("Must call MoveNext() before accessing Current");
                    }
                    else if (state < 0)
                    {
                        throw new ObjectDisposedException(typeof(Enumerator).FullName);
                    }
                    return memory.Span[state - 1];
                }
            }

            object IEnumerator.Current => Current;

            public Enumerator(in PlotData<T> data)
            {
                memory = data.memory;
                state = 0;
            }

            public void Dispose()
            {
                state = -1;
            }

            public bool MoveNext()
            {
                if (state < 0 || state >= memory.Length)
                {
                    return false;
                }
                ++state;
                return true;
            }

            public void Reset()
            {
                state = 0;
            }
        }
    }

    #region 2D Stuff (needs unsafe/hacks)

    public class PlotData2MemoryManager<T> : MemoryManager<T>
    {
        private readonly T[,] array;

        public PlotData2MemoryManager(T[,] array)
        {
            this.array = array ?? throw new ArgumentNullException(nameof(array));
        }

        public unsafe override Span<T> GetSpan()
        {
            // Would require /unsafe
            return new Span<T>(Unsafe.AsPointer(ref array[0, 0]), array.Length);
        }

        public unsafe override MemoryHandle Pin(int elementIndex = 0)
        {
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            // Would require /unsafe
            return new MemoryHandle(Unsafe.AsPointer(ref array[0, 0]), handle, null);
        }

        public override void Unpin()
        {
            // Already done by GCHandle.Free when MemoryHandle is disposed
        }

        protected override void Dispose(bool disposing)
        {
        }
    }

    public readonly struct PlotData2<T> : IReadOnlyCollection<T>
    {
        private readonly Memory<T> memory;
        private readonly int stride;

        // Slice bounds
        private readonly int startCol;
        private readonly int startRow;
        private readonly int lengthCol;
        private readonly int lengthRow;

        private readonly bool contiguous;

        public int Columns => lengthCol;
        public int Rows => lengthRow;

        public bool IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        // Would we even want to expose this if we allow slicing?
        public ReadOnlyMemory<T> Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => memory;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => contiguous ? memory.Length : lengthCol * lengthRow;
        }

        int IReadOnlyCollection<T>.Count => Length;

        public bool IsEmpty => lengthCol == 0 && lengthRow == 0;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => memory.Span[contiguous ? index : GetSlicedIndex(index)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Plot data is read-only");
                }
                else
                {
                    memory.Span[contiguous ? index : GetSlicedIndex(index)] = value;
                }
            }
        }

        // 2D access
        public T this[int row, int col]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => memory.Span[contiguous ? col + stride * row : GetSlicedIndex(col, row)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Plot data is read-only");
                }
                else
                {
                    memory.Span[contiguous ? col + stride * row : GetSlicedIndex(col, row)] = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PlotData2(in Memory<T> memory, bool readOnly, int stride, int startCol, int startRow, int lengthCol, int lengthRow)
        {
            this.memory = memory;
            IsReadOnly = readOnly;
            this.stride = stride;

            this.startCol = startCol;
            this.startRow = startRow;
            this.lengthCol = lengthCol;
            this.lengthRow = lengthRow;

            contiguous = startCol == 0 && startRow == 0 && lengthCol == stride && lengthRow <= memory.Length / stride;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PlotData2(in Memory<T> memory, bool readOnly, int stride, int rows = -1)
            : this(memory, readOnly, stride, 0, 0, stride, rows < 0 ? memory.Length / stride : rows)
        {
        }

        public PlotData2(in ReadOnlyMemory<T> memory, int stride = 0)
            : this(MemoryMarshal.AsMemory(memory), true, stride > 0 ? stride : memory.Length)
        {
        }

        public PlotData2(in Memory<T> memory, int stride = 0)
            : this(memory, false, stride > 0 ? stride : memory.Length)
        {
        }

        public PlotData2(T[,] data, bool readOnly = false)
            : this(new PlotData2MemoryManager<T>(data).Memory, readOnly, data.GetLength(1), data.GetLength(0))
        {
        }

        public PlotData2(int rows, int columns)
            : this(new T[columns * rows], false, columns, rows)
        {
        }

        public PlotData2<T> AsReadOnly()
        {
            return new PlotData2<T>(memory, true, stride, startCol, startRow, lengthCol, lengthRow);
        }

        private int GetSlicedIndex(int index)
        {
            var y = Math.DivRem(index, lengthCol, out int x);
            if (index < 0 || y >= lengthRow)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return (startRow + y) * stride + (startCol + x);
        }

        private int GetSlicedIndex(int col, int row)
        {
            if (col < 0 || col >= lengthCol)
            {
                throw new ArgumentOutOfRangeException(nameof(col));
            }
            if (row < 0 || row >= lengthRow)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }
            return (startRow + row) * stride + (startCol + col);
        }

        public bool TryGetWritable(out Memory<T> memory)
        {
            if (!IsReadOnly)
            {
                memory = this.memory;
                return true;
            }
            memory = default;
            return false;
        }

        public PlotData2<U> Cast<U>()
        {
            if (TryCast(out PlotData2<U> data))
            {
                return data;
            }
            throw new InvalidCastException($"Cannot cast {typeof(T).FullName} data to {typeof(U).FullName} data");
        }

        public bool TryCast<U>(out PlotData2<U> data)
        {
            if (this is PlotData2<U> uData)
            {
                data = uData;
                return true;
            }
            data = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlotData2<T> Slice(int startCol, int startRow)
        {
            return Slice(startCol, startRow, lengthCol - startCol, lengthRow - startRow);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlotData2<T> Slice(int startCol, int startRow, int columns, int rows)
        {
            if (columns < 0 || columns > lengthCol)
            {
                throw new ArgumentOutOfRangeException(nameof(columns));
            }
            if (rows < 0 || rows > lengthRow)
            {
                throw new ArgumentOutOfRangeException(nameof(rows));
            }
            return new PlotData2<T>(
                memory, IsReadOnly, stride,
                startCol + this.startCol,
                startRow + this.startRow,
                columns, rows);
        }

        public PlotData<T> Flatten()
        {
            if (contiguous)
            {
                var flattened = memory.Slice(0, stride * lengthRow);
                if (IsReadOnly)
                {
                    return new PlotData<T>((ReadOnlyMemory<T>)flattened);
                }
                else
                {
                    return new PlotData<T>(flattened);
                }
            }
            else
            {
                // time to copy the sub-ranges
                throw new NotImplementedException();
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PlotData2<T>(in ReadOnlyMemory<T> memory)
        {
            return new PlotData2<T>(memory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PlotData2<T>(in Memory<T> memory)
        {
            return new PlotData2<T>(memory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>(in PlotData2<T> data)
        {
            return data.memory;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PlotData2<T>(T[,] array)
        {
            return new PlotData2<T>(array);
        }

        public static explicit operator Memory<T>(in PlotData2<T> data)
        {
            if (data.TryGetWritable(out var memory))
            {
                return memory;
            }
            throw new InvalidCastException("Memory is read-only");
        }

        public static explicit operator PlotData<T>(in PlotData2<T> data)
        {
            return data.Flatten();
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly ReadOnlyMemory<T> memory;

            private int state;

            public T Current
            {
                get
                {
                    if (state == 0)
                    {
                        throw new InvalidOperationException("Must call MoveNext() before accessing Current");
                    }
                    else if (state < 0)
                    {
                        throw new ObjectDisposedException(typeof(Enumerator).FullName);
                    }
                    return memory.Span[state - 1];
                }
            }

            object IEnumerator.Current => Current;

            public Enumerator(in PlotData2<T> data)
            {
                memory = data.memory;
                state = 0;
            }

            public void Dispose()
            {
                state = -1;
            }

            public bool MoveNext()
            {
                if (state < 0 || state >= memory.Length)
                {
                    return false;
                }
                ++state;
                return true;
            }

            public void Reset()
            {
                state = 0;
            }
        }
    }

    #endregion
}
