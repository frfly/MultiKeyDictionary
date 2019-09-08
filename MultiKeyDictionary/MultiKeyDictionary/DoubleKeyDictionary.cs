using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace MultiKeyDictionary
{
    public class DoubleKeyDictionary<TLeftKey, TRightKey, TValue>
    {
        private readonly double _loadFactor = 1.5;
        private readonly IEqualityComparer<TLeftKey> _leftComparer;
        private readonly IEqualityComparer<TRightKey> _rightComparer;
        private int _size;
        private int _count;
        private List<Entry>[][] _buckets;

        private struct Entry
        {
            public int LeftHashCode;
            public int RightHashCode;
            public TLeftKey LeftKey;
            public TRightKey RightKey;
            public TValue Value;
        }

        public virtual IReadOnlyList<TValue> GetValuesByLeftKey(TLeftKey key)
        {
            var leftHashCode = (_leftComparer.GetHashCode(key) & 0x7FFFFFFF) % _size;

            var bucketRow = _buckets[leftHashCode];
            var result = new List<TValue>();

            for (var i = 0; i < bucketRow.Length; i++)
            {
                var chain = bucketRow[i];
                for (var j = 0; chain != null && j < chain.Count; j++)
                {
                    var entry = chain[j];
                    if (entry.LeftHashCode == leftHashCode && _leftComparer.Equals(entry.LeftKey, key))
                    {
                        result.Add(entry.Value);
                    }
                }
            }

            return result;
        }

        public virtual IReadOnlyList<TValue> GetValuesByRightKey(TRightKey key)
        {
            var rightHashCode = (_rightComparer.GetHashCode(key) & 0x7FFFFFFF) % _size;

            var result = new List<TValue>();

            for (var i = 0; i < _buckets.Length; i++)
            {
                var bucketRow = _buckets[i];
                var chain = bucketRow[rightHashCode];
                for (var j = 0; chain != null && j < chain.Count; j++)
                {
                    var entry = chain[j];
                    if (entry.RightHashCode == rightHashCode && _rightComparer.Equals(entry.RightKey, key))
                    {
                        result.Add(entry.Value);
                    }
                }
            }

            return result;
        }

        public DoubleKeyDictionary() : this(0, null, null)
        {
        }

        public DoubleKeyDictionary(int capacity) : this(capacity, null, null)
        {
        }

        public DoubleKeyDictionary(int capacity,
            IEqualityComparer<TLeftKey> leftKeyEqualityComparer,
            IEqualityComparer<TRightKey> rightKeyEqualityComparer)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (capacity > 0)
            {
                Initialize(capacity);
            }

            _leftComparer = leftKeyEqualityComparer ?? EqualityComparer<TLeftKey>.Default;
            _rightComparer = rightKeyEqualityComparer ?? EqualityComparer<TRightKey>.Default;
        }


        public virtual TValue Get(TLeftKey leftKey, TRightKey rightKey)
        {
            if (!TryGetValue(leftKey, rightKey, out var value))
            {
                throw new KeyNotFoundException();
            }

            return value;
        }

        public virtual void Add(TLeftKey leftKey, TRightKey rightKey, TValue value)
        {
            if (leftKey == null || rightKey == null)
            {
                throw new ArgumentNullException();
            }

            if (_buckets == null) Initialize(0);

            var leftHashCode = (_leftComparer.GetHashCode(leftKey) & 0x7FFFFFFF) % _size;
            var rightHashCode = (_rightComparer.GetHashCode(rightKey) & 0x7FFFFFFF) % _size;

            var bucket = _buckets[leftHashCode][rightHashCode];
            if (bucket == null)
            {
                bucket = new List<Entry>();
                bucket.Add(new Entry
                {
                    LeftHashCode = leftHashCode,
                    RightHashCode = rightHashCode,
                    LeftKey = leftKey,
                    RightKey = rightKey,
                    Value = value
                });
                _buckets[leftHashCode][rightHashCode] = bucket;
                _count++;
                if (_loadFactor < (double) _count / (_size * _size))
                {
                    Resize();
                }

                return;
            }

            foreach (var bucketEntry in bucket)
            {
                if (bucketEntry.LeftHashCode == leftHashCode && _leftComparer.Equals(bucketEntry.LeftKey, leftKey) &&
                    bucketEntry.RightHashCode == rightHashCode &&
                    _rightComparer.Equals(bucketEntry.RightKey, rightKey))
                {
                    throw new ArgumentException();
                }
            }

            bucket.Add(new Entry
            {
                LeftHashCode = leftHashCode,
                RightHashCode = rightHashCode,
                LeftKey = leftKey,
                RightKey = rightKey,
                Value = value
            });
            _count++;
            if (_loadFactor < (double) _count / (_size * _size))
            {
                Resize();
            }
        }

        public virtual bool TryGetValue(TLeftKey leftKey, TRightKey rightKey, out TValue value)
        {
            var leftHashCode = (_leftComparer.GetHashCode(leftKey) & 0x7FFFFFFF) % _size;
            var rightHashCode = (_rightComparer.GetHashCode(rightKey) & 0x7FFFFFFF) % _size;

            var bucket = _buckets[leftHashCode][rightHashCode];

            if (bucket == null)
            {
                value = default(TValue);
                return false;
            }

            foreach (var bucketEntry in bucket)
            {
                if (bucketEntry.LeftHashCode == leftHashCode && _leftComparer.Equals(bucketEntry.LeftKey, leftKey) &&
                    bucketEntry.RightHashCode == rightHashCode &&
                    _rightComparer.Equals(bucketEntry.RightKey, rightKey)) ;
                {
                    value = bucketEntry.Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        public void Resize()
        {
            var newSize = HashHelpers.ExpandPrime(_count);

            var newBuckets = new List<Entry>[newSize][];
            for (var i = 0; i < newSize; i++)
            {
                newBuckets[i] = new List<Entry>[newSize];
            }

            var oldBuckets = _buckets;
            var oldSize = _size;

            _buckets = newBuckets;
            _size = newSize;

            for (var i = 0; i < oldSize; i++)
            {
                for (var j = 0; j < oldSize; j++)
                {
                    var chain = oldBuckets[i][j];
                    for (var k = 0; chain != null && k < chain.Count; k++)
                    {
                        var entry = chain[k];
                        Add(entry.LeftKey, entry.RightKey, entry.Value);
                    }
                }
            }
        }

        private void Initialize(int capacity)
        {
            var size = HashHelpers.GetPrime(capacity);
            _buckets = new List<Entry>[size][];
            _size = size;
            for (var i = 0; i < size; i++)
            {
                _buckets[i] = new List<Entry>[size];
            }
        }
    }
}