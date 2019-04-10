using System;

namespace Lithnet.GoogleApps
{
    public class PoolItem<T> : IDisposable
    {
        private readonly Pool<T> containingPool;

        public PoolItem(Pool<T> pool, T item)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            this.containingPool = pool;
            this.Item = item;
        }

        public void Dispose()
        {
            if (this.containingPool.IsDisposed)
            {
                IDisposable disposable = this.Item as IDisposable;
                disposable?.Dispose();
            }
            else
            {
                this.containingPool.Return(this);
            }
        }

        public T Item { get; }
    }
}
