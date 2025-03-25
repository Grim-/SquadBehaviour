using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SquadBehaviour
{
    public static class PatrolUtil
    {
        public static IntVec3 GetTopLeftCell(IEnumerable<IntVec3> cellList)
        {
            return cellList.OrderBy(c => c.z).ThenBy(c => c.x).First();
        }

        public static IntVec3 FindNearestPointOnPath(Zone_PatrolPath PatrolZone, IntVec3 position)
        {
            List<IntVec3> pathCells = PatrolZone.GetPatrolCells();

            if (pathCells.Count == 0)
                return IntVec3.Invalid;

            return PatrolZone.GetNearestCell(position, pathCells);
        }

        public static IntVec3 GetNextPatrolPoint(Zone_PatrolPath patrolZone, IntVec3 currentPosition, bool clockwise = true, PatrolMode patrolMode = PatrolMode.PingPong)
        {
            List<IntVec3> pathCells = patrolZone.GetPatrolCells();
            if (pathCells.Count <= 1)
                return IntVec3.Invalid;

            int closestIndex = -1;
            float closestDist = float.MaxValue;
            for (int i = 0; i < pathCells.Count; i++)
            {
                float dist = CellDistanceSquared(currentPosition, pathCells[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }

            if (closestIndex == -1)
                return IntVec3.Invalid;

            int nextIndex;

            switch (patrolMode)
            {
                case PatrolMode.Loop:
                    if (clockwise)
                        nextIndex = (closestIndex + 1) % pathCells.Count;
                    else
                        nextIndex = (closestIndex - 1 + pathCells.Count) % pathCells.Count;
                    break;

                case PatrolMode.PingPong:
                    bool isReversing = (closestIndex == 0 && !clockwise) || (closestIndex == pathCells.Count - 1 && clockwise);
                    if (isReversing)
                    {
                        if (clockwise)
                            nextIndex = closestIndex - 1;
                        else
                            nextIndex = closestIndex + 1;
                    }
                    else
                    {
                        if (clockwise)
                            nextIndex = closestIndex + 1;
                        else
                            nextIndex = closestIndex - 1;
                    }

                    if (nextIndex < 0)
                        nextIndex = 1;
                    else if (nextIndex >= pathCells.Count)
                        nextIndex = pathCells.Count - 2;
                    break;

                case PatrolMode.OneWay:
                    if (clockwise)
                    {
                        nextIndex = closestIndex + 1;
                        if (nextIndex >= pathCells.Count)
                            return pathCells[closestIndex];
                    }
                    else
                    {
                        nextIndex = closestIndex - 1;
                        if (nextIndex < 0)
                            return pathCells[closestIndex];
                    }
                    break;

                default:
                    if (clockwise)
                        nextIndex = (closestIndex + 1) % pathCells.Count;
                    else
                        nextIndex = (closestIndex - 1 + pathCells.Count) % pathCells.Count;
                    break;
            }

            return pathCells[nextIndex];
        }
        public static float CellDistanceSquared(IntVec3 a, IntVec3 b)
        {
            return (a - b).LengthHorizontalSquared;
        }
    }
}