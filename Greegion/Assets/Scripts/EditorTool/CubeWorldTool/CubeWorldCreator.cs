using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using static TileMapUtilities;
using static CubeWorldUtilities;

[EditorTool("CubeWorldCreator")]
public class CubeWorldCreator : EditorTool
{
    #region 私有变量
    private static CubeMapSettings settings;
    
    //World Positions
    private Vector3 mousePosition;
    private Vector3 emptyPosition;//空方块位置
    private Vector3 hitPosition;//射线指向方块位置
    private Vector3 lastOperationPosition;//上一次进行操作的方块位置
    private Vector3? recordPosition;//点击鼠标时保存的方块位置
    
    private Ray currentRay;
    private RaycastHit currentHit;
    private ToolMode toolMode;
    private bool SupportMode;//自动向下加满方块
    private bool FillMode;//填满方块
    private bool AlignMode;//按照X轴或是Z轴进行摆放
    private const int MaxFillRange = 100;
    private const int MaxFillCount = 5000; // 防止无限填充
    #endregion
    
    private HashSet<Vector3> previewPositions = new HashSet<Vector3>();
    private bool isSpacePressed = false;
    private bool isClosedSpace = true;
    public override void OnActivated()
    {
        base.OnActivated();
        Selection.activeObject = null;
    }

    #region 界面输入
    public override void OnToolGUI(EditorWindow window)
    {
        if(settings == null) settings = CubeMapSettings.Load();
        
        HandleInput();
        RayDetection();
        window.Repaint();
    }

    private void HandleInput()
    {
        if (Event.current.alt) return;

        FillMode = Event.current.control && Event.current.shift;
        SupportMode = Event.current.control;
        AlignWorldXZ();
        
        switch (Event.current.button)
        {
            //左键
            case 0:
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
                {
                    if (FillMode)
                    {
                        toolMode = ToolMode.Fill;
                        FillClosedSpace();
                    }
                    else
                    {
                        toolMode = ToolMode.Create;
                        OnCreateCube();
                    }

                    Event.current.Use();
                }

                break;

            //右键
            case 1:
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
                {
                    toolMode = ToolMode.Delete;
                    OnDeleteCube();
                    Event.current.Use();
                }
                break;
        }

        if (Event.current.type != EventType.MouseUp) return;
        toolMode = ToolMode.None;
        recordPosition = null;
    }
    #endregion

    #region 检测定位
    private void RayDetection()
    {
        // 获取鼠标位置并将其转换为世界空间中的射线
        mousePosition = Event.current.mousePosition;
        var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        CalculatePositions(ray);
        DrawWireCube(emptyPosition);
    }
    private void CalculatePositions(Ray ray)
    {
        if (Physics.Raycast(ray, out currentHit))
        {
            hitPosition = AlignToGrid(currentHit.transform.position, GridSize);
            emptyPosition = FindNearestEmptyPosition(hitPosition, currentHit.normal);
        }
        else
        {
            var intersectPoint = GetYZeroIntersectionPoint(ray);
            emptyPosition = AlignToGrid(intersectPoint, GridSize);
            emptyPosition.y = 0;

            if (emptyPosition == Vector3.zero)
            {
                emptyPosition = new Vector3(0, 0, 0);
            }
        }
    }
    #endregion


    #region 创建删除
    private void OnCreateCube()
    {
        //if (emptyPosition == lastPlacePosition) return;
        
        //刷新记录位置
        recordPosition ??= emptyPosition;
        
        // 记录撤销操作，放置方块
        Undo.RecordObject(this, "Place Cube");
        var positionToPlace = new Vector3(emptyPosition.x, recordPosition.Value.y, emptyPosition.z);
        CreateCubeAtPosition(positionToPlace);
        lastOperationPosition = positionToPlace;
    }
    
    private void CreateCubeAtPosition(Vector3 position)
    {
        // 如果是 FillMode 的话向下补齐方块，直到 Y=0
        for (var y = SupportMode ? position.y : 0; y >= 0; y -= GridSize)
        {
            if (SupportMode) position.y = y;
            if (IsPositionOccupied(position, GridSize)) continue;

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<MeshRenderer>().sharedMaterial = settings.mat;
            cube.transform.position = position;
            cube.tag = "Untagged";
            cube.layer = LayerMask.NameToLayer("Ground");
            

            if (GameObject.Find("LevelRoot"))
            {
                cube.transform.SetParent(GameObject.Find("LevelRoot").transform);
            }
            else
            {
                var root = new GameObject
                {
                    name = "LevelRoot",
                    transform =
                    {
                        position = Vector3.zero
                    }
                };

                cube.transform.SetParent(root.transform);
            }
            
            Undo.RegisterCreatedObjectUndo(cube, "Place Cube");
        }
    }

    private void AlignWorldXZ()
    {
        if (!recordPosition.HasValue || !Event.current.shift) return;
        
        var directionToFix = emptyPosition - recordPosition;
        var lockedX = Mathf.Abs(directionToFix.Value.x);
        var lockedZ = Mathf.Abs(directionToFix.Value.z);
            
        if (lockedX <  lockedZ)
        {
            emptyPosition.x = recordPosition.Value.x;
            hitPosition.x = recordPosition.Value.x;
        }
        else
        {
            emptyPosition.z = recordPosition.Value.z;
            hitPosition.z = recordPosition.Value.z;
        }
    }

