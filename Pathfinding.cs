using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;
    public Grid grid;

    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = this.GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 pathStart, Vector3 targetPos)
    {
        StartCoroutine(FindPath(pathStart, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // 接收寻路的
        Vector3[] waypoints = new Vector3[0];
        // 寻路是否成功
        bool pathSuccess = false;

        // 获取起点终点
        Node startNode = grid.GetNodeFromWorldPoint(startPos);
        Node targetNode = grid.GetNodeFromWorldPoint(targetPos);
        
        // 起点和目标点都能走时我们才进行计算
        if(startNode.walkable && targetNode.walkable)
        {
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
                    pathSuccess = true;
                    break;
                }

                // 查看每个相邻节点
                foreach (Node neighbourNode in grid.GetNeighbours(currentNode))
                {
                    // 如果相邻节点unwalkable或者已经在closeSet里面了 啥也不干
                    if (!neighbourNode.walkable || closeSet.Contains(neighbourNode))
                    {
                        continue;
                    }
                    // 计算从当前节点来看的neighbourNode的gCost
                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbourNode) + neighbourNode.movementPenalty;
                    // 如果新的gCost更小 或者这是第一次考虑此neighbourNode
                    if (newMovementCostToNeighbour < neighbourNode.gCost || !openSet.Contains(neighbourNode))
                    {
                        // 更新此neighbourNode的Cost
                        neighbourNode.gCost = newMovementCostToNeighbour;
                        neighbourNode.hCost = GetDistance(neighbourNode, targetNode);
                        neighbourNode.parent = currentNode;

                        if (!openSet.Contains(neighbourNode))
                        {
                            openSet.Add(neighbourNode);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbourNode);
                        }
                    }
                }
            }
        }
       
        yield return null;
        if(pathSuccess)
        {
            // 回溯节点以获取路径
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            // 如果一系列路径节点在一个方向上，则取最终的那个节点
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i-1].worldPosition);
            }
            directionOld = directionNew;
        }

        return waypoints.ToArray();
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
