using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public static class SquadWidgets
    {
        public static void DrawHostilitySelector(Rect hostilityRect, Texture2D icon, Action<SquadHostility> setHostilityAction)
        {
            Widgets.DrawBoxSolidWithOutline(hostilityRect, Color.clear, Color.white * 0.6f);
            if (Widgets.ButtonImage(hostilityRect, icon, true, "Change Squad Hostility"))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (SquadHostility formation in Enum.GetValues(typeof(SquadHostility)))
                {
                    options.Add(new FloatMenuOption(
                        formation.ToString(),
                        delegate { setHostilityAction(formation); }
                    ));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            if (Mouse.IsOver(hostilityRect))
            {
                TooltipHandler.TipRegion(hostilityRect, "Change Squad Hostility");
            }
        }

        public static void DrawFormationSelector(Rect rect, Texture2D icon, Action<FormationDef> setFormationAction, string tooltip = "Change Formation")
        {
            Widgets.DrawBoxSolidWithOutline(rect, Color.clear, Color.white * 0.6f);
            if (Widgets.ButtonImage(rect, icon, true, tooltip))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (FormationDef formation in DefDatabase<FormationDef>.AllDefs)
                {
                    options.Add(new FloatMenuOption(
                        formation.label,
                        delegate { setFormationAction(formation); }
                    ));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }

        public static void DrawStateSelector(Rect rect, SquadMemberState currentState, Action<SquadMemberState> setStateAction, bool useGrid = false)
        {
            Widgets.DrawBoxSolidWithOutline(rect, Color.clear, Color.white * 0.6f);

            if (Widgets.ButtonImage(rect, GetCommandTexture(currentState), true, GetSquadStateString(currentState)))
            {
                if (useGrid)
                {
                    List<FloatMenuGridOption> options = new List<FloatMenuGridOption>();

                    options.Add(new FloatMenuGridOption(GetCommandTexture(SquadMemberState.CalledToArms), () =>
                    {
                        setStateAction(SquadMemberState.CalledToArms);
                    }, null, new TipSignal(GetSquadStateString(SquadMemberState.CalledToArms))));

                    options.Add(new FloatMenuGridOption(GetCommandTexture(SquadMemberState.AtEase), () =>
                    {
                        setStateAction(SquadMemberState.AtEase);
                    }, null, new TipSignal(GetSquadStateString(SquadMemberState.AtEase))));

                    Find.WindowStack.Add(new FloatMenuGrid(options));
                }
                else
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    options.Add(new FloatMenuOption("Call to arms", () =>
                    {
                        setStateAction(SquadMemberState.CalledToArms);
                    }));

                    options.Add(new FloatMenuOption("At Ease", () =>
                    {
                        setStateAction(SquadMemberState.AtEase);
                    }));

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
        }

        public static void DrawOrderFloatGrid(Rect rect, Texture2D icon, Action<SquadOrderDef, LocalTargetInfo> onClickAction)
        {
            Widgets.DrawBoxSolidWithOutline(rect, Color.clear, Color.white * 0.6f);

            if (Widgets.ButtonImage(rect, icon, true, "Extra Orders"))
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
                                onClickAction?.Invoke(item, target);
                            });
                        }, null, new TipSignal(item.defName)));
                    }
                    else
                    {
                        extraOrders.Add(new FloatMenuGridOption(item.Icon, () =>
                        {
                            onClickAction?.Invoke(item, null);
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
        public static Texture2D GetHostilityTexture(SquadHostility state)
        {
            switch (state)
            {
                case SquadHostility.None:
                    return TexCommand.ForbidOn;
                case SquadHostility.Aggressive:
                    return TexCommand.Draft;
                case SquadHostility.Defensive:
                    return TexCommand.Draft;
                default:
                    return TexCommand.Draft;
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

    //public static class SquadWidgets
    //{
    //    public static void DrawGlobalHostilitySelector(Comp_PawnSquadLeader leader, Rect hostilityRect)
    //    {
    //        if (Widgets.ButtonImage(hostilityRect, TexCommand.Attack, true, "Change Squad Hostility"))
    //        {
    //            List<FloatMenuOption> options = new List<FloatMenuOption>();
    //            foreach (SquadHostility formation in Enum.GetValues(typeof(SquadHostility)))
    //            {
    //                options.Add(new FloatMenuOption(
    //                    formation.ToString(),
    //                    delegate { leader.SetHositilityResponse(formation); }
    //                ));
    //            }
    //            Find.WindowStack.Add(new FloatMenu(options));
    //        }
    //        if (Mouse.IsOver(hostilityRect))
    //        {
    //            TooltipHandler.TipRegion(hostilityRect, "Change Squad Hostility");
    //        }
    //    }

    //    public static void DrawSquadHostilitySelector(Squad squad, Rect hostilityRect)
    //    {
    //        if (Widgets.ButtonImage(hostilityRect, TexCommand.AttackMelee, true, "Change Squad Hostility"))
    //        {
    //            List<FloatMenuOption> options = new List<FloatMenuOption>();
    //            foreach (SquadHostility formation in Enum.GetValues(typeof(SquadHostility)))
    //            {
    //                options.Add(new FloatMenuOption(
    //                    formation.ToString(),
    //                    delegate { squad.SetHositilityResponse(formation); }
    //                ));
    //            }
    //            Find.WindowStack.Add(new FloatMenu(options));
    //        }
    //        if (Mouse.IsOver(hostilityRect))
    //        {
    //            TooltipHandler.TipRegion(hostilityRect, "Change Squad Hostility");
    //        }
    //    }

    //    public static void DrawGlobalFormationSelector(Comp_PawnSquadLeader leader, Rect rect, string tooltip = "Change Formation")
    //    {
    //        if (Widgets.ButtonImage(rect, leader.FormationType.Icon, true, tooltip))
    //        {
    //            List<FloatMenuOption> options = new List<FloatMenuOption>();
    //            foreach (FormationDef formation in DefDatabase<FormationDef>.AllDefs)
    //            {
    //                options.Add(new FloatMenuOption(
    //                    formation.label,
    //                    delegate { leader.SetFormation(formation); }
    //                ));
    //            }
    //            Find.WindowStack.Add(new FloatMenu(options));
    //        }

    //        if (Mouse.IsOver(rect))
    //        {
    //            TooltipHandler.TipRegion(rect, tooltip);
    //        }
    //    }

    //    public static void DrawSquadFormationSelector(Squad squad, Rect rect, string tooltip = "Change Formation")
    //    {
    //        if (Widgets.ButtonImage(rect, squad.FormationType.Icon, true, tooltip))
    //        {
    //            List<FloatMenuOption> options = new List<FloatMenuOption>();
    //            foreach (FormationDef formation in DefDatabase<FormationDef>.AllDefs)
    //            {
    //                options.Add(new FloatMenuOption(
    //                    formation.label,
    //                    delegate { squad.SetFormation(formation); }
    //                ));
    //            }
    //            Find.WindowStack.Add(new FloatMenu(options));
    //        }

    //        if (Mouse.IsOver(rect))
    //        {
    //            TooltipHandler.TipRegion(rect, tooltip);
    //        }
    //    }

    //    public static void DrawSquadStateSelector(Squad squad, Rect rect)
    //    {
    //        if (Widgets.ButtonImage(rect, GetCommandTexture(SquadMemberState.CalledToArms), true, GetSquadStateString(SquadMemberState.CalledToArms)))
    //        {
    //            List<FloatMenuOption> options = new List<FloatMenuOption>();

    //            options.Add(new FloatMenuOption("Call to arms", () =>
    //            {
    //                squad.SetSquadState(SquadMemberState.CalledToArms);
    //            }));

    //            options.Add(new FloatMenuOption("At Ease", () =>
    //            {
    //                squad.SetSquadState(SquadMemberState.AtEase);
    //            }));

    //            Find.WindowStack.Add(new FloatMenu(options));
    //        }
    //    }
    //    public static void DrawGlobalStateSelector(Comp_PawnSquadLeader leader, Rect rect)
    //    {            Widgets.DrawBoxSolidWithOutline(rect, Color.clear, Color.white * 0.6f);
    //        if (Widgets.ButtonImage(rect, GetCommandTexture(leader.SquadState), true, GetSquadStateString(leader.SquadState)))
    //        {
    //            List<FloatMenuGridOption> options = new List<FloatMenuGridOption>();

    //            options.Add(new FloatMenuGridOption(GetCommandTexture(SquadMemberState.CalledToArms), () =>
    //            {
    //                leader.SetAllState(SquadMemberState.CalledToArms);
    //            }, null, new TipSignal(GetSquadStateString(SquadMemberState.CalledToArms))));

    //            options.Add(new FloatMenuGridOption(GetCommandTexture(SquadMemberState.AtEase), () =>
    //            {
    //                leader.SetAllState(SquadMemberState.AtEase);
    //            }, null, new TipSignal(GetSquadStateString(SquadMemberState.AtEase))));

    //            Find.WindowStack.Add(new FloatMenuGrid(options));
    //        }
    //    }

    //    public static void DrawGlobalOrderFloatGrid(Comp_PawnSquadLeader leader, Rect rect, Action<Comp_PawnSquadLeader, SquadOrderDef, LocalTargetInfo> onClickAction )
    //    {
    //        Widgets.DrawBoxSolidWithOutline(rect, Color.clear, Color.white * 0.6f);

    //        if (Widgets.ButtonImage(rect, TexCommand.Attack, true, "Extra Orders"))
    //        {
    //            List<FloatMenuGridOption> extraOrders = new List<FloatMenuGridOption>();

    //            foreach (var item in DefDatabase<SquadOrderDef>.AllDefsListForReading)
    //            {
    //                if (item.requiresTarget)
    //                {
    //                    extraOrders.Add(new FloatMenuGridOption(item.Icon, () =>
    //                    {
    //                        Find.Targeter.BeginTargeting(item.targetingParameters, (LocalTargetInfo target) =>
    //                        {
    //                            onClickAction?.Invoke(leader, item, target);
    //                            //leader.IssueGlobalOrder(item, target);
    //                        });
    //                    }, null, new TipSignal(item.defName)));
    //                }
    //                else
    //                {
    //                    extraOrders.Add(new FloatMenuGridOption(item.Icon, () =>
    //                    {
    //                        onClickAction?.Invoke(leader, item, null);
    //                        //leader.IssueGlobalOrder(item, null);
    //                    }, null, new TipSignal(item.defName)));
    //                }
    //            }

    //            Find.WindowStack.Add(new FloatMenuGrid(extraOrders));
    //        }
    //    }

    //    public static string GetSquadStateString(SquadMemberState state)
    //    {
    //        switch (state)
    //        {
    //            case SquadMemberState.DoNothing:
    //                return "Do nothin";
    //            case SquadMemberState.CalledToArms:
    //                return "Call to arms";
    //            case SquadMemberState.AtEase:
    //                return "Stand down";
    //            default:
    //                return "invalid state";
    //        }
    //    }

    //    public static Texture2D GetCommandTexture(SquadMemberState state)
    //    {
    //        switch (state)
    //        {
    //            case SquadMemberState.DoNothing:
    //                return TexCommand.ForbidOn;
    //            case SquadMemberState.CalledToArms:
    //                return TexCommand.Draft;
    //            case SquadMemberState.AtEase:
    //                return TexCommand.HoldOpen;
    //            default:
    //                return TexCommand.DesirePower;
    //        }
    //    }
    //}
}