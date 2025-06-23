using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class Squad : IExposable, ILoadReferenceable, IRenameable
    {
        public int squadID = -1;
        public int uniqueID = -1;
        public string squadName = "Squad";
        public Pawn Leader;
        private Comp_PawnSquadLeader _SquadLeader;
        public Comp_PawnSquadLeader SquadLeader
        {
            get
            {
                if (_SquadLeader == null)
                {
                    if (Leader.TryGetSquadLeader(out Comp_PawnSquadLeader leader))
                    {
                        _SquadLeader = leader;
                    }
                }
                return _SquadLeader;
            }
        }

        public string RenamableLabel { get => squadName; set => squadName = value; }

        public string BaseLabel => "Squad";

        public string InspectLabel => "squad";

        public FormationDef FormationType = SquadDefOf.ColumnFormation;
        private FormationWorker formationWorker;
        public List<Pawn> Members = new List<Pawn>();
        public SquadHostility HostilityResponse = SquadHostility.None;
        public SquadMemberState SquadState = SquadMemberState.CalledToArms;
        public SquadDutyDef squadDuty = null;
        public float AggresionDistance = 10f;
        public float FollowDistance = 10f;
        public bool InFormation = true;
        public bool IsVisible = true;

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


        #region Target
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
        #endregion


        #region Orders
        public void IssueMemberOrder(Pawn member, SquadOrderDef duty, LocalTargetInfo target)
        {
            if (!Members.Contains(member) || !member.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                return;
            squadMember.IssueOrder(duty, target);
        }
        public void IssueSquadOrder(SquadOrderDef duty, LocalTargetInfo target, bool isGlobal = false)
        {
            foreach (var member in Members)
            {
                if (member.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                {
                    squadMember.IssueOrder(duty, target, isGlobal);
                }
            }
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
        public void SetSquadState(SquadMemberState squadMemberState)
        {
            SquadState = squadMemberState;

            foreach (var member in Members)
            {
                if (member.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                {
                    squadMember.SetCurrentMemberState(SquadState);
                }
            }
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
        #endregion

        #region Membership
        public void AddMember(Pawn pawn)
        {
            if (pawn.TryGetSquadLeader(out Comp_PawnSquadLeader pawnSquadLeader) && pawnSquadLeader.IsLeaderRoleActive)
            {
                Messages.Message($"Cannot add {pawnSquadLeader.SquadLeaderPawn.Label} to squad, since they are a squad leader.", MessageTypeDefOf.RejectInput);
                return;
            }

            if (pawn.TryGetSquadMember(out Comp_PawnSquadMember squadMember))
            {
                if (Members.Contains(pawn))
                {
                    return;
                }

                // Check if the pawn is an animal and apply conditions
                if (pawn.RaceProps.Animal)
                {
                    if (!squadMember.CanEverJoinSquadOf(this.SquadLeader))
                    {
                        Messages.Message($"Cannot add animal {pawn.Label} to squad. It is explicitly disallowed by its definition.", MessageTypeDefOf.RejectInput);
                        return;
                    }
                }

                if (pawn.RaceProps.IsMechanoid)
                {
                    if (!squadMember.CanEverJoinSquadOf(this.SquadLeader))
                    {
                        Messages.Message($"Cannot add mech {pawn.Label} to squad. Squad Leader must be mechanitor.", MessageTypeDefOf.RejectInput);
                        return;
                    }
                }


                if (squadMember.AssignedSquad != null)
                {
                    squadMember.LeaveSquad();
                }

                squadMember.SetSquadLeader(this.SquadLeader.SquadLeaderPawn);
                squadMember.AssignedSquad = this;
                squadMember.CurrentStance = this.squadDuty;

                if (this.SquadLeader != null)
                {
                    squadMember.SetCurrentMemberState(this.SquadState);
                    squadMember.CurrentStance = this.squadDuty;
                }

                Members.Add(pawn);
                Messages.Message($"Assigned {this.squadName} squad to {pawn.Label}", MessageTypeDefOf.PositiveEvent);
            }
        }
        public void RemoveMember(Pawn pawn)
        {
            if (Members.Contains(pawn) && pawn.TryGetSquadMember(out Comp_PawnSquadMember squadMember))
            {
                Members.Remove(pawn);
                squadMember.SetSquadLeader(null);
                squadMember.AssignedSquad = null;
                squadMember.CurrentStance = null;
            }
        }
        public bool TryMergeFrom(Squad OtherSquad)
        {
            if (OtherSquad == null || OtherSquad.Members == null || OtherSquad.Members.Count == 0 || OtherSquad.squadID == this.squadID)
            {
                return false;
            }


            foreach (var item in OtherSquad.Members.ToList())
            {
                AddMember(item);
            }

            return true;
        }
        public void DisbandSquad()
        {
            foreach (var item in Members.ToArray())
            {
                this.RemoveMember(item);
            }
            this.Members.Clear();
            this.SquadLeader.RemoveSquad(this.squadID);
        }
        #endregion


        public IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation)
        {
            if (!Members.Contains(pawn))
            {
                Log.Message("Tried to get formation position for {pawn} but they are not a member of the squad");
                return IntVec3.Invalid;
            }
            if (formationWorker != null)
            {
                return formationWorker.GetFormationPosition(pawn, Origin.ToVector3(), Members.IndexOf(pawn), OriginRotation, Members.Count);
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

    public class SquadAnimalExtension : DefModExtension
    {
        public bool canEverJoinSquad = false;

        public List<SquadSkillRequirements> skillRequirements;
        public List<TraitDef> traitsRequired;
        public List<HediffDef> hediffRequired;
        public List<GeneDef> genesRequired;

        public bool CanSquadLeaderCommand(Comp_PawnSquadLeader squadLeader)
        {
            if (!canEverJoinSquad)
            {
                return false;
            }

            if (!squadLeader.CanCommandAnimals)
            {
                return false;
            }

            if (skillRequirements.NullOrEmpty() && traitsRequired.NullOrEmpty() && hediffRequired.NullOrEmpty() && genesRequired.NullOrEmpty())
            {
                return true;
            }

            if (!skillRequirements.NullOrEmpty())
            {
                if (squadLeader.SquadLeaderPawn.skills == null) 
                    return false;
                foreach (SquadSkillRequirements req in skillRequirements)
                {
                    SkillRecord skill = squadLeader.SquadLeaderPawn.skills.GetSkill(req.skillDef);
                    if (skill == null || skill.Level < req.minLevel)
                    {
                        return false;
                    }
                }
            }

            if (!traitsRequired.NullOrEmpty())
            {
                if (squadLeader.SquadLeaderPawn.story == null || squadLeader.SquadLeaderPawn.story.traits == null) 
                    return false;
                foreach (TraitDef traitDef in traitsRequired)
                {
                    if (!squadLeader.SquadLeaderPawn.story.traits.HasTrait(traitDef))
                    {
                        return false;
                    }
                }
            }

            if (!hediffRequired.NullOrEmpty())
            {
                if (squadLeader.SquadLeaderPawn.health == null || squadLeader.SquadLeaderPawn.health.hediffSet == null) 
                    return false;
                foreach (HediffDef hediffDef in hediffRequired)
                {
                    if (!squadLeader.SquadLeaderPawn.health.hediffSet.HasHediff(hediffDef))
                    {
                        return false;
                    }
                }
            }

            if (!genesRequired.NullOrEmpty())
            {
                if (!ModLister.BiotechInstalled || squadLeader.SquadLeaderPawn.genes == null) 
                    return false;

                foreach (GeneDef geneDef in genesRequired)
                {
                    if (!squadLeader.SquadLeaderPawn.genes.HasActiveGene(geneDef))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class SquadSkillRequirements
    {
        public SkillDef skillDef;
        public int minLevel;
    }
}

