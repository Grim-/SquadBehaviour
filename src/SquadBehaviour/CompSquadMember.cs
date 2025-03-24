using Verse;

namespace SquadBehaviour
{
    public class CompSquadMember : ThingComp, ISquadMember
    {
        private Pawn referencedPawn;
        private SquadMemberState squadMemberState = SquadMemberState.CalledToArms;
        private SquadMemberState preDefendState = SquadMemberState.CalledToArms;
        private IntVec3 defendPoint = IntVec3.Invalid;

        public IntVec3 DefendPoint => defendPoint;
        public bool HasDefendPoint => defendPoint != IntVec3.Invalid;
        public Pawn Pawn => this.parent as Pawn;
        public SquadMemberState CurrentState => squadMemberState;

        private ISquadLeader _SquadLeader;
        public ISquadLeader SquadLeader
        {
            get
            {
                if (_SquadLeader == null)
                {
                    if (referencedPawn.TryGetSquadLeader(out ISquadLeader squadLeader))
                    {
                        _SquadLeader =  squadLeader;
                    }
                }
                return _SquadLeader;
            }
        }


        private Squad _AssignedSquad = null;
        public Squad AssignedSquad { get => _AssignedSquad; set => _AssignedSquad = value; }


        public SquadStanceDef _CurrentStance = null;
        SquadStanceDef ISquadMember.CurrentStance { get => _CurrentStance; set => _CurrentStance = value; }

        public void SetSquadLeader(Pawn squadLeader)
        {
            referencedPawn = squadLeader;
        }

        public void SetDefendPoint(IntVec3 targetPoint)
        {
            defendPoint = targetPoint;
            preDefendState = squadMemberState;
            SetCurrentMemberState(SquadMemberState.DefendPoint);
        }

        public void ClearDefendPoint()
        {
            defendPoint = IntVec3.Invalid;
            SetCurrentMemberState(preDefendState);
        }

        public void Notify_SquadMemberAttacked()
        {

        }

        public void Notify_SquadChanged()
        {

        }

        public void SetCurrentMemberState(SquadMemberState newState)
        {
            squadMemberState = newState;

            if (Pawn?.CurJob != null)
            {
                Pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
            }
        }

        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra();

            if (SquadLeader != null && SquadLeader.SquadLeaderPawn != null)
            {
                baseString += $"Squad Leader : {SquadLeader.SquadLeaderPawn.Name}";
            }

            return baseString;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref _AssignedSquad, "assignedSquad");
            Scribe_References.Look(ref referencedPawn, "referencedPawn");
            Scribe_Values.Look(ref squadMemberState, "squadMemberState");
            Scribe_Values.Look(ref preDefendState, "preDefendState");
            Scribe_Values.Look(ref defendPoint, "defendPoint");
        }
    }
}
