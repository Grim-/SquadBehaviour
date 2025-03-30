using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class CompSquadMember : ThingComp, ISquadMember
    {
        private Pawn referencedPawn;
        private SquadMemberState squadMemberState = SquadMemberState.CalledToArms;
        private SquadMemberState preDefendState = SquadMemberState.CalledToArms;
        private IntVec3 defendPoint = IntVec3.Invalid;

        public IntVec3 DefendPoint => defendPoint;
        public bool HasDefendPoint => defendPoint != IntVec3.Invalid;
        public Pawn Pawn => this.parent as Pawn;
        public SquadMemberState CurrentState => squadMemberState;

        private ISquadLeader _SquadLeader;
        public ISquadLeader SquadLeader
        {
            get
            {
                if (_SquadLeader == null)
                {
                    if (referencedPawn.TryGetSquadLeader(out ISquadLeader squadLeader))
                    {
                        _SquadLeader =  squadLeader;
                    }
                }
                return _SquadLeader;
            }
        }


        private Squad _AssignedSquad = null;
        public Squad AssignedSquad { get => _AssignedSquad; set => _AssignedSquad = value; }


        private SquadDutyDef _CurrentStance = null;
        SquadDutyDef ISquadMember.CurrentStance { get => _CurrentStance; set => _CurrentStance = value; }


        private Zone_PatrolPath _AssignedPatrol = null;
        Zone_PatrolPath ISquadMember.AssignedPatrol { get => _AssignedPatrol; set => _AssignedPatrol = value; }


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

        public override void PostDraw()
        {
            base.PostDraw();
            if (squadMemberState == SquadMemberState.CalledToArms && _CurrentStance != null && _CurrentStance.Tex != null)
            {
                Vector3 overheadPos = Pawn.DrawPos;
                overheadPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                overheadPos.y += 0.1f; // Adjust to avoid z-fighting.
                overheadPos.z += 0.5f; // Adjust vertical position above the pawn.

                Vector2 onScreenPos = overheadPos.MapToUIPosition();
                Rect iconRect = new Rect(onScreenPos.x - 16f, onScreenPos.y - 32f, 32f, 32f); // Adjust size and position.

                GUI.DrawTexture(iconRect, _CurrentStance.Tex);
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
            Scribe_Values.Look(ref preDefendState, "preDefendState");
            Scribe_Values.Look(ref defendPoint, "defendPoint");
        }


    }
}
