﻿using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class SquadOrder_Attack : SquadOrderWorker
    {
       // public override bool IsSquadOrder => false;

        public override bool CanExecuteOrder(LocalTargetInfo Target)
        {
            if (Target != null && Target.Thing != null || Target.Pawn != null)
            {
                return true;
            }

            return false;
        }

        public override void ExecuteOrder(LocalTargetInfo Target)
        {
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, Target);
            job.playerForced = true;
            job.killIncappedTarget = true;
            SquadMember.Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        public override void ExecuteOrderGlobal(LocalTargetInfo Target)
        {
            foreach (var member in this.SquadMember.AssignedSquad.Members)
            {
                if (member.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                {
                    Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, Target);
                    job.playerForced = true;
                    job.killIncappedTarget = true;
                    squadMember.Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
            }
        }
    }
}
