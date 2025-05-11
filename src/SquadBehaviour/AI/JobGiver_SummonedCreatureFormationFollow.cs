using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_SummonedCreatureFormationFollow : JobGiver_AIFollowMaster
    {
        protected override Pawn GetFollowee(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember memeber))
            {
                return memeber.SquadLeader.SquadLeaderPawn;
            }
            return null;
        }

        protected override float GetRadius(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember memeber))
            {
                return memeber.AssignedSquad.FollowDistance;
            }

            return 1f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Pawn followee = GetFollowee(pawn);
            if (followee == null)
            {
                Log.Error($"Followee is null for {pawn.LabelShort}");
                return null;
            }

            if (!followee.Spawned)
            {
                Log.Message($"Followee {followee.LabelShort} is not spawned");
                return null;
            }

            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember memeber))
            {
                return null;
            }


            if (memeber.SquadLeader.SquadLeaderPawn == null)
            {
                return null;
            }

            var activeUndead = memeber.SquadLeader.AllSquadsPawns;
            if (activeUndead == null || !activeUndead.Contains(pawn))
            {
                return null;
            }
            if (!JobDriver_FormationFollow.FarEnoughAndPossibleToStartJob(pawn, followee, memeber.SquadLeader, GetRadius(pawn)))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(SquadDefOf.Squad_FormationFollow, followee);
            job.expiryInterval = 200;
            job.followRadius = memeber.AssignedSquad.FollowDistance;
            job.SetTarget(TargetIndex.A, followee);
            job.reportStringOverride = $"Following {memeber.SquadLeader.SquadLeaderPawn.LabelCap} in formation";
            return job;
        }
    }
}
