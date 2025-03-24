using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class Designator_PatrolPath : Designator_Zone
    {
        private List<IntVec3> currentPath = new List<IntVec3>();

        public Designator_PatrolPath()
        {
            defaultLabel = "Create patrol path";
            defaultDesc = "Create a patrol path on the map.";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/SmoothSurface", true);
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 cell)
        {
            if (!cell.InBounds(Map) || !cell.Walkable(Map))
                return false;

            return true;
        }

        public override void DesignateSingleCell(IntVec3 cell)
        {
            if (!currentPath.Contains(cell))
            {
                currentPath.Add(cell);
                Map.designationManager.AddDesignation(new Designation(cell, SquadDefOf.DesignatePatrolRoute, null));
            }
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            base.DesignateMultiCell(cells);

            foreach (var cell in cells)
            {
                if (!currentPath.Contains(cell))
                {
                    currentPath.Add(cell);
                    Map.designationManager.AddDesignation(new Designation(cell, SquadDefOf.DesignatePatrolRoute, null));
                }
            }
        }
    }
}
