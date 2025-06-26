using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{

    [StaticConstructorOnStartup]
    public static class SquadWidgets
    {

        public static Texture2D AtEaseTex = ContentFinder<Texture2D>.Get("AtEaseUIIcon");
        public static Texture2D CalledToArmsTex = ContentFinder<Texture2D>.Get("CalledToArmsUIIcon");
        public static Texture2D DefensiveTex = ContentFinder<Texture2D>.Get("DefensiveUIIcon");
        public static Texture2D AggresiveTex = ContentFinder<Texture2D>.Get("AggresiveUIIcon");

        public static Texture2D SettingsMinimize = ContentFinder<Texture2D>.Get("MinimizeUIIcon");
        public static Texture2D SettingsMaimize = ContentFinder<Texture2D>.Get("MaximizeUIIcon");


        public static Texture2D OrdersUIIcon = ContentFinder<Texture2D>.Get("OrdersUIIcon");


        public static Texture2D SquadUIIcon = ContentFinder<Texture2D>.Get("SquadUIIcon");
        public static Texture2D ArrowDownUIIcon = ContentFinder<Texture2D>.Get("ArrowDownUIIcon");
        public static Texture2D ArrowRightUIIcon = ContentFinder<Texture2D>.Get("ArrowRightUIIcon");
        public static Texture2D DisbandUIIcon = ContentFinder<Texture2D>.Get("DisbandUIIcon");
        public static Texture2D MergeUIIcon = ContentFinder<Texture2D>.Get("MergeUIIcon");

        public static Texture2D HoldFormationUIIcon = ContentFinder<Texture2D>.Get("HoldFormationUIIcon");
        public static Texture2D BreakFormationUIIcon = ContentFinder<Texture2D>.Get("BreakFormationUIIcon");
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
        public static void DrawAttackModeSelector(Rect rect, Texture2D icon, Action<SquadMemberAllowedAttacks> toggleAttackMode, string tooltip = "Change Allowed Attack")
        {
            Widgets.DrawBoxSolidWithOutline(rect, Color.clear, Color.white * 0.6f);
            if (Widgets.ButtonImage(rect, icon, true, tooltip))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                options.Add(new FloatMenuOption(SquadMemberAllowedAttacks.ALL.ToString(), () => toggleAttackMode(SquadMemberAllowedAttacks.ALL)));
                options.Add(new FloatMenuOption(SquadMemberAllowedAttacks.MELEE.ToString(), () => toggleAttackMode(SquadMemberAllowedAttacks.MELEE)));
                options.Add(new FloatMenuOption(SquadMemberAllowedAttacks.RANGED.ToString(), () => toggleAttackMode(SquadMemberAllowedAttacks.RANGED)));


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

            if (Widgets.ButtonImage(rect, icon, true, "Orders"))
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
                        }, null, new TipSignal(item.LabelCap)));
                    }
                    else
                    {
                        extraOrders.Add(new FloatMenuGridOption(item.Icon, () =>
                        {
                            onClickAction?.Invoke(item, null);
                        }, null, new TipSignal(item.LabelCap)));
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
                    return AggresiveTex;
                case SquadHostility.Defensive:
                    return DefensiveTex;
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
                    return CalledToArmsTex;
                case SquadMemberState.AtEase:
                    return AtEaseTex;
                default:
                    return AtEaseTex;
            }
        }
    }
}