using System;
using System.Collections.Generic;

namespace MultiKeyDictionary
{
    public class DoubleKeyDictionary<TLeftKey, TRightKey, TValue>
    {
        private IEqualityComparer<TLeftKey> leftComparer;
        private IEqualityComparer<TRightKey> rightComparer;
        private int size;
        private int capacity;

        //private int[][] buckets;
        //private Entry[][] entries;
        private List<Entry>[][] buckets;

        private struct Entry
        {
            public int leftHashCode;
            public int rightHashCode;
            public TLeftKey LeftKey;
            public TRightKey RightKey;
            public TValue Value;
        }

        public DoubleKeyDictionary(int capacity,
            IEqualityComparer<TLeftKey> leftKeyEqualityComparer,
            IEqualityComparer<TRightKey> rightKeyEqualityComparer)
        {
            this.leftComparer = leftKeyEqualityComparer;
            this.rightComparer = rightKeyEqualityComparer;
            this.capacity = capacity;
        }

        private void Initialize(int capacity)
        {
            var size = HashHelpers.GetPrime(capacity);
            buckets = new List<Entry>[size][];
            this.size = size;
            for (var i = 0; i < size; i++)
            {
                buckets[i] = new List<Entry>[size];
            }
        }

        private void Insert(TLeftKey leftKey, TRightKey rightKey, TValue value)
        {
            if (leftKey == null || rightKey == null)
            {
                throw new ArgumentNullException();
            }

            if (buckets == null) Initialize(0);

            var leftHashCode = leftComparer.GetHashCode(leftKey) % size;
            var rightHashCode = rightComparer.GetHashCode(rightKey) % size;

            var bucket = buckets[leftHashCode][rightHashCode];
            if (bucket == null)
            {
                bucket = new List<Entry>();
                bucket.Add(new Entry
                {
                    leftHashCode = leftHashCode,
                    rightHashCode = rightHashCode,
                    LeftKey = leftKey,
                    RightKey = rightKey,
                    Value = value
                });
                return;
            }

            foreach (var bucketEntry in bucket)
            {
                if (bucketEntry.leftHashCode == leftHashCode && leftComparer.Equals(bucketEntry.LeftKey, leftKey) ||
                    bucketEntry.rightHashCode == rightHashCode &&
                    rightComparer.Equals(bucketEntry.RightKey, rightKey)) ;
                {
                    throw new ArgumentException();
                }
            }

            bucket.Add(new Entry
            {
                leftHashCode = leftHashCode,
                rightHashCode = rightHashCode,
                LeftKey = leftKey,
                RightKey = rightKey,
                Value = value
            });
        }

        public bool TryGetValue(TLeftKey leftKey, TRightKey rightKey, out TValue value)
        {
            var leftHashCode = leftComparer.GetHashCode(leftKey) % size;
            var rightHashCode = rightComparer.GetHashCode(rightKey) % size;

            var bucket = buckets[leftHashCode][rightHashCode];

            if (bucket == null)
            {
                value = default(TValue);
                return false;
            }

            foreach (var bucketEntry in bucket)
            {
                if (bucketEntry.leftHashCode == leftHashCode && leftComparer.Equals(bucketEntry.LeftKey, leftKey) ||
                    bucketEntry.rightHashCode == rightHashCode &&
                    rightComparer.Equals(bucketEntry.RightKey, rightKey)) ;
                {
                    value = bucketEntry.Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }
    }
}