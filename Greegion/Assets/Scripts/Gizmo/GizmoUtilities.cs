using UnityEditor;
using UnityEngine;

public class GizmoUtilities 
{
    public static void DrawCube(Vector3 center, Vector3 size, Color color, float opacity)
    {
        var storeGizmoColor = Gizmos.color;

        //实心
        Gizmos.color = new Color(color.r, color.g, color.b, opacity);
        Gizmos.DrawCube(center, size);
        
        //虚线
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = storeGizmoColor;
    }

    public static void DrawArrowCube(Vector3 center, Vector3 size, Vector3 direction, Color color, float opacity,
        float arrowSize = 1)
    {
        var storeGizmoColor = Gizmos.color;
        var storeHandleColor = Handles.color;

        //实心
        Gizmos.color = new Color(color.r, color.g, color.b, opacity);
        Gizmos.DrawCube(center, size);

        //虚线
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);

        Handles.color = color;
        Handles.ArrowHandleCap(0, new Vector3(center.x, center.y - arrowSize / 2, center.z),
            Quaternion.LookRotation(direction), 1f, EventType.Repaint);

        Gizmos.color = storeGizmoColor;
        Handles.color = storeHandleColor;
    }
}
