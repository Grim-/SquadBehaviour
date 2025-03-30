﻿using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace SquadBehaviour
{
    public class CompSquadLeader : ThingComp, ISquadLeader
    {
        private HashSet<Pawn> activeSquadMembers = new HashSet<Pawn>();
        private List<ISquadMember> _SquadMembers = new List<ISquadMember>();
        private float _FollowDistance = 5f;
        private FormationDef _FormationType = SquadDefOf.ColumnFormation;
        private bool InFormation = true;
        private bool ShowExtraOrders = true;
        private SquadHostility SquadHostilityResponse = SquadHostility.Aggressive;
        private Lord SquadLord = null;

        public Pawn SquadLeaderPawn => this.parent as Pawn;
        Lord ISquadLeader.SquadLord { get => this.SquadLord; set => this.SquadLord = value; }
        public IntVec3 LeaderPosition => SquadLeaderPawn.Position;
        public List<Pawn> SquadMembersPawns => activeSquadMembers.ToList();
        public List<ISquadMember> SquadMembers => _SquadMembers;
        public FormationDef FormationType => _FormationType;
        public float FollowDistance => _FollowDistance;
        // bool ISquadLeader.InFormation => this.InFormation;
        bool ISquadLeader.ShowExtraOrders { get => this.ShowExtraOrders; set => this.ShowExtraOrders = value; }
        public SquadHostility HostilityResponse => SquadHostilityResponse;

        public float AggresionDistance => FollowDistance;


        private SquadMemberState _SquadState = SquadMemberState.CalledToArms;
        public SquadMemberState SquadState => _SquadState;

        private Dictionary<int, Squad> _ActiveSquads = new Dictionary<int, Squad>();
        public Dictionary<int, Squad> ActiveSquads => _ActiveSquads;


        public void SetFormation(FormationDef formationType) => _FormationType = formationType;
        public void SetFollowDistance(float distance) => _FollowDistance = distance;
        public void SetInFormation(bool inFormation) => InFormation = inFormation;
        public void ToggleInFormation() => InFormation = !InFormation;
        public void SetHositilityResponse(SquadHostility squadHostilityResponse) => SquadHostilityResponse = squadHostilityResponse;

        public bool AddToSquad(Pawn pawn)
        {
            if (!activeSquadMembers.Contains(pawn))
            {
                activeSquadMembers.Add(pawn);
                if (pawn.IsPartOfSquad(out ISquadMember squadMember))
                {
                    _SquadMembers.Add(squadMember);
                }
                return true;
            }
            return false;
        }

        public bool RemoveFromSquad(Pawn pawn, bool kill = true, bool alsoDestroy = false)
        {
            if (activeSquadMembers.Remove(pawn))
            {
                if (pawn.IsPartOfSquad(out ISquadMember squadMember))
                {
                    _SquadMembers.Remove(squadMember);
                }

                if (kill && pawn.Spawned && !pawn.health.Dead)
                {
                    pawn.Kill(null);
                }

                if (alsoDestroy && !pawn.Destroyed)
                {
                    pawn.Destroy();
                }
                return true;
            }
            return false;
        }

        public void SetAllState(SquadMemberState squadMemberState)
        {
            foreach (var creature in activeSquadMembers)
            {

            }

            _SquadState = squadMemberState;
        }

        public bool IsPartOfSquad(Pawn pawn) => activeSquadMembers.Contains(pawn);

        public IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation)
        {
            foreach (var squad in _ActiveSquads.OrderBy(x => x.Key))
            {
                if (squad.Value.Members.Contains(pawn))
                {
                    return squad.Value.GetFormationPositionFor(pawn, Origin, OriginRotation);
                }
            }

            return IntVec3.Invalid;
        }


        public void IssueGlobalOrder(SquadOrderDef orderDef, LocalTargetInfo target)
        {
            foreach (var squad in ActiveSquads)
            {
                squad.Value.IssueSquadOrder(orderDef, target);
            }
        }

        public void IssueSquadOrder(Squad squad, SquadOrderDef orderDef, LocalTargetInfo target)
        {
            if (squad != null)
            {
                squad.IssueSquadOrder(orderDef, target);
            }
        }

        public IntVec3 GetFormationPositionFor(Pawn pawn) => GetFormationPositionFor(pawn, SquadLeaderPawn.Position, SquadLeaderPawn.Rotation);

        public bool AddSquad(int squadID, List<Pawn> startingMembers)
        {
            if (!_ActiveSquads.ContainsKey(squadID))
            {
                _ActiveSquads.Add(squadID, CreateSquad(squadID));
                return true;
            }

            return false;
        }

        public Squad GetOrAddSquad(int squadID)
        {
            if (_ActiveSquads.ContainsKey(squadID))
            {
                return _ActiveSquads[squadID];
            }

            return CreateSquad(squadID);
        }

        public bool RemoveSquad(int squadID)
        {
            if (_ActiveSquads.ContainsKey(squadID))
            {
                _ActiveSquads.Remove(squadID);
                return true;
            }
            return false;
        }


        public Squad CreateSquad(int squadID)
        {
            return new Squad(squadID, this.parent as Pawn, FormationType, SquadHostilityResponse);
        }


        public bool HasAnySquad()
        {
            return _ActiveSquads != null && _ActiveSquads.Count > 0;
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref activeSquadMembers, "activeSquadMembers", LookMode.Reference);
            Scribe_Collections.Look(ref _ActiveSquads, "activeSquads", LookMode.Value, LookMode.Deep);
            Scribe_Defs.Look(ref _FormationType, "formationType");
            Scribe_Values.Look(ref _FollowDistance, "followDistance", 5f);
            Scribe_Values.Look(ref InFormation, "inFormation", true);
            Scribe_Values.Look(ref SquadHostilityResponse, "SquadHostilityResponse");
        }
    }
}
