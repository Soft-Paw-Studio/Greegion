using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeMapView : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/CubeMapView")]
    public static void ShowExample()
    {
        CubeMapView wnd = GetWindow<CubeMapView>();
        wnd.titleContent = new GUIContent("CubeMapView");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        VisualElement tree = m_VisualTreeAsset.CloneTree();
        tree.style.flexGrow = 1;
        root.Add(tree);
    }

    private void OnInspectorUpdate()
    {

        //var obj = rootVisualElement.Q<ObjectField>();
        //if(obj == null)return;
        //Debug.Log("validate");
        //var test = rootVisualElement.Q<VisualElement>("Test");
        //test.style.backgroundImage = new StyleBackground(AssetPreview.GetAssetPreview(obj.value));

    }

    private void OnValidate()
    {

    }
}
