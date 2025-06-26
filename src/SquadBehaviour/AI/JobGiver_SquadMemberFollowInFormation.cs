using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_SquadMemberFollowInFormation : JobGiver_AIFollowMaster
    {
        protected override Pawn GetFollowee(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember memeber))
            {
                return memeber.SquadLeader.Pawn;
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
                Log.Error($" SquadMemberFollowInFormation : Followee is null for {pawn.LabelShort}");
                return null;
            }

            if (!followee.Spawned)
            {
                Log.Message($"SquadMemberFollowInFormation : Followee {followee.LabelShort} is not spawned");
                return null;
            }

            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember memeber))
            {
                Log.Message($"SquadMemberFollowInFormation : {followee.LabelShort} is not part of a squad.");
                return null;
            }


            if (memeber.SquadLeader.Pawn == null)
            {
                Log.Message($"SquadMemberFollowInFormation : {followee.LabelShort} has a null squad leader");
                return null;
            }

            var activeUndead = memeber.SquadLeader.AllSquadsPawns;
            if (activeUndead == null || !activeUndead.Contains(pawn))
            {
                return null;
            }
            if (!FarEnoughAndPossibleToStartJob(pawn, followee, memeber.SquadLeader, GetRadius(pawn)))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(SquadDefOf.Squad_FormationFollow, followee);
            job.expiryInterval = 200;
            job.followRadius = memeber.AssignedSquad.FollowDistance;
            job.locomotionUrgency = LocomotionUrgency.Sprint;
            job.SetTarget(TargetIndex.A, followee);
            job.reportStringOverride = $"Following {memeber.SquadLeader.Pawn.LabelCap} in formation";
            return job;
        }

        public static bool FarEnoughAndPossibleToStartJob(Pawn follower, Pawn followee, Comp_PawnSquadLeader undeadMaster, float radius)
        {
            if (radius <= 0f)
            {
                Log.ErrorOnce($"Checking formation follow job with radius <= 0. pawn={follower.ToStringSafe<Pawn>()}",
                    follower.thingIDNumber ^ 843254009);
                return false;
            }
            IntVec3 targetCell = undeadMaster.GetFormationPositionFor(follower);

            return follower.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly);
        }
    }
}
