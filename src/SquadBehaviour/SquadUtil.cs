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



        private static Dictionary<Pawn, Comp_PawnSquadLeader> SquadLeaderCache = new Dictionary<Pawn, Comp_PawnSquadLeader>();

        public static bool TryGetSquadLeader(this Pawn pawn, out Comp_PawnSquadLeader SquadLeader)
        {
            if (pawn.TryGetComp(out Comp_PawnSquadLeader pawnSquadMember))
            {
                SquadLeader = pawnSquadMember;
                return true;
            }

            //if (SquadLeaderCache.ContainsKey(pawn) && SquadLeaderCache[pawn] != null)
            //{
            //    SquadLeader = SquadLeaderCache[pawn];
            //    return true;
            //}
            //else
            //{
            //    SquadLeader = null;

            //    if (pawn.TryGetComp(out Comp_PawnSquadLeader pawnSquadMember))
            //    {
            //        if (pawnSquadMember.IsLeaderRoleActive)
            //        {
            //            SquadLeader = pawnSquadMember;
            //            //SquadLeaderCache.Add(pawn, pawnSquadMember);
            //            return true;
            //        }

            //    }
            //        //foreach (var hediff in pawn.health.hediffSet.hediffs)
            //        //{
            //        //    if (hediff is ISquadLeader squadLeaderHediff)
            //        //    {
            //        //        SquadLeader = squadLeaderHediff;
            //        //        SquadLeaderCache.Add(pawn, SquadLeader);
            //        //        return true;
            //        //    }
            //        //}
            //}
            SquadLeader = null;
            return false;
        }


        private static Dictionary<Pawn, Comp_PawnSquadMember> SquadMemberCache = new Dictionary<Pawn, Comp_PawnSquadMember>();

        public static bool TryGetSquadMember(this Pawn pawn, out Comp_PawnSquadMember SquadMember)
        {
            if (pawn.TryGetComp(out Comp_PawnSquadMember pawnSquadMember))
            {
                SquadMember = pawnSquadMember;
                //SquadMemberCache.Add(pawn, pawnSquadMember);
                return true;

            }


            //if (SquadMemberCache.ContainsKey(pawn) && SquadMemberCache[pawn] != null)
            //{
            //    SquadMember = SquadMemberCache[pawn];
            //    return true;
            //}
            //else
            //{
            //    SquadMember = null;


            //    if (pawn.TryGetComp(out Comp_PawnSquadMember pawnSquadMember))
            //    {
            //        if (pawnSquadMember.AssignedSquad != null)
            //        {
            //            SquadMember = pawnSquadMember;
            //            SquadMemberCache.Add(pawn, pawnSquadMember);
            //            return true;
            //        }

            //    }

            //    //foreach (var hediff in pawn.health.hediffSet.hediffs)
            //    //{
            //    //    if (hediff is ISquadMember squadMemberHediff)
            //    //    {
            //    //        SquadMember = squadMemberHediff;
            //    //        SquadMemberCache.Add(pawn, SquadMember);
            //    //        return true;
            //    //    }
            //    //}
            //}


            SquadMember = null;
            return false;
        }

        public static bool IsPartOfSquad(this Pawn pawn, out Comp_PawnSquadMember SquadMember)
        {
            SquadMember = null;
            if (pawn.TryGetComp(out Comp_PawnSquadMember pawnSquadMember))
            {
                //Log.Message($"Is part of a squad {pawn.Label}");
                SquadMember = pawnSquadMember;
                return true;

            }
            return false;
        }

    }
}
