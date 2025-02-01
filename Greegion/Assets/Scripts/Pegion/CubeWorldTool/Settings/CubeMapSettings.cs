using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Cube/Settings")]
public class CubeMapSettings : ScriptableObject
{
    private static readonly string SettingsPath = "Assets/Scripts/CubeMapSettings.asset";

    public static CubeMapSettings Load()
    {
        CubeMapSettings settings = AssetDatabase.LoadAssetAtPath<CubeMapSettings>(SettingsPath);
        if (settings == null)
        {
            settings = CreateInstance<CubeMapSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }

    public Color hitCubeColor = Color.red;
    public Color emptyCubeColor = Color.cyan;
    public Color fillCubeColor = Color.blue;
    public Color deleteCubeColor = new Color(1, 0.5f, 0.5f);
    public Material mat;
}
