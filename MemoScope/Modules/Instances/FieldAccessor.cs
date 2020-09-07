using MemoScope.Core;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoScope.Modules.Instances
{
    public class FieldAccessor
    {
        public ClrDump ClrDump { get; }
        public ClrType ClrType { get; }
        public ulong Address { get; set; }

        public FieldAccessor(ClrDump clrDump, ClrType clrType)
        {
            ClrDump = clrDump;
            ClrType = clrType;
        }
        private string lastArg = null;
        private List<string> lastFields;
        private readonly Dictionary<string, List<string>> cacheField = new Dictionary<string, List<string>>();
        private object Eval(string arg)
        {
            List<string> fields;
            if (ReferenceEquals(arg, lastArg))
            {
                fields = lastFields;
            }
            else
            {
                if (!cacheField.TryGetValue(arg, out fields))
                {
                    fields = arg.Split('.').ToList();
                    cacheField[arg] = fields;
                }
                lastFields = fields;
                lastArg = arg;
            }
            return ClrDump.GetFieldValueImpl(Address, ClrType, fields);
        }

        internal static string GetFuncName(ClrElementType elementType)
        {
            switch (elementType)
            {
                case ClrElementType.Boolean:
                    return nameof(_bool);
                case ClrElementType.Double:
                    return nameof(_double);
                case ClrElementType.Float:
                    return nameof(_float);
                case ClrElementType.Char:
                    return nameof(_char);
                case ClrElementType.Int16:
                    return nameof(_short);
                case ClrElementType.Int32:
                    return nameof(_int);
                case ClrElementType.Int64:
                    return nameof(_long);
                case ClrElementType.Int8:
                    return nameof(_byte);
                case ClrElementType.String:
                    return nameof(_string);
                case ClrElementType.UInt16:
                    return nameof(_ushort);
                case ClrElementType.UInt32:
                    return nameof(_uint);
                case ClrElementType.UInt64:
                    return nameof(_ulong);
                case ClrElementType.Pointer:
                case ClrElementType.NativeInt:
                case ClrElementType.NativeUInt:
                    return nameof(_ptr);
                default:
                    return nameof(_obj);
            }
        }

        public char _char(string arg) => (char)Eval(arg);

        public short _short(string arg) => (short)Eval(arg);

        public ushort _ushort(string arg) => (ushort)Eval(arg);
        public double _double(string arg)
        {
            var val = Eval(arg);
            return val != null ? (double)val : double.NaN;
        }
        public bool _bool(string arg) => (bool)Eval(arg);
        public int _int(string arg) => (int)Eval(arg);
        public long _long(string arg) => (long)Eval(arg);
        public string _string(string arg) => (string)Eval(arg);
        public float _float(string arg) => (float)Eval(arg);
        public byte _byte(string arg) => (byte)Eval(arg);
        public uint _uint(string arg) => (uint)Eval(arg);
        public ulong _ulong(string arg) => (ulong)Eval(arg);
        public DateTime _datetime(string arg) => (DateTime)Eval(arg);
        public decimal _decimal(string arg) => (decimal)Eval(arg);
        public object _obj(string arg) => Eval(arg);
        public long _ptr(string arg)
        {
            var ptr = Eval(arg);
            return (long)ptr;
        }
        public override string ToString()
        {
            var obj = ClrDump.GetSimpleValue(Address, ClrType);
            return obj != null ? obj.ToString() : string.Empty;
        }
    }
}