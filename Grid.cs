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
    // 地形类别 寻路的移动花销相关
    public TerrainType[] walkableRegionType;
    // unwalkable的权重 需要大于walkable layer中的最大权重
    public int obstacleProximityPenalty = 10;

    // Grid中Node的最大数量
    public int MaxSize
    {
        get { return gridSizeX * gridSizeY; }
    }

    // 所有能够行走的layer
    LayerMask walkableMask;
    // 方便节点构造时判断自身类型使用的字典
    Dictionary<int, int> walkableRegionDictionary = new Dictionary<int, int>();
    // grid是二位的node数组
    Node[,] grid;
    // grid中的node的直径
    float nodeDiameter;
    // Grid中的node在x,y轴的数量
    int gridSizeX, gridSizeY;

    // 路径
    public List<Node> path;
    // Editor只绘制路径Gizmos
    public bool displayGridGizmos;
    // Editor中的节点权重可视化
    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    private void Awake()
    {
        // 根据grid的尺寸和node的尺寸计算node的数量并填入二维数组
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (TerrainType region in walkableRegionType)
        {
            walkableMask.value = walkableMask | region.terrainMask.value;
            walkableRegionDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }

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
                // 设置节点移动权重
                int movementPenalty = 0;
                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    walkableRegionDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }
                if(walkable == false)
                {
                    movementPenalty = obstacleProximityPenalty;
                }

                // 创建每个node实例并给成员赋值
                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }

        BlurPenaltyMap(2);
    }

    // 使网格的权重图更平滑
    void BlurPenaltyMap(int blurSize) // blurSize定义我们想要一个节点和多大的面积计算模糊，1就意味着和周围一圈，2就是周围两圈
    {
        // kernelSize就是一个节点的模糊计算涉及多大范围 比如3 X 3范围
        int kernelSize = blurSize * 2 + 1;
        // kernelExtents是只一个kernal的中心到边缘之间有几个节点 比如3 X 3的kernelSize有一个
        int kernelExtents = (kernelSize - 1) / 2;

        // 用于保存水平和竖直的计算结果
        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        // 水平方向的遍历
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0 ,y] += grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                penaltiesHorizontalPass[x,y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        // 竖直方向的遍历
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            // 将y = 0时得到的blur penalty赋值给grid
            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                // 得到的结果 取个整数近似值
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                grid[x,y].movementPenalty = blurredPenalty;

                if(blurredPenalty > penaltyMax)
                    penaltyMax = blurredPenalty;
                if(blurredPenalty < penaltyMin)
                    penaltyMin = blurredPenalty;
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

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX); //减一是因为gridSize是1开始的
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    // 在Scene面板中显示grid
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            foreach (Node node in grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, node.movementPenalty));

                Gizmos.color = node.walkable ? Gizmos.color : Color.red;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * nodeDiameter);
            }
        }
    }
}

[System.Serializable]
public class TerrainType
{
    public LayerMask terrainMask;
    public int terrainPenalty;
}
