using UnityEngine;

public static class CubeMapInputs
{
    public static ToolMode mode = ToolMode.None;
    
    public enum ToolMode
    {
        None,
        Create,
        CreateFill,
        CreateDrag,
        CreateDragFill,
        Delete,
        DeleteAbove,
        DeleteDrag,
        DeleteDragAbove,
    }

    public static ToolMode GetToolMode()
    {
        switch (Event.current.button)
        {
            case 0: // 左键
                if (!Event.current.alt)// 保留Alt键时的视角旋转操作
                {
                    switch (Event.current.type)
                    {
                        case EventType.MouseDown:
                            return Event.current.control ? ToolMode.CreateFill : ToolMode.Create;
                        case EventType.MouseDrag:
                            return Event.current.control ? ToolMode.CreateDragFill : ToolMode.CreateDrag;
                    }
                }
                break;
            case 1: // 右键
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        return Event.current.control ? ToolMode.DeleteAbove : ToolMode.Delete;
                    case EventType.MouseDrag:
                        return Event.current.control ? ToolMode.DeleteDragAbove : ToolMode.DeleteDrag;
                }
                break;
        }
        return mode;
    }
}
