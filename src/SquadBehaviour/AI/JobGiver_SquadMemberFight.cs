using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{

    public class JobGiver_SquadMemberFight : JobGiver_AIFightEnemy
    {
        protected override bool OnlyUseAbilityVerbs
        {
            get { return false; }
        }
        protected override bool OnlyUseRangedSearch
        {
            get { return false; }
        }

        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
        {
            dest = IntVec3.Invalid;
            if (pawn == null || pawn.mindState == null)
            {
                return false;
            }
            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return false;
            }

            Thing enemyTarget = pawn.mindState.enemyTarget;
            Thing squadTarget = squadMember.AssignedSquad.FindTargetForMember(pawn);
            if (squadTarget != null)
            {
                enemyTarget = squadTarget;
            }

            Verb attackVerb = pawn.TryGetAttackVerb(enemyTarget, true, false);
            bool result = squadMember.AssignedSquad.TryFindCastPositionFor(pawn, enemyTarget, attackVerb, pawn.Position, 99f, out dest);
            return result;
        }

        protected int LastRepositionFormationTick = -1;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.Spawned || pawn.mindState == null)
            {
                return null;
            }

            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return null;
            }

            if (Current.Game.tickManager.TicksGame > LastRepositionFormationTick + 100 && squadMember != null && squadMember.AssignedSquad != null && squadMember.AssignedSquad.IsHoldingFormation && squadMember.AssignedSquad.InFormation)
            {
                IntVec3 formationPos = squadMember.AssignedSquad.SquadLeader.GetFormationPositionFor(pawn);
                float distToFormation = pawn.Position.DistanceTo(formationPos);

                if (distToFormation > squadMember.AssignedSquad.MaxFormationDistance)
                {
                    Job moveJob = JobMaker.MakeJob(JobDefOf.Goto, formationPos);
                    moveJob.expiryInterval = 30;
                    moveJob.locomotionUrgency = LocomotionUrgency.Sprint;
                    moveJob.checkOverrideOnExpire = true;
                    LastRepositionFormationTick = Current.Game.tickManager.TicksGame;
                    return moveJob;
                }
            }

            //this.UpdateEnemyTarget(pawn);
            Thing enemyTarget = squadMember.AssignedSquad.FindTargetForMember(pawn, squadMember.AssignedSquad.AggresionDistance);

            if (enemyTarget == null)
            {
                return null;
            }


            Verb verb = pawn.TryGetAttackVerb(enemyTarget, !pawn.IsColonist, false);
            if (verb == null || verb.ApparelPreventsShooting())
            {
                return null;
            }

            return AttackJobUtil.TryGetAttackNearbyEnemyJob(squadMember);
        }

        protected override bool ShouldLoseTarget(Pawn pawn)
        {
            return base.ShouldLoseTarget(pawn);
        }

        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            return base.ExtraTargetValidator(pawn, target);
        }

        protected override Thing FindAttackTarget(Pawn pawn)
        {
            float maxRange = 99f;

            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) && squadMember.AssignedSquad != null)
            {
                Thing squadTarget = squadMember.AssignedSquad.FindTargetForMember(pawn);
                if (squadTarget != null)
                {
                    return squadTarget;
                }
                else
                {
                    Thing foundTarget = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedAutoTargetable | TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedReachable | TargetScanFlags.IgnoreNonCombatants, (Thing thing) =>
                    {
                        return thing.Faction.HostileTo(pawn.Faction);
                    }, 0, maxRange);

                    if (foundTarget != null)
                    {
                        return foundTarget;
                    }
                }
            }
            Thing abilityTarget = squadMember.AssignedSquad.FindTargetForMember(pawn);
            return abilityTarget;
        }
    }
}