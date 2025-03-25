using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_SquadPatrol : ThinkNode_JobGiver
    {


        // How close does a pawn need to be to consider a point reached
        private const float ReachedPointDistanceSquared = 1.5f * 1.5f;

        public JobGiver_SquadPatrol()
        {

        }

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

            // Make sure the patrol tracker is using the current zone
            if (squadMember.PatrolTracker.PatrolZone != patrolZone)
            {
                squadMember.PatrolTracker.SetPatrolZone(patrolZone);
            }

            // Check if we are close enough to the current point to consider it reached
            IntVec3 currentPoint = squadMember.PatrolTracker.CurrentPoint;
            if (currentPoint.IsValid)
            {
                float distanceSquared = (pawn.Position - currentPoint).LengthHorizontalSquared;
                if (distanceSquared <= ReachedPointDistanceSquared)
                {
                    // Mark the current point as reached
                    squadMember.PatrolTracker.MarkCurrentPointReached();
                }
            }

            IntVec3 targetPoint;

            // If we've reached the current point, advance to the next
            if (squadMember.PatrolTracker.HasReachedCurrentPoint())
            {
                targetPoint = squadMember.PatrolTracker.AdvanceToNextPoint();
            }
            else
            {
                // Otherwise, continue to the current point
                targetPoint = currentPoint;

                // If the current point is invalid, try to get a valid next point
                if (!targetPoint.IsValid && squadMember.PatrolTracker.HasPatrolPath)
                {
                    targetPoint = squadMember.PatrolTracker.AdvanceToNextPoint();
                }
            }

            if (!targetPoint.IsValid)
            {
                Log.Message($"[{pawn.LabelShort}] Target point is invalid - returning null job");
                return null;
            }

            bool canReach = pawn.CanReach(targetPoint, PathEndMode.OnCell, Danger.Deadly);
            if (!canReach)
            {
                // If can't reach the target point, try initializing to closest point
                squadMember.PatrolTracker.InitializeToClosestPoint(pawn.Position);
                targetPoint = squadMember.PatrolTracker.CurrentPoint;

                canReach = pawn.CanReach(targetPoint, PathEndMode.OnCell, Danger.Deadly);
                if (!canReach)
                {
                    return null;
                }
            }

            Job job = JobMaker.MakeJob(JobDefOf.Goto, targetPoint);
            job.locomotionUrgency = LocomotionUrgency.Walk;
            job.expiryInterval = PatrolJobExpireInterval;
            job.checkOverrideOnExpire = true;
            return job;
        }
    }
}
