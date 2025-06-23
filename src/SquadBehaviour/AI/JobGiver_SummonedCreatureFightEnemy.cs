using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
	public class JobGiver_SummonedCreatureFightEnemy : JobGiver_AIFightEnemy
	{
		protected override bool OnlyUseAbilityVerbs
		{
			get { return true; }
		}
		protected override bool OnlyUseRangedSearch
		{
			get { return true; }
		}
		protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
		{
			dest = IntVec3.Invalid;
			if (pawn == null || pawn.mindState == null || pawn.mindState.enemyTarget == null || pawn.abilities == null)
			{
				return false;
			}
			Thing enemyTarget = pawn.mindState.enemyTarget;
			Ability ability = AbilityUtility.GetBestAbility(pawn, enemyTarget);
			if (ability == null)
			{
				return false;
			}
			return AbilityUtility.TryFindCastPosition(pawn, enemyTarget, ability, out dest);
		}
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.Spawned || pawn.mindState == null)
			{
				return null;
			}

			if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) && !squadMember.AbilitiesAllowed)
			{
				return null;
			}

			this.UpdateEnemyTarget(pawn);
			Thing enemyTarget = pawn.mindState.enemyTarget;
			if (enemyTarget == null)
			{
				return null;
			}
			if (!AbilityUtility.CanUseAbilitiesOnTarget(pawn, enemyTarget))
			{
				return null;
			}
			Ability ability = AbilityUtility.GetBestAbility(pawn, enemyTarget);
			if (ability != null)
			{
				return AbilityUtility.GetAbilityJob(pawn, enemyTarget, ability);
			}
			IntVec3 intVec;
			if (!this.TryFindShootingPosition(pawn, out intVec, null))
			{
				return null;
			}
			if (intVec == pawn.Position)
			{
				return JobMaker.MakeJob(JobDefOf.Wait_Combat, 30, true);
			}
			Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
			job.expiryInterval = 30;
			job.checkOverrideOnExpire = true;
			return job;
		}
		protected override bool ShouldLoseTarget(Pawn pawn)
		{
			return base.ShouldLoseTarget(pawn) || !AbilityUtility.CanUseAbilitiesOnTarget(pawn, pawn.mindState.enemyTarget);
		}
		protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
		{
			return base.ExtraTargetValidator(pawn, target) && AbilityUtility.CanUseAbilitiesOnTarget(pawn, target);
		}

		protected override Thing FindAttackTarget(Pawn pawn)
		{
			if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) && squadMember.AssignedSquad != null)
			{
				return squadMember.AssignedSquad.FindTargetForMember(pawn);
			}

			return AbilityUtility.FindBestAbilityTarget(pawn);
		}
	}
}
