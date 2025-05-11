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
        private Comp_PawnSquadLeader master;
        private static readonly Vector2 BaseSize = new Vector2(140f, 80f);
        private static readonly Color BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private const float ButtonGridWidth = 140f;
        private const float ButtonSize = 35f;

        public Gizmo_FormationControl(Comp_PawnSquadLeader master)
        {
            this.master = master;
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            float width = BaseSize.x;
            if (master.ShowExtraOrders && master.ActiveSquads != null && master.ActiveSquads.Count > 0)
            {
                width += master.ActiveSquads.Count * ButtonGridWidth;
            }
            return width;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect baseRect = new Rect(topLeft.x, topLeft.y, BaseSize.x, BaseSize.y);
            GUI.DrawTexture(baseRect, Command.BGTex);

            float buttonY = topLeft.y;
            float buttonX = topLeft.x;

            Rect toggleRect = new Rect(buttonX, buttonY, ButtonSize, ButtonSize);
            DrawShowSquadOverviewToggle(toggleRect);
            buttonX += ButtonSize;

            Rect formationRect = new Rect(buttonX, buttonY, ButtonSize, ButtonSize);
            SquadWidgets.DrawFormationSelector(master, formationRect);
            buttonX += ButtonSize;

            Rect dutyRect = new Rect(buttonX, buttonY, ButtonSize, ButtonSize);
            DrawDutyFloatGrid(dutyRect);
            buttonX += ButtonSize;

            Rect orderRect = new Rect(buttonX, buttonY, ButtonSize, ButtonSize);
            SquadWidgets.DrawOrderFloatGrid(master, orderRect);

            if (master.ShowExtraOrders && master.ActiveSquads != null && master.ActiveSquads.Count > 0)
            {
                float xPosition = topLeft.x + BaseSize.x;
                float extraWidth = master.ActiveSquads.Count * ButtonGridWidth;
                Rect extraRect = new Rect(xPosition, topLeft.y, extraWidth, BaseSize.y);
                GUI.DrawTexture(extraRect, Command.BGTex);

                float squadButtonX = xPosition;
                foreach (KeyValuePair<int, Squad> squad in master.ActiveSquads)
                {
                    Rect squadCellRect = new Rect(squadButtonX, topLeft.y, ButtonGridWidth, BaseSize.y);
                    DrawSquadContainer(squadCellRect, squad.Value);
                    squadButtonX += ButtonGridWidth;
                }
            }

            return new GizmoResult(GizmoState.Clear);
        }

        private void DrawShowSquadOverviewToggle(Rect rect)
        {
            if (Widgets.ButtonImage(rect, master.ShowExtraOrders ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                master.ShowExtraOrders = !master.ShowExtraOrders;
            }

            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, "Show Extra Orders");
            }
        }

        private void DrawSquadContainer(Rect rect, Squad squad)
        {
            Widgets.Label(rect, $"Squad {squad.squadID}");
        }

        private void DrawDutyFloatGrid(Rect rect)
        {
            SquadWidgets.DrawGlobalStateSelector(this.master, rect);
        }

        private void DrawOrderFloatGrid(Rect rect)
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
                                master.IssueGlobalOrder(item, target);
                            });
                        }, null, new TipSignal(item.defName)));
                    }
                    else
                    {
                        extraOrders.Add(new FloatMenuGridOption(item.Icon, () =>
                        {
                            master.IssueGlobalOrder(item, null);
                        }, null, new TipSignal(item.defName)));
                    }
                }

                Find.WindowStack.Add(new FloatMenuGrid(extraOrders));
            }
        }
    }
}