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
        protected FormationDef _MetaFormationType = SquadDefOf.ColumnFormation;
        public FormationDef MetaFormationType => _MetaFormationType;
        #region Properties
        public Pawn Pawn => this.parent as Pawn;
        public Lord SquadLord { get => this._SquadLord; set => this._SquadLord = value; }
        public virtual IntVec3 LeaderPosition => Pawn.Position;
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

        public int cellsBetweenSquads = 5;
        public IntVec3 squadsOffset = new IntVec3(0, 0, -1);

        public bool CanEverBeLeader => this.Pawn.RaceProps.Humanlike && !this.Pawn.WorkTagIsDisabled(WorkTags.Violent);


        public Dictionary<int, bool> squadFoldouts = new Dictionary<int, bool>();
        public Dictionary<int, bool> settingsFoldouts = new Dictionary<int, bool>();

        public bool CanCommandAnimals(out string cantCommandReason)
        {
            cantCommandReason = "";
            if (Prefs.DevMode && DebugSettings.godMode)
            {
                return true;
            }
            var animalSkill = this.Pawn.skills.GetSkill(SkillDefOf.Animals);
            if (animalSkill == null || animalSkill.Level < 8)
            {
                cantCommandReason = "requires 8 animal skill to command animals";
                return false;
            }
              
            Log.Message($"CanCommandAnimals check for {Pawn.Label}: level={animalSkill.Level}, levelInt={animalSkill.levelInt}");
            return animalSkill.Level >= 8;
        }
        public bool CanCommandMechs => this.Pawn.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant);

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
            return new Squad(squadID, this.Pawn, FormationType, SquadHostilityResponse);
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

        public virtual bool TryGetSquadByID(int squadID, out Squad squad)
        {
            squad = null;
            if (HasSquadByID(squadID))
            {
                squad = _ActiveSquads[squadID];
                return true;
            }
            return false;
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

        public virtual void SetMetaFormation(FormationDef metaFormationType)
        {
            _MetaFormationType = metaFormationType;
        }

        public virtual IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation)
        {
            if (IsPartOfAnySquad(pawn, out Squad squad))
            {
                //var orderedSquads = _ActiveSquads.OrderBy(x => x.Key).ToList();
                //int squadIndex = orderedSquads.FindIndex(s => s.Value == squad);

                //if (squadIndex != -1)
                //{
                //    IntVec3 squadCenter = GetMetaFormationPosition(squadIndex, Origin, OriginRotation, orderedSquads.Count);
                //    return squad.GetFormationPositionFor(pawn, squadCenter, OriginRotation);
                //}
                return squad.GetFormationPositionFor(pawn, Origin + squadsOffset, OriginRotation);
            }

            return IntVec3.Invalid;
        }


        public virtual IntVec3 GetMetaFormationPosition(int squadIndex, IntVec3 origin, Rot4 originRotation, int totalSquads)
        {
            float totalWidth = (totalSquads - 1) * cellsBetweenSquads;
            float startOffset = -totalWidth / 2f;
            origin += squadsOffset;

            Vector3 offsetForSquad = new Vector3(startOffset + squadIndex * cellsBetweenSquads, 0, 0);

            return origin + new IntVec3(Mathf.RoundToInt(offsetForSquad.x), 0, Mathf.RoundToInt(offsetForSquad.z));
        }

        public virtual IntVec3 GetFormationPositionFor(Pawn pawn, bool userLeaderRotation = false)
        {
            Rot4 rot = userLeaderRotation ? Pawn.Rotation : Rot4.North;

            return GetFormationPositionFor(pawn, Pawn.Position, rot);
        }
        #endregion

        #region Order Management


        /// <summary>
        /// Issue an order to all squads and units
        /// </summary>
        /// <param name="duty"></param>
        /// <param name="target"></param>
        /// <param name="isGlobal"></param>
        public void IssueGlobalOrder(SquadOrderDef duty, LocalTargetInfo target, bool isGlobal = false)
        {
            foreach (var item in ActiveSquads)
            {
                foreach (var member in item.Value.Members)
                {
                    IssueOrder(member, duty, target, isGlobal);
                }
            }
        }


        /// <summary>
        /// Issue an order to all units in a squad
        /// </summary>
        /// <param name="squad"></param>
        /// <param name="duty"></param>
        /// <param name="target"></param>
        /// <param name="isGlobal"></param>
        public void IssueSquadOrder(Squad squad, SquadOrderDef duty, LocalTargetInfo target, bool isGlobal = false)
        {
            foreach (var member in squad.Members)
            {
                IssueOrder(member, duty, target, isGlobal);
            }
        }


        /// <summary>
        /// Issue an order to a specific unit.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="duty"></param>
        /// <param name="target"></param>
        /// <param name="isGlobal"></param>
        public void IssueOrder(Pawn pawn, SquadOrderDef duty, LocalTargetInfo target, bool isGlobal = false)
        {
            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                SquadOrderWorker squadOrderWorker = duty.CreateWorker(this, squadMember);
                if (squadOrderWorker.CanExecuteOrder(target))
                {
                    if (isGlobal)
                    {
                        squadOrderWorker.ExecuteOrderGlobal(target);
                    }
                    else
                    {
                        squadOrderWorker.ExecuteOrder(target);
                    }                 
                }
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
            Scribe_Values.Look(ref cellsBetweenSquads, "cellsBetweenSquads", 5);
            Scribe_Values.Look(ref InFormation, "inFormation", true);
            Scribe_Values.Look(ref IsLeaderRoleActive, "IsLeaderRoleActive", false);
            Scribe_Values.Look(ref SquadHostilityResponse, "SquadHostilityResponse");
        }
        #endregion
    }
}