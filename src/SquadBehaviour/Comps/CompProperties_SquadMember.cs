using System.Collections.Generic;
using System.Text;
using Verse;

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
        protected Pawn referencedPawn;
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
                if (_SquadLeader == null)
                {
                    if (referencedPawn != null && referencedPawn.TryGetSquadLeader(out Comp_PawnSquadLeader squadLeader))
                    {
                        _SquadLeader = squadLeader;
                    }
                }
                return _SquadLeader;
            }
        }


        private Squad _AssignedSquad = null;
        public Squad AssignedSquad { get => _AssignedSquad; set => _AssignedSquad = value; }


        public SquadDutyDef _CurrentStance = null;
        public SquadDutyDef CurrentStance { get => _CurrentStance; set => _CurrentStance = value; }


        private Zone_PatrolPath _AssignedPatrol = null;
        public Zone_PatrolPath AssignedPatrol { get => _AssignedPatrol; set => _AssignedPatrol = value; }


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


        protected bool _CanUseAbilities = false;
        public bool AbilitiesAllowed
        {
            get => _CanUseAbilities;
            set => _CanUseAbilities = value;
        }

        public void IssueOrder(SquadOrderDef orderDef, LocalTargetInfo target)
        {
            SquadOrderWorker squadOrderWorker = orderDef.CreateWorker(SquadLeader, this);

            if (squadOrderWorker.CanExecuteOrder(target))
            {
                squadOrderWorker.ExecuteOrder(target);
            }
        }

        public void SetSquadLeader(Pawn squadLeader)
        {
            referencedPawn = squadLeader;
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (SquadLeader != null)
            {
                yield return new Gizmo_SquadMemberInfo(this);
            }

            yield return new Command_Action()
            {
                defaultLabel = $"Current State {this.CurrentState}",
                action = () =>
                {
                    List<FloatMenuOption> gridOptions = new List<FloatMenuOption>();
                    gridOptions.Add(new FloatMenuOption("Call To Arms", () =>
                    {
                        this.SetCurrentMemberState(SquadMemberState.CalledToArms);
                    }));

                    gridOptions.Add(new FloatMenuOption("At Ease", () =>
                    {
                        this.SetCurrentMemberState(SquadMemberState.AtEase);
                    }));

                    gridOptions.Add(new FloatMenuOption("Do Nothing", () =>
                    {
                        this.SetCurrentMemberState(SquadMemberState.DoNothing);
                    }));

                    Find.WindowStack.Add(new FloatMenu(gridOptions));
                }
            };
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
            Scribe_References.Look(ref referencedPawn, "referencedPawn");
            Scribe_Values.Look(ref squadMemberState, "squadMemberState");
            Scribe_Values.Look(ref defendPoint, "defendPoint");
        }

    }
}
