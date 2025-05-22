using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace SquadBehaviour
{
    public class CompProperties_SquadLeader : CompProperties
    {
        public CompProperties_SquadLeader()
        {
            compClass = typeof(Comp_PawnSquadLeader);
        }
    }

    public class Comp_PawnSquadLeader : ThingComp
    {
        #region Fields
        protected float _FollowDistance = 5f;
        protected FormationDef _FormationType = SquadDefOf.ColumnFormation;
        protected bool InFormation = true;
        public bool ShowExtraOrders = false;
        protected SquadHostility SquadHostilityResponse = SquadHostility.Aggressive;
        protected Lord _SquadLord = null;
        protected SquadMemberState _SquadState = SquadMemberState.CalledToArms;
        protected Dictionary<int, Squad> _ActiveSquads = new Dictionary<int, Squad>();
        #endregion

        #region Properties
        public Pawn SquadLeaderPawn => this.parent as Pawn;
        public Lord SquadLord { get => this._SquadLord; set => this._SquadLord = value; }
        public virtual IntVec3 LeaderPosition => SquadLeaderPawn.Position;
        public virtual List<Pawn> AllSquadsPawns
        {
            get
            {
                List<Pawn> allPawns = new List<Pawn>();
                foreach (var item in _ActiveSquads.OrderBy(x => x.Key))
                {
                    foreach (var member in item.Value.Members)
                    {
                        allPawns.Add(member);
                    }
                }
                return allPawns;
            }
        }


        public bool IsLeaderRoleActive = false;

        public FormationDef FormationType => _FormationType;
        public float FollowDistance => _FollowDistance;
        public SquadHostility HostilityResponse => SquadHostilityResponse;
        public float AggresionDistance => FollowDistance;
        public SquadMemberState SquadState => _SquadState;
        public Dictionary<int, Squad> ActiveSquads => _ActiveSquads;
        #endregion

        #region Configuration Methods



        public void SetAsActiveSquadLeader()
        {
            this.IsLeaderRoleActive = true;
        }

        public void SetAsInactiveSquadLeader()
        {
            if (this.IsLeaderRoleActive)
            {
                foreach (var squad in ActiveSquads.ToArray())
                {
                    squad.Value.DisbandSquad();
                    RemoveSquad(squad.Key);
                }
            }

            this.IsLeaderRoleActive = false;
        }


        public virtual void SetFormation(FormationDef formationType)
        {
            _FormationType = formationType;

            foreach (var squad in ActiveSquads)
            {
                squad.Value.SetFormation(FormationType);
            }
        }

        public virtual void SetFollowDistance(float distance)
        {
            _FollowDistance = distance;

            foreach (var squad in ActiveSquads)
            {
                squad.Value.SetFollowDistance(distance);
            }
        }

        public virtual void SetInFormation(bool inFormation)
        {
            InFormation = inFormation;

            foreach (var squad in ActiveSquads)
            {
                squad.Value.SetInFormation(InFormation);
            }
        }

        public virtual void ToggleInFormation()
        {
            InFormation = !InFormation;

            foreach (var squad in ActiveSquads)
            {
                squad.Value.SetInFormation(InFormation);
            }
        }

        public virtual void SetHositilityResponse(SquadHostility squadHostilityResponse)
        {
            SquadHostilityResponse = squadHostilityResponse;
        }

        public virtual void SetAllState(SquadMemberState squadMemberState)
        {
            foreach (var creature in AllSquadsPawns)
            {
                if (creature.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                {
                    squadMember.SetCurrentMemberState(squadMemberState);
                }
            }

            _SquadState = squadMemberState;
        }
        #endregion

        #region Squad Member Management
        public virtual bool AddToSquad(Pawn pawn)
        {
            if (SquadLeaderPawn == null)
            {
                Log.Message($"CompSquadLeader Cannot add {pawn} to squad, SquadLeaderPawn is null.");
                return false;
            }

            if (IsPartOfAnySquad(pawn, out Squad squad))
            {
                squad.SquadLeader.RemoveFromSquad(pawn);
            }
            else
            {
                Squad newSquad = GetFirstOrAddSquad();

                return AddToSquad(pawn, newSquad.squadID);
            }
            return false;
        }

        public virtual bool AddToSquad(Pawn pawn, int squadID)
        {
            if (SquadLeaderPawn == null)
            {
                Log.Message($"CompSquadLeader Cannot add {pawn} to squad, SquadLeaderPawn is null.");
                return false;
            }

            if (!HasSquadByID(squadID))
            {
                //no squad of that ID
                return false;
            }

            //remove from any existing squad
            if (IsPartOfAnySquad(pawn, out Squad squad))
            {
                squad.RemoveMember(pawn);
            }

            Squad foundSquad = GetSquadByID(squadID);
       
            if (foundSquad != null)
            {
                foundSquad.Leader = this.SquadLeaderPawn;
                foundSquad.AddMember(pawn);
                return true;
            }

            return false;
        }

        public virtual bool RemoveFromSquad(Pawn pawn, bool kill = false, bool alsoDestroy = false)
        {
            if (IsPartOfAnySquad(pawn, out Squad squad))
            {
                if (squad.Members.Contains(pawn))
                {
                    squad.RemoveMember(pawn);
                }

                if (kill && pawn.Spawned && !pawn.health.Dead)
                {
                    pawn.Kill(null);
                }

                if (alsoDestroy)
                {
                    if (!pawn.Destroyed)
                    {
                        pawn.Destroy();
                    }
                }

                return true;
            }
            return false;
        }
        #endregion

        #region Squad Management
        public virtual bool HasSquadByID(int squadID)
        {
            return _ActiveSquads.ContainsKey(squadID);
        }

        public virtual Squad GetSquadByID(int squadID)
        {
            if (HasSquadByID(squadID))
            {
                return _ActiveSquads[squadID];
            }

            return null;
        }

        public virtual bool AddSquad(int squadID)
        {
            if (!_ActiveSquads.ContainsKey(squadID))
            {
                _ActiveSquads.Add(squadID, CreateSquad(squadID));
                return true;
            }

            return false;
        }

        public virtual Squad GetOrAddSquad(int squadID)
        {
            if (_ActiveSquads.ContainsKey(squadID))
            {
                return _ActiveSquads[squadID];
            }

            return CreateSquad(squadID);
        }

        public virtual bool RemoveSquad(int squadID)
        {
            if (_ActiveSquads.ContainsKey(squadID))
            {
                _ActiveSquads.Remove(squadID);
                return true;
            }
            return false;
        }

        public virtual Squad GetFirstOrAddSquad()
        {
            if (_ActiveSquads == null || _ActiveSquads.Count == 0)
            {
                Squad newSquad = CreateSquad(1);
                _ActiveSquads.Add(1, newSquad);
                return newSquad;
            }
            return _ActiveSquads.Values.First();
        }

        public virtual Squad CreateSquad(int squadID)
        {
            return new Squad(squadID, this.SquadLeaderPawn, FormationType, SquadHostilityResponse);
        }

        public virtual bool HasAnySquad()
        {
            return _ActiveSquads != null && _ActiveSquads.Count > 0;
        }
        #endregion

        #region Formation Management
        public virtual IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation)
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

        public virtual IntVec3 GetFormationPositionFor(Pawn pawn) => GetFormationPositionFor(pawn, SquadLeaderPawn.Position, SquadLeaderPawn.Rotation);
        #endregion

        #region Order Management
        public virtual void IssueGlobalOrder(SquadOrderDef orderDef, LocalTargetInfo target)
        {
            foreach (var squad in ActiveSquads)
            {
                squad.Value.IssueSquadOrder(orderDef, target);
            }
        }

        public virtual void IssueSquadOrder(Squad squad, SquadOrderDef orderDef, LocalTargetInfo target)
        {
            if (squad != null)
            {
                squad.IssueSquadOrder(orderDef, target);
            }
        }
        #endregion

        #region Utility Methods
        public bool IsPartOfAnySquad(Pawn pawn, out Squad squad)
        {
            squad = null;

            foreach (var item in _ActiveSquads)
            {
                if (item.Value.Members.Contains(pawn))
                {
                    squad = item.Value;
                    return true;
                }
            }

            return false;
        }
        #endregion
        //public override IEnumerable<Gizmo> CompGetGizmosExtra()
        //{
        //    foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        //    {
        //        yield return gizmo;
        //    }

        //    if (IsLeaderRoleActive)
        //    {
        //        yield return new Gizmo_SquadLeader(this);
        //    }
        //}
        #region Overrides
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref _ActiveSquads, "activeSquads", LookMode.Value, LookMode.Deep);
            Scribe_Defs.Look(ref _FormationType, "formationType");
            Scribe_Values.Look(ref _FollowDistance, "followDistance", 5f);
            Scribe_Values.Look(ref InFormation, "inFormation", true);
            Scribe_Values.Look(ref IsLeaderRoleActive, "IsLeaderRoleActive", false);
            Scribe_Values.Look(ref SquadHostilityResponse, "SquadHostilityResponse");
        }
        #endregion
    }
}