using MemoScope.Core.Bookmarks;
using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace MemoScope.Core.ProcessInfo
{
    public class ClrDumpInfo
    {
        private static readonly XmlSerializer XML = new XmlSerializer(typeof(ClrDumpInfo));

        [XmlIgnore]
        public string DumpPath { get; private set; }
        private Dictionary<ulong, Bookmark> dicoBookmarks;

        public List<Bookmark> Bookmarks { get; internal set; }
        public ProcessInfo ProcessInfo { get; set; }

        public ClrDumpInfo() => Bookmarks = new List<Bookmark>();

        public ClrDumpInfo(string dumpPath) : this() => DumpPath = dumpPath;

        public static ClrDumpInfo Load(string dumpPath)
        {
            ClrDumpInfo clrDumpInfo = null;
            string clrDumpInfoPath = GetClrDumpInfoPath(dumpPath);
            try
            {
                if (File.Exists(clrDumpInfoPath))
                {
                    using (var reader = new StreamReader(clrDumpInfoPath))
                    {
                        var processInfoObj = XML.Deserialize(reader);
                        clrDumpInfo = processInfoObj as ClrDumpInfo;
                        clrDumpInfo.Init(dumpPath);
                    }
                }
            }
            finally
            {
                if (clrDumpInfo == null)
                {
                    clrDumpInfo = new ClrDumpInfo(dumpPath)
                    {
                        ProcessInfo = new ProcessInfo()
                    };
                    clrDumpInfo.Init();
                    clrDumpInfo.Save();
                }
            }

            return clrDumpInfo;
        }

        private void Init(string dumpPath)
        {
            DumpPath = dumpPath;
            Init();
        }

        private void Init() => dicoBookmarks = Bookmarks.ToDictionary(bookmark => bookmark.Address);

        private static string GetClrDumpInfoPath(string dumpPath) => Path.ChangeExtension(dumpPath, "xml");

        public void Save()
        {
            var dir = Path.GetDirectoryName(DumpPath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string clrDumpInfoPath = GetClrDumpInfoPath(DumpPath);
            using (var writer = new StreamWriter(clrDumpInfoPath))
            {
                XML.Serialize(writer, this);
            }
        }

        internal void RemoveBookmark(ulong address)
        {
            if (dicoBookmarks.TryGetValue(address, out Bookmark bookmark))
            {
                dicoBookmarks.Remove(address);
                Bookmarks.Remove(bookmark);
                Save();
            }
        }

        internal void AddBookmark(ulong address, ClrType clrType)
        {
            if (dicoBookmarks?.ContainsKey(address) == false)
            {
                var bookmark = new Bookmark(address, clrType.Name);
                dicoBookmarks[address] = bookmark;
                Bookmarks.Add(bookmark);
                Save();
            }
        }

        public Bookmark GetBookmark(ulong address)
        {
            dicoBookmarks.TryGetValue(address, out Bookmark bookmark);
            return bookmark;
        }

        internal void InitProcessInfo(Process process) => ProcessInfo = new ProcessInfo(process);
    }
}
