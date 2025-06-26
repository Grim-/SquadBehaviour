using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsAtFormationPosition : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) && squadMember != null)
            {
                return squadMember.SquadLeader.GetFormationPositionFor(pawn).DistanceTo(pawn.Position) < 1;
            }
            return false;
        }

    }
}
