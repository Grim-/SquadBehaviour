using RimWorld;
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

            if (!pawn.IsPartOfSquad(out ISquadMember squadMember) || squadMember.AssignedSquad == null)
            {
                return false;
            }

            if (squadMember.AssignedSquad.HasEnemiesNearby())
            {
                return true;
            }

            if (pawn.mindState.enemyTarget != null)
            {
                return true;
            }

            if (squadMember.AssignedSquad.LeaderHasValidTarget())
            {
                return true;
            }

            return false;
        }
    }
}
