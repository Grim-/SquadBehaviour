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
                if (pawn == null || pawn.health.Dead || !pawn.IsColonist)
                    return;

                Rect groupRect = rect;

                Widgets.BeginGroup(groupRect);

                float xPosition = CharacterCardUtility.PawnCardSize(pawn).x - 90f;

                xPosition += 43f;

                Rect leaderButtonRect = new Rect(xPosition, 4f, 22f, 22f);

                if (Mouse.IsOver(leaderButtonRect))
                {
                    TooltipHandler.TipRegion(leaderButtonRect, "Set as a squad leader");
                }

                if (Widgets.ButtonImage(leaderButtonRect, leaderIcon, true, null))
                {

                    if (pawn.TryGetComp(out Comp_PawnSquadLeader squadLeader))
                    {
                        if (squadLeader.IsLeaderRoleActive)
                        {
                            squadLeader.SetSquadLeader(false);
                            Messages.Message(pawn.LabelShort + " is no longer a squad leader", pawn, MessageTypeDefOf.NeutralEvent, false);
                        }
                        else
                        {
                            squadLeader.SetSquadLeader(true);
                            Messages.Message(pawn.LabelShort + " is a squad leader", pawn, MessageTypeDefOf.NeutralEvent, false);
                        }
                    }


                }

                Widgets.EndGroup();
            }
        }

        [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
        public static class Patch_MentalStateHandler_TryStartMentalState
        {
            public static bool Prefix(Pawn ___pawn, ref bool __result)
            {
                if (___pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_MindState), "StartFleeingBecauseOfPawnAction")]
        public static class Patch_Pawn_MindState_StartFleeingBecauseOfPawnAction
        {
            public static bool Prefix(Pawn ___pawn)
            {
                return !___pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember);
            }
        }

        [HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetFleeJob")]
        public static class Patch_JobGiver_ConfigurableHostilityResponse_TryGetFleeJob
        {
            public static bool Prefix(Pawn pawn, ref Job __result)
            {
                if (pawn.Faction == Faction.OfPlayer && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                {
                    __result = null;
                    return false;
                }
                return true;
            }
        }
    }
}
