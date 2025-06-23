using RimWorld;
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
        protected SquadHostility SquadHostilityResponse = SquadHostility.Defensive;
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


        public bool CanCommandAnimals
        {
            get
            {
                var animalSkill = this.SquadLeaderPawn.skills.GetSkill(SkillDefOf.Animals);
                if (animalSkill == null)
                    return false;

                Log.Message($"CanCommandAnimals check for {SquadLeaderPawn.Label}: level={animalSkill.Level}, levelInt={animalSkill.levelInt}");

                return animalSkill.Level >= 8;
            }
        }
        public bool CanCommandMechs => this.SquadLeaderPawn.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant);

        public bool IsLeaderRoleActive = false;

        public FormationDef FormationType => _FormationType;
        public float FollowDistance => _FollowDistance;
        public SquadHostility HostilityResponse => SquadHostilityResponse;
        public float AggresionDistance => FollowDistance;
        public SquadMemberState SquadState => _SquadState;
        public Dictionary<int, Squad> ActiveSquads => _ActiveSquads;
        #endregion

        #region Configuration Methods



        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            DisbandAllSquads();
            base.Notify_Killed(prevMap, dinfo);
        }

        public void SetSquadLeader(bool isActive)
        {
            this.IsLeaderRoleActive = isActive;

            if (!this.IsLeaderRoleActive)
            {
                if (ActiveSquads.Count > 0)
                {
                    DisbandAllSquads();
                }    
            }         
        }

        public virtual void SetFormation(FormationDef formationType)
        {
            _FormationType = formationType;

            foreach (var squad in ActiveSquads)
            {
                squad.Value.SetFormation(FormationType);
            }
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


        #region Squad Management

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

        public virtual void DisbandSquad(Squad squad)
        {
            if (ActiveSquads.ContainsKey(squad.squadID))
            {
                ActiveSquads[squad.squadID].DisbandSquad();
                RemoveSquad(squad.squadID);
            }
        }

        public virtual void DisbandAllSquads()
        {
            foreach (var squad in ActiveSquads.ToArray())
            {
                DisbandSquad(squad.Value);
            }
        }

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