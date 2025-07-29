using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ToolAddPrefabs : OdinEditorWindow
{
    public static string ToolAddPrefabsFileName = "tool_prefabs_editor.json";
    [Serializable]
    public enum CategoryPrefabEditor
    {
        Pieces,
        Hazards,
        Bloogs,
        Gameplay,
        Lever,
        SpriteShape,
        Scenario,
        Other
    }

    [Serializable]
    public enum WorldPrefabEditor
    {
        Nenhum,
        Cave,
        ForestGrove,
        Ice,
        Mountain,
        Volcanic,
    }

    [ValueDropdown("GetCategoryPrefab")]
    [LabelText("Categoria")]
    public string CategoryPrefab;

    [ValueDropdown("GetWorldrefab")]
    [LabelText("Bioma")]
    public string WorldPrefab;

    [MenuItem("Tools/Add Prefabs")]
    private static void OpenWindow()
    {
        GetWindow<ToolAddPrefabs>().Show();
    }

    [LabelText("Nome do Prefab")]
    public string PrefabName;

    private List<string> GetCategoryPrefab()
    {
        List<string> category = new List<string>();
        foreach (var name in System.Enum.GetNames(typeof(CategoryPrefabEditor)))
        {
            category.Add(name);
        }
        return category;
    }

    private List<string> GetWorldrefab()
    {
        List<string> world = new List<string>();
        foreach (var name in System.Enum.GetNames(typeof(WorldPrefabEditor)))
        {
            world.Add(name);
        }
        return world;
    }

    [Button("Adicionar prefab")]
    private void SavePrefabPath()
    {
        if (string.IsNullOrEmpty(CategoryPrefab))
        {
            ShowNotification(new GUIContent("Preencha a categoria"));
            return;
        }

        if (string.IsNullOrEmpty(PrefabName))
        {
            ShowNotification(new GUIContent("Preencha o nome do prefab"));
            return;
        }

        if (string.IsNullOrEmpty(WorldPrefab))
        {
            ShowNotification(new GUIContent("Preencha o mundo"));
            return;
        }

        string[] guids = AssetDatabase.FindAssets($"{PrefabName} t:prefab");
        if (guids.Length == 0)
        {
            ShowNotification(new GUIContent("Esse prefab não está no projeto, verifique o nome"));
            return;
        }

        string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);

        string jsonPath = Path.Combine(Application.dataPath, ToolAddPrefabsFileName);
        List<PrefabData> prefabDataList = new List<PrefabData>();

        if (File.Exists(jsonPath))
        {
            string existingJson = File.ReadAllText(jsonPath);
            prefabDataList = JsonUtility.FromJson<PrefabDataList>(existingJson)?.Data ?? new List<PrefabData>();
        }

        var world = WorldPrefab == "Nenhum" ? string.Empty : WorldPrefab;
        if (prefabDataList.Exists(data => data.PrefabPath == prefabPath && data.World == world && data.Category == CategoryPrefab))
        {
            EditorWindow.focusedWindow.ShowNotification(new GUIContent("Esse prefab já foi adicionado!"));
            return;
        }
        prefabDataList.Add(new PrefabData
        {
            Category = CategoryPrefab,
            PrefabPath = prefabPath,
            World = world,
            PrefabName = PrefabName
        });

        PrefabDataList dataList = new PrefabDataList { Data = prefabDataList };
        string json = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(jsonPath, json);

        EditorWindow.focusedWindow.ShowNotification(new GUIContent($"Adicionado com sucesso! \n Commite o arquivo {ToolAddPrefabsFileName}"));
    }

    [System.Serializable]
    public class PrefabData
    {
        public string Category;
        public string World;
        public string PrefabPath;
        public string PrefabName;
    }

    [System.Serializable]
    public class PrefabDataList
    {
        public List<PrefabData> Data;
    }
}
