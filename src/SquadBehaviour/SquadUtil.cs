using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalHasNoLeader : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn != null && pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember.SquadLeader == null;
        }
    }
    public static class SquadUtil
    {
        public static bool CanEverBeSquadLeader(this Pawn pawn)
        {
            if (pawn == null) 
                return false;

            // Must be humanlike
            if (!pawn.RaceProps.Humanlike) 
                return false;

            // Exclude AI personas
            if (pawn.RaceProps.IsMechanoid) 
                return false;

            if (pawn.Faction != null && pawn.Faction == Faction.OfPlayer)
            {
                return true;
            }

            //// Exclude pawns who can't walk
            //if (pawn.health?.capacities?.CapableOf(PawnCapacityDefOf.Moving) == false) return false;

            //// Exclude pawns who can't manipulate
            //if (pawn.health?.capacities?.CapableOf(PawnCapacityDefOf.Manipulation) == false) return false;

            //if (pawn.Downed || pawn.Dead)
            //    return false;

            // Can potentially be a squad leader
            return true;
        }

        //stub
        public static bool CanEverBeSquadMember(this Pawn pawn)
        {
            if (pawn == null)
                return false;
            return true;
        }


        public static bool TryGetSquadLeader(this Pawn pawn, out ISquadLeader SquadLeader)
        {
            SquadLeader = null;
            if (!CanEverBeSquadLeader(pawn))
            {
                return false;
            }

            // Check for comp first
            CompSquadLeader comp = pawn.GetComp<CompSquadLeader>();
            if (comp != null && comp.HasAnySquad())
            {
                SquadLeader = comp;
                return true;
            }

            // Then check hediffs
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is ISquadLeader squadLeaderHediff && squadLeaderHediff.ActiveSquads != null && squadLeaderHediff.ActiveSquads.Count > 0)
                {
                    SquadLeader = squadLeaderHediff;
                    return true;
                }
            }

            return false;
        }

        public static bool IsPartOfSquad(this Pawn pawn, out ISquadMember SquadMember)
        {
            SquadMember = null;

            // Check for comp first
            CompSquadMember comp = pawn.GetComp<CompSquadMember>();
            if (comp != null && comp.SquadLeader != null)
            {
                SquadMember = comp;
                return true;
            }

            // Then check hediffs
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is ISquadMember squadMemberHediff && squadMemberHediff.SquadLeader != null)
                {
                    SquadMember = squadMemberHediff;
                    return true;
                }
            }

            return false;
        }

    }
}
