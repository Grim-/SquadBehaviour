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

	//  public class JobGiver_SummonedCreatureFightEnemy : JobGiver_AIFightEnemies
	//  {
	//      protected Pawn Master = null;

	//      protected override bool OnlyUseAbilityVerbs => true;

	//      protected override Job TryGiveJob(Pawn pawn)
	//{
	//	this.UpdateEnemyTarget(pawn);
	//	Thing enemyTarget = pawn.mindState.enemyTarget;
	//	if (enemyTarget == null)
	//	{
	//		return null;
	//	}
	//	Pawn pawn2;
	//	if ((pawn2 = (enemyTarget as Pawn)) != null && pawn2.IsPsychologicallyInvisible())
	//	{
	//		return null;
	//	}
	//	bool flag = !this.DisableAbilityVerbs;
	//	if (flag)
	//	{
	//		Job abilityJob = this.GetAbilityJob(pawn, enemyTarget);
	//		if (abilityJob != null)
	//		{
	//			return abilityJob;
	//		}
	//	}
	//	if (this.OnlyUseAbilityVerbs)
	//	{
	//		IntVec3 intVec;
	//		if (!this.TryFindShootingPosition(pawn, out intVec, null))
	//		{
	//			return null;
	//		}
	//		if (intVec == pawn.Position)
	//		{
	//			return JobMaker.MakeJob(JobDefOf.Wait_Combat, this.ExpiryInterval_Ability.RandomInRange, true);
	//		}
	//		Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
	//		job.expiryInterval = this.ExpiryInterval_Ability.RandomInRange;
	//		job.checkOverrideOnExpire = true;
	//		return job;
	//	}
	//	else
	//	{
	//		Verb verb = pawn.TryGetAttackVerb(enemyTarget, flag, this.allowTurrets);
	//		if (verb == null)
	//		{
	//			return null;
	//		}
	//		if (verb.verbProps.IsMeleeAttack)
	//		{
	//			return this.MeleeAttackJob(pawn, enemyTarget);
	//		}
	//		bool flag2 = CoverUtility.CalculateOverallBlockChance(pawn, enemyTarget.Position, pawn.Map) > 0.01f;
	//		bool flag3 = pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
	//		bool flag4 = verb.CanHitTarget(enemyTarget);
	//		bool flag5 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
	//		if ((flag2 && flag3 && flag4) || (flag5 && flag4))
	//		{
	//			return JobMaker.MakeJob(JobDefOf.Wait_Combat, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
	//		}
	//		IntVec3 intVec2;
	//		if (!this.TryFindShootingPosition(pawn, out intVec2, null))
	//		{
	//			return null;
	//		}
	//		if (intVec2 == pawn.Position)
	//		{
	//			return JobMaker.MakeJob(JobDefOf.Wait_Combat, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
	//		}
	//		Job job2 = JobMaker.MakeJob(JobDefOf.Goto, intVec2);
	//		job2.expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange;
	//		job2.checkOverrideOnExpire = true;
	//		return job2;
	//	}
	//}
	//private  bool CanTarget(Pawn pawn, TargetInfo target)
	//{
	//	if (pawn == null || target == null || pawn.abilities == null || pawn.abilities.abilities.NullOrEmpty())
	//	{
	//		return false;
	//	}

	//	// Find a usable combat ability
	//	AbilityDef selectedAbility = SelectBestAbility(pawn);
	//	if (selectedAbility == null)
	//	{
	//		return false;
	//	}

	//	// Check if the selected ability can target the enemy
	//	if (!selectedAbility.verbProperties.targetParams.CanTarget(target, null))
	//	{
	//		return false;
	//	}

	//	Ability ability = pawn.abilities.GetAbility(selectedAbility, false);
	//	return ability != null && ability.CanApplyOn(target);
	//}

	//private AbilityDef SelectBestAbility(Pawn pawn)
	//{
	//	if (pawn.abilities == null || pawn.abilities.abilities.NullOrEmpty())
	//	{
	//		return null;
	//	}

	//	// Try to find a combat ability
	//	return pawn.abilities.abilities
	//		.Where(a => !a.OnCooldown)
	//		.Select(a => a.def)
	//		.FirstOrDefault();
	//}

	//protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
	//{
	//	return base.ExtraTargetValidator(pawn, target) && CanTarget(pawn, target);
	//}
	//private Job GetAbilityJob(Pawn pawn, Thing enemyTarget)
	//{
	//	if (pawn.abilities == null)
	//	{
	//		return null;
	//	}
	//	List<Ability> list = pawn.abilities.AICastableAbilities(enemyTarget, true);
	//	if (list.NullOrEmpty<Ability>())
	//	{
	//		return null;
	//	}
	//	if (pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted))
	//	{
	//		for (int i = 0; i < list.Count; i++)
	//		{
	//			if (list[i].verb.CanHitTarget(enemyTarget))
	//			{
	//				return list[i].GetJob(enemyTarget, enemyTarget);
	//			}
	//		}
	//		for (int j = 0; j < list.Count; j++)
	//		{
	//			LocalTargetInfo localTargetInfo = list[j].AIGetAOETarget();
	//			if (localTargetInfo.IsValid)
	//			{
	//				return list[j].GetJob(localTargetInfo, localTargetInfo);
	//			}
	//		}
	//		for (int k = 0; k < list.Count; k++)
	//		{
	//			if (list[k].verb.targetParams.canTargetSelf)
	//			{
	//				return list[k].GetJob(pawn, pawn);
	//			}
	//		}
	//	}
	//	return null;
	//}
	////protected override Job TryGiveJob(Pawn pawn)
	////{
	////    if (pawn == null)
	////    {
	////        Log.Error("TryGiveJob called with null pawn");
	////        return null;
	////    }

	////    //this.chaseTarget = true;
	////    this.allowTurrets = true;
	////    this.ignoreNonCombatants = true;
	////    this.humanlikesOnly = false;

	////    Job job = base.TryGiveJob(pawn);

	////    if (job != null)
	////    {
	////        job.reportStringOverride = "Engaged in Squad combat";
	////    }

	////    if (pawn.mindState != null)
	////    {
	////        pawn.mindState.canFleeIndividual = false;
	////    }

	////    return job;
	////}

	////protected override Thing FindAttackTarget(Pawn pawn)
	////{
	////    if (pawn.IsPartOfSquad(out ISquadMember squadMember))
	////    {
	////        if (squadMember.AssignedSquad.HostilityResponse == SquadHostility.Aggressive)
	////        {


	////            IAttackTarget attackTarget = AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, new Predicate<Thing>(this.IsGoodTarget)
	////                , 0, squadMember.AssignedSquad.FollowDistance, squadMember.SquadLeader.LeaderPosition);

	////            if (attackTarget != null)
	////            {
	////                return attackTarget.Thing;
	////            }
	////        }
	////    }
	////    return base.FindAttackTarget(pawn);


	////}
	////protected virtual bool IsGoodTarget(Thing thing)
	////{
	////    Pawn pawn;
	////    return (pawn = (thing as Pawn)) != null && pawn.Spawned && !pawn.Downed && !pawn.IsPsychologicallyInvisible();
	////}
	////protected override Pawn GetDefendee(Pawn pawn)
	////{
	////    if (pawn.IsPartOfSquad(out ISquadMember squadMember))
	////    {
	////        return squadMember.SquadLeader.SquadLeaderPawn;
	////    }
	////    return null;
	////}

	////protected override float GetFlagRadius(Pawn pawn)
	////      {
	////          if (pawn.IsPartOfSquad(out ISquadMember squadMember))
	////          {
	////              return squadMember.AssignedSquad.AggresionDistance;
	////          }
	////          return 10f;
	////      }

	////      protected override IntVec3 GetFlagPosition(Pawn pawn)
	////      {
	////          if (pawn.IsPartOfSquad(out ISquadMember squadMember))
	////          {
	////              return squadMember.SquadLeader.SquadLeaderPawn.Position;
	////          }

	////          return IntVec3.Invalid;
	////      }
	//  }
}
