using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class SquadOrder_DefendPoint : SquadOrderWorker
    {
        //public override bool IsSquadOrder => false;

        public override bool CanExecuteOrder(LocalTargetInfo Target)
        {
            if (Target.Cell != IntVec3.Invalid && Target.Cell.Walkable(SquadMember.Pawn.Map))
            {
                return true;
            }

            return false;
        }

        public override void ExecuteOrder(LocalTargetInfo Target)
        {
            SquadMember.SetDefendPoint(Target.Cell);
            SquadMember.CurrentStance = SquadDefOf.DefendPoint;
        }

        public override void ExecuteOrderGlobal(LocalTargetInfo Target)
        {
            throw new System.NotImplementedException();
        }
    }
}
