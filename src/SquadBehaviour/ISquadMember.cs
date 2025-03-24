using Verse;

namespace SquadBehaviour
{
    public interface ISquadMember
    {
        IntVec3 DefendPoint { get; }
        bool HasDefendPoint { get; }

        Pawn Pawn { get; }

        SquadMemberState CurrentState { get; }
        SquadDutyDef CurrentStance { get; set; }

        Zone_PatrolPath AssignedPatrol { get; set; }

        Squad AssignedSquad { get; set; }


        void SetSquadLeader(Pawn squadLeader);
        void SetDefendPoint(IntVec3 targetPoint);
        void ClearDefendPoint();

        void Notify_SquadMemberAttacked();
        void Notify_SquadChanged();

        void SetCurrentMemberState(SquadMemberState newState);
        ISquadLeader SquadLeader { get; }
    }
}
