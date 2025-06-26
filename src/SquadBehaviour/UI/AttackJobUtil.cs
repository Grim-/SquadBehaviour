using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public static class AttackJobUtil
	{
		public static Job TryGetAttackNearbyEnemyJob(Comp_PawnSquadMember squadPawn, float maxRange = -1f)
		{
			Pawn pawn = squadPawn.Pawn;

			if (pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return null;
			}

			Thing thing = squadPawn.AssignedSquad.FindTargetForMember(pawn, maxRange);
			if (thing == null)
			{
				return null;
			}

			return TryGetAttackNearbyEnemyJob(pawn, thing);
		}
		public static Job TryGetAttackNearbyEnemyJob(Comp_PawnSquadMember squadPawn, Thing Target)
		{
			Pawn pawn = squadPawn.Pawn;
			Thing thing = Target;
			if (thing == null)
			{
				return null;
			}

			return TryGetAttackNearbyEnemyJob(pawn, thing);
		}
		public static Job TryGetAttackNearbyEnemyJob(Pawn pawn, Thing Target)
		{
			if (Target == null)
			{
				return null;
			}

			if (pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return null;
			}

			Thing thing = Target;

			Verb verb = pawn.TryGetAttackVerb(thing, !pawn.IsColonist, false);
			if (verb != null && verb.ApparelPreventsShooting())
			{
				return null;
			}

			//if (verb.IsMeleeAttack && pawn.CanReachImmediate(thing, PathEndMode.Touch))
			//{
			//	return JobMaker.MakeJob(JobDefOf.AttackStatic, thing);
			//}

			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, thing);
			job.maxNumStaticAttacks = 2;
			job.killIncappedTarget = true;
			job.expiryInterval = 2000;
			//job.endIfCantShootTargetFromCurPos = true;
			return job;
		}

	
	}
}
