using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{

    public class JobGiver_StandAndStareShort : JobGiver_StandAndStare
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            //let the base run but return another job
            base.TryGiveJob(pawn);

            Job job = JobMaker.MakeJob(SquadDefOf.SquadMember_InterruptableWait);
            job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, this.locomotionUrgency);
            job.expiryInterval = 100;
            return job;
        }
    }

    public interface IInterruptableJob
    {
        void Interupt();
    }

    public class JobDriver_InterruptableWait : JobDriver_Wait, IInterruptableJob
    {
        public void Interupt()
        {
            this.EndJobWith(JobCondition.InterruptForced);
        }
    }
}
