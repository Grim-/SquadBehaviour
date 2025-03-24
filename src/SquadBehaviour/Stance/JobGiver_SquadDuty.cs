using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public abstract class JobGiver_SquadDuty : ThinkNode_JobGiver
    {
        protected SquadDutyDef duty;

        public void SetDuty(SquadDutyDef dutyDef)
        {
            duty = dutyDef;
        }

        protected bool IsSquadLeaderNearby(Pawn pawn, out Pawn leader, float distance = -1f)
        {
            leader = null;
            if (!pawn.IsPartOfSquad(out ISquadMember squadMember))
                return false;

            if (squadMember.SquadLeader?.SquadLeaderPawn == null)
                return false;

            leader = squadMember.SquadLeader.SquadLeaderPawn;
            if (distance < 0)
                distance = duty?.props?.maxDistance ?? 15f;

            return pawn.Position.InHorDistOf(leader.Position, distance);
        }

        protected virtual bool ShouldRespondToThreats(Pawn pawn)
        {
            return duty?.respondToThreats ?? false;
        }

        protected virtual bool ShouldMaintainFormation(Pawn pawn)
        {
            return duty?.maintainsFormation ?? false;
        }

        protected virtual Thing FindNearbyThreat(Pawn pawn, float radius)
        {
            return null;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (ShouldRespondToThreats(pawn))
            {
                Thing threat = FindNearbyThreat(pawn, 25f);
                if (threat != null)
                {
                    Job attackJob = JobMaker.MakeJob(JobDefOf.AttackMelee, threat);
                    return new ThinkResult(attackJob, this);
                }
            }
            return TryGiveSquadDutyJob(pawn, jobParams);
        }
        protected abstract ThinkResult TryGiveSquadDutyJob(Pawn pawn, JobIssueParams jobParams);
    }

    //SquadDuy
    //Called To Arms
    //Params, In formation, hostility response, 

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
