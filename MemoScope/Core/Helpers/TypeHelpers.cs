﻿using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace MemoScope.Core.Helpers
{
    public static class TypeHelpers
    {
        private static readonly Regex fieldNameRegex = new Regex("^<(.*)>k__BackingField$", RegexOptions.Compiled);
        private static readonly Dictionary<string, string> aliasCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, Tuple<Color, Color>> colorCache = new Dictionary<string, Tuple<Color, Color>>();
        private static readonly Tuple<Color, Color> defaultTuple = new Tuple<Color, Color>(Color.Transparent, Color.Transparent);

        public static void ResetCaches()
        {
            aliasCache.Clear();
            colorCache.Clear();
        }

        public static string RealName(this ClrInstanceField field, string backingFieldSuffix = " [*]") => RealName(field.Name, backingFieldSuffix);

        public static string RealName(string fieldName, string backingFieldSuffix = " [*]")
        {
            var match = fieldNameRegex.Match(fieldName);

            if (match.Success)
            {
                string realName = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(backingFieldSuffix))
                {
                    realName += backingFieldSuffix;
                }
                return realName;
            }
            return fieldName;
        }

        public static string ManageAlias(ClrType type) => type != null ? ManageAlias(type.Name) : "????";

        public static string ManageAlias(string typeName) => ManageAlias(typeName, MemoScopeSettings.Instance.TypeAliases);
        public static Tuple<Color, Color> GetAliasColor(string typeName) => colorCache.TryGetValue(typeName, out Tuple<Color, Color> c) ? c : defaultTuple;
        public static string ManageAlias(string typeName, List<TypeAlias> typeAliases)
        {
            if (typeName == null)
            {
                return null;
            }
            if (aliasCache.TryGetValue(typeName, out string alias))
            {
                return alias;
            }
            int aliasIndex = -1;
            if (typeName.IndexOf('<') < 0)
            {
                string name = CheckAlias(typeName, typeAliases, ref aliasIndex);
                if (aliasIndex >= 0)
                {
                    var typeAlias = typeAliases[aliasIndex];
                    var colors = new Tuple<Color, Color>(typeAlias.BackColor, typeAlias.ForeColor);
                    colorCache[typeName] = colors;
                    colorCache[name] = colors;
                }
                return name;
            }

            string res = "";
            string buf = "";
            bool isArray = typeName.EndsWith("[]");
            for (int i = 0; i < typeName.Length; i++)
            {
                char c = typeName[i];
                switch (c)
                {
                    case ' ':
                        break;
                    case '<':
                    case '>':
                    case ',':
                        if (!string.IsNullOrEmpty(buf))
                        {
                            string newBuf = CheckAlias(buf, typeAliases, ref aliasIndex);
                            res += newBuf;
                        }
                        res += c + " ";
                        buf = "";
                        break;
                    default:
                        buf += c;
                        break;
                }
            }
            if (aliasIndex >= 0)
            {
                var typeAlias = typeAliases[aliasIndex];
                var colors = new Tuple<Color, Color>(typeAlias.BackColor, typeAlias.ForeColor);
                colorCache[res] = colors;
                colorCache[typeName] = colors;
            }
            if (isArray) res += "[ ]";
            return res;
        }

        private static string CheckAlias(string buf, List<TypeAlias> typeAliases, ref int aliasIndex)
        {
            for (int i = 0; i < typeAliases.Count; i++)
            {
                var typeAlias = typeAliases[i];
                if (typeAlias.Active && buf.Contains(typeAlias.OldTypeName))
                {
                    aliasIndex = Math.Max(i, aliasIndex);
                    buf = buf.Replace(typeAlias.OldTypeName, typeAlias.NewTypeName);
                }
            }
            return buf;
        }
    }
}