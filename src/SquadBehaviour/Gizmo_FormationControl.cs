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
            return BaseSize.x + (master.ShowExtraOrders ? ButtonGridWidth : 0);
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


            //Widgets.DrawLineHorizontal(baseRect.x, formationRect.yMax + 4, GetWidth(maxWidth) - 8f);

            //next row
            Rect standardOrderGrid = new Rect(baseRect.x, formationRect.yMax + 10f, ButtonGridWidth, 40f);
            DrawStandardOrderGrid(standardOrderGrid);


            if (master.ShowExtraOrders)
            {
                //grid to the far right
                Rect RightButtonGrid = new Rect(baseRect.max.x, topLeft.y, ButtonGridWidth, BaseSize.y);
                DrawGridButtons(RightButtonGrid);
            }


            return new GizmoResult(GizmoState.Clear);
        }

        private void DrawStandardOrderGrid(Rect rect)
        {
            GridLayout gridLayout = new GridLayout(rect, 4, 1);
            Rect cellRect = gridLayout.GetCellRect(0, 0);
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


                options.Add(new FloatMenuGridOption(GetCommandTexture(SquadMemberState.DoNothing), () =>
                {
                    master.SetAllState(SquadMemberState.DoNothing);
                }, null, new TipSignal(GetSquadStateString(SquadMemberState.DoNothing))));

                Find.WindowStack.Add(new FloatMenuGrid(options));
            }



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
                                master.ExecuteSquadOrder(item, target);
                            });
                        }, null, new TipSignal(item.defName)));
                    }
                    else
                    {
                        extraOrders.Add(new FloatMenuGridOption(item.Icon, () =>
                        {
                            master.ExecuteSquadOrder(item, null);
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
                case SquadMemberState.DefendPoint:
                    return "Defend point";
                case SquadMemberState.Patrol:
                    return "Patrol";
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
                case SquadMemberState.DefendPoint:
                    return TexCommand.SquadAttack;
                case SquadMemberState.Patrol:
                    return TexCommand.SquadAttack;
                default:
                    return TexCommand.DesirePower;
            }

        }

        private void DrawGridButtons(Rect GridButtonRect)
        {
            GUI.DrawTexture(GridButtonRect, Command.BGTex);

            GridLayout gridLayout = new GridLayout(GridButtonRect, 3, 2);
            //DrawSquadOrders(gridLayout);
        }

        LocalTargetInfo selectedTarget = null;
        private void DrawSquadOrders(GridLayout gridLayout)
        {
            int startX = 0;
            int startY = 0;

            foreach (var item in DefDatabase<SquadOrderDef>.AllDefsListForReading)
            {
                GUI.DrawTexture(gridLayout.GetCellRect(startX, startY), Command.BGTex);

                if (Widgets.ButtonImage(gridLayout.GetCellRect(startX, startY), item.Icon, true, item.defName))
                {
                    if (item.requiresTarget)
                    {
                        Find.Targeter.BeginTargeting(item.targetingParameters, (LocalTargetInfo target) =>
                        {
                            master.ExecuteSquadOrder(item, target);
                        });
                    }
                    else
                    {
                        master.ExecuteSquadOrder(item, null);
                    }
                }

                startX++;

                if (startX == 3)
                {
                    startX = 0;
                    startY++;
                }
            }
        }
    }
}
