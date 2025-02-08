using UnityEditor;
using UnityEngine;

public static class TileMapUtilities
{
    public enum AlignMode
    {
        None,X,Y
    }
    
    public const float GridSize = 1f;
    public const int brushSize = 1; // 笔刷大小, 默认是1x1
    
    /// <summary>
    /// Aligns a position to the nearest grid point based on the specified grid size.
    /// </summary>
    public static Vector3 AlignToGrid(Vector3 position, float gridSize)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }
    
    /// <summary>
    /// Checks if a position is occupied by any object using a box check with a given size.
    /// </summary>
    public static bool IsPositionOccupied(Vector3 position, float gridSize)
    {
        return Physics.CheckBox(position, Vector3.one * gridSize * 0.4f);
    }
    
    /// <summary>
    /// Calculates the intersection point of a ray with the Y = 0 plane.
    /// </summary>
    public static Vector3 GetYZeroIntersectionPoint(Ray ray)
    {
        if (ray.direction.y == 0) return ray.origin; // Default fallback if ray is parallel to Y axis
        var distanceToYZero = -ray.origin.y / ray.direction.y;
        return ray.origin + ray.direction * distanceToYZero;
    }
    
    /// <summary>
    /// Gets all colliders at a specified position using a box check with a given size.
    /// </summary>
    public static Collider[] GetCollidersAtPosition(Vector3 position, float gridSize)
    {
        return Physics.OverlapBox(position, Vector3.one * gridSize * 0.4f);
    }
    
    /// <summary>
    /// Get nearest empty position.
    /// </summary>
    /// <param name="hitPoint"></param>
    /// <param name="normal"></param>
    /// <returns></returns>
    public static Vector3 FindNearestEmptyPosition(Vector3 hitPoint, Vector3 normal)
    {
        var alignedHitPoint = AlignToGrid(hitPoint, GridSize);
        var offsetDirection = AlignToGrid(normal, GridSize);

        if (!IsPositionOccupied(alignedHitPoint, GridSize))
        {
            return alignedHitPoint;
        }

        var offsetPosition = alignedHitPoint + offsetDirection * GridSize;
        if (!IsPositionOccupied(offsetPosition, GridSize))
        {
            return offsetPosition;
        }

        Vector3[] directions = {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };

        foreach (var dir in directions)
        {
            var checkPosition = alignedHitPoint + dir * GridSize;
            if (!IsPositionOccupied(checkPosition, GridSize))
            {
                return checkPosition;
            }
        }

        return alignedHitPoint;
    }


    /// <summary>
    /// Draw a handle wire cube in the world position.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="label"></param>
    public static void DrawWireCubes(Vector3 startPosition)
    {
        for (int x = 0; x < brushSize; x++)
        {
            for (int z = 0; z < brushSize; z++)
            {
                var position = startPosition + new Vector3(x * GridSize, 0, z * GridSize);
                Handles.DrawWireCube(position, Vector3.one * GridSize);
                //Handles.Label(position,Vector3Int.FloorToInt(position).ToString());
            }
        }
    }
    
    public static void DrawWireCubeRange(Vector3 startPosition, Color color)
    {
        // 计算起始位置，使得整个方块区域围绕鼠标中心
        Vector3 startAligned = AlignToGrid(startPosition, GridSize);
        float startY = startPosition.y;
        float minY = 0; // Y轴最小值为0

        Handles.color = color;

        Vector3 drawPosition = startAligned;
        float y = startY; // 当前方块的高度

        // 计算绘制区域的最小Y值
        while (y >= minY)
        {
            if (!IsPositionOccupied(new Vector3(drawPosition.x, y, drawPosition.z), GridSize))
            {
                DrawWireCubes(new Vector3(drawPosition.x, y, drawPosition.z));
            }
            y -= GridSize;
        }
    }
}