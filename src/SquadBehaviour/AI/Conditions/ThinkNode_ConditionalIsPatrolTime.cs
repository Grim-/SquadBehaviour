using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsPatrolTime : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return squadMember != null && squadMember.Pawn.GetTimeAssignment() == TimeAssignmentDefOf.Work;
            }

            return false;
        }
    }
}
