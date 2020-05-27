using System;

namespace PulseCheck.Data.Repositories
{
    public class BaseRepository : IDisposable
    {
        protected readonly IbexContext _context;
        private bool disposed = false;

        public BaseRepository(IbexContext context)
        {
            this._context = context;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
