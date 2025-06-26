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
        List<Pawn> AllSquadsPawns { get; }
        FormationDef FormationType { get; }

        Dictionary<int, Squad> ActiveSquads { get; }

        ///squad creation
        bool AddSquad(int squadID, List<Pawn> startingMembers = null);
        bool RemoveSquad(int squadID);
        Squad GetFirstOrAddSquad();

        SquadMemberState SquadState { get; }

        bool ShowExtraOrders { get; set; }



        //squad management
        bool IsPartOfAnySquad(Pawn pawn, out Squad squad);

        /// <summary>
        /// Adds the pawn to the next available squad, if none are available one is created.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        bool AddToSquad(Pawn pawn);
        bool AddToSquad(Pawn pawn, int squadID);
        bool HasSquadByID(int squadID);
        Squad GetSquadByID(int squadID);

        bool RemoveFromSquad(Pawn pawn, bool kill = true, bool alsoDestroy = false);
        void SetAllState(SquadMemberState squadMemberState);
        void ToggleInFormation();
        void SetFormation(FormationDef formationType);
        void SetFollowDistance(float distance);
        void SetInFormation(bool inFormation);
        void IssueGlobalOrder(SquadOrderDef orderDef, LocalTargetInfo target);
        void IssueSquadOrder(Squad squad, SquadOrderDef orderDef, LocalTargetInfo target);
        void SetHositilityResponse(SquadHostility squadHostilityResponse);

        IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation);
        IntVec3 GetFormationPositionFor(Pawn pawn);
    }
}
