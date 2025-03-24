using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobDriver_FormationFollow : JobDriver_FollowClose
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil formationToil = ToilMaker.MakeToil("FormationFollow");

            formationToil.tickAction = () =>
            {
                Pawn followee = this.TargetA.Pawn;
                float followRadius = this.job.followRadius;

                if (!this.pawn.pather.Moving || this.pawn.IsHashIntervalTick(30))
                {
                    if (!this.pawn.IsPartOfSquad(out ISquadMember squadMember))
                    {
                        Log.Message("Is not part of a squad.");
                        base.EndJobWith(JobCondition.Errored);
                        return;
                    }

                    IntVec3 targetCell = squadMember.SquadLeader.GetFormationPositionFor(pawn);

                    if (this.pawn.Position != targetCell)
                    {

                        if (!this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
                        {
                            IntVec3 newTargetCell = CellFinder.StandableCellNear(targetCell, this.pawn.Map, 2f);
                            if (this.pawn.CanReach(newTargetCell, PathEndMode.OnCell, Danger.Deadly))
                            {
                                targetCell = newTargetCell;
                            }
                        }

                        if (!this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
                        {
                            Log.Message("cant reach target");
                            base.EndJobWith(JobCondition.Incompletable);
                            return;
                        }

                        this.pawn.pather.StartPath(targetCell, PathEndMode.OnCell);
                        this.locomotionUrgencySameAs = followee;
                    }
                    else if (!followee.pather.Moving)
                    {
                        base.EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                }
            };

            formationToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return formationToil;
        }

        public override bool IsContinuation(Job j)
        {
            return this.job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }

        public static bool FarEnoughAndPossibleToStartJob(Pawn follower, Pawn followee, ISquadLeader undeadMaster, float radius)
        {
            if (radius <= 0f)
            {
                Log.ErrorOnce($"Checking formation follow job with radius <= 0. pawn={follower.ToStringSafe<Pawn>()}",
                    follower.thingIDNumber ^ 843254009);
                return false;
            }
            IntVec3 targetCell = undeadMaster.GetFormationPositionFor(follower);

            return follower.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly);
        }
    }


}
