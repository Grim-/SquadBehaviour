using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsSquadInFormation : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            if (!pawn.IsPartOfSquad(out ISquadMember squadLeader))
            {
                return false;
            }

            if (squadLeader.AssignedSquad == null)
            {
                return false;
            }


            bool result = squadLeader.AssignedSquad.InFormation;
            return result;
        }
    }
}
