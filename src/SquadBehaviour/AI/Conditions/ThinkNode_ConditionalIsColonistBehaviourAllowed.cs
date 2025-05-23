﻿using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsColonistBehaviourAllowed : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember undead))
            {
                return undead.CurrentState == SquadMemberState.AtEase;
            }
            return false;
        }

    }
}
