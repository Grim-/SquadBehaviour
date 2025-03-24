using RimWorld;
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
    }
}
