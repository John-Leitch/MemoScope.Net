using BrightIdeasSoftware;
using MemoScope.Core.Data;
using WinFwk.UITools;

namespace MemoScope.Modules.TypeDetails
{
    public abstract class AbstractTypeInformation : TreeNodeInformationAdapter<AbstractTypeInformation>, ITypeNameData
    {
        [OLVColumn(Title = "Type", FillsFreeSpace = true)]
        public string TypeName { get; protected set; }
    }
}
