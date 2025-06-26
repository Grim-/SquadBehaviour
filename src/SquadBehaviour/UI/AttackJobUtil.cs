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
		public static Job TryGetAttackNearbyEnemyJob(Pawn pawn, Thing Target, float maxRange = -1f)
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

			//float distanceToTarget = thing.Position.DistanceTo(pawn.Position);

   //         if (distanceToTarget > maxRange)
			//{
			//	Job waitJob = JobMaker.MakeJob(JobDefOf.Wait_Combat, thing);
			//	waitJob.maxNumStaticAttacks = 2;
			//	waitJob.expiryInterval = 30;
			//	return waitJob;
			//}


            if (verb.IsMeleeAttack || pawn.CanReachImmediate(thing, PathEndMode.Touch))
            {
                return JobMaker.MakeJob(JobDefOf.AttackMelee, thing);
            }

            Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, thing);
			job.maxNumStaticAttacks = 2;
			job.killIncappedTarget = true;
			job.expiryInterval = 2000;
			job.endIfCantShootTargetFromCurPos = true;
			return job;
		}

	
	}
}
