﻿using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalSquadHasTarget : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.mindState == null)
            {
                return false;
            }

            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) || squadMember.AssignedSquad == null)
            {
                return false;
            }

            if (squadMember.AssignedSquad.HasSquadTarget())
            {
                return true;
            }

            return false;
        }
    }
}
