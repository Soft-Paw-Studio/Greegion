using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using static TileMapUtilities;

//[EditorTool("CubeEditor")]
public class CubeMapEditor : EditorTool
{
    public static CubeMapSettings settings;
    public static CubeMapSheets sheet;
    
    #region 私有变量
    private Vector3 emptyPosition;
    private Vector3 hitPosition;
    private Vector3 fixedPosition;
    private Vector3? lastOperationPosition;
    private Ray currentRay;
    private AlignMode alignMode;
    #endregion

    #region 工具GUI
    public override void OnToolGUI(EditorWindow window)
    {
        if(settings == null) settings = CubeMapSettings.Load();
        currentRay = DetectAndDrawCubes();
        HandleInput();
        window.Repaint();
    }

    /// <summary>
    /// 处理用户输入
    /// </summary>
    ///
    private void HandleInput()
    {
        switch (Event.current.button)
        {
            case 0: // 左键
                
                var directionToFix = emptyPosition - fixedPosition;
                var lockedX = Mathf.Abs(directionToFix.x);
                var lockedZ = Mathf.Abs(directionToFix.z);
                
                if (!Event.current.alt) // 保留Alt键时的视角旋转操作
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        if (Event.current.control)
                        {
                            OnCreateCubeWithFill(); // Ctrl + 左键: 创建并填充方块
                        }
                        else
                        {
                            OnCreateOrDragCube(); // 创建单个方块
                        }

                        Event.current.Use();
                        fixedPosition = emptyPosition;
                    }

                    if (Event.current.type == EventType.MouseDrag)
                    {
                        if (lockedX <  lockedZ)
                        {
                            emptyPosition.x = fixedPosition.x;
                        }
                        else
                        {
                            emptyPosition.z = fixedPosition.z;
                        }
                        
                        if (Event.current.control)
                        {
                            OnCreateCubeWithFill(true); // Ctrl + 左键: 创建并填充方块
                        }
                        else
                        {
                            OnCreateOrDragCube(true); // 连续创建方块
                        }

                        Event.current.Use();
                    }
                }

                break;

