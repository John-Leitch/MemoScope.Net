using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace MemoScope.Core.Bookmarks
{
    public class BookmarkMgr
    {
        private readonly string bookmarkPath;
        private XmlSerializer xml;
        private XmlSerializer XML
        {
            get
            {
                if (xml == null)
                {
                    xml = new XmlSerializer(typeof(List<Bookmark>));
                }
                return xml;
            }
        }

        private Dictionary<ulong, Bookmark> bookmarks = new Dictionary<ulong, Bookmark>();

        public BookmarkMgr(string dumpPath) => bookmarkPath = Path.ChangeExtension(dumpPath, "xml");

        public List<Bookmark> GetBookmarks()
        {
            if (File.Exists(bookmarkPath))
            {
                using (var reader = new StreamReader(bookmarkPath))
                {
                    var bookmarksObj = XML.Deserialize(reader);
                    var bookmarkList = bookmarksObj as List<Bookmark>;
                    bookmarks = bookmarkList.ToDictionary(bookmark => bookmark.Address);
                }
            }
            return bookmarks.Values.ToList();
        }

        public void Remove(ulong address)
        {
            if (bookmarks?.ContainsKey(address) == true)
            {
                bookmarks.Remove(address);
                SaveBookmarks();
            }
        }

        public Bookmark Get(ulong address) => bookmarks.TryGetValue(address, out Bookmark bookmark) ? bookmark : null;

        public void Add(ulong address, ClrType clrType)
        {
            if (bookmarks?.ContainsKey(address) == false)
            {
                bookmarks[address] = new Bookmark(address, clrType.Name);
                SaveBookmarks();
            }
        }

        public void SaveBookmarks()
        {
            if (bookmarks == null)
            {
                return;
            }

            var dir = Path.GetDirectoryName(bookmarkPath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (var reader = new StreamWriter(bookmarkPath))
            {
                XML.Serialize(reader, bookmarks.Values.ToList());
            }
        }
    }
}
