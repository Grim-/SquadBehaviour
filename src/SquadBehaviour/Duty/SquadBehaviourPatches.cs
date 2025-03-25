//using HarmonyLib;
//using RimWorld;
//using Verse;
//using Verse.AI;

//namespace SquadBehaviour
//{
//    public static class SquadBehaviourPatches
//    {
//        [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
//        public static class Patch_MentalStateHandler_TryStartMentalState
//        {
//            public static bool Prefix(Pawn ___pawn, ref bool __result)
//            {
//                if (___pawn.IsPartOfSquad(out ISquadMember squadMember))
//                {
//                    __result = false;
//                    return false;
//                }
//                return true;
//            }
//        }

//        [HarmonyPatch(typeof(Pawn_MindState), "StartFleeingBecauseOfPawnAction")]
//        public static class Patch_Pawn_MindState_StartFleeingBecauseOfPawnAction
//        {
//            public static bool Prefix(Pawn ___pawn)
//            {
//                return !___pawn.IsPartOfSquad(out ISquadMember squadMember);
//            }
//        }

//        [HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetFleeJob")]
//        public static class Patch_JobGiver_ConfigurableHostilityResponse_TryGetFleeJob
//        {
//            public static bool Prefix(Pawn pawn, ref Job __result)
//            {
//                if (pawn.Faction == Faction.OfPlayer && pawn.IsPartOfSquad(out ISquadMember squadMember))
//                {
//                    __result = null;
//                    return false;
//                }
//                return true;
//            }
//        }    
//    }
//}
