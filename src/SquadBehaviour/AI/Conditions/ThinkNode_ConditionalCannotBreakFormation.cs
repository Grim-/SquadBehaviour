using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalCannotBreakFormation : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return false;
            }

            if (squadMember.AssignedSquad == null)
            {
                return false;
            }

            return squadMember.AssignedSquad.IsHoldingFormation;
        }
    }
}