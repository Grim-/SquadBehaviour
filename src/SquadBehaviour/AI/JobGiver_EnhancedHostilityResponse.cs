using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SquadBehaviour
{

    public class JobGiver_EnhancedHostilityResponse : JobGiver_ConfigurableHostilityResponse
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.playerSettings == null || !pawn.playerSettings.UsesConfigurableHostilityResponse)
			{
				return null;
			}
			if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
			{
				return null;
			}
			if (pawn.Downed)
			{
				return null;
			}

			if (ModsConfig.AnomalyActive)
			{
				Lord lord = pawn.GetLord();
				if (((lord != null) ? lord.LordJob : null) is LordJob_PsychicRitual)
				{
					return null;
				}
			}

            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
				return base.TryGiveJob(pawn);
			}
			else
			{
                if (squadMember.CurrentState == SquadMemberState.DoNothing)
                {
					return null;
                }

				Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, 
					TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, 
					null, 0f, 8f, default(IntVec3), float.MaxValue, false, true, false, false);

                if (thing != null)
                {
					float distanceToNearestTarget = thing.Position.DistanceTo(pawn.Position);
                    if (distanceToNearestTarget < squadMember.AssignedSquad.AggresionDistance)
                    {
						return AttackJobUtil.TryGetAttackNearbyEnemyJob(pawn);
					}
				}

				switch (pawn.playerSettings.hostilityResponse)
				{
					case HostilityResponseMode.Ignore:
						return null;
					case HostilityResponseMode.Attack:
						return AttackJobUtil.TryGetAttackNearbyEnemyJob(pawn);
					case HostilityResponseMode.Flee:
						return AttackJobUtil.TryGetFleeJob(pawn);
					default:
						return null;
				}
			}
		}
	}
}
