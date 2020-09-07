using MemoScope.Core.Cache;
using MemoScope.Core.ProcessInfo;
using System;
using System.Collections.Generic;
using System.IO;

namespace MemoScope.Modules.Explorer
{
    public class FileData : AbstractDumpExplorerData
    {
        public override FileInfo FileInfo { get; }

        private readonly ProcessInfo processInfo;
        public FileData(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            Name = fileInfo.Name;
            Icon = Properties.Resources.file_extension_bin;
            var clrDumpInfo = ClrDumpInfo.Load(fileInfo.FullName);
            processInfo = clrDumpInfo.ProcessInfo;
        }

        public override long Size => FileInfo.Length / 1000000;
        public override bool CanExpand => false;
        public override List<AbstractDumpExplorerData> Children => null;
        public override string GetCachePath() => ClrDumpCache.GetCachePath(FileInfo.FullName);

        public override long? HandleCount => processInfo?.HandleCount;
        public override string CommandLine => processInfo?.CommandLine;
        public override DateTime? DumpTime => processInfo == null || processInfo.DumpTime == DateTime.MinValue ? null : (DateTime?)processInfo.StartTime;

        public override string MachineName => processInfo?.MachineName;
        public override long? PagedMemory => processInfo?.PagedMemory / 1000000;
        public override long? PeakPagedMemory => processInfo?.PeakPagedMemory / 1000000;
        public override long? PeakVirtualMemory => processInfo?.PeakVirtualMemory / 1000000;
        public override long? PeakWorkingSet => processInfo?.PeakWorkingSet / 1000000;
        public override long? PrivateMemory => processInfo?.PrivateMemory / 1000000;
        public override DateTime? StartTime => processInfo == null || processInfo.StartTime == DateTime.MinValue ? null : (DateTime?)processInfo.StartTime;

        public override TimeSpan? TotalProcessorTime => processInfo == null || processInfo.TotalProcessorTime == TimeSpan.Zero ? null : (TimeSpan?)processInfo.TotalProcessorTime;

        public override string UserName => processInfo?.UserName;
        public override TimeSpan? UserProcessorTime => processInfo == null || processInfo.UserProcessorTime == TimeSpan.Zero ? null : (TimeSpan?)processInfo.UserProcessorTime;

        public override long? VirtualMemory => processInfo?.VirtualMemory / 1000000;
        public override long? WorkingSet => processInfo?.WorkingSet / 1000000;
    }
}