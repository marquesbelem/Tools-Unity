using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ToolAddPrefabs;

public class PrefabWindow : EditorWindow
{
    private List<PrefabData> prefabEntries = new();
    private Dictionary<string, Dictionary<string, List<PrefabData>>> categorizedPrefabs;

    private string[] categories;
    private int selectedCategory = 0;

    private string[] currentBiomes = new string[0];
    private int selectedBiome = 0;

    [MenuItem("Level/Prefabs", false, 1)]
    private static void OpenWindow()
    {
        GetWindow<PrefabWindow>("Prefab Spawner").Show();
    }

    private void OnEnable()
    {
        LoadPrefabData();
        CategorizePrefabs();
    }

    private void LoadPrefabData()
    {
        string jsonPath = Path.Combine(Application.dataPath, ToolAddPrefabs.ToolAddPrefabsFileName);

        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"Config file not found at: {jsonPath}");
            prefabEntries = new();
            return;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        var prefabDataList = JsonConvert.DeserializeObject<PrefabDataList>(jsonContent);

        if (prefabDataList == null)
        {
            Debug.LogError("Failed to parse JSON config.");
            prefabEntries = new();
            return;
        }

        prefabEntries = prefabDataList.Data;
    }

    private void CategorizePrefabs()
    {
        // Agrupa por categoria, depois por bioma
        categorizedPrefabs = prefabEntries
            .GroupBy(p => string.IsNullOrEmpty(p.Category) ? "Uncategorized" : p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(p => string.IsNullOrEmpty(p.World) ? "Nenhum" : p.World)
                      .ToDictionary(bg => bg.Key, bg => bg.ToList())
            );

        categories = categorizedPrefabs.Keys.ToArray();

        // Atualiza biomas da primeira categoria
        if (categories.Length > 0)
        {
            UpdateBiomeTabs(categories[0]);
        }
    }

    private void UpdateBiomeTabs(string category)
    {
        if (categorizedPrefabs.TryGetValue(category, out var biomeDict))
        {
            currentBiomes = biomeDict.Keys.ToArray();
            selectedBiome = 0;
        }
        else
        {
            currentBiomes = new string[0];
        }
    }

    private Vector2 scrollPosition;

    private void OnGUI()
    {
        if (categories == null || categories.Length == 0)
        {
            EditorGUILayout.HelpBox("No prefab data loaded.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Create Image Reference", GUILayout.Height(30)))
        {
            CreateImageReference();
        }
        GUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        selectedCategory = GUILayout.Toolbar(selectedCategory, categories);
        string currentCategory = categories[selectedCategory];
        if (EditorGUI.EndChangeCheck())
        {
            UpdateBiomeTabs(currentCategory);
        }

        GUILayout.Space(10);

        if (currentBiomes.Length == 0)
        {
            EditorGUILayout.HelpBox("No biomes in this category.", MessageType.Info);
            return;
        }

        selectedBiome = GUILayout.Toolbar(selectedBiome, currentBiomes);
        string currentBiome = currentBiomes[selectedBiome];

        // GUILayout.Space(10);

        if (!categorizedPrefabs.TryGetValue(currentCategory, out var biomes)) return;
        if (!biomes.TryGetValue(currentBiome, out var prefabs)) return;

        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
        {
            scrollPosition = scrollView.scrollPosition;

            foreach (var prefab in prefabs)
            {
                GUI.backgroundColor = Color.yellow; // Set button color to yellow  
                if (GUILayout.Button(prefab.PrefabName, GUILayout.Height(30)))
                {
                    SpawnPrefab(prefab);
                }
                GUI.backgroundColor = Color.white; // Reset button color to default  
            }
        }
    }

    private void SpawnPrefab(PrefabData prefabData)
    {
        if (prefabData == null)
        {
            ShowNotification(new GUIContent("Invalid prefab."));
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabData.PrefabPath);

        if (prefab == null)
        {
            ShowNotification(new GUIContent($"Prefab not found: {prefabData.PrefabPath}"));
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instance != null)
        {
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);

            if (prefabData.PrefabPath.Contains("Shape") || prefabData.PrefabPath.Contains("shape"))
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
    }

    private static void CreateImageReference()
    {
        string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"Referencia n�o encontrada.");
            return;
        }

        string newPath = "Assets/_Game/Sprites/Refs/Level Ref";

        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }

        var pathList = path.Split("/");
        newPath = newPath + "/" + pathList[pathList.Length - 1];

        if (File.Exists(newPath))
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "File Already Exists", // T�tulo da janela de di�logo
                "The file '" + newPath + "' already exists. Do you want to overwrite it?", // Mensagem
                "Yes, Overwrite",     // Texto do bot�o "OK"
                "No, Cancel"          // Texto do bot�o "Cancelar"
            );

            if (overwrite)
            {
                File.Delete(newPath);
                File.Copy(path, newPath);
            }
        }
        else
        {
            File.Copy(path, newPath);
        }

        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(newPath) as TextureImporter;
        if (importer != null)
        {
            Undo.RecordObject(importer, "Change Texture Resolution");

            importer.maxTextureSize = 8192;
            importer.SaveAndReimport();
        }

        string fullPath = "Assets/_Game/Prefabs/Game/LevelImageReference.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(newPath);

        if (prefab == null)
        {
            Debug.LogError($"Prefab n�o encontrado em: {fullPath}");
            return;
        }

        if (image == null)
        {
            Debug.LogError($"Sprite n�o encontrado em: {newPath}");
            return;
        }

        var width = image.texture.width;
        var height = image.texture.height;

        if (height % 1080 != 0)
        {
            Debug.LogError($"Sprite N�o est� na dimens�o correta de 1920/1080 : {newPath}");
            return;
        }

        float value = (float)width / 1920;
        float value2 = value - (int)value;

        if (value2 != 0 && value2 != 0.5f)
        {
            Debug.LogError($"Sprite N�o est� na dimens�o correta de 1920/1080 : {newPath}");
            return;
        }

        GameObject spawnedObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (spawnedObject != null)
        {
            Undo.RegisterCreatedObjectUndo(spawnedObject, "Spawn Prefab");
            Selection.activeGameObject = spawnedObject;
            PrefabUtility.UnpackPrefabInstance(spawnedObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        var spriteRenderer = spawnedObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = image;

        float screenHeightWorldUnits = 11.25f * 2.0f;
        float gameObjectHeightWorldUnits = spriteRenderer.bounds.size.y;

        float scaleFactor = screenHeightWorldUnits / gameObjectHeightWorldUnits;

        spawnedObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

        if (width != 1920)
        {
            float moveValue = 40 / 4;
            moveValue *= (width / 1920);

            spawnedObject.transform.position = new Vector3(moveValue, 0, 0);
        }
    }
}

[System.Serializable]
public class PrefabDataList
{
    public List<PrefabData> Data;
}

