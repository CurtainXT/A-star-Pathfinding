using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyPerfect.City;

namespace ATMC
{
    public class Unit : MonoBehaviour
    {
        public Transform target;
        public VehicleController controller;
        public float speed = 1;
        public bool bNeedRequestPath = true;
        List<Path> path;
        int pathIndex;
        int pathPositionIndex;


        void Update()
        {
            if(bNeedRequestPath)
            {
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                bNeedRequestPath = false;
            }

        }

        public void OnPathFound(List<Path> newPath, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                path = newPath;
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
        }

        IEnumerator FollowPath()
        {
            Vector3 currentWaypoint = path[0].pathPositions[0].position;

            while (true)
            {
                if (pathIndex >= path.Count)
                {
                    yield break;
                }
                controller.GetNextWaypoint(currentWaypoint);
                while (true)
                {
                    if ((transform.position - currentWaypoint).sqrMagnitude < 2f)
                    {
                        pathPositionIndex++;
                        if (pathPositionIndex >= path[pathIndex].pathPositions.Count)
                        {
                            pathIndex++;
                            pathPositionIndex = 0;
                        }
                        currentWaypoint = path[pathIndex].pathPositions[pathPositionIndex].position;
                        break;
                    }
                    yield return null;
                }
                //transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            }
        }

        private void OnDrawGizmos()
        {
            if (path != null)
            {
                for (int i = pathIndex; i < path.Count; i++)
                {

                    for (int j = 1; j < path[i].pathPositions.Count; j++)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(path[i].pathPositions[j].position, Vector3.one * 0.3f);

                        //if (j == pathPositionIndex)
                        //{
                        //    Gizmos.DrawLine(transform.position, path[i].pathPositions[j].position);
                        //}
                        //else
                        //{
                        //    Gizmos.DrawLine(path[i].pathPositions[j - 1].position, path[i].pathPositions[j].position);
                        //}
                    }

                }
            }
        }
    }
}