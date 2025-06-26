using System.Collections.Generic;
using Verse;

namespace SquadBehaviour
{
    public class PatrolTracker : IExposable
    {
        private Pawn parentPawn;
        private Comp_PawnSquadMember squadMember => parentPawn.GetComp<Comp_PawnSquadMember>();
        private List<IntVec3> patrolPoints = new List<IntVec3>();
        private int currentIndex = 0;
        private bool isReversing = false;
        private PatrolMode patrolMode = PatrolMode.PingPong;
        private Zone_PatrolPath patrolZone;
        private bool reachedCurrentPoint = false;

        public PatrolMode PatrolMode => patrolMode;
        public bool HasPatrolPath => patrolPoints != null && patrolPoints.Count > 1;
        public Zone_PatrolPath PatrolZone => patrolZone;

        public PatrolTracker()
        {

        }

        public PatrolTracker(Pawn squadMember, PatrolMode mode = PatrolMode.PingPong)
        {
            patrolMode = mode;
            this.parentPawn = squadMember;
        }

        public void SetPatrolZone(Zone_PatrolPath zone)
        {
            patrolZone = zone;
            if (zone != null)
            {
                patrolPoints = new List<IntVec3>(zone.orderedCells);
                Reset();
            }
            else
            {
                ClearPatrol();
            }
        }

        public void UpdateFromZone()
        {
            if (patrolZone != null)
            {
                patrolPoints = new List<IntVec3>(patrolZone.orderedCells);
                if (currentIndex >= patrolPoints.Count)
                {
                    Reset();
                }
            }
        }

        public void SetPatrolPoints(IEnumerable<IntVec3> points)
        {
            patrolPoints = new List<IntVec3>(points);
            Reset();
        }

        public void ClearPatrol()
        {
            if (patrolZone != null)
            {
             
            }

            patrolPoints.Clear();
            patrolZone = null;
            Reset();
        }

        public void SetPatrolMode(PatrolMode mode)
        {
            patrolMode = mode;
        }

        public void Reset()
        {
            currentIndex = 0;
            isReversing = false;
            reachedCurrentPoint = false;
        }

        public IntVec3 CurrentPoint
        {
            get
            {
                if (patrolPoints.Count == 0 || currentIndex < 0 || currentIndex >= patrolPoints.Count)
                    return IntVec3.Invalid;
                return patrolPoints[currentIndex];
            }
        }

        public void MarkCurrentPointReached()
        {
            reachedCurrentPoint = true;
        }

        public bool HasReachedCurrentPoint()
        {
            return reachedCurrentPoint;
        }

        public IntVec3 AdvanceToNextPoint()
        {
            if (patrolPoints.Count <= 1)
                return IntVec3.Invalid;

            if (patrolZone != null && patrolZone.orderedCells.Count != patrolPoints.Count)
            {
                UpdateFromZone();
            }

            int nextIndex;
            switch (patrolMode)
            {
                case PatrolMode.Loop:
                    nextIndex = (currentIndex + 1) % patrolPoints.Count;
                    break;
                case PatrolMode.PingPong:
                    if (isReversing)
                    {
                        nextIndex = currentIndex - 1;
                        if (nextIndex < 0)
                        {
                            isReversing = false;
                            nextIndex = 1;
                        }
                    }
                    else
                    {
                        nextIndex = currentIndex + 1;
                        if (nextIndex >= patrolPoints.Count)
                        {
                            isReversing = true;
                            nextIndex = patrolPoints.Count - 2;
                        }
                    }
                    break;
                default:
                    nextIndex = (currentIndex + 1) % patrolPoints.Count;
                    break;
            }

            currentIndex = nextIndex;
            reachedCurrentPoint = false;
            return patrolPoints[nextIndex];
        }

        public IntVec3 GetClosestPoint(IntVec3 position)
        {
            if (patrolPoints.Count == 0)
                return IntVec3.Invalid;

            int closestIndex = 0;
            float closestDist = float.MaxValue;
            IntVec3 closestPos = IntVec3.Invalid;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                float dist = (position - patrolPoints[i]).LengthHorizontalSquared;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                    closestPos = patrolPoints[i];
                }
            }

            currentIndex = closestIndex;
            reachedCurrentPoint = false;
            return closestPos;
        }


        public void ExposeData()
        {
            Scribe_Collections.Look(ref patrolPoints, "patrolPoints", LookMode.Value);
            Scribe_Values.Look(ref currentIndex, "currentIndex", 0);
            Scribe_Values.Look(ref isReversing, "isReversing", false);
            Scribe_Values.Look(ref patrolMode, "patrolMode", PatrolMode.PingPong);
            Scribe_References.Look(ref patrolZone, "patrolZone");
            Scribe_Values.Look(ref reachedCurrentPoint, "reachedCurrentPoint", false);
        }
    }
}
