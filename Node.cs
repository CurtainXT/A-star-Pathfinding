using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    // node能走吗？会根据是否与障碍物重合判断
    public bool walkable;
    // node中心的世界坐标位置
    public Vector3 worldPosition;
    // 在Grid二维坐标中的位置
    public int gridX;
    public int gridY;
    // 节点移动权重
    public int movementPenalty;
    // 节点到起点的距离
    public int gCost;
    // 节点到终点的距离
    public int hCost;
    // 父节点 也就是路径中该节点的上一个节点
    public Node parent;
    // IHeapItem接口实现
    int heapIndex;

    // 构造函数
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;
    }

    // fCost为gCost + hCost 所以写个属性就行
    public int fCost
    {
        get { return gCost + hCost; }
    }

    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    // HeapItem CompareTo实现
    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0) // fCost相等 则比较hCost
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare; //fCost/hCost更小的优先级更大
    }
}
