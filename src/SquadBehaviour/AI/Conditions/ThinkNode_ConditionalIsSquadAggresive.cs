using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsSquadAggresive : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) && squadMember != null)
            {
                return squadMember.AssignedSquad.HostilityResponse == SquadHostility.Aggressive;
            }
            return false;
        }
    }
}
