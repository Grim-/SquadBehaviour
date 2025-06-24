using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class Zone_PatrolPath : Zone
    {
        // Core data
        public HashSet<IntVec3> orderedCells = new HashSet<IntVec3>();
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
                orderedCells = new HashSet<IntVec3>();

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
                orderedCells = new HashSet<IntVec3>();

            if (!orderedCells.Contains(c))
            {
                orderedCells.Add(c);
            }

            orderedCells.OrderBy(x => x.DistanceTo(this.Position));
        }

        public override void RemoveCell(IntVec3 c)
        {
            base.RemoveCell(c);

            if (orderedCells != null && orderedCells.Contains(c))
            {
                orderedCells.Remove(c);
            }
        }



        public List<IntVec3> GetPatrolCells()
        {
            if (cells.Count == 0)
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
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref orderedCells, "orderedCells", LookMode.Value);
            Scribe_Values.Look(ref allowAutoPatrol, "allowAutoPatrol", true);
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
    }

    public enum PatrolMode
    {
        Loop,     
        PingPong,  
        OneWay      
    }
}