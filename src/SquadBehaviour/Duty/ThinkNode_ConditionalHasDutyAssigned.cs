using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalHasDutyAssigned : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || !pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                return false;

            SquadDutyDef stance = squadMember._CurrentStance;
            return stance != null;
        }
    }
}
