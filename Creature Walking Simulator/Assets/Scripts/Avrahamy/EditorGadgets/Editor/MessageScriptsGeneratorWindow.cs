using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Avrahamy.Collections;
using Avrahamy.Utils;

namespace Avrahamy.EditorGadgets {
    public class MessageScriptsGeneratorWindow : EditorWindow {
        private const string WINDOW_TITLE = "Message Scripts Generator";
        private const string CONSTRUCTOR_MEMBER_INIT_FORMAT = @"            this.{0} = {0};
";
        private const string PREFS_MESSAGE_NAME = "MSGW_MessageName";
        private const string PREFS_CREATE_MENU_PATH = "MSGW_CreateMenuPath";
        private const string PREFS_NAMESPACE_NAME = "MSGW_NamespaceName";
        private const string PREFS_CREATE_PREFIX = "MSGW_Create";
        private const string PREFS_CREATE_GENERATOR_SO = "MSGW_CreateGeneratorSO";
        private const string PREFS_CREATE_PREDICATE_SO = "MSGW_CreatePredicateSO";
        private const string PREFS_USE_PARAMETERS = "MSGW_UseParameters";
        private const string PREFS_PARAM_TYPE_PREFIX = "MSGW_ParamType";
        private const string PREFS_PARAM_NAME_PREFIX = "MSGW_ParamName";
        private const string PREFS_PENDING_GENERATOR_SO = "MSGW_PendingGeneratorSO";
        private const string PREFS_PENDING_PREDICATE_SO = "MSGW_PendingPredicateSO";
        private const string PREFS_PENDING_SO_PATH = "MSGW_PendingSOPath";

        private enum ScriptType {
            Message,
            MessageGenerator,
            MessageGeneratorComponent,
            MessagePredicate,
        }

