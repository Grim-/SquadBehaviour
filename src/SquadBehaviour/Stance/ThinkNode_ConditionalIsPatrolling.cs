using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsPatrolling : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out ISquadMember squadMember))
            {
                return squadMember != null && squadMember.CurrentState == SquadMemberState.Patrol && squadMember.AssignedPatrol != null;
            }

            return false;
        }
    }

    public class ThinkNode_ConditionalAlreadyPatrolling : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out ISquadMember squadMember))
            {
                return pawn.CurJob != null &&
                 pawn.CurJob.def == JobDefOf.Goto &&
                 squadMember.CurrentState == SquadMemberState.Patrol;
            }

            return false;
        }
    }

    public class SquadOrder_PatrolZone : SquadOrderWorker
    {
        //public override bool IsSquadOrder => false;

        public override bool CanExecuteOrder(LocalTargetInfo Target)
        {
            return true;
        }

        public override void ExecuteOrder(LocalTargetInfo Target)
        {
            List<FloatMenuOption> option = new List<FloatMenuOption>();

            foreach (var item in SquadLeader.SquadLeaderPawn.Map.zoneManager.AllZones.Where(x => x is Zone_PatrolPath patrolPathZone).ToList())
            {
                option.Add(new FloatMenuOption($"Patrol Zone {item.ID}.", () =>
                {
                    foreach (var squad in SquadLeader.ActiveSquads)
                    {
                        foreach (var squadmember in squad.Value.Members)
                        {
                            if (squadmember.IsPartOfSquad(out ISquadMember squadMember))
                            {
                                squadMember.AssignedPatrol = (Zone_PatrolPath)item;
                                squadMember.SetCurrentMemberState(SquadMemberState.Patrol);
                            }
                        }
                    }                 
                }));
            }

            Find.WindowStack.Add(new FloatMenu(option));
        }
    }
    // Check if enemies are nearby during patrol
    public class ThinkNode_ConditionalEnemiesNearPatrol : ThinkNode_Conditional
    {
        public int regions = 3;

        protected override bool Satisfied(Pawn pawn)
        {
            if (!pawn.Spawned)
                return false;

            return PawnUtility.EnemiesAreNearby(pawn, regions);
        }
    }

    public class JobGiver_SquadPatrol : ThinkNode_JobGiver
    {
        private bool clockwisePatrol = true;

        protected virtual int PatrolJobExpireInterval
        {
            get { return 300; }
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            if (!pawn.IsPartOfSquad(out ISquadMember squadMember))
            {
                return null;
            }

            Zone_PatrolPath patrolZone = squadMember.AssignedPatrol as Zone_PatrolPath;
            if (patrolZone == null)
            {
                return null;
            }

            var orderedCells = patrolZone.GetOrderedPatrolCells(true);

            if (orderedCells.Count == 0)
            {
                Log.Message($"[{pawn.LabelShort}] No patrol cells - returning null job");
                return null;
            }

            IntVec3 closestPoint = patrolZone.FindNearestPointOnPath(pawn.Position);

            IntVec3 nextPoint = patrolZone.GetNextPatrolPoint(closestPoint, clockwisePatrol);

            if (pawn.Position == nextPoint)
            {
                nextPoint = patrolZone.GetNextPatrolPoint(nextPoint, clockwisePatrol);
            }

            if (!nextPoint.IsValid)
            {
                Log.Message($"[{pawn.LabelShort}] Next point is invalid - returning null job");
                return null;
            }


            bool canReach = pawn.CanReach(nextPoint, PathEndMode.OnCell, Danger.Deadly);

            if (!canReach)
            {
                //Log.Message($"[{pawn.LabelShort}] Cannot reach next point - returning null job");
                return null;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Goto, nextPoint);
            job.locomotionUrgency = LocomotionUrgency.Walk;
            job.expiryInterval = PatrolJobExpireInterval;
            job.checkOverrideOnExpire = true;
            return job;
        }
    }
}
