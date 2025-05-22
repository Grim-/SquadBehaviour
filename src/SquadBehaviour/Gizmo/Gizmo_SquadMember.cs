using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    [StaticConstructorOnStartup]
    public class Gizmo_SquadMember : Gizmo
    {
        private Comp_PawnSquadMember member;
        private static readonly Vector2 BaseSize = new Vector2(180f, 80f);
        private static readonly Color BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        private static readonly Texture2D ClearDutyIcon = TexCommand.ClearPrioritizedWork;
        private static readonly Texture2D StateIcon = TexCommand.ClearPrioritizedWork;
        private static readonly Texture2D OrdersIcon = TexCommand.SquadAttack;
        private static readonly Texture2D AbilitiesIcon = TexCommand.DesirePower;


        private float margin = 8f;

        public Gizmo_SquadMember(Comp_PawnSquadMember member)
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

            GUI.DrawTexture(baseRect, Command.BGTex);

            float currentY = baseRect.y + margin / 2;

            DrawHeader(new Rect(baseRect.x + margin / 2, currentY, baseRect.width - margin, 20f));
            currentY += 18f;

            Rect dutyRect = new Rect(baseRect.x + margin / 2, currentY, baseRect.width - margin, 18f);
            Widgets.Label(dutyRect, $"Duty: {member._CurrentStance?.defName ?? "None"}");

            currentY += 18f;

            float buttonSize = 22f;
            float buttonPadding = 2f;
            float startX = baseRect.xMax - (buttonSize * 3) - (buttonPadding * 2) - 4f;
            float buttonY = baseRect.yMax - buttonSize - 4f;


            Rect clearDutyRect = new Rect(startX, buttonY, buttonSize, buttonSize);
            if (Widgets.ButtonImage(clearDutyRect, ClearDutyIcon))
            {
                member._CurrentStance = null;
                member.ClearDefendPoint();
            }
            if (Mouse.IsOver(clearDutyRect))
            {
                TooltipHandler.TipRegion(clearDutyRect, "Clear current duty");
            }

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


            Rect abilitiesRect = new Rect(stateRect.xMax + buttonPadding, buttonY, buttonSize, buttonSize);
            if (Widgets.ButtonImage(abilitiesRect, AbilitiesIcon))
            {
                member.AbilitiesAllowed = !member.AbilitiesAllowed;
            }

            // Draw colored background to indicate current state
            if (member.AbilitiesAllowed)
            {
                Widgets.DrawBoxSolid(abilitiesRect.ContractedBy(2f), new Color(0.2f, 0.8f, 0.2f, 0.3f));
            }
            else
            {
                Widgets.DrawBoxSolid(abilitiesRect.ContractedBy(2f), new Color(0.8f, 0.2f, 0.2f, 0.3f));
            }

            if (Mouse.IsOver(abilitiesRect))
            {
                TooltipHandler.TipRegion(abilitiesRect, member.AbilitiesAllowed ?
                    "Abilities allowed: Yes (click to disable)" :
                    "Abilities allowed: No (click to enable)");
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

            float iconSize = 16f;
            float padding = 4f;
            Rect iconRect = new Rect(baseRect.x + padding, baseRect.y, iconSize, iconSize);
            Rect labelRect = new Rect(iconRect.xMax + padding, baseRect.y, baseRect.width - iconSize - padding * 2, baseRect.height);

            if (member._CurrentStance != null && member._CurrentStance.Tex != null)
            {
                //GUI.DrawTexture(iconRect, member.CurrentStance.Tex);

                if (Mouse.IsOver(iconRect))
                {
                    TooltipHandler.TipRegion(baseRect, member._CurrentStance.label);
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
        }
    }
}