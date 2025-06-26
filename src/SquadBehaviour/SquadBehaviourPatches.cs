using HarmonyLib;
using LudeonTK;
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


                if (Widgets.ButtonImage(leaderButtonRect, SquadWidgets.SquadUIIcon, true, null) && pawn.TryGetComp(out Comp_PawnSquadLeader squadLeader))
                {
                    if (!squadLeader.CanEverBeLeader)
                    {
                        Messages.Message(pawn.LabelShort + " is not capable of leading a squad.", pawn, MessageTypeDefOf.NegativeEvent, false);
                        return;
                    }

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

                Widgets.EndGroup();
            }
        }


        [TweakValue("MagicAndMyths")]
        public static float IconXOffset = 2f;
        [TweakValue("MagicAndMyths")]
        public static float IconYOffset = 0f;
        [HarmonyPatch(typeof(PawnUIOverlay), "DrawPawnGUIOverlay")]
        public static class PawnUIOverlay_DrawPawnGUIOverlay_Patch
        {
            public static void Postfix(PawnUIOverlay __instance, Pawn ___pawn)
            {
                if (!___pawn.Spawned || ___pawn.Map.fogGrid.IsFogged(___pawn.Position))
                {
                    return;
                }

                if (!___pawn.RaceProps.Humanlike)
                {
                    if (___pawn.RaceProps.Animal)
                    {
                        if (!Prefs.AnimalNameMode.ShouldDisplayAnimalName(___pawn))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (!___pawn.IsColonyMech)
                        {
                            return;
                        }
                        if (!Prefs.MechNameMode.ShouldDisplayMechName(___pawn))
                        {
                            return;
                        }
                    }
                }

                if (___pawn.IsMutant && !___pawn.mutant.Def.showLabel)
                {
                    return;
                }

                Vector2 labelPos = GenMapUI.LabelDrawPosFor(___pawn, -0.6f);

                string pawnName = ___pawn.LabelShort;
                GameFont originalFont = Text.Font;
                Text.Font = GameFont.Tiny;
                float textWidth = Text.CalcSize(pawnName).x;
                Text.Font = originalFont;


                Vector2 iconPos = new Vector2(labelPos.x + textWidth / 2 + IconXOffset, labelPos.y + IconYOffset);
                Rect iconRect = new Rect(iconPos.x, iconPos.y, 16f, 16f);

                if (___pawn.TryGetSquadLeader(out Comp_PawnSquadLeader squadLeader) && squadLeader.IsLeaderRoleActive)
                {
                    GUI.DrawTexture(iconRect, SquadWidgets.SquadUIIcon);
                }
                else if(___pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                {
                    if (squadMember.CurrentStance != null && squadMember.CurrentStance.Tex != null)
                    {
                        GUI.DrawTexture(iconRect, squadMember.CurrentStance.Tex);
                    }          
                }

            }
        }

        //[HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
        //public static class Patch_MentalStateHandler_TryStartMentalState
        //{
        //    public static bool Prefix(Pawn ___pawn, ref bool __result)
        //    {
        //        if (___pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
        //        {
        //            __result = false;
        //            return false;
        //        }
        //        return true;
        //    }
        //}

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
