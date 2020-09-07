using Microsoft.Diagnostics.Runtime;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoScope.Core.Dac
{
    public abstract class AbstractDacFinder : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern LibrarySafeHandle LoadLibrary(string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        internal LibrarySafeHandle dbgHelpLib;
        protected Process process;
        protected readonly string searchPath;

        protected abstract bool SymCleanup(IntPtr hProcess);
        protected abstract bool SymInitialize(IntPtr hProcess, string symPath, bool fInvadeProcess);
        protected abstract void InitDbgHelpModule();
        protected abstract bool SymFindFileInPath(IntPtr hProcess, string searchPath, string filename, uint id, uint two, uint three, uint flags, StringBuilder filePath, IntPtr callback, IntPtr context);

        protected AbstractDacFinder(string localCache)
        {
            searchPath = $"SRV*{localCache}*http://msdl.microsoft.com/download/symbols";
            process = Process.GetCurrentProcess();
        }

        public void Init()
        {
            InitDbgHelpModule();

            if (dbgHelpLib.IsInvalid)
                throw new InvalidOperationException("Could not load dbghelp.dll", new Win32Exception(Marshal.GetLastWin32Error()));

            if (!SymInitialize(process.Handle, searchPath, false))
                throw new InvalidOperationException("SymInitialize: Unexpected error occured.", new Win32Exception(Marshal.GetLastWin32Error()));
        }

        public string FindDac(ClrInfo clrInfo) => FindDac(clrInfo.DacInfo.FileName, clrInfo.DacInfo.TimeStamp, clrInfo.DacInfo.FileSize);

        public string FindDac(string dacname, uint timestamp, uint fileSize)
        {
            StringBuilder symbolFile = new StringBuilder(2048);
            return SymFindFileInPath(process.Handle, searchPath, dacname, timestamp, fileSize, 0, 0x02, symbolFile, IntPtr.Zero, IntPtr.Zero)
                ? symbolFile.ToString()
                : throw new InvalidOperationException($"Unable to find dac file '{dacname}' in symbol server.", new Win32Exception(Marshal.GetLastWin32Error()));
        }

        internal class LibrarySafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public LibrarySafeHandle() : base(true)
            {
            }
            public LibrarySafeHandle(IntPtr handle) : base(true) => SetHandle(handle);
            protected override bool ReleaseHandle() => FreeLibrary(this.handle);
        }

        public void Dispose()
        {
            if (process != null)
            {
                try
                {
                    SymCleanup(process.Handle);
                }
                finally
                {
                    process.Dispose();
                }

                process = null;
            }
            if (dbgHelpLib?.IsClosed != false)
                return;
            dbgHelpLib.Dispose();
            dbgHelpLib = null;
        }
    }
}