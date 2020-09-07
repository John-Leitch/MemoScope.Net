using System;
using System.Windows.Forms.DataVisualization.Charting;

namespace MemoScope.Modules.Process
{
    public class ProcessInfoValue
    {
        public string Name { get; }
        public string Alias { get; }
        public string GroupName { get; }
        public string Format { get; }
        public Func<ProcessWrapper, object> ValueGetter { get; }
        public Series Series { get; set; }
        public object Value { get; private set; }

        public ProcessInfoValue(string name, string alias, string groupName, Func<ProcessWrapper, object> valueGetter, string format)
        {
            Name = name;
            Alias = alias;
            GroupName = groupName;
            ValueGetter = valueGetter;
            Format = format;
        }

        public object GetValue(ProcessWrapper proc)
        {
            try
            {
                var o = ValueGetter(proc);
                Value = o;
                return string.Format(Format, o);
            }
            catch
            {
                return "Err";
            }
        }

        public void Reset() => Series?.Points.Clear();
    }
}