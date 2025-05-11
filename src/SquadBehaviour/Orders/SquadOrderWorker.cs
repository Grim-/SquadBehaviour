using Verse;

namespace SquadBehaviour
{
    public abstract class SquadOrderWorker
    {
        public Comp_PawnSquadLeader SquadLeader;
        public Comp_PawnSquadMember SquadMember;

        public SquadOrderSettings SquadOrderSettings;
        public abstract bool CanExecuteOrder(LocalTargetInfo Target);

        public abstract void ExecuteOrder(LocalTargetInfo Target);
    }
}
