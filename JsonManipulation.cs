using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.IO;
using UnityEditor;
using UnityEngine;


    public class JsonManipulation : EditorWindow
    {
        private string m_JsonFolderPath = "Assets/StreamingAssets/Levels";

        #region Update Fields
        private string m_TargetRootField = "";
        private string m_TargeChildField = "";
        private string m_TargetFieldNewValue = "";
        #endregion

        #region Replace Filed
        private string m_SearchRootField = "";
        private string m_ReplaceField = "";
        #endregion

        #region Add Field
        private string m_RootAddField = "";
        private string m_NewField = "";
        private string m_NewFieldValue = "";
        #endregion

        #region Update Props Color
        private Color m_NewColor;
        #endregion

        [MenuItem("Tools/Manipulation JSON")]
        public static void ShowWindow()
        {
            GetWindow<JsonManipulation>("Manipulation JSON");
        }

        private void OnGUI()
        {
            m_JsonFolderPath = EditorGUILayout.TextField("JSON Folder Path", m_JsonFolderPath);
            GUILayout.Space(20);
            DrawUpdateFields();
            GUILayout.Space(20);
            DrawReplaceFields();
            GUILayout.Space(20);
            DrawAddFieldInJson();
            GUILayout.Space(20);
            DrawAddColorInProps();
        }

        private void DrawAddColorInProps()
        {
            GUIStyle sectionStyle = new GUIStyle(GUI.skin.box);
            sectionStyle.normal.background = MakeTex(2, 2, new Color(0.46f, 0.46f, 0.46f, 0.5f));

            EditorGUILayout.BeginVertical(sectionStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("New Color to props", GUILayout.Width(200));
                    m_NewColor = EditorGUILayout.ColorField(m_NewColor);
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Modify Color in JSONs"))
                {
                    AddColorInProps();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAddFieldInJson()
        {
            GUIStyle sectionStyle = new GUIStyle(GUI.skin.box);
            sectionStyle.normal.background = MakeTex(2, 2, new Color(0.46f, 0.46f, 0.46f, 0.5f));

            EditorGUILayout.BeginVertical(sectionStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Where to add Field (Root Property)", GUILayout.Width(200));
                    m_RootAddField = EditorGUILayout.TextField(m_RootAddField);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("    Add Field", GUILayout.Width(200));
                    m_NewField = EditorGUILayout.TextField(m_NewField);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("    New Value", GUILayout.Width(200));
                    m_NewFieldValue = EditorGUILayout.TextField(m_NewFieldValue);
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Add Field in JSONs"))
                {
                    AddFieldInJson();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawReplaceFields()
        {
            GUIStyle sectionStyle = new GUIStyle(GUI.skin.box);
            sectionStyle.normal.background = MakeTex(2, 2, new Color(0.46f, 0.46f, 0.46f, 0.5f));

            EditorGUILayout.BeginVertical(sectionStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Search Field", GUILayout.Width(200));
                    m_SearchRootField = EditorGUILayout.TextField(m_SearchRootField);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Replace Field", GUILayout.Width(200));
                    m_ReplaceField = EditorGUILayout.TextField(m_ReplaceField);
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Replace Field in JSONs"))
                {
                    ReplaceFieldInJsons();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawUpdateFields()
        {
            GUIStyle sectionStyle = new GUIStyle(GUI.skin.box);
            sectionStyle.normal.background = MakeTex(2, 2, new Color(0.46f, 0.46f, 0.46f, 0.5f));

            EditorGUILayout.BeginVertical(sectionStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Target Field Parent (Root Property)", GUILayout.Width(200));
                    m_TargetRootField = EditorGUILayout.TextField(m_TargetRootField);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("     Target Field (Child Property)", GUILayout.Width(200));
                    m_TargeChildField = EditorGUILayout.TextField(m_TargeChildField);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("New value", GUILayout.Width(200));
                    m_TargetFieldNewValue = EditorGUILayout.TextField(m_TargetFieldNewValue);
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Update Value in JSONs"))
                {
                    UpdateJsons();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void UpdateJsons()
        {
            string[] jsonFiles = Directory.GetFiles(m_JsonFolderPath, "*.json");

            foreach (string filePath in jsonFiles)
            {
                string json = File.ReadAllText(filePath);
                JObject jsonData = JObject.Parse(json);

                if (jsonData[m_TargetRootField] is JArray childrenArray)
                {
                    foreach (JObject child in childrenArray)
                        child[m_TargeChildField] = m_TargetFieldNewValue;
                }
                else
                {
                    if (string.IsNullOrEmpty(m_TargeChildField))
                    {
                        jsonData[m_TargetRootField] = m_TargetFieldNewValue;
                    }
                    else
                    {
                        var children = jsonData[m_TargetRootField][m_TargeChildField];
                        if (children != null)
                            jsonData[m_TargetRootField][m_TargeChildField] = m_TargetFieldNewValue;
                    }
                }

                string updatedJson = jsonData.ToString(Formatting.Indented);
                File.WriteAllText(filePath, updatedJson);
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Manipulation JSON", "JSONs updated successfully!", "OK");
        }

        private void ReplaceFieldInJsons()
        {
            string[] jsonFiles = Directory.GetFiles(m_JsonFolderPath, "*.json");

            foreach (string filePath in jsonFiles)
            {
                string json = File.ReadAllText(filePath);
                json = json.Replace(m_SearchRootField, m_ReplaceField);
                File.WriteAllText(filePath, json);
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Manipulation JSON", "Fields replaced successfully!", "OK");
        }

        private void AddFieldInJson()
        {
            string[] jsonFiles = Directory.GetFiles(m_JsonFolderPath, "*.json");

            foreach (string filePath in jsonFiles)
            {
                string json = File.ReadAllText(filePath);
                JObject jsonData = JObject.Parse(json);

                if (jsonData[m_RootAddField] is JArray propConfigs)
                {
                    foreach (JObject propConfig in propConfigs)
                        propConfig[m_NewField] = m_NewFieldValue;
                }
                else
                {
                    jsonData[m_NewField] = m_NewFieldValue;
                }
                string updatedJson = jsonData.ToString(Formatting.Indented);
                File.WriteAllText(filePath, updatedJson);
            }

            AssetDatabase.Refresh();
            Debug.Log("Property added to PropConfigs successfully!");
        }

        private void AddColorInProps()
        {
            string[] jsonFiles = Directory.GetFiles(m_JsonFolderPath, "*.json");

            foreach (string filePath in jsonFiles)
            {
                string json = File.ReadAllText(filePath);
                JObject jsonData = JObject.Parse(json);

                if (jsonData["PropConfigs"] is JArray propConfigs)
                {
                    foreach (JObject propConfig in propConfigs)
                    {
                        propConfig["Color"] = new JObject
                        {
                            { "r", m_NewColor.r },
                            { "g", m_NewColor.g },
                            { "b", m_NewColor.b },
                            { "a", m_NewColor.a }
                        };

                    }
                    string updatedJson = jsonData.ToString(Formatting.Indented);
                    File.WriteAllText(filePath, updatedJson);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Property added to PropConfigs successfully!");
        }

        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

    }
