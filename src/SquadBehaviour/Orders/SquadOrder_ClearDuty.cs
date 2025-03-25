using Verse;

namespace SquadBehaviour
{
    public class SquadOrder_ClearDuty : SquadOrderWorker
    {
        //public override bool IsSquadOrder => false;

        public override bool CanExecuteOrder(LocalTargetInfo Target)
        {
            return true;
        }

        public override void ExecuteOrder(LocalTargetInfo Target)
        {
            SquadMember.CurrentStance = null;
        }
    }
}
