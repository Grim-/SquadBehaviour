using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

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
        public SquadHostility HostilityResponse = SquadHostility.Defensive;
        public SquadMemberState SquadState = SquadMemberState.CalledToArms;
        //public SquadMemberAllowedAttacks SquadAttackState = SquadMemberAllowedAttacks.ALL;
        public SquadDutyDef squadDuty = null;
        public float AggresionDistance = 30f;
        public float FollowDistance = 10f;
        public bool InFormation = true;
        public bool IsVisible = true;
        public bool IsHoldingFormation = false;

        public float MaxFormationDistance = 3f;



        public IntVec3 SquadOffset = IntVec3.Zero;


        public float MaxAttackDistanceFor(Pawn pawn)
        {
            //if (pawn.CurrentEffectiveVerb != null)
            //{
            //    if (IsHoldingFormation && pawn.CurrentEffectiveVerb.IsMeleeAttack)
            //    {
            //        return MaxFormationDistance;
            //    }

            //    return pawn.CurrentEffectiveVerb.EffectiveRange;
            //}
            return AggresionDistance;
        }


        protected Thing SquadTarget = null;
        public Squad()
        {

        }

        public Squad(int squadID, Pawn leader, FormationDef formationType, SquadHostility hostility = SquadHostility.Defensive)
        {
            this.squadID = squadID;
            this.uniqueID = Find.UniqueIDsManager.GetNextThingID();
            this.squadName = NameGenerator.GenerateName(RulePackDefOf.NamerArtWeaponMelee);
            Leader = leader;
            SetFormation(formationType);
            HostilityResponse = hostility;
        }


        #region Target
        public Thing FindTargetForMember(Pawn member, float maxRange = -1f)
        {
            if (!Members.Contains(member))
            {
                Log.Warning($"Tried to find target for {member} but they are not a member of the squad");
                return null;
            }

            if (maxRange < 0)
            {
                maxRange = MaxAttackDistanceFor(member);
            }

            Thing foundTarget = null;

            // First priority: Check if squad has a valid cached target
            if (SquadTarget != null && SquadTarget.Spawned && !SquadTarget.Destroyed && IsValidTarget(member, SquadTarget))
            {
                float distToSquadTarget = member.Position.DistanceTo(SquadTarget.Position);
                if (distToSquadTarget <= maxRange)
                {
                    foundTarget = SquadTarget;
                    //if (Prefs.DevMode) Log.Message($"[Squad] {member.LabelShort} using squad target: {SquadTarget.LabelShort}");
                }
            }

            // Second priority: Check leader's target
            if (foundTarget == null && Leader != null && Leader.Spawned && !Leader.Dead &&
                Leader.mindState != null && Leader.mindState.enemyTarget != null)
            {
                Thing leaderTarget = Leader.mindState.enemyTarget;
                if (IsValidTarget(member, leaderTarget))
                {
                    float distToLeaderTarget = member.Position.DistanceTo(leaderTarget.Position);
                    if (distToLeaderTarget <= maxRange)
                    {
                        foundTarget = leaderTarget;
                        SquadTarget = leaderTarget; // Cache it
                       // if (Prefs.DevMode) Log.Message($"[Squad] {member.LabelShort} using leader's target: {leaderTarget.LabelShort}");
                    }
                }
            }

            // Third priority: Check member's own enemy target
            if (foundTarget == null && member.mindState != null && member.mindState.enemyTarget != null)
            {
                Thing memberTarget = member.mindState.enemyTarget;
                if (IsValidTarget(member, memberTarget))
                {
                    float distToMemberTarget = member.Position.DistanceTo(memberTarget.Position);
                    if (distToMemberTarget <= maxRange)
                    {
                        foundTarget = memberTarget;
                        SquadTarget = memberTarget; // Cache it
                        //if (Prefs.DevMode) Log.Message($"[Squad] {member.LabelShort} using own enemy target: {memberTarget.LabelShort}");
                    }
                }
            }

            // Fourth priority: Search for nearby enemies
            if (foundTarget == null)
            {
                // Use RimWorld's standard attack target finder
                Thing searchTarget = (Thing)AttackTargetFinder.BestAttackTarget(
                    member,
                    TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns |
                    TargetScanFlags.NeedReachableIfCantHitFromMyPos |
                    TargetScanFlags.NeedAutoTargetable |
                    TargetScanFlags.LOSBlockableByGas |
                    TargetScanFlags.NeedActiveThreat,
                    (Thing t) => IsValidTarget(member, t),
                    0f,
                    maxRange,
                    default(IntVec3),
                    float.MaxValue,
                    true
                );

                if (searchTarget != null)
                {
                    foundTarget = searchTarget;
                    SquadTarget = searchTarget; // Cache it
                    //if (Prefs.DevMode) Log.Message($"[Squad] {member.LabelShort} found new target: {searchTarget.LabelShort}");
                }
            }

            if (foundTarget == null && Prefs.DevMode)
            {
                //Log.Message($"[Squad] {member.LabelShort} could not find any valid target within range {maxRange}");
            }

            return foundTarget;
        }


        public bool HasSquadTarget()
        {
            return GetSquadTarget() != null;
        }



        public bool TryFindCastPositionFor(Pawn pawn, Thing target, Verb verb, IntVec3 defendPos, float maxRange, out IntVec3 dest)
        {
           return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = target,
                verb = verb,
                //10% less than verb can reach
                maxRangeFromTarget = verb.verbProps.range - (verb.verbProps.range > 0 ? verb.verbProps.range * 0.1f : 0),
                locus = defendPos,
                maxRangeFromLocus = maxRange,
                wantCoverFromTarget = (verb.verbProps.range > 7f)
            }, out dest);
        }

        public void ValidateSquadTarget()
        {
            if (SquadTarget != null && (!SquadTarget.Spawned || SquadTarget.Destroyed))
            {
                SquadTarget = null;
            }
        }

        public Thing GetSquadTarget(float maxRange = -1f)
        {
            ValidateSquadTarget();

            if (SquadTarget != null)
            {
                return SquadTarget;
            }

            foreach (var item in Members)
            {
                Thing validTarget = FindTargetForMember(item, maxRange);

                if (validTarget != null)
                {
                    SquadTarget = validTarget;
                    break;
                }
            }

            return SquadTarget;
        }


        public bool IsValidTarget(Pawn member, Thing target)
        {
            if (target == null)
            {
                return false;
            }

            if (!target.Spawned || target.DestroyedOrNull())
            {
                return false;
            }

            if (target is Pawn pawn && pawn.DeadOrDowned)
            {
                return false;
            }

            if (target.Faction == null || target.Faction == member.Faction)
            {
                return false;
            }

            if (HostilityResponse == SquadHostility.Aggressive)
            {
                return target.Faction != Leader.Faction;
            }

            return target.Faction.HostileTo(member.Faction);
        }

        public bool LeaderHasValidTarget()
        {
            if (Leader != null && Leader.Spawned && !Leader.Dead &&
                Leader.mindState != null &&
                Leader.mindState.enemyTarget != null &&
                Leader.mindState.enemyTarget.Spawned &&
                IsValidTarget(Leader, Leader.mindState.enemyTarget))
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
                Thing target = FindTargetForMember(member, AggresionDistance);
                if (IsValidTarget(member, target))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion


        #region Orders

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

        //public void ToggleAttackState(SquadMemberAllowedAttacks attack)
        //{
        //    this.SquadAttackState = attack;
        //    foreach (var member in Members)
        //    {
        //        if (member.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
        //        {
        //            squadMember.SetAttackMode(attack);
        //        }
        //    }
        //}

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

        public void SetIsHoldingFormation(bool shouldHoldPosition)
        {
            IsHoldingFormation = shouldHoldPosition;
        }
        #endregion

        #region Membership
        public void AddMember(Pawn pawn)
        {
            if (pawn.TryGetSquadLeader(out Comp_PawnSquadLeader pawnSquadLeader) && pawnSquadLeader.IsLeaderRoleActive)
            {
                Messages.Message($"Cannot add {pawnSquadLeader.Pawn.Label} to squad, since they are a squad leader.", MessageTypeDefOf.RejectInput);
                return;
            }

            if (pawn.TryGetSquadMember(out Comp_PawnSquadMember squadMember))
            {
                if (Members.Contains(pawn))
                {
                    return;
                }

                if (!squadMember.CanEverJoinSquadOf(this.SquadLeader, out string reason))
                {
                    Messages.Message(reason, MessageTypeDefOf.RejectInput);
                    return;
                }

                if (squadMember.AssignedSquad != null)
                {
                    squadMember.LeaveSquad();
                }

                squadMember.AssignToSquad(this);

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
                squadMember.UnAssignSquad();
            }
        }
        public bool ReorderMember(Pawn pawn, int newIndex)
        {
            if (!Members.Contains(pawn))
            {
                Log.Warning($"Tried to reorder {pawn} but they are not a member of the squad");
                return false;
            }

            if (newIndex < 0 || newIndex >= Members.Count)
            {
                Log.Warning($"Invalid index {newIndex} for squad with {Members.Count} members");
                return false;
            }

            int currentIndex = Members.IndexOf(pawn);
            if (currentIndex == newIndex)
            {
                return true;
            }

            Members.RemoveAt(currentIndex);
            Members.Insert(newIndex, pawn);
            return true;
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

            OtherSquad.DisbandSquad();

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


        public IntVec3 GetFormationPositionFor(Pawn pawn, IntVec3 Origin, Rot4 OriginRotation, IntVec2 unitSize = default(IntVec2))
        {
            if (!Members.Contains(pawn))
            {
                Log.Message("Tried to get formation position for {pawn} but they are not a member of the squad");
                return IntVec3.Invalid;
            }
            if (formationWorker != null)
            {
                return formationWorker.GetFormationPosition(pawn, (SquadOffset + Origin).ToVector3(), Members.IndexOf(pawn), OriginRotation, Members.Count);
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
            Scribe_Collections.Look(ref Members, "Members", LookMode.Reference);
            Scribe_References.Look(ref Leader, "Leader");
            Scribe_Defs.Look(ref squadDuty, "squadDuty");
            Scribe_Defs.Look(ref FormationType, "FormationType");
            Scribe_Values.Look(ref uniqueID, "uniqueID");
            Scribe_Values.Look(ref squadID, "squadID");
            Scribe_Values.Look(ref SquadOffset, "SquadOffset");
            Scribe_Values.Look(ref squadName, "squadName");
            Scribe_Values.Look(ref HostilityResponse, "HostilityResponse");
            Scribe_Values.Look(ref IsHoldingFormation, "ShouldHoldFormation");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (FormationType != null)
                {
                    SetFormation(FormationType);
                }
            }

        }
        public string GetUniqueLoadID()
        {
            return "Squad_" + this.uniqueID;
        }
    }

    public class SquadSkillRequirements
    {
        public SkillDef skillDef;
        public int minLevel;
    }
}

