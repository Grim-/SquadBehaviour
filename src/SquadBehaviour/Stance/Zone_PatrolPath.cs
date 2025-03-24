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
        public List<IntVec3> orderedCells = new List<IntVec3>();
        public bool allowAutoPatrol = true;
        public int patrolSpeed = 1;

        private bool needsReordering = false;

        public Zone_PatrolPath()
        {
        }

        public Zone_PatrolPath(ZoneManager zoneManager) : base("PatrolPath".Translate(), zoneManager)
        {
            color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        }

        public override bool IsMultiselectable
        {
            get { return true; }
        }


        public void AddToPath(IntVec3 c)
        {
            if (orderedCells == null)
                orderedCells = new List<IntVec3>();

            if (!orderedCells.Contains(c))
            {
                orderedCells.Add(c);
                needsReordering = true;
            }

            if (!cells.Contains(c))
            {
                AddCell(c);
            }
        }

        public override void AddCell(IntVec3 c)
        {
            base.AddCell(c);

            if (orderedCells == null || !orderedCells.Contains(c))
            {
                if (orderedCells == null)
                    orderedCells = new List<IntVec3>();

                orderedCells.Add(c);
                needsReordering = true;
            }
        }

        public override void RemoveCell(IntVec3 c)
        {
            base.RemoveCell(c);

            if (orderedCells != null && orderedCells.Contains(c))
            {
                orderedCells.Remove(c);
                needsReordering = true;
            }
        }

        protected override Color NextZoneColor
        {
            get { return new Color(0.2f, 0.8f, 0.2f); }
        }

        private void ReorderCellsVisually()
        {
            if (orderedCells == null || orderedCells.Count <= 1)
                return;

            foreach (IntVec3 cell in cells)
            {
                if (!orderedCells.Contains(cell))
                    orderedCells.Add(cell);
            }

            orderedCells.RemoveAll(c => !cells.Contains(c));

            orderedCells = orderedCells
                .OrderBy(c => c.z) 
                .ThenBy(c => c.x)  
                .ToList();

            needsReordering = false;
        }

        private void OptimizePatrolOrder()
        {
            if (orderedCells == null || orderedCells.Count <= 1)
                return;

            List<IntVec3> unvisited = new List<IntVec3>(orderedCells);
            List<IntVec3> optimized = new List<IntVec3>();

            IntVec3 current = unvisited.OrderBy(c => c.z).ThenBy(c => c.x).First();
            optimized.Add(current);
            unvisited.Remove(current);

            while (unvisited.Count > 0)
            {
                IntVec3 nearest = FindNearestCell(current, unvisited);
                optimized.Add(nearest);
                unvisited.Remove(nearest);
                current = nearest;
            }

            orderedCells = optimized;
            needsReordering = false;
        }

        private IntVec3 FindNearestCell(IntVec3 from, List<IntVec3> cells)
        {
            float minDist = float.MaxValue;
            IntVec3 nearest = IntVec3.Invalid;

            foreach (IntVec3 cell in cells)
            {
                float dist = (cell - from).LengthHorizontalSquared;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = cell;
                }
            }

            return nearest;
        }

        public List<IntVec3> GetOrderedPatrolCells(bool perimeterOnly = false)
        {
            if (orderedCells == null)
                orderedCells = new List<IntVec3>();


            if (needsReordering || orderedCells.Count != cells.Count)
            {
                ReorderCellsVisually();
            }


            if (perimeterOnly)
            {
                CellRect zoneRect = CellRect.FromCellList(cells);
                return zoneRect.EdgeCells.Where(c => cells.Contains(c)).ToList();
            }

            return new List<IntVec3>(orderedCells);
        }

        public IntVec3 FindNearestPointOnPath(IntVec3 position)
        {
            List<IntVec3> pathCells = GetOrderedPatrolCells();

            if (pathCells.Count == 0)
                return IntVec3.Invalid;

            IntVec3 closest = pathCells[0];
            float closestDist = (position - closest).LengthHorizontalSquared;

            foreach (var cell in pathCells)
            {
                float dist = (position - cell).LengthHorizontalSquared;
                if (dist < closestDist)
                {
                    closest = cell;
                    closestDist = dist;
                }
            }

            return closest;
        }

        public IntVec3 GetNextPatrolPoint(IntVec3 currentPosition, bool clockwise = true)
        {
            List<IntVec3> pathCells = GetOrderedPatrolCells();

            if (pathCells.Count <= 1)
                return IntVec3.Invalid;

            int closestIndex = -1;
            float closestDist = float.MaxValue;

            for (int i = 0; i < pathCells.Count; i++)
            {
                float dist = (currentPosition - pathCells[i]).LengthHorizontalSquared;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }

            if (closestIndex == -1)
                return IntVec3.Invalid;

            int nextIndex;
            if (clockwise)
            {
                nextIndex = (closestIndex + 1) % pathCells.Count;
            }
            else
            {
                nextIndex = (closestIndex - 1 + pathCells.Count) % pathCells.Count;
            }

            return pathCells[nextIndex];
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref orderedCells, "orderedCells", LookMode.Value);
            Scribe_Values.Look(ref allowAutoPatrol, "allowAutoPatrol", true);
            Scribe_Values.Look(ref patrolSpeed, "patrolSpeed", 1);

            // Initialize if null after loading
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (orderedCells == null)
                    orderedCells = new List<IntVec3>();

                needsReordering = true;
                OptimizePatrolOrder();
            }
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
            List<IntVec3> pathCells = GetOrderedPatrolCells();

            if (pathCells.Count > 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("PatrolPathLength".Translate() + ": " + pathCells.Count + " " + "Cells".Translate());
                float totalDistance = 0f;
                for (int i = 0; i < pathCells.Count - 1; i++)
                {
                    totalDistance += pathCells[i].DistanceTo(pathCells[i + 1]);
                }

                if (pathCells.Count > 1)
                {
                    totalDistance += pathCells[pathCells.Count - 1].DistanceTo(pathCells[0]);

                    stringBuilder.AppendLine("TotalPatrolDistance".Translate() + ": " + totalDistance.ToString("F1") + " m");

                    float averagePawnSpeed = 4.5f * patrolSpeed;
                    float estimatedTime = totalDistance / averagePawnSpeed;
                    stringBuilder.AppendLine("EstimatedPatrolTime".Translate() + ": " + estimatedTime.ToString("F1") + " " + "Seconds".Translate());
                }
            }

            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Toggle
            {
                defaultLabel = "CommandAllowAutoPatrol".Translate(),
                defaultDesc = "CommandAllowAutoPatrolDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/SquadAttack", true),
                isActive = () => allowAutoPatrol,
                toggleAction = delegate ()
                {
                    allowAutoPatrol = !allowAutoPatrol;
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "CommandReversePatrolOrder".Translate(),
                defaultDesc = "CommandReversePatrolOrderDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/SquadAttack", true),
                action = delegate ()
                {
                    if (orderedCells != null && orderedCells.Count > 0)
                    {
                        orderedCells.Reverse();
                    }
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "CommandOptimizePatrolOrder".Translate(),
                defaultDesc = "CommandOptimizePatrolOrderDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/SquadAttack", true),
                action = delegate ()
                {
                    OptimizePatrolOrder();
                }
            };
        }

        public override IEnumerable<Gizmo> GetZoneAddGizmos()
        {
            yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_PatrolPath_Expand>();
            yield break;
        }
    }
}
