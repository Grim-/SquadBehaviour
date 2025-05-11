using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsAtDefendPoint : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) &&
                squadMember.DefendPoint != IntVec3.Invalid)
            {

                IntVec3 position = IntVec3.Invalid;
                if (squadMember.AssignedSquad.InFormation)
                {
                    position = squadMember.SquadLeader.GetFormationPositionFor(pawn, squadMember.DefendPoint, Rot4.North);
                }
                else
                {
                    position = squadMember.DefendPoint;
                }


                if (squadMember.Pawn.Position.InHorDistOf(position, 1))
                {
                    return true;
                }            
            }
            return false;
        }
    }
}
