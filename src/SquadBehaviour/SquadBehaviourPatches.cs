using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    [StaticConstructorOnStartup]
    public static class SquadBehaviourPatches
    {
        static SquadBehaviourPatches()
        {
            //foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.thingClass == typeof(Pawn)))
            //{
            //    if (def.comps == null)
            //        def.comps = new List<CompProperties>();

            //    if (def.comps?.OfType<CompProperties_SquadLeader>().Any() == false)
            //    {
            //        def.comps.Add(new CompProperties_SquadLeader());
            //    }
                  

            //    if (def.comps?.OfType<CompProperties_SquadMember>().Any() == false)
            //    {
            //        def.comps.Add(new CompProperties_SquadMember());
            //    }            
            //}

            Harmony harmony = new Harmony("com.emo.squadbehaviours");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(CharacterCardUtility))]
        [HarmonyPatch("DrawCharacterCard")]
        [HarmonyPatch(new Type[] { typeof(Rect), typeof(Pawn), typeof(Action), typeof(Rect), typeof(bool) })]
        [StaticConstructorOnStartup]
        public static class CharacterCard_DrawCharacterCard_Patch
        {
            private static Texture2D leaderIcon = ContentFinder<Texture2D>.Get("LeaderUIIcon", true);

            public static void Postfix(Rect rect, Pawn pawn, Action randomizeCallback = null, Rect creationRect = default(Rect), bool showName = true)
            {
                if (pawn == null || pawn.health.Dead || !showName)
                    return;

                if (pawn.IsFreeColonist && pawn.Spawned && !pawn.IsQuestLodger())
                {
                    Rect groupRect = rect;

                    Widgets.BeginGroup(groupRect);

                    float xPosition = CharacterCardUtility.PawnCardSize(pawn).x - 85f;

                    xPosition += 43f;

                    Rect leaderButtonRect = new Rect(xPosition, 0f, 30f, 30f);

                    if (Mouse.IsOver(leaderButtonRect))
                    {
                        TooltipHandler.TipRegion(leaderButtonRect, "Set as a squad leader");
                    }
    
                    if (Widgets.ButtonImage(leaderButtonRect, leaderIcon, true, null))
                    {

                        if (pawn.TryGetComp(out Comp_PawnSquadLeader squadLeader))
                        {
                            squadLeader.IsLeaderRoleActive = !squadLeader.IsLeaderRoleActive;
                        }

                        Messages.Message("Set " + pawn.LabelShort + " as a squad leader",
                            pawn, MessageTypeDefOf.NeutralEvent, false);
                    }

                    Widgets.EndGroup();
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
