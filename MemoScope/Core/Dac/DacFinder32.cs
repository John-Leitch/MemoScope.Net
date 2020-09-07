using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoScope.Core.Dac
{
    public class DacFinder32 : AbstractDacFinder
    {
        private const string libDbghelpDll = "libs\\_x86\\dbghelp.dll";

        public DacFinder32(string searchPath) : base(searchPath)
        {
        }

        [DllImport(libDbghelpDll, EntryPoint = "SymInitialize", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SymInitialize32(IntPtr hProcess, string symPath, bool fInvadeProcess);

        [DllImport(libDbghelpDll, EntryPoint = "SymCleanup", SetLastError = true)]
        private static extern bool SymCleanup32(IntPtr hProcess);

        [DllImport(libDbghelpDll, EntryPoint = "SymFindFileInPath", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SymFindFileInPath32(IntPtr hProcess, string searchPath, string filename, uint id, uint two, uint three, uint flags, StringBuilder filePath, IntPtr callback, IntPtr context);

        protected override bool SymCleanup(IntPtr hProcess) => SymCleanup32(hProcess);

        protected override bool SymInitialize(IntPtr hProcess, string symPath, bool fInvadeProcess) => SymInitialize32(hProcess, symPath, fInvadeProcess);

        protected override void InitDbgHelpModule() => dbgHelpLib = LoadLibrary(libDbghelpDll);

        protected override bool SymFindFileInPath(IntPtr hProcess, string searchPath, string filename, uint id, uint two, uint three, uint flags, StringBuilder filePath, IntPtr callback, IntPtr context) => SymFindFileInPath32(hProcess, searchPath, filename, id, two, three, flags, filePath, callback, context);
    }
}