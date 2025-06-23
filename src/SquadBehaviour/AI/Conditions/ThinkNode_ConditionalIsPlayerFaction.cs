using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsPlayerFaction : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.Faction == RimWorld.Faction.OfPlayer;
        }
    }
}
