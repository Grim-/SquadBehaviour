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
            if (pawn == null)
            {
                return false;
            }

            if (!pawn.Spawned)
            {
                return false;
            }

            if (!pawn.IsPartOfSquad(out ISquadMember squadMember))
            {
                Log.Message("squadLeader is null");
                return false;
            }
            if (pawn.mindState?.enemyTarget != null || squadMember.SquadLeader.SquadLeaderPawn.mindState?.enemyTarget != null)
            {
                return true;
            }

            //not sure if should add this
            //if (squadMember.SquadLeader.SquadMembersPawns != null)
            //{
            //    foreach (var item in squadMember.SquadLeader.SquadMembersPawns)
            //    {
            //        if (item.mindState?.enemyTarget != null || item.mindState?.meleeThreat != null)
            //        {
            //            return true;
            //        }

            //    }
            //}

            //if (squadMember.AssignedSquad != null)
            //{
            //    foreach (var item in squadMember.SquadLeader.SquadMembersPawns)
            //    {
            //        if (item.mindState?.enemyTarget != null || item.mindState?.meleeThreat != null)
            //        {
            //            return true;
            //        }

            //    }
            //}

            if (pawn.Spawned && squadMember.SquadLeader.SquadLeaderPawn.Spawned)
            {
                if (PawnUtility.EnemiesAreNearby(pawn, Mathf.RoundToInt(squadMember.AssignedSquad.AggresionDistance), true))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