    private void OnDeleteCube()
    {
        //if (hitPosition == lastPlacePosition) return;

        //刷新记录位置
        var position = currentHit.transform == null ? emptyPosition : hitPosition;
        recordPosition ??= position;

        Undo.RecordObject(this, "Delete Cube");
        var positionToDelete = new Vector3(position.x, recordPosition.Value.y, position.z);
        DeleteCubeAtPosition(positionToDelete); // 删除多个方块
        lastOperationPosition =  position;
    }

    private void DeleteCubeAtPosition(Vector3 position)
    {
        var colliders = GetCollidersAtPosition(position, GridSize);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer != LayerMask.NameToLayer("Ground")) continue;
            Undo.DestroyObjectImmediate(collider.gameObject);
        }
    }
    
    private void FillClosedSpace()
    {
        HashSet<Vector3> filledPositions = new HashSet<Vector3>();
        Queue<Vector3> positionsToCheck = new Queue<Vector3>();

        positionsToCheck.Enqueue(emptyPosition);
        Vector3 minBounds = emptyPosition;
        Vector3 maxBounds = emptyPosition;
        
        int fillCount = 0;
        
        while (positionsToCheck.Count > 0)
        {
            Vector3 currentPosition = positionsToCheck.Dequeue();

            if (filledPositions.Contains(currentPosition) || IsPositionOccupied(currentPosition,GridSize))
                continue;

            CreateCubeAtPosition(currentPosition);
            filledPositions.Add(currentPosition);
            fillCount++;
            if (IsOutOfRange(currentPosition, emptyPosition) || fillCount >= MaxFillCount)
            {
                Debug.LogWarning("填充已超出最大范围或数量，可能是开放形状。填充操作已终止。");
                // 撤销所有已创建的方块
                Undo.PerformUndo();
                return;
            }
            // 更新边界
            minBounds = Vector3.Min(minBounds, currentPosition);
            maxBounds = Vector3.Max(maxBounds, currentPosition);
            
            // 检查六个方向
            //CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.up * GridSize);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.down * GridSize,true);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.left * GridSize);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.right * GridSize);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.forward * GridSize);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.back * GridSize);
        }
    }
    #endregion

    #region 辅助方法
    private bool IsOutOfRange(Vector3 position, Vector3 startPosition)
    {
        return Mathf.Abs(position.x - startPosition.x) > MaxFillRange * GridSize ||
               Mathf.Abs(position.y - startPosition.y) > MaxFillRange * GridSize ||
               Mathf.Abs(position.z - startPosition.z) > MaxFillRange * GridSize;
    }
    
    private void CheckAndEnqueuePosition(Queue<Vector3> queue, Vector3 position,bool lockY = false)
    {
        if (lockY && position.y < 0) return;
        // 可以在这里添加额外的检查,例如限制填充的范围
        if (!IsPositionOccupied(position,GridSize))
        {
            queue.Enqueue(position);
        }
    }
    #endregion
    private void CalculatePreviewArea()
    {
        Vector3 startPosition = emptyPosition;
        previewPositions.Clear();
        Queue<Vector3> positionsToCheck = new Queue<Vector3>();

        positionsToCheck.Enqueue(startPosition);

        int previewCount = 0;

        while (positionsToCheck.Count > 0 && previewCount < MaxFillCount)
        {
            Vector3 currentPosition = positionsToCheck.Dequeue();

            if (previewPositions.Contains(currentPosition) || IsPositionOccupied(currentPosition,GridSize))
                continue;

            if (IsOutOfRange(currentPosition, startPosition))
            {
                isClosedSpace = false;
                previewPositions.Clear();
                return;
            }

            previewPositions.Add(currentPosition);
            previewCount++;

            // 检查六个方向
            //CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.up * GridSize);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.down * GridSize,true);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.left * GridSize);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.right * GridSize);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.forward * GridSize);
            CheckAndEnqueuePosition(positionsToCheck, currentPosition + Vector3.back * GridSize);
        }
        
        if (previewCount >= MaxFillCount)
        {
            isClosedSpace = false;
            previewPositions.Clear();
        }
    }

    private void DrawPreviewArea()
    {
        Handles.color = new Color(0.5f, 0f, 0.5f, 1f); // 半透明紫色
        Handles.DrawWireCube(hitPosition, Vector3.one * GridSize);
        Handles.color = new Color(0.5f, 0f, 0.5f, 0.5f); // 半透明紫色
        foreach (Vector3 position in previewPositions)
        {
            Handles.DrawWireCube(position, Vector3.one * GridSize);
        }
    }
    #region 画面效果
    private void DrawWireCube(Vector3 position)
    {
        switch (toolMode)
        {
            case ToolMode.None:
                Handles.color = settings.emptyCubeColor;
                break;
            case ToolMode.Create:
                Handles.color = settings.hitCubeColor;
                break;
            case ToolMode.Delete:
                Handles.color = settings.deleteCubeColor;
                break;
            case ToolMode.Fill:
                Handles.color = settings.fillCubeColor;

                break;
        }

        if (FillMode)
        {
            CalculatePreviewArea();
            DrawPreviewArea();
        }
        if (recordPosition != null)
        {
            Handles.DrawWireCube(lastOperationPosition, Vector3.one * GridSize);
        }
        else
        {
            // 如果是 FillMode 的话向下补齐方块，直到 Y=0
            for (var y = SupportMode ? position.y : 0; y >= 0; y -= GridSize)
            {
                if (SupportMode) position.y = y;
                if (IsPositionOccupied(position, GridSize)) continue;
                Handles.DrawWireCube(position, Vector3.one * GridSize);
            }
        }
    }
    #endregion
}
