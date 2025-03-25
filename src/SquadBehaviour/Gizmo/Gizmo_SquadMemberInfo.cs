using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    [StaticConstructorOnStartup]
    public class Gizmo_SquadMemberInfo : Gizmo
    {
        private ISquadMember member;
        private static readonly Vector2 BaseSize = new Vector2(180f, 80f);
        private static readonly Color BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Cache icons for better performance
        private static readonly Texture2D ClearDutyIcon = ContentFinder<Texture2D>.Get("UI/Commands/ClearPrioritizedWork", true);
        private static readonly Texture2D StateIcon = ContentFinder<Texture2D>.Get("UI/Icons/MentalStateCategories/MentalStateCategorySafe", true);
        private static readonly Texture2D OrdersIcon = TexCommand.SquadAttack;


        private float margin = 8f;

        public Gizmo_SquadMemberInfo(ISquadMember member)
        {
            this.member = member;
            Order = -99f;
        }

        public override float GetWidth(float maxWidth)
        {
            return BaseSize.x;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect baseRect = new Rect(topLeft.x, topLeft.y, BaseSize.x, BaseSize.y);

            // Draw main background
            GUI.DrawTexture(baseRect, Command.BGTex);

            float currentY = baseRect.y + margin /2;

            DrawHeader(new Rect(baseRect.x + margin / 2, currentY, baseRect.width - margin, 20f));
            currentY += 18f;

            // Current duty
            Rect dutyRect = new Rect(baseRect.x + margin / 2, currentY, baseRect.width - margin, 18f);
            Widgets.Label(dutyRect, $"Duty: {member.CurrentStance?.defName ?? "None"}");

            currentY += 18f;

            // Action buttons - at the bottom right
            float buttonSize = 22f;
            float buttonPadding = 2f;
            float startX = baseRect.xMax - (buttonSize * 3) - (buttonPadding * 2) - 4f;
            float buttonY = baseRect.yMax - buttonSize - 4f;

            // Button 1: Clear duty
            Rect clearDutyRect = new Rect(startX, buttonY, buttonSize, buttonSize);
            if (Widgets.ButtonImage(clearDutyRect, ClearDutyIcon))
            {
                member.CurrentStance = null;
                member.ClearDefendPoint();
            }
            if (Mouse.IsOver(clearDutyRect))
            {
                TooltipHandler.TipRegion(clearDutyRect, "Clear current duty");
            }

            // Button 2: Change state
            Rect stateRect = new Rect(clearDutyRect.xMax + buttonPadding, buttonY, buttonSize, buttonSize);
            if (Widgets.ButtonImage(stateRect, StateIcon))
            {
                List<FloatMenuOption> stateOptions = new List<FloatMenuOption>();

                stateOptions.Add(new FloatMenuOption("Call to Arms", () => {
                    member.SetCurrentMemberState(SquadMemberState.CalledToArms);
                }));

                stateOptions.Add(new FloatMenuOption("At Ease", () => {
                    member.SetCurrentMemberState(SquadMemberState.AtEase);
                }));

                stateOptions.Add(new FloatMenuOption("Do Nothing", () => {
                    member.SetCurrentMemberState(SquadMemberState.DoNothing);
                }));

                Find.WindowStack.Add(new FloatMenu(stateOptions));
            }
            if (Mouse.IsOver(stateRect))
            {
                TooltipHandler.TipRegion(stateRect, "Set member state");
            }

            return new GizmoResult(GizmoState.Clear);
        }


        private void DrawHeader(Rect baseRect)
        {
            Text.Anchor = TextAnchor.UpperCenter;

            // Member name and squad ID
            string memberInfo = $"{member.Pawn.NameShortColored}";
            if (member.AssignedSquad != null)
            {
                memberInfo += $" - [Squad {member.AssignedSquad.squadID}]";
            }

            // Create rects for icon and label
            float iconSize = 16f;
            float padding = 4f;
            Rect iconRect = new Rect(baseRect.x + padding, baseRect.y, iconSize, iconSize);
            Rect labelRect = new Rect(iconRect.xMax + padding, baseRect.y, baseRect.width - iconSize - padding * 2, baseRect.height);


            // Show label with icon if one exists, otherwise just show the label
            if (member.CurrentStance != null && member.CurrentStance.Tex != null)
            {
                GUI.DrawTexture(iconRect, member.CurrentStance.Tex); 
                
                if (Mouse.IsOver(iconRect))
                {
                    TooltipHandler.TipRegion(baseRect, member.CurrentStance.label);
                }
            }
            else
            {
                GUI.DrawTexture(iconRect, TexButton.CloseXBig);
            }


            if (Widgets.ButtonInvisible(iconRect))
            {
                List<FloatMenuOption> orderOptions = new List<FloatMenuOption>();

                foreach (SquadOrderDef orderDef in DefDatabase<SquadOrderDef>.AllDefsListForReading)
                {
                    if (orderDef.requiresTarget)
                    {
                        orderOptions.Add(new FloatMenuOption(orderDef.defName, () => {
                            Find.Targeter.BeginTargeting(orderDef.targetingParameters, (LocalTargetInfo target) => {
                                member.IssueOrder(orderDef, target);
                            });
                        }));
                    }
                    else
                    {
                        orderOptions.Add(new FloatMenuOption(orderDef.defName, () => {
                            member.IssueOrder(orderDef, null);
                        }));
                    }
                }

                if (orderOptions.Count == 0)
                {
                    orderOptions.Add(new FloatMenuOption("No orders available", null));
                }

                Find.WindowStack.Add(new FloatMenu(orderOptions));
            }

            Widgets.DrawHighlightIfMouseover(iconRect);


            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(labelRect, memberInfo);

            if (Mouse.IsOver(baseRect))
            {
                string tooltip = "";
                if (member.AssignedSquad != null && member.AssignedSquad.SquadLeader != null)
                {
                    tooltip += $"Leader: {member.AssignedSquad.SquadLeader.SquadLeaderPawn.LabelShort}";
                }
                if (!string.IsNullOrEmpty(tooltip))
                {
                    TooltipHandler.TipRegion(baseRect, tooltip);
                }
            }

          
           // Widgets.DrawBoxSolidWithOutline(baseRect, Color.white * 0.2f, Color.white * 0.15f);
        }

        private void DrawStateIndicator(Rect baseRect)
        {
            SquadMemberState currentState = member.CurrentState;

            // State color indicator
            Color stateColor;
            string stateTooltip;

            switch (currentState)
            {
                case SquadMemberState.CalledToArms:
                    stateColor = new Color(0.8f, 0.2f, 0.2f);
                    stateTooltip = "Combat Ready";
                    break;
                case SquadMemberState.AtEase:
                    stateColor = new Color(0.2f, 0.7f, 0.2f);
                    stateTooltip = "At Ease";
                    break;
                case SquadMemberState.DoNothing:
                    stateColor = new Color(0.6f, 0.6f, 0.6f);
                    stateTooltip = "Idle";
                    break;
                default:
                    stateColor = Color.yellow;
                    stateTooltip = "Unknown State";
                    break;
            }

            // Draw a small colored dot
            Rect stateRect = new Rect(baseRect.x + 4f, baseRect.yMax - 12f, 8f, 8f);
            GUI.color = stateColor;
            GUI.DrawTexture(stateRect, BaseContent.WhiteTex);
            GUI.color = Color.white;

            if (Mouse.IsOver(stateRect))
            {
                TooltipHandler.TipRegion(stateRect, stateTooltip);
            }
        }
    }
}