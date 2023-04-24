using System;
using System.Runtime.InteropServices;

namespace Bit.iOS
{
  internal static class BitwardenClientWrapper
  {
#if Android
        const string DllName = "libBitwardenC.so";
#else
    const string DllName = "__Internal";
#endif
      
    [DllImport(DllName, EntryPoint = "init")]
    internal static extern BitwardenClientSafeHandle init(string settings);
    
    [DllImport(DllName, EntryPoint = "free_mem")]
        internal static extern void free_mem(IntPtr clientPtr);

    [DllImport(DllName, EntryPoint = "run_command")]
        internal static extern string run_command(string loginRequest, IntPtr clientPtr);
  }
}