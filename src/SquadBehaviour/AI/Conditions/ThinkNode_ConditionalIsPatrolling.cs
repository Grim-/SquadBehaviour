using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalHasPatrolZone : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return squadMember != null && squadMember.PatrolTracker.PatrolZone != null;
            }

            return false;
        }
    }
}
