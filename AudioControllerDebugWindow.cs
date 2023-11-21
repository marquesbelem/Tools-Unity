using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class AudioControllerDebugWindow : EditorWindow
{
    private static AudioControllerDebugWindow _instance;
    public static AudioControllerDebugWindow Instance
    {
        get
        {
            if (_instance == null) _instance = GetWindow<AudioControllerDebugWindow>("Audio Snapshot Debug");
            return _instance;
        }
    }

    private Vector2 _scrollPos = new Vector2();

    private const string LABEL_KEY = "Key";
    private const string LABEL_STATE = "State";
    private const string LABEL_STACK = "Stack Trace";
    private const int SPACE_LINE_LABEL = 5;
    private bool _showStack;

    [MenuItem("FMOD/Audio Snapshot Debug")]
    static void Init()
    {
        Debug.Log(Instance.name);
    }

    private void OnGUI()
    {
        AudioController.Debug = EditorGUILayout.Toggle("Log played sound", AudioController.Debug);
        EditorGUILayout.LabelField("MusicManager:");

        if (Application.isPlaying)
        {
            foreach (var pair in MusicManager.Channels)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(pair.Key.ToString());
                pair.Value.eventInstance.getDescription(out var description);
                description.getPath(out var path);
                EditorGUILayout.LabelField(path);
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.LabelField("AudioController:");
        
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        var state = new GUIStyleState();
        var style = new GUIStyle();


        #region 2º Linha - Cabeçalho
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.MaxWidth(600),GUILayout.MinWidth(400));
        GUILayout.Box(LABEL_KEY);
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.MaxWidth(100), GUILayout.MinWidth(100));
        GUILayout.Box(LABEL_STATE);
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.MaxWidth(100), GUILayout.MinWidth(100));
        GUILayout.Box(string.Empty);
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.MaxWidth(100), GUILayout.MinWidth(100));
        GUILayout.Box(string.Empty);
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.MaxWidth(300), GUILayout.MinWidth(300));
        GUILayout.Box(LABEL_STACK);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        #endregion

        #region 3º Linha - Conteudo
        foreach (KeyValuePair<string, EventInstance> snapshotKvp in FMODAddonsDebug.SnapList)
        {
            snapshotKvp.Value.getDescription(out var description);
            description.getPath(out var path);
            snapshotKvp.Value.getPlaybackState(out var playbackState);

            if (!string.IsNullOrEmpty(path))
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical(GUILayout.MaxWidth(600),GUILayout.MinWidth(400));
                var label = $"{path}";
                GUILayout.Label(label);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.MaxWidth(100), GUILayout.MinWidth(100));
                var labelPlaybackState = $"{playbackState}";
                state.textColor = playbackState == PLAYBACK_STATE.PLAYING ? Color.green : Color.grey;
                style.normal = state;
                GUILayout.Label(labelPlaybackState, style);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                if (GUILayout.Button("START", GUILayout.MinWidth(100), GUILayout.MaxWidth(100), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)))
                    snapshotKvp.Value.start();
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                if (GUILayout.Button("STOP", GUILayout.MinWidth(100), GUILayout.MaxWidth(100), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)))
                    snapshotKvp.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.MaxWidth(300), GUILayout.MinWidth(300));
                if (FMODAddonsDebug.StackTraceList.TryGetValue(snapshotKvp.Key, out var stackTrace))
                {
                    _showStack = EditorGUILayout.Toggle(string.Empty, _showStack);

                    if (_showStack)
                    {
                        GUILayout.Label(stackTrace);
                    }
                }
                GUILayout.EndVertical();

                GUILayout.Space(SPACE_LINE_LABEL);
                GUILayout.EndHorizontal();
            }
        }
        #endregion

        EditorGUILayout.EndScrollView();
    }
}
