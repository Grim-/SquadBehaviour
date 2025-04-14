using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class Squad : IExposable, ILoadReferenceable
    {
        public int squadID = -1;
        public int uniqueID = -1;
        public string squadName = "Squad";
        public Pawn Leader;
        private ISquadLeader _SquadLeader;
        public ISquadLeader SquadLeader
        {
            get
            {
                if (_SquadLeader == null)
                {
                    if (Leader.TryGetSquadLeader(out ISquadLeader leader))
                    {
                        _SquadLeader = leader;
                    }
                }
                return _SquadLeader;
            }
        }
        public FormationDef FormationType = SquadDefOf.ColumnFormation;
        private FormationWorker formationWorker;
        public List<Pawn> Members = new List<Pawn>();
        public SquadHostility HostilityResponse = SquadHostility.Defensive;
        public SquadDutyDef squadDuty;
        public float AggresionDistance = 10f;
        public float FollowDistance = 10f;
        public bool InFormation = true;
        public Squad()
        {
        }
        public Squad(int squadID, Pawn leader, FormationDef formationType, SquadHostility hostility)
        {
            this.squadID = squadID;
            this.uniqueID = Find.UniqueIDsManager.GetNextThingID();
            Leader = leader;
            SetFormation(formationType);
            HostilityResponse = hostility;
        }

        public Thing FindTargetForMember(Pawn member)
        {
            if (!Members.Contains(member))
            {
                Log.Warning($"Tried to find target for {member} but they are not a member of the squad");
                return null;
            }

            if (Leader != null && Leader.Spawned && !Leader.Dead && Leader.mindState != null &&
                Leader.mindState.enemyTarget != null && IsValidTarget(member, Leader.mindState.enemyTarget))
            {
                return Leader.mindState.enemyTarget;
            }

            if (member.Spawned &&
                PawnUtility.EnemiesAreNearby(member, 9, true, Mathf.RoundToInt(AggresionDistance)))
            {
                return AbilityUtility.FindBestAbilityTarget(member);
            }

            return AbilityUtility.FindBestAbilityTarget(member);
        }

        public bool IsValidTarget(Pawn member, Thing target)
        {
            return target != null && target.Spawned && !target.Destroyed &&
                   AbilityUtility.CanUseAbilitiesOnTarget(member, target);
        }

        public bool LeaderHasValidTarget()
        {
            if (Leader != null &&
                Leader.Spawned &&
                !Leader.Dead &&
                Leader.mindState != null &&
                Leader.mindState.enemyTarget != null &&
                Leader.mindState.enemyTarget.Spawned)
            {
                return true;
            }

            return false;
        }

        public bool HasEnemiesNearby()
        {
            if (LeaderHasValidTarget())
            {
                return true;
            }

            foreach (var member in Members)
            {
                if (member.Spawned &&
                    PawnUtility.EnemiesAreNearby(member, 9, true, Mathf.RoundToInt(AggresionDistance)))
                {
                    return true;
                }
            }

            return false;
        }

        public void IssueSquadOrder(SquadOrderDef duty, LocalTargetInfo target)
        {
            foreach (var member in Members)
            {
                if (member.IsPartOfSquad(out ISquadMember squadMember))
                {
                    squadMember.IssueOrder(duty, target);
                }
            }
        }
        public void IssueMemberOrder(Pawn member, SquadOrderDef duty, LocalTargetInfo target)
        {
            if (!Members.Contains(member) || !member.IsPartOfSquad(out ISquadMember squadMember))
                return;
            squadMember.IssueOrder(duty, target);
        }
        public void SetHositilityResponse(SquadHostility squadHostilityResponse)
        {
            HostilityResponse = squadHostilityResponse;
        }
        public void SetFormation(FormationDef formationType)
        {
            FormationType = formationType;
            formationWorker = formationType.CreateWorker();
        }
        public void SetSquadDuty(SquadDutyDef squadDuty)
        {
            this.squadDuty = squadDuty;
        }
        public void SetFollowDistance(float distance)
        {
            FollowDistance = distance;
        }
        public void SetInFormation(bool inFormation)
        {
            InFormation = inFormation;
        }
        public void AddMember(Pawn pawn)
        {
            if (!Members.Contains(pawn))
            {
                Members.Add(pawn);
                if (pawn.TryGetSquadMember(out ISquadMember squadMember))
                {
                    squadMember.AssignedSquad = this;
                    squadMember.SetSquadLeader(this.SquadLeader.SquadLeaderPawn);
                }
            }
        }
        public void RemoveMember(Pawn pawn)
        {
            if (Members.Contains(pawn))
            {
                Members.Remove(pawn);
                if (pawn.TryGetSquadMember(out ISquadMember squadMember))
                {
                    squadMember.SetSquadLeader(null);
                    squadMember.AssignedSquad = null;
                }
            }
        }
        public IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation)
        {
            if (!Members.Contains(pawn))
            {
                Log.Message("Tried to get formation position for {pawn} but they are not a member of the squad");
                return IntVec3.Invalid;
            }
            if (formationWorker != null)
            {
                return formationWorker.GetFormationPosition(Origin.ToVector3(), Members.IndexOf(pawn), OriginRotation, Members.Count);
            }
            else
            {
                return FormationUtils.GetFormationPosition(
                    FormationUtils.FormationType.Column,
                    Origin.ToVector3(),
                    OriginRotation,
                    Members.IndexOf(pawn),
                    Members.Count);
            }
        }
        public void ExposeData()
        {
            Scribe_References.Look(ref Leader, "Leader");
            Scribe_Defs.Look(ref squadDuty, "squadDuty");
            Scribe_Collections.Look(ref Members, "Members", LookMode.Reference);
            Scribe_Values.Look(ref uniqueID, "uniqueID");
            Scribe_Defs.Look(ref FormationType, "FormationType");
            Scribe_Values.Look(ref squadID, "squadID");
            Scribe_Values.Look(ref HostilityResponse, "HostilityResponse");
        }
        public string GetUniqueLoadID()
        {
            return "Squad_" + this.uniqueID;
        }
    }
}
