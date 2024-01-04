using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = System.Object;

namespace Avrahamy.EditorGadgets {
    public class PropertyHandler {
        private static MethodInfo getHandler;
        private static object[] getHandlerParams;

        private object handler;
        private Type type;

        private PropertyInfo propertyDrawerInfo;
        private MethodInfo guiHandler;
        private MethodInfo heightHandler;
        private object[] guiParams;
        private object[] heightParams;

        public PropertyDrawer propertyDrawer {
            get { return propertyDrawerInfo.GetValue(handler, null) as PropertyDrawer; }
        }

        static PropertyHandler() {
            getHandler = Type.GetType("UnityEditor.ScriptAttributeUtility, UnityEditor")
                .GetMethod("GetHandler", BindingFlags.NonPublic | BindingFlags.Static);
            getHandlerParams = new object[1];
        }

        private PropertyHandler(object handler) {
            this.handler = handler;

            type = handler.GetType();
            propertyDrawerInfo = type.GetProperty("propertyDrawer", BindingFlags.NonPublic | BindingFlags.Instance);
            guiHandler = type.GetMethod("OnGUI", BindingFlags.Public | BindingFlags.Instance);
            heightHandler = type.GetMethod("GetPropertyHeight", BindingFlags.Public | BindingFlags.Instance)
                ?? type.GetMethod("GetHeight", BindingFlags.Public | BindingFlags.Instance);
            guiParams = new object[guiHandler.GetParameters().Length];
            heightParams = new object[heightHandler.GetParameters().Length];
        }

        public bool OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren) {
            guiParams[0] = position;
            guiParams[1] = property;
            guiParams[2] = label;
            if (guiParams.Length > 3) {
                guiParams[3] = includeChildren;
            }

            if (guiHandler == null) return false;
            if (handler == null) return false;
            var result = guiHandler.Invoke(handler, guiParams);
            if (result == null) return false;
            //try {
                return (bool)result;
            /*} catch {
                getHandlerParams[0] = property;
                handler = getHandler.Invoke(null, getHandlerParams);
                type = handler.GetType();
                propertyDrawerInfo = type.GetProperty("propertyDrawer", BindingFlags.NonPublic | BindingFlags.Instance);
                guiHandler = type.GetMethod("OnGUI", BindingFlags.Public | BindingFlags.Instance);
                heightHandler = type.GetMethod("GetPropertyHeight", BindingFlags.Public | BindingFlags.Instance);
                guiParams = new object[guiHandler.GetParameters().Length];
                guiParams[0] = position;
                guiParams[1] = property;
                guiParams[2] = label;
                if (guiParams.Length > 3) {
                    guiParams[3] = includeChildren;
                }
                return (bool)guiHandler.Invoke(handler, guiParams);
            }*/
        }

        public float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            heightParams[0] = property;
            heightParams[1] = label;
            if (heightParams.Length > 2) {
                heightParams[2] = true;
            }

            return (float)heightHandler.Invoke(handler, heightParams);
        }

        public static PropertyHandler GetHandler(SerializedProperty property) {
            PopulateTypeToDrawer();
            if (typeToDrawerType.TryGetValue(property.GetFieldType(), out var drawerType)) {
                if (!drawerTypeToDrawerInstance.TryGetValue(drawerType, out var drawer)) {
                    getHandlerParams[0] = property;
                    drawer = Activator.CreateInstance(drawerType);

                    // Populate field info.
                    var fieldInfoField = typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
                    var fieldInfo = property.GetFieldInfo();
                    fieldInfoField.SetValue(drawer, fieldInfo);

                    drawerTypeToDrawerInstance[drawerType] = drawer;
                }
                return new PropertyHandler(drawer);
            }
            getHandlerParams[0] = property;
            return new PropertyHandler(getHandler.Invoke(null, getHandlerParams));
        }

        /// <summary>
        /// Type to PropertyDrawer types for that type
        /// </summary>
        private static Dictionary<Type, Type> typeToDrawerType;

        /// <summary>
        /// PropertyDrawer types to instances of that type
        /// </summary>
        private static Dictionary<Type, object> drawerTypeToDrawerInstance;

        private static void PopulateTypeToDrawer() {
            if (typeToDrawerType != null) return;

            typeToDrawerType = new Dictionary<Type, Type>();
            drawerTypeToDrawerInstance = new Dictionary<Type, object>();
            var propertyDrawerType = typeof (PropertyDrawer);
            var targetType = typeof (CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            var useForChildren = typeof (CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());

            foreach (var type in types) {
                if (propertyDrawerType.IsAssignableFrom(type)) {
                    var typeName = type.AssemblyQualifiedName;
                    if (!typeName.StartsWith("Product.")
                        && !typeName.StartsWith("Avrahamy.")
                        && !typeName.StartsWith("Product.")
                        && !typeName.StartsWith("BitStrap.")) continue;
                    if (type == typeof(BitStrap.NonNullableDrawer)) continue;
                    var customPropertyDrawers = type.GetCustomAttributes(true).OfType<CustomPropertyDrawer>().ToList();
                    foreach (var propertyDrawer in customPropertyDrawers) {
                        var targetedType = (Type)targetType.GetValue(propertyDrawer);
                        typeToDrawerType[targetedType] = type;

                        var useThisForChildren = (bool) useForChildren.GetValue(propertyDrawer);
                        if (useThisForChildren) {
                            var childTypes = types.Where(t => targetedType.IsAssignableFrom(t) && t != targetedType);
                            foreach (var childType in childTypes) {
                                typeToDrawerType[childType] = type;
                            }
                        }
                    }

                }
            }
        }
    }
}