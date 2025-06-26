using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SquadBehaviour
{
    public class JobGiver_EnhancedHostilityResponse : JobGiver_ConfigurableHostilityResponse
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction != Faction.OfPlayer)
            {
                return null;
            }

            //if (pawn.playerSettings != null && !pawn.playerSettings.UsesConfigurableHostilityResponse)
            //{
            //    return null;
            //}

            if (pawn.RaceProps.Humanlike && PawnUtility.PlayerForcedJobNowOrSoon(pawn))
            {
                return null;
            }
            if (pawn.Downed)
            {
                return null;
            }


            if (ModsConfig.AnomalyActive)
            {
                Lord lord = pawn.GetLord();
                if (lord != null && lord.LordJob != null && lord.LordJob is LordJob_PsychicRitual)
                {
                    return null;
                }
            }

            if (!pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                return base.TryGiveJob(pawn);
            }
            else
            {
                if (squadMember.CurrentState == SquadMemberState.DoNothing || squadMember.AssignedSquad.HostilityResponse == SquadHostility.None)
                {
                    return null;
                }

                float maxDist = squadMember.AssignedSquad.MaxAttackDistanceFor(pawn);

                Thing thing = squadMember.AssignedSquad.FindTargetForMember(pawn);

                if (thing != null)
                {
                    float distanceToNearestTarget = thing.Position.DistanceTo(pawn.Position);
                    if (distanceToNearestTarget <= maxDist)
                    {
                        return AttackJobUtil.TryGetAttackNearbyEnemyJob(pawn, thing);
                    }
                }
                return null;
            }
        }
    }
}