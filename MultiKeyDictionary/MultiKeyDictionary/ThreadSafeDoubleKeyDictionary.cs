using System.Collections.Generic;
using System.Threading;

namespace MultiKeyDictionary
{
    /// <summary>Потокобезопасный словарь</summary>
    public class ThreadSafeDoubleKeyDictionary<TLeftKey, TRightKey, TValue> :
        DoubleKeyDictionary<TLeftKey, TRightKey, TValue>
    {
        private ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        public override void Add(TLeftKey leftKey, TRightKey rightKey, TValue value)
        {
            _readerWriterLock.EnterWriteLock();
            try
            {
                base.Add(leftKey, rightKey, value);
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        public override TValue Get(TLeftKey leftKey, TRightKey rightKey)
        {
            _readerWriterLock.EnterReadLock();
            try
            {
                return base.Get(leftKey, rightKey);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        public override bool TryGetValue(TLeftKey leftKey, TRightKey rightKey, out TValue value)
        {
            _readerWriterLock.EnterReadLock();
            try
            {
                return base.TryGetValue(leftKey, rightKey, out value);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        public override IReadOnlyList<TValue> GetValuesByLeftKey(TLeftKey key)
        {
            _readerWriterLock.EnterReadLock();
            try
            {
                return base.GetValuesByLeftKey(key);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        public override IReadOnlyList<TValue> GetValuesByRightKey(TRightKey key)
        {
            _readerWriterLock.EnterReadLock();
            try
            {
                return base.GetValuesByRightKey(key);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }
    }
}