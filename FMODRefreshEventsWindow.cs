using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class FMODRefreshEventsWindow : EditorWindow
{
    private static FMODRefreshEventsWindow _instance;
    public static FMODRefreshEventsWindow Instance
    {
        get
        {
            if (_instance == null) _instance = GetWindow<FMODRefreshEventsWindow>("FMOD Refresh Events");
            return _instance;
        }
    }

    #region Character Sheet
    private static string _shieldHitSFX;
    private static string _shieldBreakSFX;
    private static string _armorHitSFXRef;
    private static string _armorBreakHitSFX;
    private static string _precisionHitSFX;
    private static string _criticalHitSFX;

    private GUIContent _characterSheetButton =
           new GUIContent
           {
               image = null,
               text = "Refresh Character Sheet",
               tooltip = "..."
           };

    private GUIContent _characterSheetEnemyButton =
          new GUIContent
          {
              image = null,
              text = "Refresh Character Sheet Enemy",
              tooltip = "..."
          };

    private GUIContent _weaponVisualButton =
          new GUIContent
          {
              image = null,
              text = "Refresh Weapon Visual",
              tooltip = "..."
          };
    #endregion

    [MenuItem("FMOD/Refresh Events")]
    static void Init()
    {
        Debug.Log(Instance.name);
    }

    private void OnGUI()
    {
        #region Character Sheet
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space(1);
            EditorGUILayout.LabelField("= REFRESH EVENTS IN CHARACTER SHEET =");
            EditorGUILayout.Space(1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Paths:");
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        _shieldHitSFX = EditorGUILayout.TextField("hit shield", _shieldHitSFX);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        _shieldBreakSFX = EditorGUILayout.TextField("hit shield break", _shieldBreakSFX);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        _armorHitSFXRef = EditorGUILayout.TextField("hit armor", _armorHitSFXRef);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        _armorBreakHitSFX = EditorGUILayout.TextField("hit armor break", _armorBreakHitSFX);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        _precisionHitSFX = EditorGUILayout.TextField("hit precision", _precisionHitSFX);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        _criticalHitSFX = EditorGUILayout.TextField("hit critical", _criticalHitSFX);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(1);

        if (GUILayout.Button(_characterSheetButton))
        {
            if (EditorUtility.DisplayDialog("Confirm Executing", "Are you confirm the change events?", "Yes", "Do Not Confirm"))
                ApplyPathInCharacterSheet(false);
        }
        if (GUILayout.Button(_characterSheetEnemyButton))
        {
            if (EditorUtility.DisplayDialog("Confirm Executing", "Are you confirm the change events?", "Yes", "Do Not Confirm"))
                ApplyPathInCharacterSheet(true);
        }

        #endregion

        #region Weapon Visual
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space(1);
            EditorGUILayout.LabelField("= REFRESH EVENTS IN WEAPON VISUAL DATA =");
            EditorGUILayout.Space(1);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button(_weaponVisualButton))
        {
            if (EditorUtility.DisplayDialog("Confirm Executing", "Are you confirm the change events?", "Yes", "Do Not Confirm"))
                ApplyPathInWeaponVisualData();
        }
        #endregion
    }

    static void ApplyPathInCharacterSheet(bool isEnemy)
    {
        var sheets = AssetDatabase.FindAssets("t:CharacterSheet")
            .Select(AssetDatabase.GUIDToAssetPath);

        foreach (var path in sheets)
        {
            var sheet = AssetDatabase.LoadAssetAtPath<CharacterSheet>(path);
            if ((isEnemy && sheet.IsEnemy) || 
                (!isEnemy && !sheet.IsEnemy))
            {
                sheet.SetEventPathSfxs(_shieldHitSFX,
                    _shieldBreakSFX,
                    _armorHitSFXRef,
                    _armorBreakHitSFX,
                    _precisionHitSFX,
                    _criticalHitSFX);
                EditorUtility.SetDirty(sheet);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void ApplyPathInWeaponVisualData()
    {
        var weapons = AssetDatabase.FindAssets("t:WeaponVisualData")
            .Select(AssetDatabase.GUIDToAssetPath);

        foreach (var path in weapons)
        {
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponVisualData>(path);
            EditorUtility.SetDirty(weapon);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}