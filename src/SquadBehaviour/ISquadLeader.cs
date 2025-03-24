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
        FormationUtils.FormationType FormationType { get; }


        Dictionary<int, Squad> ActiveSquads { get; }

        bool AddSquad(int squadID, List<Pawn> startingMembers = null);
        bool RemoveSquad(int squadID);

        SquadMemberState SquadState { get; }

        //float AggresionDistance { get; }
        //float FollowDistance { get; }
        //bool InFormation { get; }

        bool ShowExtraOrders { get; set; }
        //SquadHostility HostilityResponse { get; }
        void SetHositilityResponse(SquadHostility squadHostilityResponse);

        void SetFormation(FormationUtils.FormationType formationType);
        void SetFollowDistance(float distance);
        void SetInFormation(bool inFormation);


        void ExecuteSquadOrder(SquadOrderDef orderDef, LocalTargetInfo target);

        bool AddToSquad(Pawn pawn);
        bool RemoveFromSquad(Pawn pawn, bool kill = true, bool alsoDestroy = false);
        void SetAllState(SquadMemberState squadMemberState);
        void ToggleInFormation();
        bool IsPartOfSquad(Pawn pawn);


        IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation);
        IntVec3 GetFormationPositionFor(Pawn pawn);
    }
}
