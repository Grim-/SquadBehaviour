using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace SquadBehaviour
{
    public class CompProperties_SquadMember : CompProperties
    {
        public CompProperties_SquadMember()
        {
            compClass = typeof(Comp_PawnSquadMember);
        }
    }


    [Flags]
    public enum SquadMemberAllowedAttacks
    {
        NONE = 0,
        MELEE = 1,
        RANGED = 2,
        ALL = MELEE | RANGED
    }

    public class Comp_PawnSquadMember : ThingComp
    {
        protected Pawn squadLeaderPawn;
        protected SquadMemberState squadMemberState = SquadMemberState.CalledToArms;
        protected IntVec3 defendPoint = IntVec3.Invalid;

        public virtual IntVec3 DefendPoint => defendPoint;
        public bool HasDefendPoint => defendPoint != IntVec3.Invalid;
        public Pawn Pawn => this.parent as Pawn;
        public SquadMemberState CurrentState => squadMemberState;

        protected SquadMemberAllowedAttacks _AttackMode = SquadMemberAllowedAttacks.ALL;

        public SquadMemberAllowedAttacks AttackModes => _AttackMode;

        private Comp_PawnSquadLeader _SquadLeader;
        public virtual Comp_PawnSquadLeader SquadLeader
        {
            get
            {
                if (squadLeaderPawn != null && squadLeaderPawn.TryGetSquadLeader(out Comp_PawnSquadLeader squadLeader))
                {
                    return squadLeader;
                }

                return null;
            }
        }


        public bool CanMeleeAttack => AttackModes == SquadMemberAllowedAttacks.ALL || AttackModes == SquadMemberAllowedAttacks.MELEE;
        public bool CanRangedAttack => AttackModes == SquadMemberAllowedAttacks.ALL || AttackModes == SquadMemberAllowedAttacks.RANGED;


        private Squad _AssignedSquad = null;
        public Squad AssignedSquad { get => _AssignedSquad; }


        public SquadDutyDef _CurrentStance = null;
        public SquadDutyDef CurrentStance { get => _CurrentStance; set => _CurrentStance = value; }


        private PatrolTracker _PatrolTracker = null;
        public PatrolTracker PatrolTracker
        {
            get
            {
                if (_PatrolTracker == null)
                {
                    _PatrolTracker = new PatrolTracker(this.Pawn);
                }

                return _PatrolTracker;
            }
        }


        public IntVec3 CustomFormationOffset = IntVec3.Invalid;


        protected bool _CanUseAbilities = true;
        public bool AbilitiesAllowed
        {
            get => _CanUseAbilities;
            set => _CanUseAbilities = value;
        }


        protected bool _IsDisobeyingOrders = false;
        public bool IsDisobeyingOrders
        {
            get => _IsDisobeyingOrders;
            set => _IsDisobeyingOrders = value;
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (this.Pawn.abilities == null)
            {
                this.Pawn.abilities = new Pawn_AbilityTracker(Pawn);
            }
        }

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);

            this.ClearCurrentDuties();
            if (_AssignedSquad != null)
            {
                _AssignedSquad.RemoveMember(this.Pawn);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            this.ClearCurrentDuties();
            if (_AssignedSquad != null)
            {
                _AssignedSquad.RemoveMember(this.Pawn);
            }
            base.PostDeSpawn(map);
        }
        public void ClearCurrentDuties()
        {
            this.CurrentStance = null;
            this.PatrolTracker.ClearPatrol();
        }


        public void CheckShouldDisobeyOrder()
        {

        }


        public void SetAttackMode(SquadMemberAllowedAttacks allowedAttacks)
        {
            this._AttackMode = allowedAttacks;
        }


        public virtual bool CanEverJoinSquadOf(Comp_PawnSquadLeader squadLeader, out string reason)
        {
            reason = "";

            if (Prefs.DevMode && DebugSettings.godMode)
            {
                return true;
            }

            SquadMemberExtension extension = this.Pawn.def.GetModExtension<SquadMemberExtension>();
            if (extension != null)
            {
                return extension.CanJoinSquads(this.Pawn, squadLeader, out reason);
            }

            if (this.Pawn.RaceProps.IsMechanoid && !squadLeader.CanCommandMechs)
            {
                reason = "Cannot command mechs";
                return false;
            }

            if (this.Pawn.RaceProps.Animal)
            {
                if (this.Pawn.RaceProps.trainability == TrainabilityDefOf.None)
                {
                    reason = "Not trainable";
                    return false;
                }

                if (!squadLeader.CanCommandAnimals(out string cantCommandReason))
                {
                    reason = cantCommandReason;
                    return false;
                }
            }

            if (this.Pawn.Faction != squadLeader.Pawn.Faction)
            {
                reason = "Different faction";
                return false;
            }

            return true;
        }



        public void SetSquadLeader(Pawn squadLeader)
        {
            squadLeaderPawn = squadLeader;
        }


        public void AssignToSquad(Squad squad)
        {
            _AssignedSquad = squad;
            SetSquadLeader(_AssignedSquad.SquadLeader.Pawn);
            CurrentStance = squad.squadDuty;

            if (this.Pawn.abilities == null)
            {
                this.Pawn.abilities = new Pawn_AbilityTracker(Pawn);
            }
        }

        public void UnAssignSquad()
        {
            if (_AssignedSquad != null)
            {
                _AssignedSquad = null;
                SetSquadLeader(null);
                CurrentStance = null;
            }
        }

        public void SetDefendPoint(IntVec3 targetPoint)
        {
            defendPoint = targetPoint;
        }


        public void ClearDefendPoint()
        {
            defendPoint = IntVec3.Invalid;
        }


        public void StartPatrolling(Zone_PatrolPath patrolPath)
        {
            this.CurrentStance = SquadDefOf.PatrolArea;
            this.PatrolTracker.SetPatrolZone(patrolPath);
        }

        public void Notify_SquadMemberAttacked()
        {

        }

        public void Notify_SquadChanged()
        {

        }
        public void Notify_OnPawnDeath()
        {

        }


        public void LeaveSquad()
        {
            if (_AssignedSquad != null)
            {
                _AssignedSquad.RemoveMember(Pawn);
                _AssignedSquad = null;
                squadLeaderPawn = null;
            }
        }


        public void SetCurrentMemberState(SquadMemberState newState)
        {
            squadMemberState = newState;

            if (Pawn?.CurJob != null)
            {
                Pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
            }
        }

        public string GetStatusReport()
        {
            StringBuilder sb = new StringBuilder();

            if (SquadLeader != null && SquadLeader.Pawn != null)
            {
                sb.Append($"Squad Leader - {SquadLeader.Pawn.Name}");
            }



            if (this.Pawn.CurJob != null)
            {
                sb.Append($"Current Job - {this.Pawn.CurJob}");
            }

            if (this._CurrentStance != null)
            {
                sb.Append($"Duty - {this._CurrentStance.label}");
            }

            sb.Append($"AttackMode - {this.AttackModes}");
            return sb.ToString();
        }


        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra();
            return baseString + GetStatusReport();
        }
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref _AssignedSquad, "assignedSquad");
            Scribe_References.Look(ref squadLeaderPawn, "referencedPawn");
            Scribe_Values.Look(ref squadMemberState, "squadMemberState");
            Scribe_Values.Look(ref CustomFormationOffset, "CustomFormationOffset", IntVec3.Invalid);
            Scribe_Values.Look(ref defendPoint, "defendPoint");
            Scribe_Values.Look(ref _AttackMode, "attackModes", SquadMemberAllowedAttacks.ALL);


            Scribe_Defs.Look(ref _CurrentStance, "currentStance");
            Scribe_Deep.Look(ref _PatrolTracker, "patrolTracker", this.Pawn, PatrolMode.Loop);
            Scribe_Values.Look(ref _CanUseAbilities, "canUseAbilities", true);
            Scribe_Values.Look(ref _IsDisobeyingOrders, "isDisobeyingOrders", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_PatrolTracker == null)
                {
                    _PatrolTracker = new PatrolTracker(this.Pawn);
                }

                if (_AssignedSquad != null)
                {
                    if (squadLeaderPawn == null && _AssignedSquad.SquadLeader != null)
                    {
                        squadLeaderPawn = _AssignedSquad.SquadLeader.Pawn;
                    }

                    if (_CurrentStance == null && _AssignedSquad.squadDuty != null)
                    {
                        _CurrentStance = _AssignedSquad.squadDuty;
                    }

                    if (this.Pawn.abilities == null)
                    {
                        this.Pawn.abilities = new Pawn_AbilityTracker(Pawn);
                    }
                }
            }
        }

    }
}
