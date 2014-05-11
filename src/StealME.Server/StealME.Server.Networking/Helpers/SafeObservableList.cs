namespace StealME.Server.Networking.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Threading;
    using System.Windows.Threading;

    public class SafeObservable<T> : IList<T>, INotifyCollectionChanged
    {
        private IList<T> collection = new List<T>();
        private Dispatcher dispatcher;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private ReaderWriterLock sync = new ReaderWriterLock();

        public SafeObservable()
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Add(T item)
        {
            if (Thread.CurrentThread == this.dispatcher.Thread)
                this.DoAdd(item);
            else
                this.dispatcher.BeginInvoke((Action)(() => { this.DoAdd(item); }));
        }

        private void DoAdd(T item)
        {
            this.sync.AcquireWriterLock(Timeout.Infinite);
            this.collection.Add(item);
            if (this.CollectionChanged != null)
                this.CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            this.sync.ReleaseWriterLock();
        }

        public void Clear()
        {
            if (Thread.CurrentThread == this.dispatcher.Thread)
                this.DoClear();
            else
                this.dispatcher.BeginInvoke((Action)(() => { this.DoClear(); }));
        }

        private void DoClear()
        {
            this.sync.AcquireWriterLock(Timeout.Infinite);
            this.collection.Clear();
            if (this.CollectionChanged != null)
                this.CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            this.sync.ReleaseWriterLock();
        }

        public bool Contains(T item)
        {
            this.sync.AcquireReaderLock(Timeout.Infinite);
            var result = this.collection.Contains(item);
            this.sync.ReleaseReaderLock();
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.sync.AcquireWriterLock(Timeout.Infinite);
            this.collection.CopyTo(array, arrayIndex);
            this.sync.ReleaseWriterLock();
        }

        public int Count
        {
            get
            {
                this.sync.AcquireReaderLock(Timeout.Infinite);
                var result = this.collection.Count;
                this.sync.ReleaseReaderLock();
                return result;
            }
        }

        public bool IsReadOnly
        {
            get { return this.collection.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            if (Thread.CurrentThread == this.dispatcher.Thread)
                return this.DoRemove(item);
            else
            {
                var op = this.dispatcher.BeginInvoke(new Func<T, bool>(this.DoRemove), item);
                if (op == null || op.Result == null)
                    return false;
                return (bool)op.Result;
            }
        }

        private bool DoRemove(T item)
        {
            this.sync.AcquireWriterLock(Timeout.Infinite);
            var index = this.collection.IndexOf(item);
            if (index == -1)
            {
                this.sync.ReleaseWriterLock();
                return false;
            }
            var result = this.collection.Remove(item);
            if (result && this.CollectionChanged != null)
                this.CollectionChanged(this, new
                    NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            this.sync.ReleaseWriterLock();
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            this.sync.AcquireReaderLock(Timeout.Infinite);
            var result = this.collection.IndexOf(item);
            this.sync.ReleaseReaderLock();
            return result;
        }

        public void Insert(int index, T item)
        {
            if (Thread.CurrentThread == this.dispatcher.Thread)
                this.DoInsert(index, item);
            else
                this.dispatcher.BeginInvoke((Action)(() => { this.DoInsert(index, item); }));
        }

        private void DoInsert(int index, T item)
        {
            this.sync.AcquireWriterLock(Timeout.Infinite);
            this.collection.Insert(index, item);
            if (this.CollectionChanged != null)
                this.CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            this.sync.ReleaseWriterLock();
        }

        public void RemoveAt(int index)
        {
            if (Thread.CurrentThread == this.dispatcher.Thread)
                this.DoRemoveAt(index);
            else
                this.dispatcher.BeginInvoke((Action)(() => { this.DoRemoveAt(index); }));
        }

        private void DoRemoveAt(int index)
        {
            this.sync.AcquireWriterLock(Timeout.Infinite);
            if (this.collection.Count == 0 || this.collection.Count <= index)
            {
                this.sync.ReleaseWriterLock();
                return;
            }
            this.collection.RemoveAt(index);
            if (this.CollectionChanged != null)
                this.CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            this.sync.ReleaseWriterLock();

        }

        public T this[int index]
        {
            get
            {
                this.sync.AcquireReaderLock(Timeout.Infinite);
                var result = this.collection[index];
                this.sync.ReleaseReaderLock();
                return result;
            }
            set
            {
                this.sync.AcquireWriterLock(Timeout.Infinite);
                if (this.collection.Count == 0 || this.collection.Count <= index)
                {
                    this.sync.ReleaseWriterLock();
                    return;
                }
                this.collection[index] = value;
                this.sync.ReleaseWriterLock();
            }

        }
    }
}
