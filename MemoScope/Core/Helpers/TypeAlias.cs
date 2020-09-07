using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace MemoScope.Core.Helpers
{
    public class TypeAlias
    {
        public bool Active { get; set; } = true;
        public string OldTypeName { get; set; }
        public string NewTypeName { get; set; }

        [XmlIgnore]
        public Color BackColor
        {
            get; set;
        }

        [Browsable(false)]
        public string BackColorRGB
        {
            get => BackColor.R + "," + BackColor.G + "," + BackColor.B;
            set
            {
                string[] c = value.Split(',');
                BackColor = Color.FromArgb(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]));
            }
        }
        [XmlIgnore]
        public Color ForeColor
        {
            get; set;
        }

        [Browsable(false)]
        public string ForeColorRGB
        {
            get => ForeColor.R + "," + ForeColor.G + "," + ForeColor.B;
            set
            {
                string[] c = value.Split(',');
                ForeColor = Color.FromArgb(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]));
            }
        }

        public override int GetHashCode() => ((OldTypeName?.GetHashCode()) ?? 0) + 37 * ((NewTypeName?.GetHashCode()) ?? 0);
        public override bool Equals(object o)
        {
            if (!(o is TypeAlias other))
            {
                return false;
            }
            bool b1 = other.NewTypeName == NewTypeName;
            bool b2 = other.OldTypeName == OldTypeName;
            return b1 && b2;
        }
        public override string ToString() => "[" + (Active ? "+" : "-") + "] " + OldTypeName + " => " + NewTypeName;
    }
}
