using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalShouldDoNothing : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || !pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) || squadMember.SquadLeader == null)
            {
                return false;
            }

            return squadMember.CurrentState == SquadMemberState.DoNothing;
        }
    }
}
