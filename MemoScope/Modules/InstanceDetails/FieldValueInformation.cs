using BrightIdeasSoftware;
using MemoScope.Core.Data;
using MemoScope.Core.Helpers;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using WinFwk.UITools;
using ClrObject = MemoScope.Core.Data.ClrObject;

namespace MemoScope.Modules.InstanceDetails
{
    internal class FieldValueInformation : TreeNodeInformationAdapter<FieldValueInformation>, IAddressData, ITypeNameData
    {
        private readonly ClrDumpObject clrDumpObject;
        private readonly string name;

        public FieldValueInformation(string name, ClrDumpObject clrDumpObject)
        {
            this.name = name;
            this.clrDumpObject = clrDumpObject;
        }

        [OLVColumn(Title = "Field Name")]
        public string Name => name;

        [OLVColumn(Title = "Value")]
        public object Value => clrDumpObject.Value;

        [OLVColumn(Title = "Address")]
        public ulong Address => clrDumpObject.Address;

        [OLVColumn(Title = "Type")]
        public string TypeName => clrDumpObject.TypeName;

        public ClrType ClrType => clrDumpObject.ClrType;

        public override bool CanExpand => !(clrDumpObject.IsPrimitiveOrString || clrDumpObject.Address == 0);
        public override List<FieldValueInformation> Children => GetChildren(clrDumpObject);

        internal static List<FieldValueInformation> GetValues(ClrDumpObject clrDumpObject) => clrDumpObject.ClrDump.Eval(() =>
                                                                                                         {
                                                                                                             var clrObject = new ClrObject(clrDumpObject.Address, clrDumpObject.ClrType, clrDumpObject.IsInterior);
                                                                                                             var l = new List<FieldValueInformation>();
                                                                                                             if (clrDumpObject.ClrType != null)
                                                                                                             {
                                                                                                                 foreach (var field in clrDumpObject.ClrType.Fields)
                                                                                                                 {
                                                                                                                     var fieldValue = clrObject[field];
                                                                                                                     var fieldValueInfo = new FieldValueInformation(field.RealName(), new ClrDumpObject(clrDumpObject.ClrDump, fieldValue.Type, fieldValue.Address, fieldValue.IsInterior));
                                                                                                                     l.Add(fieldValueInfo);
                                                                                                                 }
                                                                                                             }
                                                                                                             return l;
                                                                                                         });

        internal static List<FieldValueInformation> GetElements(ClrDumpObject clrDumpObject) => clrDumpObject.ClrDump.Eval(() =>
                                                                                                          {
                                                                                                              var clrObject = new ClrObject(clrDumpObject.Address, clrDumpObject.ClrType, clrDumpObject.IsInterior);
                                                                                                              var l = new List<FieldValueInformation>();
                                                                                                              int length = clrDumpObject.ClrType.GetArrayLength(clrDumpObject.Address);
                                                                                                              var n = Math.Min(length, 1024);
                                                                                                              for (int i = 0; i < n; i++)
                                                                                                              {
                                                                                                                  var fieldValue = clrObject[i];
                                                                                                                  var fieldValueInfo = new FieldValueInformation($"[{i}]", new ClrDumpObject(clrDumpObject.ClrDump, fieldValue.Type, fieldValue.Address, fieldValue.IsInterior));
                                                                                                                  l.Add(fieldValueInfo);
                                                                                                              }
                                                                                                              return l;
                                                                                                          });

        public static List<FieldValueInformation> GetChildren(ClrDumpObject clrDumpObject) => clrDumpObject.ClrType?.IsArray == true ? GetElements(clrDumpObject) : GetValues(clrDumpObject);
    }
}