            case 1: // 右键

                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.control)
                    {
                        OnDeleteCubesWithFill(); // Ctrl + 右键: 删除并清空方块
                    }
                    else
                    {
                        OnDeleteCube(); // 删除单个方块
                    }

                    Event.current.Use();
                    fixedPosition = emptyPosition;
                }

                if (Event.current.type == EventType.MouseDrag)
                {
                    if (Event.current.control)
                    {
                        OnDeleteCubesWithFill(); // Ctrl + 右键: 连续删除并清空方块
                    }
                    else
                    {
                        OnDeleteCube(true); // 连续删除方块
                    }

                    Event.current.Use();
                }
                break;
        }
        
        // 在鼠标拖拽时计算鼠标位置变化
        if (Event.current.type == EventType.MouseDrag)
        {

        }
    }
    #endregion
    
    #region 创建方块
    private void OnCreateOrDragCube(bool useFixedY = false) 
    {
        // 如果当前空位置与上次操作位置相同，则不执行放置操作
        if (emptyPosition == lastOperationPosition) return;

        // 记录撤销操作，放置方块
        Undo.RecordObject(this, "Place Cube");
        var positionToPlace = useFixedY ? new Vector3(emptyPosition.x, fixedPosition.y, emptyPosition.z) : emptyPosition;
        PlaceCubesAtPosition(positionToPlace);
        lastOperationPosition = emptyPosition;// 更新上次操作位置
        
        // 重新计算下一个可放置方块的位置
        RecalculatePositions();
    }
    private void OnCreateCubeWithFill(bool useFixedY = false)
    {
        if (emptyPosition == lastOperationPosition) return;

        Undo.RecordObject(this, "Place Cube with Fill");
        var positionToPlace = useFixedY ? new Vector3(emptyPosition.x, fixedPosition.y, emptyPosition.z) : emptyPosition;
        PlaceCubesToYZero(positionToPlace);  // 从当前方块位置向下补齐至Y=0
        lastOperationPosition = emptyPosition;
        RecalculatePositions();
        DrawWireCubes(positionToPlace, settings.fillCubeColor);
    }
    private void PlaceCubesAtPosition(Vector3 startPosition)
    {
        for (int x = 0; x < brushSize; x++)
        {
            for (int z = 0; z < brushSize; z++)
            {
                var position = startPosition + new Vector3(x * GridSize, 0, z * GridSize);
                if (IsPositionOccupied(position, GridSize)) continue;

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.GetComponent<MeshRenderer>().sharedMaterial = settings.mat;
                
                cube.transform.position = position;
                cube.tag = "Untagged";
                Undo.RegisterCreatedObjectUndo(cube, "Place Cube");
            }
        }
    }
    private void OnDeleteCube(bool useFixedY = false)
    {
        if (emptyPosition == lastOperationPosition) return;

        Undo.RecordObject(this, "Delete Cube");
        var positionToDelete = useFixedY ? new Vector3(hitPosition.x, fixedPosition.y, hitPosition.z) : hitPosition;
        DeleteCubesAtPosition(positionToDelete); // 删除多个方块
        lastOperationPosition = emptyPosition;
        
        RecalculatePositions();
    }
    private void OnDeleteCubesWithFill()
    {
        if (hitPosition == lastOperationPosition) return;

        Undo.RecordObject(this, "Delete Cubes with Fill");
        DeleteCubesAndAbove(hitPosition);
        lastOperationPosition = hitPosition;
        RecalculatePositions();
        DrawWireCubes(hitPosition, settings.deleteCubeColor);
    }
    private void DeleteCubesAndAbove(Vector3 startPosition)
    {
        for (int x = 0; x < brushSize; x++)
        {
            for (int z = 0; z < brushSize; z++)
            {
                var position = startPosition + new Vector3(x * GridSize, 0, z * GridSize);
                DeleteCubesAtAndAbovePosition(position);
            }
        }
    }
    private void DeleteCubesAtAndAbovePosition(Vector3 position)
    {
        var colliders = GetCollidersAtPosition(position, GridSize);
        foreach (var collider in colliders)
        {
            if (!collider.gameObject.CompareTag("Untagged")) continue;
            Undo.DestroyObjectImmediate(collider.gameObject);
        }
    }
    private void DeleteCubesAtPosition(Vector3 startPosition)
    {
        for (int x = 0; x < brushSize; x++)
        {
            for (int z = 0; z < brushSize; z++)
            {
                var position = startPosition + new Vector3(x * GridSize, 0, z * GridSize);
                var colliders = GetCollidersAtPosition(position, GridSize);
                foreach (var collider in colliders)
                {
                    if (!collider.gameObject.CompareTag("Untagged")) continue;
                    Undo.DestroyObjectImmediate(collider.gameObject);
                }
            }
        }
    }

    #endregion
    
    #region 检测与定位
    private Ray DetectAndDrawCubes()
    {
        // 获取鼠标位置并将其转换为世界空间中的射线
        var mousePos = Event.current.mousePosition;
        var ray = HandleUtility.GUIPointToWorldRay(mousePos);
        
        // 计算并返回当前可放置方块的位置，同时绘制辅助线框
        CalculatePositions(ray);
        if (Event.current.control)
        {
            DrawWireCubeRange(emptyPosition, settings.emptyCubeColor);
        }
        else
        {
            DrawWireCubes(emptyPosition,settings.emptyCubeColor);   
        }

        return ray;
    }
    private void CalculatePositions(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hitPosition = AlignToGrid(hit.transform.position, GridSize);
            emptyPosition = FindNearestEmptyPosition(hitPosition, hit.normal);
            
            if (Event.current.shift)
            {
                emptyPosition = hitPosition;
            }
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
    private void RecalculatePositions()
    { 
        currentRay = DetectAndDrawCubes();
        CalculatePositions(currentRay);
    }
    #endregion
    
    #region 辅助方法
    private void PlaceCubesToYZero(Vector3 startPosition)
    {
        for (int x = 0; x < brushSize; x++)
        {
            for (int z = 0; z < brushSize; z++)
            {
                Vector3 position = startPosition + new Vector3(x * GridSize, 0, z * GridSize);

                // 向下补齐方块，直到Y=0平面
                for (float y = position.y; y >= 0; y -= GridSize)
                {
                    var pos = new Vector3(position.x, y, position.z);
                    if (IsPositionOccupied(pos, GridSize)) continue;

                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = pos;
                    cube.tag = "Untagged";
                    Undo.RegisterCreatedObjectUndo(cube, "Place Cube");
                }
            }
        }
    }// 向下补齐方块至Y=0
    private void DrawWireCubes(Vector3 centerPosition, Color color)
    {
        Handles.color = color;
        TileMapUtilities.DrawWireCubes(centerPosition);
    }
    #endregion
}

