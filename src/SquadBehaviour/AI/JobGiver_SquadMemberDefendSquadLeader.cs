using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_SquadMemberDefendSquadLeader : JobGiver_AIDefendPawn
    {
        protected Pawn Master = null;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null)
            {
                Log.Error("TryGiveJob called with null pawn");
                return null;
            }

            this.chaseTarget = true;
            this.allowTurrets = true;
            this.ignoreNonCombatants = true;
            this.humanlikesOnly = false;

            Job job = base.TryGiveJob(pawn);

            if (job != null)
            {
                job.reportStringOverride = "Defending Squad Leader.";
            }

            if (pawn.mindState != null)
            {
                pawn.mindState.canFleeIndividual = false;
            }

            return job;
        }

        protected override Pawn GetDefendee(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadLeader))
            {
                return squadLeader.SquadLeader.Pawn;
            }
            return null;
        }

        protected override float GetFlagRadius(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return squadMember.AssignedSquad.FollowDistance;
            }

            return 10f;
        }

        protected override IntVec3 GetFlagPosition(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return squadMember.SquadLeader.LeaderPosition;
            }
            return base.GetFlagPosition(pawn);
        }
    }
}
