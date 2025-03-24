﻿using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsDefendingPoint : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember.CurrentState == SquadMemberState.DefendPoint)
            {
                return true;
            }
            return false;
        }
    }
}
