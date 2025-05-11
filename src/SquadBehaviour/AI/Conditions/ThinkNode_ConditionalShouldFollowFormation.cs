using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalShouldFollowFormation : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || !pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return false;
            }


            if (squadMember.SquadLeader == null || squadMember.SquadLeader.SquadLeaderPawn == null)
            {
                return false;
            }

            Pawn SquadLeader = squadMember.SquadLeader.SquadLeaderPawn;

            if (SquadLeader == null)
            {
                return false;
            }

            return squadMember.AssignedSquad.InFormation;
        }
    }
}
