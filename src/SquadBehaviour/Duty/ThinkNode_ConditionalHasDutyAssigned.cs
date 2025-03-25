using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalHasDutyAssigned : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || !pawn.IsPartOfSquad(out ISquadMember squadMember))
                return false;

            SquadDutyDef stance = squadMember.CurrentStance;
            return stance != null;
        }
    }
}
