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
                return squadMember != null && squadMember.AssignedPatrol != null;
            }

            return false;
        }
    }
}
