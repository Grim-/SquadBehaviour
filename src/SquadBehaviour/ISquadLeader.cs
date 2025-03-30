using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace SquadBehaviour
{
    public interface ISquadLeader
    {
        Pawn SquadLeaderPawn { get; }

        Lord SquadLord { get; set; }

        IntVec3 LeaderPosition { get; }
        List<Pawn> SquadMembersPawns { get; }
        List<ISquadMember> SquadMembers { get; }
        FormationDef FormationType { get; }


        Dictionary<int, Squad> ActiveSquads { get; }

        bool AddSquad(int squadID, List<Pawn> startingMembers = null);
        bool RemoveSquad(int squadID);

        SquadMemberState SquadState { get; }

        bool ShowExtraOrders { get; set; }
        void SetHositilityResponse(SquadHostility squadHostilityResponse);

        void SetFormation(FormationDef formationType);
        void SetFollowDistance(float distance);
        void SetInFormation(bool inFormation);

        bool AddToSquad(Pawn pawn);
        bool RemoveFromSquad(Pawn pawn, bool kill = true, bool alsoDestroy = false);
        void SetAllState(SquadMemberState squadMemberState);
        void ToggleInFormation();
        bool IsPartOfSquad(Pawn pawn);


        void IssueGlobalOrder(SquadOrderDef orderDef, LocalTargetInfo target);
        void IssueSquadOrder(Squad squad, SquadOrderDef orderDef, LocalTargetInfo target);

        IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation);
        IntVec3 GetFormationPositionFor(Pawn pawn);
    }
}
