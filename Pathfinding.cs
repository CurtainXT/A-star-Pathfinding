using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public Transform seeker, target;

    public Grid grid;

    private void Awake()
    {
        grid = this.GetComponent<Grid>();
    }

    private void Update()
    {
        FindPath(seeker.position, target.position);
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // 获取起点终点
        Node startNode = grid.GetNodeFromWorldPoint(startPos);
        Node targetNode = grid.GetNodeFromWorldPoint(targetPos);
        
        // Open列表 存放所有预选的节点
        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closeSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closeSet.Add(currentNode);
            
            // 碰到终点了
            if (currentNode == targetNode)
            {
                // 回溯节点以获取路径
                RetracePath(startNode, targetNode);
                return;
            }

            // 查看每个相邻节点
            foreach (Node neighbourNode in grid.GetNeighbours(currentNode))
            {
                // 如果相邻节点unwalkable或者已经在closeSet里面了 啥也不干
                if(!neighbourNode.walkable || closeSet.Contains(neighbourNode))
                {
                    continue;
                }
                // 计算从当前节点来看的neighbourNode的gCost
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbourNode);
                // 如果新的gCost更小 或者这是第一次考虑此neighbourNode
                if(newMovementCostToNeighbour < neighbourNode.gCost || !openSet.Contains(neighbourNode))
                {
                    // 更新此neighbourNode的Cost
                    neighbourNode.gCost = newMovementCostToNeighbour;
                    neighbourNode.hCost = GetDistance(neighbourNode, targetNode);
                    neighbourNode.parent = currentNode;

                    if(!openSet.Contains(neighbourNode))
                    {
                        openSet.Add(neighbourNode);
                    }
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        else
        {
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}
