using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsAtFormationPosition : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember != null)
            {
                IntVec3 targetCell = squadMember.SquadLeader.GetFormationPositionFor(pawn);
                bool result = pawn.Position.InHorDistOf(targetCell, 1);

               // Log.Message($"Is At Formation Position? {result}");
                return result;
            }
            return false;
        }

    }
}
