using System;
using System.Collections.Generic;
using System.Reflection;

namespace Avrahamy.Utils {
    public static class TypeExtensions {
        public static FieldInfo GetFieldIncludingBase(this Type type, string fieldName, BindingFlags bindingFlags) {
            while (type != typeof(object)) {
                var fieldInfo = type.GetField(fieldName, bindingFlags);
                if (fieldInfo != null) {
                    return fieldInfo;
                }
                type = type.BaseType;
            }
            return null;
        }

        public static List<FieldInfo> GetFieldsIncludingBase(this Type type, BindingFlags bindingFlags) {
            var fieldInfos = new List<FieldInfo>();
            var fieldInfoNames = new HashSet<string>();
            while (type != typeof(object)) {
                var fields = type.GetFields(bindingFlags);
                if (fields != null && fields.Length > 0) {
                    foreach (var field in fields) {
                        var isNewField = fieldInfoNames.Add(field.Name);
                        if (isNewField) {
                            fieldInfos.Add(field);
                        }
                    }
                }
                type = type.BaseType;
            }
            return fieldInfos;
        }

        public static Type GetType(string typeName) {
            if (typeName.StartsWith("Avrahamy", StringComparison.Ordinal)) {
                return Type.GetType(typeName + ", avrahamy", false, true);
            }
            return Type.GetType(typeName + ", product");
        }
    }
}
