using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_SquadDefendPoint : JobGiver_AIDefendPoint
    {
        // Override the shooting position finder to use your squad system
        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
        {
            // First check if there's an enemy to target
            Thing enemyTarget = pawn.mindState.enemyTarget;
            Verb verb = verbToUse ?? pawn.TryGetAttackVerb(enemyTarget, !pawn.IsColonist, false);

            if (verb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }

            // Get the defense position from squad system
            IntVec3 defendPos = IntVec3.Invalid;
            float defendRadius = 0f;

            if (pawn.IsPartOfSquad(out ISquadMember squadMember) &&
                squadMember.CurrentState == SquadMemberState.DefendPoint)
            {
                if (squadMember.AssignedSquad.InFormation)
                {
                    defendPos = squadMember.SquadLeader.GetFormationPositionFor(pawn, squadMember.DefendPoint, Rot4.North);
                }
                else
                {
                    defendPos = squadMember.DefendPoint;
                }
                defendRadius = 5f; // Adjust as needed
            }

            // Use the CastPositionFinder with our squad-based position
            if (defendPos.IsValid)
            {
                return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
                {
                    caster = pawn,
                    target = enemyTarget,
                    verb = verb,
                    maxRangeFromTarget = 9999f,
                    locus = defendPos,
                    maxRangeFromLocus = defendRadius,
                    wantCoverFromTarget = (verb.verbProps.range > 7f)
                }, out dest);
            }

            dest = IntVec3.Invalid;
            return false;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            // Update enemy target (this is from the base class)
            this.UpdateEnemyTarget(pawn);
            Thing enemyTarget = pawn.mindState.enemyTarget;

            if (enemyTarget == null)
            {
                // No enemy - fall back to wait at position
                return StayAtDefendPointJob(pawn);
            }

            // Check for psychologically invisible enemies
            if (enemyTarget is Pawn enemyPawn && enemyPawn.IsPsychologicallyInvisible())
            {
                return StayAtDefendPointJob(pawn);
            }

            // Try to use abilities regardless of colonist status
            if (pawn.abilities != null && !this.DisableAbilityVerbs)
            {
                Job abilityJob = GetAbilityJob(pawn, enemyTarget);
                if (abilityJob != null)
                {
                    return abilityJob;
                }
            }

            // Handle the rest just like the base class
            if (this.OnlyUseAbilityVerbs)
            {
                IntVec3 intVec;
                if (!this.TryFindShootingPosition(pawn, out intVec, null))
                {
                    return StayAtDefendPointJob(pawn);
                }

                if (intVec == pawn.Position)
                {
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, this.ExpiryInterval_Ability.RandomInRange, true);
                }

                Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
                job.expiryInterval = this.ExpiryInterval_Ability.RandomInRange;
                job.checkOverrideOnExpire = true;
                return job;
            }
            else
            {
                // Check for valid attack verb (copied from base class)
                bool allowAbilities = !this.DisableAbilityVerbs;
                Verb verb = pawn.TryGetAttackVerb(enemyTarget, allowAbilities, this.allowTurrets);

                if (verb == null)
                {
                    return StayAtDefendPointJob(pawn);
                }

                if (verb.verbProps.IsMeleeAttack)
                {
                    return this.MeleeAttackJob(pawn, enemyTarget);
                }

                // Check if current position is good for shooting
                bool hasCover = CoverUtility.CalculateOverallBlockChance(pawn, enemyTarget.Position, pawn.Map) > 0.01f;
                bool canStand = pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
                bool canHitFromHere = verb.CanHitTarget(enemyTarget);
                bool isClose = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;

                if ((hasCover && canStand && canHitFromHere) || (isClose && canHitFromHere))
                {
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
                }

                // Find a shooting position
                IntVec3 shootPos;
                if (!this.TryFindShootingPosition(pawn, out shootPos, null))
                {
                    return StayAtDefendPointJob(pawn);
                }

                if (shootPos == pawn.Position)
                {
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
                }

                Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, shootPos);
                gotoJob.expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange;
                gotoJob.checkOverrideOnExpire = true;
                return gotoJob;
            }
        }

        // Copy of the game's GetAbilityJob method but without the colonist check
        private Job GetAbilityJob(Pawn pawn, Thing enemyTarget)
        {
            if (pawn.abilities == null)
            {
                return null;
            }

            List<Ability> castableAbilities = pawn.abilities.AICastableAbilities(enemyTarget, true);
            if (castableAbilities.NullOrEmpty())
            {
                return null;
            }

            if (pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted))
            {
                // Try abilities that can directly hit the target
                for (int i = 0; i < castableAbilities.Count; i++)
                {
                    if (castableAbilities[i].verb.CanHitTarget(enemyTarget))
                    {
                        return castableAbilities[i].GetJob(enemyTarget, enemyTarget);
                    }
                }

                // Try AOE abilities
                for (int j = 0; j < castableAbilities.Count; j++)
                {
                    LocalTargetInfo aoeTarget = castableAbilities[j].AIGetAOETarget();
                    if (aoeTarget.IsValid)
                    {
                        return castableAbilities[j].GetJob(aoeTarget, aoeTarget);
                    }
                }

                // Try self-targeting abilities
                for (int k = 0; k < castableAbilities.Count; k++)
                {
                    if (castableAbilities[k].verb.targetParams.canTargetSelf)
                    {
                        return castableAbilities[k].GetJob(pawn, pawn);
                    }
                }
            }

            return null;
        }

        // Helper to create a job to stay at defend point
        private Job StayAtDefendPointJob(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out ISquadMember squadMember) &&
                squadMember.CurrentState == SquadMemberState.DefendPoint)
            {
                IntVec3 defendPos;
                if (squadMember.AssignedSquad.InFormation)
                {
                    defendPos = squadMember.SquadLeader.GetFormationPositionFor(pawn, squadMember.DefendPoint, Rot4.North);
                }
                else
                {
                    defendPos = squadMember.DefendPoint;
                }

                // If not at defend position, go there
                if (!pawn.Position.InHorDistOf(defendPos, 0.9f))
                {
                    Job goToJob = JobMaker.MakeJob(JobDefOf.Goto, defendPos);
                    goToJob.expiryInterval = 120;
                    goToJob.checkOverrideOnExpire = true;
                    return goToJob;
                }

                // Wait in combat stance
                Job waitJob = JobMaker.MakeJob(JobDefOf.Wait_Combat);
                waitJob.expiryInterval = 120;
                waitJob.checkOverrideOnExpire = true;
                return waitJob;
            }

            return null;
        }
        // Override to provide a fallback behavior when no enemies are present
        //protected override Job TryGiveJob(Pawn pawn)
        //{
        //    // Try to get the standard combat job
        //    Job combatJob = base.TryGiveJob(pawn);
        //    if (combatJob != null)
        //    {
        //        return combatJob;
        //    }

        //    // If no enemies to fight, make sure pawn stays at defend position
        //    if (pawn != null && pawn.IsPartOfSquad(out ISquadMember squadMember) &&
        //        squadMember.CurrentState == SquadMemberState.DefendPoint)
        //    {
        //        IntVec3 defendPos;
        //        if (squadMember.AssignedSquad.InFormation)
        //        {
        //            defendPos = squadMember.SquadLeader.GetFormationPositionFor(pawn, squadMember.DefendPoint, Rot4.North);
        //        }
        //        else
        //        {
        //            defendPos = squadMember.DefendPoint;
        //        }

        //        // If not at defend position, go there
        //        if (!pawn.Position.InHorDistOf(defendPos, 0.9f))
        //        {
        //            Job goToJob = JobMaker.MakeJob(JobDefOf.Goto, defendPos);
        //            goToJob.expiryInterval = 120;
        //            goToJob.checkOverrideOnExpire = true;
        //            return goToJob;
        //        }

        //        // If at position, wait in combat stance
        //        Job waitJob = JobMaker.MakeJob(JobDefOf.Wait_Combat);
        //        waitJob.expiryInterval = 120; // Recheck every 2 seconds
        //        waitJob.checkOverrideOnExpire = true;
        //        return waitJob;
        //    }

        //    return null;
        //}
    }


    //public class JobGiver_SquadDefendPoint : JobGiver_AIDefendPoint
    //{
    //    protected override IntVec3 GetFlagPosition(Pawn pawn)
    //    {
    //        if (pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember.DefendPoint != IntVec3.Invalid)
    //        {
    //            return squadMember.DefendPoint;
    //        }

    //        return base.GetFlagPosition(pawn);
    //    }
    //}
}
