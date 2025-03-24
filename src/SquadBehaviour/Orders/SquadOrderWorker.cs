using Verse;

namespace SquadBehaviour
{
    public abstract class SquadOrderWorker
    {
        public ISquadLeader SquadLeader;
        public ISquadMember SquadMember;

        public SquadOrderSettings SquadOrderSettings;
        public abstract bool CanExecuteOrder(LocalTargetInfo Target);

        public abstract void ExecuteOrder(LocalTargetInfo Target);
    }
}
