using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SquadBehaviour
{
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



        private static Dictionary<Pawn, ISquadLeader> SquadLeaderCache = new Dictionary<Pawn, ISquadLeader>();

        public static bool TryGetSquadLeader(this Pawn pawn, out ISquadLeader SquadLeader)
        {
            if (SquadLeaderCache.ContainsKey(pawn) && SquadLeaderCache[pawn] != null)
            {
                SquadLeader = SquadLeaderCache[pawn];
                return true;
            }
            else
            {
                SquadLeader = null;
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is ISquadLeader squadLeaderHediff)
                    {
                        SquadLeader = squadLeaderHediff;
                        SquadLeaderCache.Add(pawn, SquadLeader);
                        return true;
                    }
                }
            }
            return false;
        }


        private static Dictionary<Pawn, ISquadMember> SquadMemberCache = new Dictionary<Pawn, ISquadMember>();

        public static bool TryGetSquadMember(this Pawn pawn, out ISquadMember SquadMember)
        {
            if (SquadMemberCache.ContainsKey(pawn) && SquadMemberCache[pawn] != null)
            {
                SquadMember = SquadMemberCache[pawn];
                return true;
            }
            else
            {
                SquadMember = null;
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is ISquadMember squadMemberHediff)
                    {
                        SquadMember = squadMemberHediff;
                        SquadMemberCache.Add(pawn, SquadMember);
                        return true;
                    }
                }
            }



            return false;
        }

        public static bool IsPartOfSquad(this Pawn pawn, out ISquadMember SquadMember)
        {
            SquadMember = null;
            if (TryGetSquadMember(pawn, out ISquadMember foundMember) && foundMember.AssignedSquad != null)
            {
                SquadMember = foundMember;
                return true;
            }
            return false;
        }

    }
}
