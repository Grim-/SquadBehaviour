using Verse;

namespace SquadBehaviour
{
    public class SquadOrder_DefendPoint : SquadOrderWorker
    {
        //public override bool IsSquadOrder => false;

        public override bool CanExecuteOrder(LocalTargetInfo Target)
        {
            if (Target.Cell != IntVec3.Invalid && Target.Cell.Walkable(SquadMember.Pawn.Map))
            {
                return true;
            }

            return false;
        }

        public override void ExecuteOrder(LocalTargetInfo Target)
        {
            SquadMember.SetDefendPoint(Target.Cell);
        }
    }
}
