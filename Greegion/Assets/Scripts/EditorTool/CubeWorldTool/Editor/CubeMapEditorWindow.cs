using System;
using UnityEditor;
using UnityEngine;

public class CubeMapEditorWindow : EditorWindow
{
    private CubeMapSheets sheets;

    [MenuItem("Window/Cube Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<CubeMapEditorWindow>("Cube Map Editor");
    }

    private void OnGUI()
    {
        sheets = (CubeMapSheets)EditorGUILayout.ObjectField("Sheets", sheets,typeof(CubeMapSheets));
    }

    private void OnValidate()
    {
        CubeMapEditor.sheet = sheets;
    }

    private void OnDisable()
    {
        CubeMapEditor.settings = null;
    }
}