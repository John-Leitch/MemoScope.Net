using BrightIdeasSoftware;
using MemoScope.Core;
using Microsoft.Diagnostics.Runtime;
using System.Diagnostics;
using System.IO;
using WinFwk.UITools;

namespace MemoScope.Modules.Modules
{
    public class ModuleInformation
    {
        private readonly ClrModule module;

        public ModuleInformation(ClrDump clrDump, ClrModule module)
        {
            this.module = module;
            DebuggingMode = module.DebuggingMode;
            Pdb = clrDump.Eval(() => module.IsFile && module.Pdb != null ? module.Pdb.FileName : null);
        }

        [OLVColumn]
        public string Name => Path.GetFileName(module.AssemblyName);
        [IntColumn]
        public ulong Size => module.Size;
        [OLVColumn]
        public DebuggableAttribute.DebuggingModes DebuggingMode { get; }
        [OLVColumn]
        public string FileName => module.FileName;

        [BoolColumn]
        public bool IsDynamic => module.IsDynamic;
        [BoolColumn]
        public bool IsFile => module.IsFile;
        [OLVColumn]
        public string Pdb { get; }
    }
}