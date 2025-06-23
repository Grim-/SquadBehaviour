using Verse;

namespace SquadBehaviour
{
    public abstract class SquadOrderWorker
    {
        public Comp_PawnSquadLeader SquadLeader;
        public Comp_PawnSquadMember SquadMember;
        public SquadOrderDef def;
        public bool IsGlobalOrder = false;

        public SquadOrderSettings SquadOrderSettings;
        public abstract bool CanExecuteOrder(LocalTargetInfo Target);

        public abstract void ExecuteOrder(LocalTargetInfo Target);

        public abstract void ExecuteOrderGlobal(LocalTargetInfo Target);
    }
}
