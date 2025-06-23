using RimWorld;
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

    public class Comp_PawnSquadMember : ThingComp
    {
        protected Pawn squadLeaderPawn;
        protected SquadMemberState squadMemberState = SquadMemberState.CalledToArms;
        protected IntVec3 defendPoint = IntVec3.Invalid;

        public virtual IntVec3 DefendPoint => defendPoint;
        public bool HasDefendPoint => defendPoint != IntVec3.Invalid;
        public Pawn Pawn => this.parent as Pawn;
        public SquadMemberState CurrentState => squadMemberState;

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



        public bool IsAnimalCommandable
        {
            get
            {
                if (this.Pawn.RaceProps.Animal && this.Pawn.def.race.trainability != TrainabilityDefOf.None)
                {
                    return true;
                }

                return false;
            }
        }


        private Squad _AssignedSquad = null;
        public Squad AssignedSquad { get => _AssignedSquad; set => _AssignedSquad = value; }


        public SquadDutyDef _CurrentStance = null;
        public SquadDutyDef CurrentStance { get => _CurrentStance; set => _CurrentStance = value; }


        private PatrolTracker _PatrolTracker = null;
        public PatrolTracker PatrolTracker
        {
            get
            {
                if (_PatrolTracker == null)
                {
                    _PatrolTracker = new PatrolTracker(this);
                }

                return _PatrolTracker;
            }
        }


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

        public void IssueOrder(SquadOrderDef orderDef, LocalTargetInfo target, bool isGlobal = false)
        {
            SquadOrderWorker squadOrderWorker = orderDef.CreateWorker(SquadLeader, this);
            squadOrderWorker.IsGlobalOrder = isGlobal;
            if (squadOrderWorker.CanExecuteOrder(target))
            {
                squadOrderWorker.ExecuteOrder(target);
            }
        }


        public void ClearCurrentDuties()
        {
            this.CurrentStance = null;
            this.PatrolTracker.ClearPatrol();
        }


        public void CheckShouldDisobeyOrder()
        {

        }


        public virtual bool CanEverJoinSquadOf(Comp_PawnSquadLeader squadLeader)
        {
            Log.Message($"=== CanEverJoinSquadOf Debug ===");
            Log.Message($"Pawn: {this.Pawn.Label}");
            Log.Message($"Is Animal: {this.Pawn.RaceProps.Animal}");
            Log.Message($"Is Mechanoid: {this.Pawn.RaceProps.IsMechanoid}");

            if (this.Pawn.RaceProps.IsMechanoid)
            {
                bool canCommand = squadLeader.CanCommandMechs;
                Log.Message($"Mech check - Can command mechs: {canCommand}");
                return canCommand;
            }

            if (this.Pawn.RaceProps.Animal)
            {
                Log.Message($"Animal skill level: {squadLeader.SquadLeaderPawn.skills.GetSkill(SkillDefOf.Animals)?.levelInt ?? -1}");
                Log.Message($"CanCommandAnimals: {squadLeader.CanCommandAnimals}");

                if (!IsAnimalCommandable)
                {
                    Log.Message("Not commandable - returning false");
                    return false;
                }

                if (!squadLeader.CanCommandAnimals)
                {
                    Log.Message("Leader cannot command animals - returning false");
                    return false;
                }

                Log.Message("Leader CAN command animals - checking extension");
                SquadAnimalExtension animalExtension = this.Pawn.def.GetModExtension<SquadAnimalExtension>();
                if (animalExtension != null)
                {
                    bool canCommand = animalExtension.CanSquadLeaderCommand(squadLeader);
                    Log.Message($"Extension check result: {canCommand}");
                    return canCommand;
                }

                Log.Message("No extension - returning true");
                return true;
            }

            bool sameFaction = this.Pawn.Faction == squadLeader.SquadLeaderPawn.Faction;
            Log.Message($"Faction check - Same faction: {sameFaction}");
            return sameFaction;
        }

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);

            if (AssignedSquad != null)
            {
                Log.Message("Removed from squad due to being dead");
                AssignedSquad.RemoveMember(this.Pawn);
            }
        }

        public void SetSquadLeader(Pawn squadLeader)
        {
            squadLeaderPawn = squadLeader;
        }

        public void SetDefendPoint(IntVec3 targetPoint)
        {
            defendPoint = targetPoint;
        }

        public void ClearDefendPoint()
        {
            defendPoint = IntVec3.Invalid;
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
            if (AssignedSquad != null)
            {
                AssignedSquad.RemoveMember(Pawn);
                AssignedSquad = null;
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

            if (SquadLeader != null && SquadLeader.SquadLeaderPawn != null)
            {
                sb.Append($"Squad Leader - {SquadLeader.SquadLeaderPawn.Name}");
            }

            if (this._CurrentStance != null)
            {
                sb.Append($"Duty - {this._CurrentStance.label}");
            }

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
            Scribe_Values.Look(ref defendPoint, "defendPoint");
        }

    }
}
