using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    // Expander designator for adding to existing patrol paths
    public class Designator_ZoneAdd_PatrolPath_Expand : Designator_ZoneAdd_PatrolPath
    {
        protected override string NewZoneLabel => "PatrolPath".Translate();

        public Designator_ZoneAdd_PatrolPath_Expand()
        {
            this.zoneTypeToPlace = typeof(Zone_PatrolPath);
            this.defaultLabel = "DesignatorPatrolPathExpand".Translate();
            this.defaultDesc = "DesignatorPatrolPathExpandDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile", true);
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_ZoneAdd;
            this.hotKey = KeyBindingDefOf.Misc7;
        }

        protected override Zone MakeNewZone()
        {
            return new Zone_PatrolPath(Find.CurrentMap.zoneManager);
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(this.Map))
                return false;

            if (c.Fogged(this.Map))
                return false;

            if (c.InNoBuildEdgeArea(this.Map))
                return "TooCloseToMapEdge".Translate();

            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Zone existingZone = this.Map.zoneManager.ZoneAt(c);
            Zone_PatrolPath existingPatrolZone = existingZone as Zone_PatrolPath;
            base.DesignateSingleCell(c);

            Zone newZone = this.Map.zoneManager.ZoneAt(c);
            Zone_PatrolPath patrolZone = newZone as Zone_PatrolPath;

            if (patrolZone != null)
            {
                patrolZone.AddToPath(c);
            }
        }
    }
}
