using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class Designator_ZoneAdd_PatrolPath : Designator_ZoneAdd
    {
        protected override string NewZoneLabel => "PatrolPath".Translate();

        public Designator_ZoneAdd_PatrolPath()
        {
            this.zoneTypeToPlace = typeof(Zone_PatrolPath);
            this.defaultLabel = "DesignatorPatrolPath".Translate();
            this.defaultDesc = "DesignatorPatrolPathDesc".Translate();
            this.icon = TexCommand.GatherSpotActive;
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


        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(this.Map))
                return false;
            if (c.Fogged(this.Map))
                return false;
            if (c.InNoBuildEdgeArea(this.Map))
                return "TooCloseToMapEdge".Translate();
            if (c.GetFirstBuilding(this.Map) != null)
                return "Cant PAtrol over buildsings";

            Zone zone = this.Map.zoneManager.ZoneAt(c);
            if (zone != null && zone.GetType() != this.zoneTypeToPlace)
                return false;

            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            base.DesignateSingleCell(c);
            Zone zone = this.Map.zoneManager.ZoneAt(c);
            Zone_PatrolPath patrolZone = zone as Zone_PatrolPath;
            if (patrolZone != null)
            {
                patrolZone.AddToPath(c);
            }
        }
    }
}
