using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalCanCheckInterval : ThinkNode_Conditional
    {
        protected int LastCheckTick = 0;

        protected override bool Satisfied(Pawn pawn)
        {
            if (Current.Game.tickManager.TicksGame >= LastCheckTick + 150)
            {
                LastCheckTick = Current.Game.tickManager.TicksGame;
                return true;
            }

            return false;
        }
    }
}
