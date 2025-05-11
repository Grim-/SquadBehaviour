using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    /// <summary>
    /// Static utility class for AI-controlled ability usage
    /// </summary>
    public static class AbilityUtility
	{
		/// <summary>
		/// Finds the best ability to use against a target
		/// </summary>
		public static Ability GetBestAbility(Pawn pawn, TargetInfo target)
		{
			if (pawn == null || !target.IsValid || pawn.abilities == null || pawn.abilities.abilities.NullOrEmpty())
			{
				return null;
			}

			List<Ability> potentialAbilities = new List<Ability>();
			foreach (Ability ability in pawn.abilities.abilities)
			{
				if (!ability.OnCooldown &&
					ability.def.verbProperties != null &&
					ability.def.verbProperties.targetParams.CanTarget(target, null) &&
					ability.CanApplyOn(target))
				{
					potentialAbilities.Add(ability);
				}
			}

			if (potentialAbilities.NullOrEmpty())
			{
				return null;
			}

			float distanceToTarget = pawn.Position.DistanceTo(target.Cell);

			Ability bestAbility = potentialAbilities
				.Where(a => a.def.verbProperties.range >= distanceToTarget)
				.OrderBy(a => a.def.verbProperties.range)
				.ThenBy(a => a.CooldownTicksRemaining)
				.FirstOrDefault();

			return bestAbility;
		}

		/// <summary>
		/// Finds a suitable position to cast an ability from
		/// </summary>
		public static bool TryFindCastPosition(Pawn pawn, Thing target, Ability ability, out IntVec3 position)
		{
			position = IntVec3.Invalid;

			if (pawn == null || target == null || ability == null)
			{
				return false;
			}

			return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
			{
				caster = pawn,
				target = target,
				verb = ability.verb,
				maxRangeFromTarget = ability.verb.verbProps.range,
				wantCoverFromTarget = false,
				preferredCastPosition = new IntVec3?(pawn.Position)
			}, out position);
		}

		/// <summary>
		/// Determines if a pawn can use any abilities against a target
		/// </summary>
		public static bool CanUseAbilitiesOnTarget(Pawn pawn, Thing target)
		{
			if (pawn == null || target == null || pawn.abilities == null || pawn.abilities.abilities.NullOrEmpty())
			{
				return false;
			}

			return GetBestAbility(pawn, target) != null;
		}

		/// <summary>
		/// Creates a job to use the ability on the target
		/// </summary>
		public static Job GetAbilityJob(Pawn pawn, Thing target, Ability ability)
		{
			if (pawn == null || target == null || ability == null)
			{
				return null;
			}

			if (ability.verb.CanHitTarget(target))
			{
				return ability.GetJob(target, target);
			}

			IntVec3 castPos;
			if (TryFindCastPosition(pawn, target, ability, out castPos))
			{
				if (castPos != pawn.Position)
				{
					Job moveJob = JobMaker.MakeJob(JobDefOf.Goto, castPos);
					moveJob.expiryInterval = 30;
					moveJob.checkOverrideOnExpire = true;
					return moveJob;
				}
				else
				{
					// in position but somehow can't hit wait a moment
					return JobMaker.MakeJob(JobDefOf.Wait_Combat, 30, true);
				}
			}

			return null;
		}

		/// <summary>
		/// Finds the most suitable threat for a pawn to attack with abilities
		/// </summary>
		public static Thing FindBestAbilityTarget(Pawn pawn, float maxRange = -1f)
		{
			if (pawn == null || pawn.abilities == null || pawn.abilities.abilities.NullOrEmpty())
			{
				return null;
			}

			if (maxRange <= 0f)
			{
				maxRange = pawn.abilities.abilities.Max(a =>
					a.def.verbProperties != null ? a.def.verbProperties.range : 0f);
				maxRange = Mathf.Max(maxRange, 20f);
			}

			return (Thing)AttackTargetFinder.BestAttackTarget(
				pawn,
				TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns |
				TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat |
				TargetScanFlags.NeedAutoTargetable,
				(Thing x) => CanUseAbilitiesOnTarget(pawn, x),
				0f,
				maxRange,
				default(IntVec3),
				float.MaxValue,
				false,
				true,
				false,
				true);
		}
	}
}
