using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsCalledToArms : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            Comp_PawnSquadMember squadMember = null;
            if (!pawn.IsPartOfSquad(out squadMember) || squadMember == null)
            {
                return false;
            }


            if (squadMember.SquadLeader == null)
            {
                return false;
            }

            bool result = squadMember.CurrentState == SquadMemberState.CalledToArms;
            //Log.Message($"Is called to arms {result}");
            return result;
        }
    }
}
