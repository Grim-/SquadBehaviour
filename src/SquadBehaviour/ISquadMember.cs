using Verse;

namespace SquadBehaviour
{
    public interface ISquadMember
    {
        IntVec3 DefendPoint { get; }
        bool HasDefendPoint { get; }


        bool AbilitiesAllowed { get; set; }

        Pawn Pawn { get; }

        SquadMemberState CurrentState { get; }
        SquadDutyDef CurrentStance { get; set; }

        Zone_PatrolPath AssignedPatrol { get; set; }
        PatrolTracker PatrolTracker { get; }

        Squad AssignedSquad { get; set; }


        void SetSquadLeader(Pawn squadLeader);
        void SetDefendPoint(IntVec3 targetPoint);
        void ClearDefendPoint();
        void IssueOrder(SquadOrderDef orderDef, LocalTargetInfo target);
        void Notify_SquadMemberAttacked();
        void Notify_SquadChanged();
        void SetCurrentMemberState(SquadMemberState newState);

        string GetStatusReport();

        ISquadLeader SquadLeader { get; }
    }
}
