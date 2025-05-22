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

            if (!pawn.RaceProps.Humanlike) 
                return false;

            if (pawn.Faction != null && pawn.Faction == Faction.OfPlayer)
            {
                return true;
            }

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
            SquadLeader = null;
            return false;
        }


        private static Dictionary<Pawn, Comp_PawnSquadMember> SquadMemberCache = new Dictionary<Pawn, Comp_PawnSquadMember>();

        public static bool TryGetSquadMember(this Pawn pawn, out Comp_PawnSquadMember SquadMember)
        {
            if (pawn.TryGetComp(out Comp_PawnSquadMember pawnSquadMember))
            {
                SquadMember = pawnSquadMember;
                return true;
            }

            SquadMember = null;
            return false;
        }

        public static bool IsPartOfSquad(this Pawn pawn, out Comp_PawnSquadMember SquadMember)
        {
            SquadMember = null;
            if (pawn.TryGetComp(out Comp_PawnSquadMember pawnSquadMember) && pawnSquadMember.AssignedSquad != null)
            {
                SquadMember = pawnSquadMember;
                return true;

            }
            return false;
        }

    }
}
