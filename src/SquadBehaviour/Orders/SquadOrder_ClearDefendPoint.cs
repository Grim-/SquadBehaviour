using Verse;

namespace SquadBehaviour
{
    public class SquadOrder_ClearDefendPoint : SquadOrderWorker
    {
        //public override bool IsSquadOrder => false;

        public override bool CanExecuteOrder(LocalTargetInfo Target)
        {
            if (SquadMember.HasDefendPoint)
            {
                return true;
            }

            return false;
        }

        public override void ExecuteOrder(LocalTargetInfo Target)
        {
            SquadMember.ClearDefendPoint();
        }
    }
}
