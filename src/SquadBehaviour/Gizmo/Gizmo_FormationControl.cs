using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    [StaticConstructorOnStartup]
    public class Gizmo_FormationControl : Gizmo
    {
        private ISquadLeader master;
        private static readonly Vector2 BaseSize = new Vector2(140f, 80f);
        private static readonly Color BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private const float ButtonGridWidth = 140f;

        private Vector2 scrollPosition = new Vector2(0, 0);


        public Gizmo_FormationControl(ISquadLeader master)
        {
            this.master = master;
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            float width = BaseSize.x;

            // Add width for extra orders if showing
            if (master.ShowExtraOrders)
            {           
                // Add width for each squad's container
                if (master.ActiveSquads != null && master.ActiveSquads.Count > 0)
                {
                    width += master.ActiveSquads.Count * ButtonGridWidth;
                }
            }



            return width;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect baseRect = new Rect(topLeft.x, topLeft.y, BaseSize.x, BaseSize.y);
            GUI.DrawTexture(baseRect, Command.BGTex);


            Rect formationRect = new Rect(baseRect.x + 5f, baseRect.y + 5f, baseRect.width - 40f, 22f);
            if (Widgets.ButtonText(formationRect, "Formation: " + master.FormationType.ToString()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (FormationUtils.FormationType formation in Enum.GetValues(typeof(FormationUtils.FormationType)))
                {
                    options.Add(new FloatMenuOption(
                        formation.ToString(),
                        delegate { master.SetFormation(formation); }
                    ));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            if (Mouse.IsOver(formationRect))
            {
                TooltipHandler.TipRegion(formationRect, "Change formation");
            }


            //at side of formation button
            Rect toggleRect = new Rect(formationRect.xMax + 4f, formationRect.y, 22f, 22f);
            if (Widgets.ButtonImage(toggleRect, master.ShowExtraOrders ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                master.ShowExtraOrders = !master.ShowExtraOrders;
            }

            if (Mouse.IsOver(toggleRect))
            {
                TooltipHandler.TipRegion(toggleRect, "Show Extra");
            }


            //next row
            Rect standardOrderGrid = new Rect(baseRect.x, formationRect.yMax + 10f, ButtonGridWidth, 40f);
            DrawStandardOrderGrid(standardOrderGrid);

            if (master.ShowExtraOrders)
            {
                if (master.ActiveSquads != null && master.ActiveSquads.Count > 0)
                {
                    float xPosition = baseRect.xMax;
                    foreach (KeyValuePair<int, Squad> squad in master.ActiveSquads)
                    {
                        Rect squadRect = new Rect(xPosition, topLeft.y, ButtonGridWidth, BaseSize.y);
                        DrawSquadContainer(squadRect, squad.Value);
                        xPosition += ButtonGridWidth;
                    }
                }
            }


            return new GizmoResult(GizmoState.Clear);
        }

        private void DrawSquadContainer(Rect rect, Squad squad)
        {
            GUI.DrawTexture(rect, Command.BGTex);
            Widgets.Label(rect.TopHalf(), $"Squad {squad.squadID}");
        }

        private void DrawStandardOrderGrid(Rect rect)
        {
            GridLayout gridLayout = new GridLayout(rect, 3, 1);
            Rect cellRect = gridLayout.GetCellRect(0, 0);
            DrawDutyFloatGrid(gridLayout);
            DrawOrderFloatGrid(gridLayout);
        }

        private void DrawDutyFloatGrid(GridLayout gridLayout)
        {
            List<FloatMenuGridOption> options = new List<FloatMenuGridOption>();

            GUI.DrawTexture(gridLayout.GetCellRect(0, 0), Command.BGTex);
            if (Widgets.ButtonImage(gridLayout.GetCellRect(0, 0), GetCommandTexture(master.SquadState), true, GetSquadStateString(master.SquadState)))
            {
                options.Add(new FloatMenuGridOption(GetCommandTexture(SquadMemberState.CalledToArms), () =>
                {
                    master.SetAllState(SquadMemberState.CalledToArms);

                }, null, new TipSignal(GetSquadStateString(SquadMemberState.CalledToArms))));

                options.Add(new FloatMenuGridOption(GetCommandTexture(SquadMemberState.AtEase), () =>
                {
                    master.SetAllState(SquadMemberState.AtEase);

                }, null, new TipSignal(GetSquadStateString(SquadMemberState.AtEase))));

                Find.WindowStack.Add(new FloatMenuGrid(options));
            }
        }

        private void DrawOrderFloatGrid(GridLayout gridLayout)
        {
            GUI.DrawTexture(gridLayout.GetCellRect(1, 0), Command.BGTex);
            if (Widgets.ButtonImage(gridLayout.GetCellRect(1, 0), TexCommand.SquadAttack, true, "Extra Orders"))
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
                                master.IssueGlobalOrder(item, target);
                            });
                        }, null, new TipSignal(item.defName)));
                    }
                    else
                    {
                        extraOrders.Add(new FloatMenuGridOption(item.Icon, () =>
                        {
                            master.IssueGlobalOrder(item, null);
                        }));

                    }
                }

                Find.WindowStack.Add(new FloatMenuGrid(extraOrders));
            }
        }


        private string GetSquadStateString(SquadMemberState CalledToArms)
        {
            switch (CalledToArms)
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

        private Texture2D GetCommandTexture(SquadMemberState CalledToArms)
        {

            switch (CalledToArms)
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