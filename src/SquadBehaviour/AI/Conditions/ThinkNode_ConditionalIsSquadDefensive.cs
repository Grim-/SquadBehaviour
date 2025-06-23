using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsSquadDefensive : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return squadMember.AssignedSquad.HostilityResponse == SquadHostility.Defensive;
            }
            return false;
        }
    }
}
