using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_MoveToDefendPoint : JobGiver_ForcedGoto
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) &&
                squadMember.DefendPoint != IntVec3.Invalid)
            {
                if (!pawn.CanReach(squadMember.DefendPoint, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                {
                    return null;
                }

                IntVec3 position = IntVec3.Invalid;
                if (squadMember.AssignedSquad.InFormation)
                {
                    position = squadMember.SquadLeader.GetFormationPositionFor(pawn, squadMember.DefendPoint, Rot4.North);
                }
                else
                {
                    position = squadMember.DefendPoint;
                }

                Job job = JobMaker.MakeJob(JobDefOf.Goto, position);
                job.locomotionUrgency = LocomotionUrgency.Sprint;
                return job;
            }
            return null;
        }
    }
}
