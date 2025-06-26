using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    // Check if enemies are nearby during patrol
    public class ThinkNode_ConditionalEnemiesNearPatrol : ThinkNode_Conditional
    {
        public int regions = 3;

        protected override bool Satisfied(Pawn pawn)
        {
            if (!pawn.Spawned)
                return false;

            return PawnUtility.EnemiesAreNearby(pawn, regions);
        }
    }
}
