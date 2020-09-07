using WinFwk.UIMessages;

namespace MemoScope.Modules.Process
{
    public class DumpRequest : AbstractUIMessage
    {
        public ProcessWrapper ProcessWrapper { get; }

        public DumpRequest(ProcessWrapper processWrapper) => ProcessWrapper = processWrapper;
    }
}