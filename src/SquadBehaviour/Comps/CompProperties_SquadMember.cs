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

        public void IssueOrder(SquadOrderDef orderDef, LocalTargetInfo target)
        {
            SquadOrderWorker squadOrderWorker = orderDef.CreateWorker(SquadLeader, this);

            if (squadOrderWorker.CanExecuteOrder(target))
            {
                squadOrderWorker.ExecuteOrder(target);
            }
        }



        public void CheckShouldDisobeyOrder()
        {

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
        public void Notify_OnPawnDeath()
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

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.HostileTo(this.Pawn) || selPawn.Faction != this.Pawn.Faction)
            {
                yield break;
            }

            if (AssignedSquad == null && selPawn.TryGetSquadLeader(out Comp_PawnSquadLeader squadLeader))
            {
                foreach (var item in squadLeader.ActiveSquads)
                {
                    yield return new FloatMenuOption($"Add to squad [{item.Value.squadName}]", () =>
                    {
                        squadLeader.AddToSquad(this.Pawn, item.Key);
                    });
                }
            }
            else if (AssignedSquad != null)
            {
                yield return new FloatMenuOption($"Remove from squad [{AssignedSquad.squadName}]", () =>
                {
                    AssignedSquad.RemoveMember(this.Pawn);
                });      
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (SquadLeader != null)
            {
                yield return new Gizmo_SquadMember(this);
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
            Scribe_Values.Look(ref defendPoint, "defendPoint");
        }

    }



    //public class DeathActionWorker_RemoveFromSquad : DeathActionWorker
    //{
    //    public override void PawnDied(Corpse corpse, Lord prevLord)
    //    {
    //        if (corpse.InnerPawn != null)
    //        {
    //            if (corpse.InnerPawn.TryGetComp(out Comp_PawnSquadMember squadMember))
    //            {
    //                squadMember.Notify_OnPawnDeath();
    //            }
    //        }
    //    }
    //}
}
