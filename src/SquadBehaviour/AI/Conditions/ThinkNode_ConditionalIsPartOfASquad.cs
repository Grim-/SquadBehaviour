using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsPartOfASquad : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            bool result = pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) && squadMember.AssignedSquad != null;
           // Log.Message($"IsPartOfSquad? {result}");
            return result;
        }
    }
}
