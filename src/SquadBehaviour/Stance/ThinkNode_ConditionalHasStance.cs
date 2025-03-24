using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalHasStance : ThinkNode_Conditional
    {
        private static Dictionary<SquadStanceDef, ThinkNode_JobGiver> jobGiverCache =
            new Dictionary<SquadStanceDef, ThinkNode_JobGiver>();

        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || !pawn.IsPartOfSquad(out ISquadMember squadMember))
                return false;

            SquadStanceDef stance = squadMember.CurrentStance;
            return stance != null;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (!Satisfied(pawn))
                return ThinkResult.NoJob;

            if (!pawn.IsPartOfSquad(out ISquadMember squadMember))
                return ThinkResult.NoJob;

            SquadStanceDef stance = squadMember.CurrentStance;
            if (stance == null)
                return ThinkResult.NoJob;

            // Get or create the JobGiver for this stance
            if (!jobGiverCache.TryGetValue(stance, out ThinkNode_JobGiver jobGiver))
            {
                jobGiver = stance.CreateJobGiver();
                if (jobGiver != null)
                {
                    jobGiverCache[stance] = jobGiver;
                }
            }

            if (jobGiver != null)
            {
                // Run the stance-specific JobGiver
                return jobGiver.TryIssueJobPackage(pawn, jobParams);
            }

            return base.TryIssueJobPackage(pawn, jobParams);
        }
    }
}
