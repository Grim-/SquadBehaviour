using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_StandAndStareShort : JobGiver_StandAndStare
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
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
