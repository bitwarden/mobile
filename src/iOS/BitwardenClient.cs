using System;

namespace Bit.iOS
{
    public class BitwardenClient  : IDisposable
    {
        private readonly BitwardenClientSafeHandle handle;

        public BitwardenClient()
        {
            handle = BitwardenClientWrapper.init("");
        }

        public string Fingerprint()
        {
            return BitwardenClientWrapper.run_command("{}", handle.Ptr);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (handle != null && !handle.IsInvalid)
                handle.Dispose();
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}