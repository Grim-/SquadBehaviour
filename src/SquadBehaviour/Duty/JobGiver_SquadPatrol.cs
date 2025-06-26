using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_SquadPatrol : ThinkNode_JobGiver
    {
        private const float ReachedPointDistanceSquared = 1.5f * 1.5f;

        public JobGiver_SquadPatrol()
        {
        }

        protected virtual int PatrolJobExpireInterval
        {
            get { return 120; }
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                Log.Error($"{pawn.Label} has is not part of a squad but is trying to patrol.");
                return null;
            }

            Zone_PatrolPath patrolZone = squadMember.PatrolTracker.PatrolZone as Zone_PatrolPath;
            if (patrolZone == null)
            {
                Log.Error($"{pawn.Label} has no assigned patrol zone but is trying to patrol.");
                return null;
            }

            if (!squadMember.PatrolTracker.HasPatrolPath)
            {
                Log.Error($"{pawn.Label} has no patrol path but is trying to patrol.");
                return null;
            }

            IntVec3 currentPoint = patrolZone.FindReachablePoint(pawn, pawn.Position);

            if (currentPoint.IsValid)
            {
                float distanceSquared = (pawn.Position - currentPoint).LengthHorizontalSquared;
                if (distanceSquared <= ReachedPointDistanceSquared)
                {
                    squadMember.PatrolTracker.MarkCurrentPointReached();
                }
            }

            IntVec3 startPoint;
            if (squadMember.PatrolTracker.HasReachedCurrentPoint())
            {
                startPoint = squadMember.PatrolTracker.AdvanceToNextPoint();
            }
            else
            {
                startPoint = currentPoint;
            }

            IntVec3 targetPoint = patrolZone.FindReachablePoint(pawn, startPoint);
            if (!targetPoint.IsValid)
            {
                Log.Error($"{pawn.Label} cannot find any valid patrol point");
                return null;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Goto, targetPoint);
            job.locomotionUrgency = LocomotionUrgency.Walk;
            job.expiryInterval = PatrolJobExpireInterval;
            job.checkOverrideOnExpire = true;
            return job;
        }
    }
}