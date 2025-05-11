using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{

    public class ThinkNode_ConditionalHasSquadLeader : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadLeader) && squadLeader.AssignedSquad.Leader != null;
        }
    }

}
