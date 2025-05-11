using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public static class SquadWidgets
    {


        public static void DrawFormationSelector(Comp_PawnSquadLeader leader, Rect rect, string tooltip = "Change Formation")
        {
            if (Widgets.ButtonImage(rect, leader.FormationType.Icon))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (FormationDef formation in DefDatabase<FormationDef>.AllDefs)
                {
                    options.Add(new FloatMenuOption(
                        formation.label,
                        delegate { leader.SetFormation(formation); }
                    ));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }

        public static void DrawGlobalStateSelector(Comp_PawnSquadLeader leader, Rect rect)
        {
            if (Widgets.ButtonImage(rect, GetCommandTexture(leader.SquadState), true, GetSquadStateString(leader.SquadState)))
            {
                List<FloatMenuGridOption> options = new List<FloatMenuGridOption>();

                options.Add(new FloatMenuGridOption(GetCommandTexture(SquadMemberState.CalledToArms), () =>
                {
                    leader.SetAllState(SquadMemberState.CalledToArms);
                }, null, new TipSignal(GetSquadStateString(SquadMemberState.CalledToArms))));

                options.Add(new FloatMenuGridOption(GetCommandTexture(SquadMemberState.AtEase), () =>
                {
                    leader.SetAllState(SquadMemberState.AtEase);
                }, null, new TipSignal(GetSquadStateString(SquadMemberState.AtEase))));
                Find.WindowStack.Add(new FloatMenuGrid(options));
            }
        }


        public static void DrawOrderFloatGrid(Comp_PawnSquadLeader leader, Rect rect)
        {
            if (Widgets.ButtonImage(rect, TexCommand.SquadAttack, true, "Extra Orders"))
            {
                List<FloatMenuGridOption> extraOrders = new List<FloatMenuGridOption>();

                foreach (var item in DefDatabase<SquadOrderDef>.AllDefsListForReading)
                {

                    if (item.requiresTarget)
                    {
                        extraOrders.Add(new FloatMenuGridOption(item.Icon, () =>
                        {
                            Find.Targeter.BeginTargeting(item.targetingParameters, (LocalTargetInfo target) =>
                            {
                                leader.IssueGlobalOrder(item, target);
                            });
                        }, null, new TipSignal(item.defName)));
                    }
                    else
                    {
                        extraOrders.Add(new FloatMenuGridOption(item.Icon, () =>
                        {
                            leader.IssueGlobalOrder(item, null);
                        }, null, new TipSignal(item.defName)));
                    }
                }

                Find.WindowStack.Add(new FloatMenuGrid(extraOrders));
            }
        }


        public static string GetSquadStateString(SquadMemberState state)
        {
            switch (state)
            {
                case SquadMemberState.DoNothing:
                    return "Do nothin";
                case SquadMemberState.CalledToArms:
                    return "Call to arms";
                case SquadMemberState.AtEase:
                    return "Stand down";
                default:
                    return "invalid state";
            }
        }

        public static Texture2D GetCommandTexture(SquadMemberState state)
        {
            switch (state)
            {
                case SquadMemberState.DoNothing:
                    return TexCommand.ForbidOn;
                case SquadMemberState.CalledToArms:
                    return TexCommand.Draft;
                case SquadMemberState.AtEase:
                    return TexCommand.HoldOpen;
                default:
                    return TexCommand.DesirePower;
            }
        }
    }
}
