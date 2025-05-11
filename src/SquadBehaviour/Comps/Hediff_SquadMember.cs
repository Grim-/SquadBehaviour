//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;
//using Verse;

//namespace SquadBehaviour
//{




//    public class Hediff_SquadMember : HediffWithComps
//    {
//        protected Pawn referencedPawn;
//        protected SquadMemberState squadMemberState = SquadMemberState.CalledToArms;
//        protected IntVec3 defendPoint = IntVec3.Invalid;

//        public virtual IntVec3 DefendPoint => defendPoint;
//        public bool HasDefendPoint => defendPoint != IntVec3.Invalid;
//        public Pawn Pawn => this.pawn;
//        public SquadMemberState CurrentState => squadMemberState;

//        private ISquadLeader _SquadLeader;
//        public virtual ISquadLeader SquadLeader
//        {
//            get
//            {
//                if (_SquadLeader == null)
//                {
//                    if (referencedPawn.TryGetSquadLeader(out Comp_PawnSquadLeader squadLeader))
//                    {
//                        _SquadLeader = squadLeader;
//                    }
//                }
//                return _SquadLeader;
//            }
//        }


//        private Squad _AssignedSquad = null;
//        public Squad AssignedSquad { get => _AssignedSquad; set => _AssignedSquad = value; }


//        private SquadDutyDef _CurrentStance = null;
//        SquadDutyDef ISquadMember.CurrentStance { get => _CurrentStance; set => _CurrentStance = value; }


//        private Zone_PatrolPath _AssignedPatrol = null;
//        Zone_PatrolPath ISquadMember.AssignedPatrol { get => _AssignedPatrol; set => _AssignedPatrol = value; }


//        private PatrolTracker _PatrolTracker = null;
//        public PatrolTracker PatrolTracker
//        {
//            get
//            {
//                if (_PatrolTracker == null)
//                {
//                    _PatrolTracker = new PatrolTracker(this);
//                }

//                return _PatrolTracker;
//            }
//        }


//        protected bool _CanUseAbilities = false;
//        public bool AbilitiesAllowed
//        {
//            get => _CanUseAbilities;
//            set => _CanUseAbilities = value;
//        }

//        public void IssueOrder(SquadOrderDef orderDef, LocalTargetInfo target)
//        {
//            SquadOrderWorker squadOrderWorker = orderDef.CreateWorker(SquadLeader, this);

//            if (squadOrderWorker.CanExecuteOrder(target))
//            {
//                squadOrderWorker.ExecuteOrder(target);
//            }
//        }

//        public void SetSquadLeader(Pawn squadLeader)
//        {
//            referencedPawn = squadLeader;
//        }

//        public void SetDefendPoint(IntVec3 targetPoint)
//        {
//            defendPoint = targetPoint;
//        }

//        public void ClearDefendPoint()
//        {
//            defendPoint = IntVec3.Invalid;
//        }

//        public void Notify_SquadMemberAttacked()
//        {

//        }

//        public void Notify_SquadChanged()
//        {

//        }

//        public void SetCurrentMemberState(SquadMemberState newState)
//        {
//            squadMemberState = newState;

//            if (Pawn?.CurJob != null)
//            {
//                Pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
//            }
//        }

//        public string GetStatusReport()
//        {
//            StringBuilder sb = new StringBuilder();

//            if (SquadLeader != null && SquadLeader.SquadLeaderPawn != null)
//            {
//                sb.Append($"Squad Leader - {SquadLeader.SquadLeaderPawn.Name}");
//            }

//            if (this._CurrentStance != null)
//            {
//                sb.Append($"Duty - {this._CurrentStance.label}");
//            }

//            return sb.ToString();
//        }

//        public override IEnumerable<Gizmo> GetGizmos()
//        {
//            if (SquadLeader != null)
//            {
//                yield return new Gizmo_SquadMemberInfo(this);
//            }

//            yield return new Command_Action()
//            {
//                defaultLabel = $"Current State {this.CurrentState}",
//                action = () =>
//                {
//                    List<FloatMenuOption> gridOptions = new List<FloatMenuOption>();
//                    gridOptions.Add(new FloatMenuOption("Call To Arms", () =>
//                    {
//                        this.SetCurrentMemberState(SquadMemberState.CalledToArms);
//                    }));

//                    gridOptions.Add(new FloatMenuOption("At Ease", () =>
//                    {
//                        this.SetCurrentMemberState(SquadMemberState.AtEase);
//                    }));

//                    gridOptions.Add(new FloatMenuOption("Do Nothing", () =>
//                    {
//                        this.SetCurrentMemberState(SquadMemberState.DoNothing);
//                    }));

//                    Find.WindowStack.Add(new FloatMenu(gridOptions));
//                }
//            };
//        }

//        public override string GetInspectString()
//        {
//            string baseString = base.GetInspectString();
//            return baseString + GetStatusReport();
//        }
//        public override void ExposeData()
//        {
//            base.ExposeData();

//            Scribe_References.Look(ref _AssignedSquad, "assignedSquad");
//            Scribe_References.Look(ref referencedPawn, "referencedPawn");
//            Scribe_Values.Look(ref squadMemberState, "squadMemberState");
//            Scribe_Values.Look(ref defendPoint, "defendPoint");
//        }


//    }
//}
