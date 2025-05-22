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

			LastCheckTick = Current.Game.tickManager.TicksAbs;

			Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn,
				TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable,
				null, 0f, 8f, default(IntVec3), float.MaxValue, false, true, false, false);

            if (thing != null && thing.Position.DistanceTo(pawn.Position) < squadMember.AssignedSquad.AggresionDistance)
            {
				return AttackJobUtil.TryGetAttackNearbyEnemyJob(pawn);
            }

			return null;
		}
	}
}
