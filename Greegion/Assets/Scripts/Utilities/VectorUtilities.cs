using UnityEngine;

public static class VectorUtilities
{
    public static void SnapToGrid(this Transform transform)
    {
        var position = transform.position;
        
        transform.position = new Vector3
        {
            x = Mathf.Round(position.x),
            y = Mathf.Round(position.y),
            z = Mathf.Round(position.z)
        };
    }
}
