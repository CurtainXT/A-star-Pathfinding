using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Grid的GameObject X、Y轴要在场景正中
public class Grid : MonoBehaviour
{
    // 用来存储unwalkable的layermask
    public LayerMask unwalkableMask;
    // grid的大小
    public Vector2 gridWorldSize; // Vector2的y对应世界坐标中的z轴
    // grid中的node的半径（node立方体边长的一半）
    public float nodeRadius;
    // 玩家的位置
    public Transform player;
    // Grid中Node的最大数量
    public int MaxSize
    {
        get { return gridSizeX * gridSizeY; }
    }

    // grid是二位的node数组
    Node[,] grid;
    // grid中的node的直径
    float nodeDiameter;
    // Grid中的node在x,y轴的数量
    int gridSizeX, gridSizeY;

    // 路径
    public List<Node> path;
    // Editor只绘制路径Gizmos
    public bool onlyDisplayPathGizmos;

    private void Start()
    {
        // 根据grid的尺寸和node的尺寸计算node的数量并填入二维数组
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    // 创建grid实例
    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        // 计算得到grid（从上往下看）左下角的世界坐标位置
        Vector3 worldButtomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2; //forward没错，y对应node的z坐标

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // 计算每一个node的世界坐标位置
                Vector3 worldPoint = worldButtomLeft +
                    Vector3.right * (x * nodeDiameter + nodeRadius) +
                    Vector3.forward * (y * nodeDiameter + nodeRadius);

                // 判断是否有obstacles，如果有就将node设置为unwalkable
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                // 创建每个node实例并给成员赋值
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    // 获取节点的相邻节点
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {                  
                    continue;// 这就是节点自己
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // 结果不能超出Grid的范围
                if(checkX >= 0 && checkY < gridSizeY && checkX < gridSizeX && checkY >= 0)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    // 通过世界坐标获得Node
    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        // 通过将坐标换算为Grid中的百分比位置来获取Node
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y; //grid的y长对应世界坐标系的z

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX); //减一是因为gridSize是1开始，我们需要index
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    // 在Scene面板中显示grid
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if(onlyDisplayPathGizmos)
        {
            if(path != null)
            {                
                foreach (var node in path)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
        }
        else
        {
            if (grid != null)
            {
                Node playerNode = GetNodeFromWorldPoint(player.position);
                foreach (Node node in grid)
                {
                    Gizmos.color = node.walkable ? Color.white : Color.red;
                    if (playerNode == node)
                    {
                        Gizmos.color = Color.cyan;
                    }
                    if (path != null)
                    {
                        if (path.Contains(node))
                            Gizmos.color = Color.green;
                    }
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
        }
    }
}
