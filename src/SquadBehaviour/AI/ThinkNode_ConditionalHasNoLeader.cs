using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalHasNoLeader : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn != null && pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember.SquadLeader == null;
        }
    }
}