        private static readonly Dictionary<ScriptType, ScriptTemplate> scriptFormatStrings =
            new Dictionary<ScriptType, ScriptTemplate> {
            {
                ScriptType.Message,
                new ScriptTemplate {
                    scriptNoParameters = @"using Avrahamy.Messages;

namespace {1} {{
    public class {2}Message : NoParamsMessage<{2}Message> {{}}
}}",
                    scriptWithParameters = @"namespace {1} {{
    public class {2}Message {{
{5}{4}
        public {2}Message({6}) {{
{7}        }}
    }}
}}",
                    parameterPropertyDeclaration = @"        public {0} {2} {{
            get {{
                return {1};
            }}
        }}

",
                    parameterMemberDeclaration = @"        private readonly {0} {1};
",
                    parameterConstructor = @"{0} {1}"
                }
            }, {
                ScriptType.MessageGenerator,
                new ScriptTemplate {
                    scriptNoParameters = @"using UnityEngine;
using Avrahamy.Messages;

namespace {1} {{
    [CreateAssetMenu(menuName = ""{0}/Messages/{3}"", fileName = ""{2}Message"")]
    public class {2}MessageGenerator : MessageGeneratorNoParams<{2}Message> {{}}
}}",
                    scriptWithParameters = @"using UnityEngine;
using Avrahamy.Messages;

namespace {1} {{
    [CreateAssetMenu(menuName = ""{0}/Messages/{3}"", fileName = ""{2}Message"")]
    public class {2}MessageGenerator : MessageGeneratorBase {{
{4}
        public override object New() {{
            return new {2}Message({6});
        }}
    }}
}}",
                    parameterPropertyDeclaration = string.Empty,
                    parameterMemberDeclaration = @"        [SerializeField] {0} {1};
",
                    parameterConstructor = @"{1}"
                }
            }, {
                ScriptType.MessageGeneratorComponent,
                new ScriptTemplate {
                    scriptNoParameters = @"using Avrahamy.Messages;

namespace {1} {{
    public class {2}MessageGeneratorComponent : MessageGeneratorComponent {{
        // TODO: Don't use a component if your use case does not require parameters.
        // TODO: Either use a generator SO or add parameters.
        public override object New() {{
            return new {2}Message();
        }}
    }}
}}",
                    scriptWithParameters = @"using UnityEngine;
using Avrahamy.Messages;

namespace {1} {{
    public class {2}MessageGeneratorComponent : MessageGeneratorComponent {{
{4}
        public override object New() {{
            return new {2}Message({6});
        }}
    }}
}}",
                    parameterPropertyDeclaration = string.Empty,
                    parameterMemberDeclaration = @"        [SerializeField] {0} {1};
",
                    parameterConstructor = @"{1}"
                }
            }, {
                ScriptType.MessagePredicate,
                new ScriptTemplate {
                    scriptNoParameters = @"using UnityEngine;
using Avrahamy.Messages;

namespace {1} {{
    [CreateAssetMenu(menuName = ""{0}/Messages/{3} Predicate"", fileName = ""{2}MessagePredicate"")]
    public class {2}MessagePredicate : MessagePredicateBase<{2}Message> {{}}
}}"
                }
            }
        };

        private string MessageClassName {
            get {
                return messageName?.Replace(" ", string.Empty);
            }
        }

        private string messageName;
        private string createMenuPath;
        private string namespaceName;
        private bool createMessage;
        private bool createGenerator;
        private bool createGeneratorSO;
        private bool createGeneratorComponent;
        private bool createPredicate;
        private bool createPredicateSO;
        private bool useParameters;
        private readonly List<(string, string)> parameters = new List<(string, string)>();
        private ScriptType focusedScript;
        private Vector2 scrollPosition;

        // NOTE: The letters after _ assigns the shortcut keys Ctrl/Cmd+Shift+M.
        [MenuItem("Window/GG/Message Scripts Generator _%#M")]
        public static void ShowWindow() {
            // Opens the window, otherwise focuses it if it’s already open.
            var window = GetWindow<MessageScriptsGeneratorWindow>(WINDOW_TITLE);
            // Sets a minimum size to the window.
            window.minSize = new Vector2(340f, 250f);
        }

        private void OnGUI() {
            var headerStyle = new GUIStyle {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = {
                    textColor = "5271AB".HexToColor()
                },
                padding = new RectOffset(10, 10, 5, 5),
            };
            GUILayout.Label("Message Scripts Generator", headerStyle);

            var notExpand = GUILayout.ExpandWidth(false);
            using (new EditorGUILayout.HorizontalScope())
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    GUILayout.Label("Name", notExpand);
                    var textWidth = Mathf.Max(
                        GUI.skin.textField.CalcSize(new GUIContent(messageName)).x + 5f,
                        20f);
                    messageName = EditorGUILayout.TextField(messageName, GUILayout.Width(textWidth));
                    GUILayout.Label("Message");
                    if (check.changed) {
                        EditorPrefs.SetString(PREFS_MESSAGE_NAME, messageName);
                    }
                }

            EditorGUILayout.HelpBox(
                "Examples:\n"
                + "Something Happened Message\n"
                + "Request Something Message",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    GUILayout.Label("Create Menu", notExpand);
                    var textWidth = Mathf.Max(
                        GUI.skin.textField.CalcSize(new GUIContent(createMenuPath)).x + 5f,
                        20f);
                    createMenuPath = EditorGUILayout.TextField(createMenuPath, GUILayout.Width(textWidth));
                    GUILayout.Label("/Messages/");
                    if (check.changed) {
                        EditorPrefs.SetString(PREFS_CREATE_MENU_PATH, createMenuPath);
                    }
                }

            using (var check = new EditorGUI.ChangeCheckScope()) {
                EditorGUIUtility.labelWidth = 80f;
                namespaceName = EditorGUILayout.TextField("Namespace", namespaceName, GUILayout.MaxWidth(300f));
                if (check.changed) {
                    EditorPrefs.SetString(PREFS_NAMESPACE_NAME, namespaceName);
                }
            }

            GUILayout.Label(string.Empty);
            GUILayout.Label("Files to create:");

            var width = GUILayout.Width(15f);
            var messageClassName = MessageClassName;
            var selectFocusedScript = false;
            using (new EditorGUILayout.HorizontalScope())
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    GUILayout.Space(10f);
                    createMessage = EditorGUILayout.Toggle(createMessage, width);
                    GUILayout.Label($"Message ({GetScriptName(messageClassName, ScriptType.Message)})", notExpand);
                    if (check.changed) {
                        EditorPrefs.SetBool(PREFS_CREATE_PREFIX + ScriptType.Message, createMessage);
                        if (createMessage) {
                            focusedScript = ScriptType.Message;
                        } else {
                            selectFocusedScript = true;
                        }
                    }
                }
            using (new EditorGUILayout.HorizontalScope()) {
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    GUILayout.Space(10f);
                    createGenerator = EditorGUILayout.Toggle(createGenerator, width);
                    GUILayout.Label($"MessageGenerator ({GetScriptName(messageClassName, ScriptType.MessageGenerator)})", notExpand);
                    if (check.changed) {
                        EditorPrefs.SetBool(PREFS_CREATE_PREFIX + ScriptType.MessageGenerator, createGenerator);
                        if (createGenerator) {
                            focusedScript = ScriptType.MessageGenerator;
                        } else {
                            selectFocusedScript = true;
                        }
                    }
                }
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    createGeneratorSO = EditorGUILayout.Toggle(createGeneratorSO, width);
                    GUILayout.Label("SO", notExpand);
                    if (check.changed) {
                        EditorPrefs.SetBool(PREFS_CREATE_GENERATOR_SO, createGeneratorSO);
                    }
                }
            }
            using (new EditorGUILayout.HorizontalScope())
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    GUILayout.Space(10f);
                    createGeneratorComponent = EditorGUILayout.Toggle(createGeneratorComponent, width);
                    GUILayout.Label($"MessageGeneratorComponent ({GetScriptName(messageClassName, ScriptType.MessageGeneratorComponent)})", notExpand);
                    if (check.changed) {
                        EditorPrefs.SetBool(PREFS_CREATE_PREFIX + ScriptType.MessageGeneratorComponent, createGeneratorComponent);
                        if (createGeneratorComponent) {
                            focusedScript = ScriptType.MessageGeneratorComponent;
                        } else {
                            selectFocusedScript = true;
                        }
                    }
                }
            using (new EditorGUILayout.HorizontalScope()) {
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    GUILayout.Space(10f);
                    createPredicate = EditorGUILayout.Toggle(createPredicate, width);
                    GUILayout.Label($"MessagePredicate ({GetScriptName(messageClassName, ScriptType.MessagePredicate)})", notExpand);
                    if (check.changed) {
                        EditorPrefs.SetBool(PREFS_CREATE_PREFIX + ScriptType.MessagePredicate, createPredicate);
                        if (createPredicate) {
                            focusedScript = ScriptType.MessagePredicate;
                        } else {
                            selectFocusedScript = true;
                        }
                    }
                }
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    createPredicateSO = EditorGUILayout.Toggle(createPredicateSO, width);
                    GUILayout.Label("SO", notExpand);
                    if (check.changed) {
                        EditorPrefs.SetBool(PREFS_CREATE_PREDICATE_SO, createPredicateSO);
                    }
                }
            }

            GUILayout.Label(string.Empty);
            using (new EditorGUILayout.HorizontalScope())
                using (var check = new EditorGUI.ChangeCheckScope()) {
                    GUILayout.Space(10f);
                    useParameters = EditorGUILayout.Toggle(useParameters, width);
                    GUILayout.Label("Parameters", notExpand);
                    if (check.changed) {
                        EditorPrefs.SetBool(PREFS_USE_PARAMETERS, useParameters);
                    }
                }

            width = GUILayout.Width(20f);
            if (useParameters) {
                if (parameters.Count == 0) {
                    parameters.Add((string.Empty, string.Empty));
                }
                var i = 0;
                while (i < parameters.Count) {
                    var parameter = parameters[i];
                    var typeName = parameter.Item1;
                    var memberName = parameter.Item2;
                    using (new EditorGUILayout.HorizontalScope()) {
                        using (var check = new EditorGUI.ChangeCheckScope()) {
                            GUILayout.Space(30f);
                            GUILayout.Label("Type", notExpand);
                            typeName = EditorGUILayout.TextField(typeName);
                            if (check.changed) {
                                parameters[i] = (typeName, memberName);
                                if (i < 5) {
                                    EditorPrefs.SetString(PREFS_PARAM_TYPE_PREFIX + i, typeName);
                                }
                            }
                        }
                        using (var check = new EditorGUI.ChangeCheckScope()) {
                            GUILayout.Space(6f);
                            GUILayout.Label("Member Name", notExpand);
                            memberName = EditorGUILayout.TextField(memberName);
                            if (check.changed) {
                                parameters[i] = (typeName, memberName);
                                if (i < 5) {
                                    EditorPrefs.SetString(PREFS_PARAM_NAME_PREFIX + i, memberName);
                                }
                            }

                            GUILayout.Space(10f);
                            if (GUILayout.Button("+", width)) {
                                parameters.Insert(i + 1, (typeName, memberName));
                                for (int j = i + 1; j < Mathf.Min(5, parameters.Count); j++) {
                                    var param = parameters[j];
                                    EditorPrefs.SetString(PREFS_PARAM_TYPE_PREFIX + i, param.Item1);
                                    EditorPrefs.SetString(PREFS_PARAM_NAME_PREFIX + i, param.Item2);
                                }
                            }
                            if (GUILayout.Button("-", width)) {
                                parameters.RemoveAt(i);
                                if (parameters.Count == 0) {
                                    useParameters = false;
                                    return;
                                }
                                for (int j = i; j < Mathf.Min(5, parameters.Count); j++) {
                                    var param = parameters[i];
                                    EditorPrefs.SetString(PREFS_PARAM_TYPE_PREFIX + i, param.Item1);
                                    EditorPrefs.SetString(PREFS_PARAM_NAME_PREFIX + i, param.Item2);
                                }
                                if (parameters.Count < 5) {
                                    EditorPrefs.DeleteKey(PREFS_PARAM_TYPE_PREFIX + parameters.Count);
                                    EditorPrefs.DeleteKey(PREFS_PARAM_NAME_PREFIX + parameters.Count);
                                }
                                --i;
                            }
                        }
                    }
                    ++i;
                }
            }
            var enabled = new List<ScriptType>();
            var index = 0;
            if (createMessage) {
                if (focusedScript == ScriptType.Message) {
                    index = enabled.Count;
                }
                enabled.Add(ScriptType.Message);
            }
            if (createGenerator) {
                if (focusedScript == ScriptType.MessageGenerator) {
                    index = enabled.Count;
                }
                enabled.Add(ScriptType.MessageGenerator);
            }
            if (createGeneratorComponent) {
                if (focusedScript == ScriptType.MessageGeneratorComponent) {
                    index = enabled.Count;
                }
                enabled.Add(ScriptType.MessageGeneratorComponent);
            }
            if (createPredicate) {
                if (focusedScript == ScriptType.MessagePredicate) {
                    index = enabled.Count;
                }
                enabled.Add(ScriptType.MessagePredicate);
            }

            if (enabled.Count > 0) {
                if (selectFocusedScript) {
                    focusedScript = enabled[0];
                }

                GUILayout.Label(string.Empty);
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Create Scripts", GUILayout.Width(120f), GUILayout.Height(40f))) {
                        CreateScripts();
                        return;
                    }
                    GUILayout.FlexibleSpace();
                }

                GUILayout.Label(string.Empty);

                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.Label($"Previewing: {GetScriptName(messageClassName, focusedScript)}");
                    if (enabled.Count > 1) {
                        if (GUILayout.Button("<", width)) {
                            index = (index + enabled.Count - 1) % enabled.Count;
                            focusedScript = enabled[index];
                        }
                        if (GUILayout.Button(">", width)) {
                            index = (index + 1) % enabled.Count;
                            focusedScript = enabled[index];
                        }
                    }
                }

                using (var scrollView = new GUILayout.ScrollViewScope(scrollPosition)) {
                    scrollPosition = scrollView.scrollPosition;
                    var previewStyle = new GUIStyle(GUI.skin.button) {
                        fontSize = 11,
                        alignment = TextAnchor.UpperLeft,
                        padding = new RectOffset(10, 10, 10, 10)
                    };
                    var previewText = GetScriptContents(
                        createMenuPath,
                        messageClassName,
                        focusedScript);
                    GUI.backgroundColor = Color.white * 0.8f;
                    GUI.contentColor = Color.white * 0.8f;
                    GUILayout.Label(
                        previewText,
                        previewStyle);
                }
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            bool createdAnySO = CreatePendingScriptableObject(PREFS_PENDING_GENERATOR_SO);
            createdAnySO =
                CreatePendingScriptableObject(PREFS_PENDING_PREDICATE_SO)
                || createdAnySO;
            if (createdAnySO) {
                EditorPrefs.DeleteKey(PREFS_PENDING_SO_PATH);
            }
        }

        private void OnEnable() {
            if (EditorPrefs.HasKey(PREFS_MESSAGE_NAME)) {
                messageName = EditorPrefs.GetString(PREFS_MESSAGE_NAME, "Request Something");
            }
            if (EditorPrefs.HasKey(PREFS_CREATE_MENU_PATH)) {
                createMenuPath = EditorPrefs.GetString(PREFS_CREATE_MENU_PATH, "Setup");
            }
            if (EditorPrefs.HasKey(PREFS_NAMESPACE_NAME)) {
                namespaceName = EditorPrefs.GetString(PREFS_NAMESPACE_NAME, "Product");
            }

            if (EditorPrefs.HasKey(PREFS_CREATE_PREFIX + ScriptType.Message)) {
                createMessage = EditorPrefs.GetBool(PREFS_CREATE_PREFIX + ScriptType.Message);
            }
            if (EditorPrefs.HasKey(PREFS_CREATE_PREFIX + ScriptType.MessageGenerator)) {
                createGenerator = EditorPrefs.GetBool(PREFS_CREATE_PREFIX + ScriptType.MessageGenerator);
            }
            if (EditorPrefs.HasKey(PREFS_CREATE_GENERATOR_SO)) {
                createGeneratorSO = EditorPrefs.GetBool(PREFS_CREATE_GENERATOR_SO);
            }
            if (EditorPrefs.HasKey(PREFS_CREATE_PREFIX + ScriptType.MessageGeneratorComponent)) {
                createGeneratorComponent = EditorPrefs.GetBool(PREFS_CREATE_PREFIX + ScriptType.MessageGeneratorComponent);
            }
            if (EditorPrefs.HasKey(PREFS_CREATE_PREFIX + ScriptType.MessagePredicate)) {
                createPredicate = EditorPrefs.GetBool(PREFS_CREATE_PREFIX + ScriptType.MessagePredicate);
            }
            if (EditorPrefs.HasKey(PREFS_CREATE_PREDICATE_SO)) {
                createPredicateSO = EditorPrefs.GetBool(PREFS_CREATE_PREDICATE_SO);
            }

            if (EditorPrefs.HasKey(PREFS_USE_PARAMETERS)) {
                useParameters = EditorPrefs.GetBool(PREFS_USE_PARAMETERS);
            }

            // Load first 5 parameters from prefs.
            parameters.Clear();
            for (int i = 0; i < 5 && EditorPrefs.HasKey(PREFS_PARAM_TYPE_PREFIX + i); i++) {
                parameters.Add((EditorPrefs.GetString(PREFS_PARAM_TYPE_PREFIX + i),
                    EditorPrefs.GetString(PREFS_PARAM_NAME_PREFIX + i)));
            }
        }

        private void CreateScripts() {
            DebugLog.Log("Creating scripts");
            var messageClassName = MessageClassName;
            var createdAssetPaths = new List<string>();

            var currentPath = EditorUtils.GetCurrentProjectFolder();
            string scriptsPath = null;

            var scriptCheckboxes = new List<(bool, ScriptType)> {
                (createMessage, ScriptType.Message),
                (createGenerator, ScriptType.MessageGenerator),
                (createGeneratorComponent, ScriptType.MessageGeneratorComponent),
                (createPredicate, ScriptType.MessagePredicate),
            };

            foreach (var checkboxAndType in scriptCheckboxes) {
                var checkbox = checkboxAndType.Item1;
                if (!checkbox) continue;
                var type = checkboxAndType.Item2;

                var scriptName = GetScriptName(messageClassName, type);
                var scriptContents = GetScriptContents(
                    createMenuPath,
                    messageClassName,
                    type);

                if (scriptsPath == null) {
                    if (currentPath.Contains("/Scripts/") && currentPath.Contains("/Messages")) {
                        scriptsPath = currentPath;
                    } else {
                        scriptsPath = EditorUtility.SaveFolderPanel(
                            $"Save '{messageName} Message' Scripts to a '/Messages' Folder",
                            currentPath,
                            string.Empty);
                        if (string.IsNullOrEmpty(scriptsPath)) {
                            DebugLog.LogError("Saving message scripts aborted");
                            return;
                        }
                        scriptsPath = scriptsPath.Substring(scriptsPath.IndexOf("Assets", StringComparison.Ordinal));
                    }
                }
                var assetPath = Path.Combine(scriptsPath, scriptName);
                File.WriteAllText(assetPath, scriptContents);
                createdAssetPaths.Add(assetPath);

                DebugLog.Log("Created " + assetPath);
            }

            var didCreateAssets = createdAssetPaths.Count > 0;
            if (didCreateAssets) {
                AssetDatabase.Refresh();
                var assets = AssetDatabase.LoadAllAssetsAtPath(createdAssetPaths.Last());
                EditorGUIUtility.PingObject(assets[0]);
            }

            if (createGeneratorSO || createPredicateSO) {
                if (!SetPendingSOPath(currentPath)) return;
            }
            if (createGeneratorSO) {
                QueueScriptableObjectCreation(messageClassName, ScriptType.MessageGenerator, PREFS_PENDING_GENERATOR_SO, !didCreateAssets);
            }
            if (createPredicateSO) {
                QueueScriptableObjectCreation(messageClassName, ScriptType.MessagePredicate, PREFS_PENDING_PREDICATE_SO, !didCreateAssets);
            }
        }

        private void QueueScriptableObjectCreation(string messageClassName, ScriptType type, string prefsKey, bool createNow) {
            var soTypeName = GetTypeName(messageClassName, type);
            if (createNow) {
                CreateScriptableObject(soTypeName);
            } else {
                EditorPrefs.SetString(prefsKey, soTypeName);
                DebugLog.Log("Will create " + soTypeName + " after compilation");
            }
        }

        private bool SetPendingSOPath(string currentPath) {
            var path = currentPath;
            if (!currentPath.Contains("/GG/Messages")) {
                path = EditorUtility.SaveFolderPanel(
                    $"Save '{messageName} Message' SOs under 'GG/Messages'",
                    Path.Combine(Application.dataPath, "GG/Messages"),
                    string.Empty);
                if (string.IsNullOrEmpty(path)) {
                    DebugLog.LogError("Saving message SOs aborted");
                    return false;
                }
                path = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));
            }
            EditorPrefs.SetString(PREFS_PENDING_SO_PATH, path);
            return true;
        }

        private bool CreatePendingScriptableObject(string prefsKey) {
            if (!EditorPrefs.HasKey(prefsKey))  return false;
            // There is a pending SO that needs to be created.
            CreateScriptableObject(EditorPrefs.GetString(prefsKey));
            EditorPrefs.DeleteKey(prefsKey);
            return true;
        }

        private void CreateScriptableObject(string typeName) {
            var soType = TypeExtensions.GetType(typeName);
            if (soType == null) {
                DebugLog.LogError($"Can't create ScriptableObject: Failed to load type '{typeName}'");
                return;
            }

            var filename = typeName.Split('.').Last().Replace("Generator", string.Empty);
            var asset = CreateInstance(soType);
            var path = EditorPrefs.GetString(PREFS_PENDING_SO_PATH, EditorUtils.GetCurrentProjectFolder());
            var assetPath = Path.Combine(path, $"{filename}.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            DebugLog.Log("Created " + assetPath);

            EditorGUIUtility.PingObject(asset);
        }

        private static string GetScriptName(string messageClassName, ScriptType scriptType) {
            return $"{messageClassName}{scriptType}.cs";
        }

        private string GetTypeName(string messageClassName, ScriptType scriptType) {
            return $"{namespaceName}.{messageClassName}{scriptType}";
        }

        private string GetScriptContents(
                string createAssetMainFolderName,
                string messageClassName,
                ScriptType scriptType) {
            if (useParameters == false
                || parameters.Count == 0
                || (parameters.Count == 1 && string.IsNullOrEmpty(parameters[0].Item1))) {
                return GetScriptContentsNoParameters(createAssetMainFolderName, messageClassName, scriptType);
            }
            return GetScriptContentsWithParameters(createAssetMainFolderName, messageClassName, scriptType);
        }

        private string GetScriptContentsNoParameters(string createAssetMainFolderName, string messageClassName, ScriptType scriptType) {
            return string.Format(
                scriptFormatStrings[scriptType].scriptNoParameters,
                createAssetMainFolderName,
                namespaceName,
                messageClassName,
                messageName);
        }

        private string GetScriptContentsWithParameters(
                string createAssetMainFolderName,
                string messageClassName,
                ScriptType scriptType) {
            var template = scriptFormatStrings[scriptType];
            if (template.scriptWithParameters == null) {
                return GetScriptContentsNoParameters(createAssetMainFolderName, messageClassName, scriptType);
            }

            var members = new string[parameters.Count];
            var properties = new string[parameters.Count];
            var constructor = new string[parameters.Count];
            var initializations = new string[parameters.Count];

            for (var i = 0; i < parameters.Count; ++i) {
                var type = parameters[i].Item1;
                var member = parameters[i].Item2;
                var property = string.IsNullOrEmpty(member)
                    ? string.Empty
                    : member.Substring(0, 1).ToUpper() + member.Substring(1);

                members[i] = string.Format(
                    template.parameterMemberDeclaration,
                    type,
                    member);
                properties[i] = string.Format(
                    template.parameterPropertyDeclaration,
                    type,
                    member,
                    property);
                constructor[i] = string.Format(
                    template.parameterConstructor,
                    type,
                    member);
                initializations[i] = string.Format(CONSTRUCTOR_MEMBER_INIT_FORMAT, member);
            }

            return string.Format(
                template.scriptWithParameters,
                createAssetMainFolderName,
                namespaceName,
                messageClassName,
                messageName,
                string.Join(string.Empty, members),
                string.Join(string.Empty, properties),
                string.Join(", ", constructor),
                string.Join(string.Empty, initializations));
        }

        private struct ScriptTemplate {
            /// <summary>
            /// Format string parameters:
            /// 0: Product name or Avrahamy.
            /// 1: Namespace (Avrahamy or Product).
            /// 2: Message class name (without 'Message' suffix).
            /// 3: Message name with spaces (without 'Message' suffix).
            /// </summary>
            public string scriptNoParameters;

            /// <summary>
            /// Format string parameters:
            /// 0: Product name or Avrahamy.
            /// 1: Namespace (Avrahamy or Product).
            /// 2: Message class name (without 'Message' suffix).
            /// 3: Message name with spaces (without 'Message' suffix).
            /// 4: Parameter member declarations.
            /// 5: Parameter property declarations.
            /// 6: Parameter constructor.
            /// 7: Member initialization in the constructor.
            /// </summary>
            public string scriptWithParameters;

            /// <summary>
            /// 0: Parameter type
            /// 1: Parameter name (camelCase)
            /// 2: Property name (PascalCase)
            /// Should include a blank line at the end.
            /// </summary>
            public string parameterPropertyDeclaration;

            /// <summary>
            /// 0: Parameter type
            /// 1: Parameter name (camelCase)
            /// Should include a blank line at the end.
            /// </summary>
            public string parameterMemberDeclaration;

            /// <summary>
            /// 0: Parameter type
            /// 1: Parameter name (camelCase)
            /// Automatically delimited with a ", ".
            /// </summary>
            public string parameterConstructor;
        }
    }
}
