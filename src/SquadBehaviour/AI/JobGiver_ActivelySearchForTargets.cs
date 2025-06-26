using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_ActivelySearchForTargets : ThinkNode_JobGiver
    {
        protected int LastCheckTick = 0;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return null;
            }

            float maxDist = squadMember.AssignedSquad.MaxAttackDistanceFor(pawn);
      
            Thing thing = squadMember.AssignedSquad.FindTargetForMember(pawn, maxDist);

            if (thing != null && thing.Position.DistanceTo(pawn.Position) < maxDist)
            {
                return AttackJobUtil.TryGetAttackNearbyEnemyJob(squadMember);
            }
            return null;
        }
    }
}