using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_SummonedCreatureFightEnemy : JobGiver_AIDefendPawn
    {
        protected Pawn Master = null;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null)
            {
                Log.Error("TryGiveJob called with null pawn");
                return null;
            }

            this.chaseTarget = true;
            this.allowTurrets = true;
            this.ignoreNonCombatants = true;
            this.humanlikesOnly = false;

            Job job = base.TryGiveJob(pawn);

            if (job != null)
            {
                job.reportStringOverride = "Engaged in Squad combat";
            }

            if (pawn.mindState != null)
            {
                pawn.mindState.canFleeIndividual = false;
            }

            return job;
        }

        protected override Thing FindAttackTarget(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out ISquadMember squadMember))
            {
                if (squadMember.AssignedSquad.HostilityResponse == SquadHostility.Aggressive)
                {


                    IAttackTarget attackTarget = AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, new Predicate<Thing>(this.IsGoodTarget)
                        , 0, squadMember.AssignedSquad.FollowDistance, squadMember.SquadLeader.LeaderPosition);

                    if (attackTarget != null)
                    {
                        return attackTarget.Thing;
                    }
                }
            }
            return base.FindAttackTarget(pawn);


        }
        protected virtual bool IsGoodTarget(Thing thing)
        {
            Pawn pawn;
            return (pawn = (thing as Pawn)) != null && pawn.Spawned && !pawn.Downed && !pawn.IsPsychologicallyInvisible();
        }
        protected override Pawn GetDefendee(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out ISquadMember squadMember))
            {
                return squadMember.SquadLeader.SquadLeaderPawn;
            }
            return null;
        }

        protected override float GetFlagRadius(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out ISquadMember squadMember))
            {
                return squadMember.AssignedSquad.AggresionDistance;
            }
            return 10f;
        }

        protected override IntVec3 GetFlagPosition(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out ISquadMember squadMember))
            {
                return squadMember.SquadLeader.SquadLeaderPawn.Position;
            }

            return IntVec3.Invalid;
        }
    }
}
