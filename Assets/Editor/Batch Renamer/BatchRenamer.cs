using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BatchRenamer {

    public class BatchRenamer : EditorWindow {

        [MenuItem("Edit/Batch Rename")]
        [MenuItem("GameObject/Batch Renamer")]
        public static void ShowWindow() {
            EditorWindow window = GetWindow<BatchRenamer>();
            window.titleContent = new GUIContent("Batch Renamer");
        }

        // GENERAL

        private readonly string GREEN = "#c9e743";
        private string[] toolbarOptions = { "Replace", "Set", "Strip", "Others" };
        private int toolbarSelection;
        private string infoBoxText = "";
        private Vector2 infoBoxScroll;

        // REPLACE

        private string replacePattern = "";
        private string replaceWith = "";
        private bool caseSensitiveReplace;

        // SET

        private string prefix = "";
        private string setName = "";
        private string suffix = "";

        // STRIP

        private readonly char[] DIGITS = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly char[] PUNCTUATION = new char[] { '.', ',', ';', ':' };
        private string customChars = "";
        private string[] stripOptions = { "Start", "End", "Both" };
        private bool stripSpaces;
        private bool stripDigits;
        private bool stripPunctuation;
        private int stripSelection;

        // OTHERS

        private string removeAfter = "";
        private string removeBefore = "";
        private string[] caseOptions = { "Default", "Lowercase", "Uppercase" };
        private int caseSelection;

        private void OnEnable() => Selection.selectionChanged += SetInfoBoxText;

        private void OnDisable() => Selection.selectionChanged -= SetInfoBoxText;

        private void OnGUI() {
            GUI.changed = false;
            toolbarSelection = GUILayout.Toolbar(toolbarSelection, toolbarOptions);

            switch (toolbarSelection) {
                case 0:
                    GUILayout.BeginHorizontal();
                    EditorGUIUtility.fieldWidth = (position.width - 350);
                    replacePattern = EditorGUILayout.TextField("Replace", replacePattern);
                    caseSensitiveReplace = EditorGUILayout.ToggleLeft("Case Sensitive", caseSensitiveReplace);
                    GUILayout.EndHorizontal();

                    replaceWith = EditorGUILayout.TextField("With", replaceWith);

                    break;

                case 1:
                    prefix = EditorGUILayout.TextField("Prefix", prefix);
                    setName = EditorGUILayout.TextField("Set", setName);
                    suffix = EditorGUILayout.TextField("Suffix", suffix);

                    break;

                case 2:
                    customChars = EditorGUILayout.TextField("Custom Characters", customChars);

                    GUILayout.BeginHorizontal("Box");
                    GUILayout.FlexibleSpace();
                    stripSpaces = EditorGUILayout.ToggleLeft("Spaces", stripSpaces);
                    stripDigits = EditorGUILayout.ToggleLeft("Digits", stripDigits);
                    stripPunctuation = EditorGUILayout.ToggleLeft("Punctuation", stripPunctuation);
                    GUILayout.EndHorizontal();

                    stripSelection = GUILayout.Toolbar(stripSelection, stripOptions);

                    break;

                case 3:
                    removeAfter = EditorGUILayout.TextField("Remove After", removeAfter);
                    removeBefore = EditorGUILayout.TextField("Remove Before", removeBefore);

                    GUILayout.BeginHorizontal();
                    caseSelection = GUILayout.Toolbar(caseSelection, caseOptions);
                    GUILayout.EndHorizontal();

                    break;
            }

            if (GUI.changed) {
                SetInfoBoxText();
            }

            GUIStyle resultInfoBoxStyle = GUI.skin.GetStyle("HelpBox");
            resultInfoBoxStyle.richText = true;
            resultInfoBoxStyle.stretchHeight = true;
            resultInfoBoxStyle.fontSize = 12;

            infoBoxScroll = EditorGUILayout.BeginScrollView(infoBoxScroll);
            EditorGUILayout.LabelField(infoBoxText, resultInfoBoxStyle);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Rename")) {
                Rename();
                SetInfoBoxText();
            }
        }

        private void Rename() {
            foreach (GameObject obj in Selection.gameObjects) {
                Undo.RecordObject(obj, "Batch Renaming");
                obj.name = GetRenamed(obj.name);
            }
        }

        private void SetInfoBoxText() {
            infoBoxText = "";
            foreach (GameObject obj in Selection.gameObjects)
                infoBoxText += $"<b>{obj.name}</b> -> <b><color={GREEN}>{GetRenamed(obj.name)}</color></b>\n";
            Repaint();
        }

        private string GetRenamed(string name) {
            switch (toolbarSelection) {
                case 0:
                    try {
                        RegexOptions regexOptions = caseSensitiveReplace ? RegexOptions.None : RegexOptions.IgnoreCase;
                        name = Regex.Replace(name, replacePattern, replaceWith, regexOptions);
                    } catch { break; }

                    break;

                case 1:
                    if (setName.Length != 0)
                        name = setName;
                    name = prefix + name;
                    name += suffix;

                    break;

                case 2:
                    switch (stripSelection) {
                        case 0:
                            name = name.TrimStart(customChars.ToCharArray());
                            name = stripSpaces ? name.TrimStart() : name;
                            name = stripDigits ? name.TrimStart(DIGITS) : name;
                            name = stripPunctuation ? name.TrimStart(PUNCTUATION) : name;

                            break;

                        case 1:
                            name = name.TrimEnd(customChars.ToCharArray());
                            name = stripSpaces ? name.TrimEnd() : name;
                            name = stripDigits ? name.TrimEnd(DIGITS) : name;
                            name = stripPunctuation ? name.TrimEnd(PUNCTUATION) : name;
                            break;

                        case 2:
                            name = name.Trim(customChars.ToCharArray());
                            name = stripSpaces ? name.Trim() : name;
                            name = stripDigits ? name.Trim(DIGITS) : name;
                            name = stripPunctuation ? name.Trim(PUNCTUATION) : name;
                            break;
                    }

                    break;

                case 3:
                    try {
                        if (removeAfter.Length != 0)
                            name = name.Split(removeAfter)[0];

                        if (removeBefore.Length != 0)
                            name = name.Split(removeBefore)[1];
                    } catch { }

                    switch (caseSelection) {
                        case 1:
                            name = name.ToLower();
                            break;

                        case 2:
                            name = name.ToUpper();
                            break;
                    }
                    break;
            }

            return name;
        }
    }
}