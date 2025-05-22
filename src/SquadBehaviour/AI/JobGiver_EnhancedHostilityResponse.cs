using RimWorld;
using System.Collections.Generic;
using UnityEngine;
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


	public static class AttackJobUtil
    {
		public static Job TryGetAttackNearbyEnemyJob(Pawn pawn)
		{
			if (pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return null;
			}
			bool isMeleeAttack = pawn.CurrentEffectiveVerb.IsMeleeAttack;
			float maxDist = 8f;
			if (!isMeleeAttack)
			{
				maxDist = Mathf.Clamp(pawn.CurrentEffectiveVerb.verbProps.range * 0.66f, 2f, 20f);
			}
			Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, null, 0f, maxDist, default(IntVec3), float.MaxValue, false, true, false, false);
			if (thing == null)
			{
				return null;
			}

			if (pawn.abilities != null && !pawn.abilities.abilities.NullOrEmpty() && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) && squadMember.AssignedSquad != null && squadMember.AbilitiesAllowed)
			{
				Thing threat = AbilityUtility.FindBestAbilityTarget(pawn);
				if (threat != null)
				{
					Ability ability = AbilityUtility.GetBestAbility(pawn, threat);
					if (ability != null)
					{
						Job abilityJob = AbilityUtility.GetAbilityJob(pawn, threat, ability);
						if (abilityJob != null)
						{
							return abilityJob;
						}
					}
				}
			}

			if (isMeleeAttack || pawn.CanReachImmediate(thing, PathEndMode.Touch))
			{
				return JobMaker.MakeJob(JobDefOf.AttackMelee, thing);
			}
			Verb verb = pawn.TryGetAttackVerb(thing, !pawn.IsColonist, false);
			if (verb == null || verb.ApparelPreventsShooting())
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, thing);
			job.maxNumStaticAttacks = 2;
			job.expiryInterval = 2000;
			job.endIfCantShootTargetFromCurPos = true;
			return job;
		}

		private static List<Thing> tmpThreats = new List<Thing>();
		public static Job TryGetFleeJob(Pawn pawn)
		{
			if (!SelfDefenseUtility.ShouldStartFleeing(pawn))
			{
				return null;
			}
			IntVec3 c;
			if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.FleeAndCower)
			{
				c = pawn.CurJob.targetA.Cell;
			}
			else
			{
				tmpThreats.Clear();
				List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
				for (int i = 0; i < potentialTargetsFor.Count; i++)
				{
					Thing thing = potentialTargetsFor[i].Thing;
					if (SelfDefenseUtility.ShouldFleeFrom(thing, pawn, false, false))
					{
						tmpThreats.Add(thing);
					}
				}
				List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
				for (int j = 0; j < list.Count; j++)
				{
					Thing thing2 = list[j];
					if (SelfDefenseUtility.ShouldFleeFrom(thing2, pawn, false, false))
					{
						tmpThreats.Add(thing2);
					}
				}
				if (!tmpThreats.Any<Thing>())
				{
					Log.Error(pawn.LabelShort + " decided to flee but there is not any threat around.");
					Region region = pawn.GetRegion(RegionType.Set_Passable);
					if (region == null)
					{
						return null;
					}
					RegionTraverser.BreadthFirstTraverse(region, (Region from, Region reg) => reg.door == null || reg.door.Open, delegate (Region reg)
					{
						List<Thing> list2 = reg.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
						for (int k = 0; k < list2.Count; k++)
						{
							Thing thing3 = list2[k];
							if (SelfDefenseUtility.ShouldFleeFrom(thing3, pawn, false, false))
							{
								tmpThreats.Add(thing3);
								Log.Warning(string.Format("  Found a viable threat {0}; tests are {1}, {2}, {3}", new object[]
								{
									thing3.LabelShort,
									thing3.Map.attackTargetsCache.Debug_CheckIfInAllTargets(thing3 as IAttackTarget),
									thing3.Map.attackTargetsCache.Debug_CheckIfHostileToFaction(pawn.Faction, thing3 as IAttackTarget),
									thing3 is IAttackTarget
								}));
							}
						}
						return false;
					}, 9, RegionType.Set_Passable);
					if (!tmpThreats.Any<Thing>())
					{
						return null;
					}
				}
				c = CellFinderLoose.GetFleeDest(pawn, tmpThreats, 23f);
				tmpThreats.Clear();
			}
			return JobMaker.MakeJob(JobDefOf.FleeAndCower, c);
		}
	}
}
