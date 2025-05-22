using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsDefendingPoint : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) && squadMember.CurrentStance == SquadDefOf.DefendPoint && squadMember.DefendPoint.IsValid)
            {
                return true;
            }
            return false;
        }
    }
}
