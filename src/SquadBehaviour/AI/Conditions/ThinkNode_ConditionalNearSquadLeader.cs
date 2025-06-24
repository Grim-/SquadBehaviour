using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalNearSquadLeader : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadLeader) && squadLeader != null && squadLeader.SquadLeader != null && squadLeader.SquadLeader.Pawn != null && squadLeader.AssignedSquad != null)
            {
                return pawn.Position.DistanceTo(squadLeader.SquadLeader.Pawn.Position) <= squadLeader.AssignedSquad.FollowDistance;
            }
            return false;
        }
    }
}
