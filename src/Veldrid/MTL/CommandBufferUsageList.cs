using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class CommandBufferUsageList<T>
    {
        private readonly List<(MTLCommandBuffer buffer, T value)> _items = new List<(MTLCommandBuffer buffer, T item)>();

        public void Add(MTLCommandBuffer cb, T value)
            => _items.Add((cb, value));

        public ItemsEnumerator EnumerateItems()
            => new ItemsEnumerator(_items);

        public RemovalEnumerator EnumerateAndRemove(MTLCommandBuffer cb)
            => new RemovalEnumerator(_items, cb);

        public bool Contains(MTLCommandBuffer cb)
        {
            foreach (var (buffer, _) in _items)
            {
                if (buffer.Equals(cb))
                    return true;
            }

            return false;
        }

        public void Clear()
            => _items.Clear();

        /// <summary>
        /// This is a basic enumerator for the list.
        /// </summary>
        public struct ItemsEnumerator : IEnumerator<T>, IEnumerable
        {
            private readonly List<(MTLCommandBuffer buffer, T value)> _list;
            private int _index;

            public ItemsEnumerator(List<(MTLCommandBuffer buffer, T value)> list)
            {
                this._list = list;
            }

            public bool MoveNext()
            {
                if (_index == _list.Count)
                    return false;

                Current = _list[_index].value;
                _index++;

                return true;
            }

            public void Reset()
            {
                _index = 0;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public ItemsEnumerator GetEnumerator() => this;

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// This is a combined enumerate + remove enumerator for the list.
        ///
        /// It works by duplicating the items that shall be retained to the end of the list
        /// and then moving them in-place to the front of the list upon disposal.
        ///
        /// The combined operation has therefore O(n) time complexity.
        /// </summary>
        public struct RemovalEnumerator : IEnumerator<T>, IEnumerable
        {
            private readonly List<(MTLCommandBuffer buffer, T value)> _list;
            private readonly MTLCommandBuffer _cb;
            private readonly int _count;
            private int _index;

            public RemovalEnumerator(List<(MTLCommandBuffer buffer, T value)> list, MTLCommandBuffer cb)
            {
                this._list = list;
                this._cb = cb;

                _count = list.Count;
                list.EnsureCapacity(_count * 2);
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (_index == _count)
                        return false;

                    if (_list[_index].buffer.Equals(_cb))
                        break;

                    // Track the item to be kept.
                    _list.Add(_list[_index]);
                    _index++;
                }

                Current = _list[_index].value;
                _index++;

                return true;
            }

            public void Reset()
            {
                _index = 0;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (_list.Count == 0)
                    return;

                int toKeepItemCount = _list.Count - _count;
                Span<(MTLCommandBuffer buffer, T value)> listSpan = CollectionsMarshal.AsSpan(_list);

                listSpan.Slice(_count, toKeepItemCount).CopyTo(listSpan);
                _list.RemoveRange(toKeepItemCount, _list.Count - toKeepItemCount);
            }

            public RemovalEnumerator GetEnumerator() => this;

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
