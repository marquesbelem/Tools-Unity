using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

    public class GetJSONFileDrawEditor
    {
        private GUIContent _openFilePanelButton =
            new()
            {
                image = null,
                text = "Select the file JSON",
                tooltip = ""
            };

        private void DisplayCredentialsGUI()
        {
            EditorGUILayout.Separator();
            GUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            _showImportfromFile = GUILayout.Toggle(_showImportfromFile, "Import accounts from Json file");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (_showImportfromFile)
            {
                DisplayJsonLoaderGUI();
            }
        
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUILayout.Separator();
        }

        private void DisplayJsonLoaderGUI()
        {
            string jsonFile = null;
            if (GUILayout.Button(_openFilePanelButton))
            {
                jsonFile = EditorUtility.OpenFilePanel("Select file json", "", "json");
            }

            if (!string.IsNullOrEmpty(jsonFile))
            {
                string fileContent = File.ReadAllText(jsonFile);
                MultipleAccounts multiple = JsonConvert.DeserializeObject<MultipleAccounts>(fileContent);

                foreach (AccountManagerData account in multiple.Accounts)
                {
                    _state.AddAccount(account);
                }
            }
        }
    }