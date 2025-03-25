using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class Designator_ZoneAdd_PatrolPath_Expand : Designator_ZoneAdd_PatrolPath
    {
        public Designator_ZoneAdd_PatrolPath_Expand()
        {
            this.defaultLabel = "DesignatorPatrolPathExpand".Translate();
            this.defaultDesc = "DesignatorPatrolPathExpandDesc".Translate();
            this.hotKey = KeyBindingDefOf.Misc8;
        }
    }
}
