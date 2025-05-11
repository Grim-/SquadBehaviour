using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_SquadDuty : ThinkNode_JobGiver
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) || squadMember._CurrentStance == null)
                return ThinkResult.NoJob;

            SquadDutyDef dutyDef = squadMember._CurrentStance;
            if (dutyDef.JobGiver == null)
            {
                Log.Error($"SquadDutyDef {dutyDef.defName} is missing a valid JobGiver.");
                return ThinkResult.NoJob;
            }

            return dutyDef.JobGiver.TryIssueJobPackage(pawn, jobParams);
        }

        protected override Job TryGiveJob(Pawn pawn) => null;
    }


    //Squad State
    //Called To Arms, At Ease
    //Params, In formation, hostility response, 


    //SquadDuty - Can be assigned per unit or per squad
    //Patrol
    //Patrol Zone
    //hostility response
    //patrol type, perimeter,area

    //Defend Point
    //point to defend
    //hostiltiy repsonse
    //defend radius
    //wander radius

    //Off Duty
    //does not interfere with normal think tree


    //Squad Orders - Can be given per unit or per squad, or everyone.
    //Attack target
    //Defend point
    //Start Patrol


}
