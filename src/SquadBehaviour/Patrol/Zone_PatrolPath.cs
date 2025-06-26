using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class Zone_PatrolPath : Zone
    {
        private ITab_PatrolPathDebug debugTab;
        public List<IntVec3> orderedCells = new List<IntVec3>();
        public bool allowAutoPatrol = true;
        public Zone_PatrolPath()
        {

        }

        public Zone_PatrolPath(ZoneManager zoneManager) : base("PatrolPath".Translate(), zoneManager)
        {
            color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        }

        public override bool IsMultiselectable => true;

        protected override Color NextZoneColor => new Color(0.2f, 0.8f, 0.2f);

        public void AddToPath(IntVec3 c)
        {
            if (orderedCells == null)
                orderedCells = new List<IntVec3>();

            if (!orderedCells.Contains(c))
            {
                orderedCells.Add(c);
            }

            if (!cells.Contains(c))
            {
                AddCell(c);
            }
        }

        public override void AddCell(IntVec3 c)
        {
            base.AddCell(c);
            if (orderedCells == null)
                orderedCells = new List<IntVec3>();

            if (!orderedCells.Contains(c))
            {
                orderedCells.Add(c);
            }
            ReorderCells();
        }

        public override void RemoveCell(IntVec3 c)
        {
            base.RemoveCell(c);

            if (orderedCells != null && orderedCells.Contains(c))
            {
                orderedCells.Remove(c);    
            }

            ReorderCells();
        }

        public override IEnumerable<InspectTabBase> GetInspectTabs()
        {
            if (Prefs.DevMode)
            {
                if (debugTab == null)
                {
                    debugTab = new ITab_PatrolPathDebug();
                }

                yield return debugTab;
            }
        }

        public List<IntVec3> GetPatrolCells()
        {
            if (orderedCells == null || orderedCells.Count == 0)
                return new List<IntVec3>();

            return new List<IntVec3>(orderedCells);
        }

        public IntVec3 GetNearestCell(IntVec3 from, IEnumerable<IntVec3> candidates)
        {
            return candidates.OrderBy(c => PatrolUtil.CellDistanceSquared(from, c)).First();
        }

        public IntVec3 FindNearestPointOnPath(IntVec3 position)
        {
            List<IntVec3> pathCells = GetPatrolCells();

            if (pathCells.Count == 0)
                return IntVec3.Invalid;

            return GetNearestCell(position, pathCells);
        }

        public IntVec3 FindReachablePoint(Pawn pawn, IntVec3 startPoint)
        {
            if (orderedCells == null || orderedCells.Count == 0)
                return IntVec3.Invalid;

            int startIndex = 0;
            if (startPoint.IsValid)
            {
                startIndex = orderedCells.IndexOf(startPoint);
                if (startIndex < 0) startIndex = 0;
            }

            for (int i = 0; i < orderedCells.Count; i++)
            {
                int index = (startIndex + i) % orderedCells.Count;
                IntVec3 point = orderedCells[index];
                bool canReach = pawn.CanReach(point, PathEndMode.OnCell, Danger.Deadly);

                if (canReach)
                {
                    return point;
                }
            }

            return IntVec3.Invalid;
        }
        private float CalculatePathDistance(List<IntVec3> path)
        {
            if (path.Count <= 1)
                return 0f;

            float totalDistance = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                totalDistance += path[i].DistanceTo(path[i + 1]);
            }

            totalDistance += path[path.Count - 1].DistanceTo(path[0]);
            return totalDistance;
        }

        public override IEnumerable<Gizmo> GetZoneAddGizmos()
        {
            yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_PatrolPath_Expand>();
            yield break;
        }
        public void ReorderCells()
        {
            if (orderedCells == null || orderedCells.Count <= 2)
                return;

            List<IntVec3> unvisited = new List<IntVec3>(orderedCells);
            List<IntVec3> newOrder = new List<IntVec3>();

            IntVec3 current = FindStartingCell(unvisited);
            newOrder.Add(current);
            unvisited.Remove(current);
            while (unvisited.Count > 0)
            {
                IntVec3 nearest = GetNearestCell(current, unvisited);
                newOrder.Add(nearest);
                unvisited.Remove(nearest);
                current = nearest;
            }

            orderedCells = newOrder;
        }

        private IntVec3 FindStartingCell(List<IntVec3> cells)
        {
            if (cells.Count == 0)
                return IntVec3.Invalid;

            CellRect rect = CellRect.FromCellList(cells);

            foreach (IntVec3 corner in rect.Corners)
            {
                if (cells.Contains(corner))
                    return corner;
            }
            foreach (IntVec3 edge in rect.EdgeCells)
            {
                if (cells.Contains(edge))
                    return edge;
            }
            return cells.First();
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<IntVec3> pathCells = GetPatrolCells();

            if (pathCells.Count > 0)
            {
                stringBuilder.AppendLine("PatrolPathLength".Translate().Trim() + ": " + pathCells.Count + " " + "Cells".Translate().Trim());

                if (pathCells.Count > 1)
                {
                    float totalDistance = CalculatePathDistance(pathCells);
                    stringBuilder.AppendLine("TotalPatrolDistance".Translate().Trim() + ": " + totalDistance.ToString("F1") + " m");
                }
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref orderedCells, "orderedCells", LookMode.Value);
            Scribe_Values.Look(ref allowAutoPatrol, "allowAutoPatrol", true);
        }
    }
}