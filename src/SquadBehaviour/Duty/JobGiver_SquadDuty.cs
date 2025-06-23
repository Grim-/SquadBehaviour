using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_SquadDuty : ThinkNode_JobGiver
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) || squadMember.CurrentStance == null)
                return ThinkResult.NoJob;

            SquadDutyDef dutyDef = squadMember.CurrentStance;
            if (dutyDef.JobGiver == null)
            {
                Log.Error($"SquadDutyDef {dutyDef.defName} is missing a valid JobGiver.");
                return ThinkResult.NoJob;
            }

            return dutyDef.JobGiver.TryIssueJobPackage(pawn, jobParams);
        }

        protected override Job TryGiveJob(Pawn pawn) => null;
    }
}
