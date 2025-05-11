using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    [StaticConstructorOnStartup]
    public static class SquadBehaviourPatches
    {
        static SquadBehaviourPatches()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.thingClass == typeof(Pawn)))
            {
                if (def.comps == null)
                    def.comps = new List<CompProperties>();

                if (def.comps?.OfType<CompProperties_SquadLeader>().Any() == false)
                {
                    def.comps.Add(new CompProperties_SquadLeader());
                }
                  

                if (def.comps?.OfType<CompProperties_SquadMember>().Any() == false)
                {
                    def.comps.Add(new CompProperties_SquadMember());
                }            
            }
        }

        //[HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
        //public static class Patch_MentalStateHandler_TryStartMentalState
        //{
        //    public static bool Prefix(Pawn ___pawn, ref bool __result)
        //    {
        //        if (___pawn.IsPartOfSquad(out ISquadMember squadMember))
        //        {
        //            __result = false;
        //            return false;
        //        }
        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(Pawn_MindState), "StartFleeingBecauseOfPawnAction")]
        //public static class Patch_Pawn_MindState_StartFleeingBecauseOfPawnAction
        //{
        //    public static bool Prefix(Pawn ___pawn)
        //    {
        //        return !___pawn.IsPartOfSquad(out ISquadMember squadMember);
        //    }
        //}

        //[HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetFleeJob")]
        //public static class Patch_JobGiver_ConfigurableHostilityResponse_TryGetFleeJob
        //{
        //    public static bool Prefix(Pawn pawn, ref Job __result)
        //    {
        //        if (pawn.Faction == Faction.OfPlayer && pawn.IsPartOfSquad(out ISquadMember squadMember))
        //        {
        //            __result = null;
        //            return false;
        //        }
        //        return true;
        //    }
        //}
    }
}
