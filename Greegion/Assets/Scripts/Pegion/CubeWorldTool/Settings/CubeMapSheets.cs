using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cube/New Sheets")]
public class CubeMapSheets : ScriptableObject
{
    public Color hitCubeColor = Color.red;
    public Color emptyCubeColor = Color.cyan;
    public Color fillCubeColor = Color.blue;
    public Color deleteCubeColor = new Color(1, 0.5f, 0.5f);
}
