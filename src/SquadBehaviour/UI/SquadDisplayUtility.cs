using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class SquadDisplayUtility
    {
        public float SquadRowHeight = 45f;
        public float MemberRowHeight = 24f;
        public float IconSize = 24f;
        public float DefaultSpacing = 5f;
        public float SettingsSectionHeight = 180f;
        public Dictionary<int, bool> squadFoldouts = new Dictionary<int, bool>();
        public Dictionary<int, bool> settingsFoldouts = new Dictionary<int, bool>();

        private const float DRAG_HIGHLIGHT_ALPHA = 0.3f;
        private const float SCROLLVIEW_WIDTH_OFFSET = 20f;
        private const float SQUAD_ENTRY_VERTICAL_SPACING = 10f;
        private const float SQUAD_HEADER_FOLDOUT_BUTTON_WIDTH = 30f;
        private const float SQUAD_HEADER_NAME_WIDTH = 120f;
        private const float SQUAD_HEADER_WIDGET_WIDTH = 30f;
        private const float DRAGGED_PAWN_LABEL_OFFSET_X = 50f;
        private const float DRAGGED_PAWN_LABEL_OFFSET_Y = 2f;
        private const float DRAGGED_PAWN_LABEL_WIDTH = 100f;
        private const float DRAGGED_PAWN_LABEL_HEIGHT = 20f;
        private const float MEMBER_ROW_HORIZONTAL_INDENT = 20f;
        private const float MEMBER_ROW_NAME_WIDTH = 100f;
        private const float MEMBER_ROW_HEALTH_BAR_WIDTH = 80f;
        private const float MEMBER_ROW_BUTTON_SIZE = 22f;
        private const float MEMBER_ROW_BUTTON_SPACING = 2f;
        private const float MEMBER_ROW_REMOVE_BUTTON_WIDTH = 60f;
        private const float SETTINGS_SECTION_HORIZONTAL_PADDING = 20f;
        private const float SETTINGS_SECTION_VERTICAL_PADDING = 10f;
        private const float SETTINGS_SECTION_ROW_HEIGHT = 30f;
        private const float SETTINGS_LABEL_WIDTH_PERCENT = 0.4f;
        private const float SETTINGS_SLIDER_WIDTH_PERCENT = 0.55f;
        private const float FOLLOW_DISTANCE_MIN = 1f;
        private const float FOLLOW_DISTANCE_MAX = 30f;
        private const float FOLLOW_DISTANCE_ROUND_TO = 0.5f;
        private const float AGGRESSION_DISTANCE_MIN = 1f;
        private const float AGGRESSION_DISTANCE_MAX = 30f;
        private const float AGGRESSION_DISTANCE_ROUND_TO = 0.5f;
        private const float FORMATION_CHECKBOX_WIDTH = 200f;
        private const float MINIMUM_LIST_HEIGHT = 50f;
        private const float BUTTON_MARGIN = 5f;


        private Dictionary<int, Rect> squadHeaderRects = new Dictionary<int, Rect>();

        private Pawn interactingPawn = null;
        private Squad interactingSquad = null;

        public SquadDisplayUtility()
        {
            squadFoldouts = new Dictionary<int, bool>();
            settingsFoldouts = new Dictionary<int, bool>();
        }

        public void DrawSquadsList(Rect contentRect, ref Vector2 scrollPosition, Dictionary<int, Squad> activeSquads, Comp_PawnSquadLeader leader)
        {
            if (activeSquads == null || activeSquads.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(contentRect, "No squads available.");
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            squadHeaderRects.Clear();

            float viewHeight = CalculateTotalHeight(activeSquads);
            Rect viewRect = new Rect(0f, 0f, contentRect.width - SCROLLVIEW_WIDTH_OFFSET, viewHeight);

            DragDropManager.UpdateDrag();

            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);

            float curY = 0f;

            foreach (var squadEntry in activeSquads.ToArray())
            {
                int squadId = squadEntry.Key;
                Squad squad = squadEntry.Value;

                if (!squadFoldouts.ContainsKey(squadId))
                {
                    squadFoldouts[squadId] = true;
                }

                if (!settingsFoldouts.ContainsKey(squadId))
                {
                    settingsFoldouts[squadId] = false;
                }

                bool expanded = squadFoldouts[squadId];
                bool settingsExpanded = settingsFoldouts[squadId];

                Rect headerRect = new Rect(0f, curY, viewRect.width, SquadRowHeight);
                squadHeaderRects[squadId] = headerRect;

                curY += DrawSquadHeader(viewRect.width, curY, squad, squadId, leader, ref expanded, ref settingsExpanded);

                squadFoldouts[squadId] = expanded;
                settingsFoldouts[squadId] = settingsExpanded;

                if (settingsExpanded && expanded)
                {
                    curY += DrawSquadSettings(viewRect.width, curY, squad);
                }

                if (expanded)
                {
                    if (squad.Members != null)
                    {
                        foreach (Pawn member in squad.Members.ToArray())
                        {
                            if (member != squad.Leader)
                            {
                                curY += DrawMemberRow(member, viewRect.width, curY, leader, squad);
                            }
                        }
                    }
                }
                curY += SQUAD_ENTRY_VERTICAL_SPACING;
            }

            Widgets.EndScrollView();

            DrawDraggedPawn();
        }

        private void DrawDraggedPawn()
        {
            if (DragDropManager.IsDragConfirmed)
            {
                Vector2 mousePos = Event.current.mousePosition;
                Rect dragIconRect = new Rect(mousePos.x - IconSize / 2, mousePos.y - IconSize / 2, IconSize, IconSize);
                GUI.DrawTexture(dragIconRect, PortraitsCache.Get(DragDropManager.DraggedPawn, new Vector2(IconSize, IconSize), Rot4.South));

                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Tiny;
                Rect labelRect = new Rect(mousePos.x - DRAGGED_PAWN_LABEL_OFFSET_X, mousePos.y + IconSize / 2 + DRAGGED_PAWN_LABEL_OFFSET_Y, DRAGGED_PAWN_LABEL_WIDTH, DRAGGED_PAWN_LABEL_HEIGHT);
                GUI.color = Color.white;
                Widgets.Label(labelRect, "Drop on squad");
                Text.Font = GameFont.Small;

                GUI.changed = true;
            }
        }

        public float DrawSquadHeader(float width, float yPos, Squad squad, int squadId, Comp_PawnSquadLeader leader,
                                     ref bool expanded, ref bool settingsExpanded)
        {
            float squadHeaderHeight = SquadRowHeight;
            Rect squadHeaderRect = new Rect(0f, yPos, width, squadHeaderHeight);

            if (squadId % 2 == 0)
            {
                Widgets.DrawHighlight(squadHeaderRect);
            }

            RowLayoutManager headerLayout = new RowLayoutManager(squadHeaderRect, DefaultSpacing);

            Rect foldoutRect = headerLayout.NextRect(SQUAD_HEADER_FOLDOUT_BUTTON_WIDTH, 0f);
            if (Widgets.ButtonImage(foldoutRect, expanded ? TexButton.Collapse : TexButton.Reveal))
            {
                expanded = !expanded;
            }

            Rect nameRect = headerLayout.NextRect(SQUAD_HEADER_NAME_WIDTH, DefaultSpacing);
            Widgets.DrawBoxSolidWithOutline(nameRect, Color.clear, Color.grey);

            if (Widgets.ButtonInvisible(nameRect))
            {
                Find.WindowStack.Add(new Dialog_RenameSquad(squad));
            }

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(nameRect, $"{squad.squadName}");
            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(nameRect))
            {
                Widgets.DrawHighlightIfMouseover(nameRect);
            }

            SquadWidgets.DrawFormationSelector(headerLayout.NextRect(SQUAD_HEADER_WIDGET_WIDTH, DefaultSpacing), squad.FormationType.Icon, squad.SetFormation);
            SquadWidgets.DrawHostilitySelector(headerLayout.NextRect(SQUAD_HEADER_WIDGET_WIDTH, DefaultSpacing), SquadWidgets.GetHostilityTexture(squad.HostilityResponse), squad.SetHositilityResponse);
            SquadWidgets.DrawStateSelector(headerLayout.NextRect(SQUAD_HEADER_WIDGET_WIDTH, DefaultSpacing), squad.SquadState, squad.SetSquadState, false);
            SquadWidgets.DrawOrderFloatGrid(headerLayout.NextRect(SQUAD_HEADER_WIDGET_WIDTH, DefaultSpacing), TexButton.ExecuteColonist, (SquadOrderDef orderDEf, LocalTargetInfo target) =>
            {
                squad.IssueSquadOrder(orderDEf, target, true);
            });

            Rect settingsRect = headerLayout.NextRect(SQUAD_HEADER_WIDGET_WIDTH, DefaultSpacing);
            if (Widgets.ButtonImage(settingsRect, settingsExpanded ? TexButton.Minus : TexButton.Plus))
            {
                settingsExpanded = !settingsExpanded;
            }
            TooltipHandler.TipRegion(settingsRect, "Advanced Settings");

            if (DragDropManager.IsDragConfirmed &&
                (DragDropManager.OriginSquad == null || DragDropManager.OriginSquad.squadID != squadId))
            {
                bool isHovering = Mouse.IsOver(squadHeaderRect);
                if (isHovering)
                {
                    Widgets.DrawBoxSolid(squadHeaderRect, new Color(0f, 1f, 0f, DRAG_HIGHLIGHT_ALPHA));

                    if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                    {
                        bool success = DragDropManager.TryDropOnSquad(squad, leader);
                        if (success)
                        {
                            Event.current.Use();
                        }
                    }
                }
            }
            return squadHeaderHeight;
        }

        private void DrawSliderSetting(ref float currentY, Rect settingsArea, string label, ref float value, FloatRange range, float roundTo)
        {
            Rect rowRect = new Rect(settingsArea.x + SETTINGS_SECTION_HORIZONTAL_PADDING, currentY, settingsArea.width - SETTINGS_SECTION_HORIZONTAL_PADDING * 2, SETTINGS_SECTION_ROW_HEIGHT);
            Rect labelRect = rowRect.LeftPart(SETTINGS_LABEL_WIDTH_PERCENT);
            Rect sliderRect = rowRect.RightPart(SETTINGS_SLIDER_WIDTH_PERCENT);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.HorizontalSlider(
                sliderRect,
                ref value,
                range,
                $"{value:F1}",
                roundTo: roundTo
            );
            currentY += SETTINGS_SECTION_ROW_HEIGHT;
        }

        private void DrawCheckboxSetting(ref float currentY, Rect settingsArea, string label, ref bool value, Action<bool> setter)
        {
            Rect checkboxRect = new Rect(settingsArea.x + SETTINGS_SECTION_HORIZONTAL_PADDING, currentY, FORMATION_CHECKBOX_WIDTH, SETTINGS_SECTION_ROW_HEIGHT);
            bool oldValue = value;
            Widgets.CheckboxLabeled(checkboxRect, label, ref value);
            if (value != oldValue)
            {
                setter(value);
            }
            currentY += SETTINGS_SECTION_ROW_HEIGHT;
        }

        private void DrawButtonWithFloatMenu(Rect buttonRect, string label, Func<List<FloatMenuOption>> getOptions)
        {
            if (Widgets.ButtonText(buttonRect, label))
            {
                List<FloatMenuOption> menuOptions = getOptions();
                if (menuOptions.Count > 0)
                {
                    Find.WindowStack.Add(new FloatMenu(menuOptions));
                }
            }
        }

        public float DrawMemberRow(Pawn pawn, float width, float yPos, Comp_PawnSquadLeader leader, Squad currentSquad, bool isLeader = false)
        {
            Rect rowRect = new Rect(MEMBER_ROW_HORIZONTAL_INDENT, yPos, width - MEMBER_ROW_HORIZONTAL_INDENT, MemberRowHeight);

            bool isBeingDragged = DragDropManager.DraggedPawn == pawn && DragDropManager.IsDragConfirmed;
            if (isBeingDragged)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.3f);
            }

            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }

            RowLayoutManager memberLayout = new RowLayoutManager(rowRect, DefaultSpacing, MEMBER_ROW_BUTTON_SPACING);

            Rect portraitRect = memberLayout.NextRect(IconSize, DefaultSpacing);
            Widgets.ThingIcon(portraitRect, pawn);

            if (Mouse.IsOver(portraitRect))
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    interactingPawn = pawn;
                    interactingSquad = currentSquad;
                    DragDropManager.StartDrag(pawn, currentSquad);
                    Event.current.Use();
                }
            }

            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
            {
                string tooltip = squadMember.GetStatusReport();
                if (!isBeingDragged)
                {
                    tooltip += "\nDrag to transfer to another squad";
                }
                TooltipHandler.TipRegion(portraitRect, tooltip);

                if (Widgets.ButtonInvisible(portraitRect, true))
                {
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    {
                        Event.current.Use();
                        ShowStateChangeMenu(squadMember);
                    }
                }
            }

            string roleLabel = isLeader ? " (Leader)" : "";
            Rect nameRect = memberLayout.NextRect(MEMBER_ROW_NAME_WIDTH, DefaultSpacing);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(nameRect, $"{pawn.LabelShort}{roleLabel}");
            Text.Anchor = TextAnchor.UpperLeft;

            float healthPct = pawn.health.summaryHealth.SummaryHealthPercent;
            Rect healthRect = memberLayout.NextRect(MEMBER_ROW_HEALTH_BAR_WIDTH, DefaultSpacing);
            Widgets.FillableBar(healthRect, healthPct);

            if (pawn.IsPartOfSquad(out Comp_PawnSquadMember member))
            {
                DrawMemberStateButton(memberLayout, member);
                DrawMemberAbilitiesButton(memberLayout, member);
                DrawMemberOrdersButton(memberLayout, member);
                Rect removeButtonRect = memberLayout.NextRect(MEMBER_ROW_REMOVE_BUTTON_WIDTH);
                if (Widgets.ButtonText(removeButtonRect, "Remove"))
                {
                    member?.LeaveSquad();
                }
            }

            if (isBeingDragged)
            {
                GUI.color = Color.white;
            }

            return MemberRowHeight;
        }

        private void DrawMemberStateButton(RowLayoutManager memberLayout, Comp_PawnSquadMember member)
        {
            Rect stateRect = memberLayout.NextRect(MEMBER_ROW_BUTTON_SIZE, MEMBER_ROW_BUTTON_SPACING);
            if (Widgets.ButtonImage(stateRect, TexCommand.Draft))
            {
                ShowStateChangeMenu(member);
            }
            if (Mouse.IsOver(stateRect))
            {
                TooltipHandler.TipRegion(stateRect, "Set member state");
            }
        }

        private void DrawMemberAbilitiesButton(RowLayoutManager memberLayout, Comp_PawnSquadMember member)
        {
            Rect abilitiesRect = memberLayout.NextRect(MEMBER_ROW_BUTTON_SIZE, MEMBER_ROW_BUTTON_SPACING);
            if (Widgets.ButtonImage(abilitiesRect, TexCommand.DesirePower))
            {
                member.AbilitiesAllowed = !member.AbilitiesAllowed;
            }

            Widgets.DrawBoxSolid(abilitiesRect.ContractedBy(2f), member.AbilitiesAllowed ? new Color(0.2f, 0.8f, 0.2f, 0.3f) : new Color(0.8f, 0.2f, 0.2f, 0.3f));

            if (Mouse.IsOver(abilitiesRect))
            {
                TooltipHandler.TipRegion(abilitiesRect, member.AbilitiesAllowed ?
                    "Abilities allowed: Yes (click to disable)" :
                    "Abilities allowed: No (click to enable)");
            }
        }

        private void DrawMemberOrdersButton(RowLayoutManager memberLayout, Comp_PawnSquadMember member)
        {
            Rect ordersRect = memberLayout.NextRect(MEMBER_ROW_BUTTON_SIZE, MEMBER_ROW_BUTTON_SPACING);
            GUI.DrawTexture(ordersRect, member.CurrentStance?.Tex ?? TexCommand.Attack);

            if (Widgets.ButtonInvisible(ordersRect))
            {
                List<FloatMenuOption> orderOptions = new List<FloatMenuOption>();
                orderOptions.Add(new FloatMenuOption($"Clear Duty", () => {
                    member.CurrentStance = null;
                    member.ClearDefendPoint();
                }));

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
            if (Mouse.IsOver(ordersRect))
            {
                TooltipHandler.TipRegion(ordersRect, member.CurrentStance?.label ?? "Set orders");
            }
            Widgets.DrawHighlightIfMouseover(ordersRect);
        }

        private float DrawSquadSettings(float width, float yPos, Squad squad)
        {
            Rect settingsRect = new Rect(MEMBER_ROW_HORIZONTAL_INDENT, yPos, width - MEMBER_ROW_HORIZONTAL_INDENT, SettingsSectionHeight);
            Widgets.DrawLightHighlight(settingsRect);

            float currentY = settingsRect.y + SETTINGS_SECTION_VERTICAL_PADDING;

            float followDistance = squad.FollowDistance;
            DrawSliderSetting(ref currentY, settingsRect, "Follow Distance:", ref followDistance, new FloatRange(FOLLOW_DISTANCE_MIN, FOLLOW_DISTANCE_MAX), FOLLOW_DISTANCE_ROUND_TO);
            if (followDistance != squad.FollowDistance)
            {
                squad.SetFollowDistance(followDistance);
            }

            float aggressionDistance = squad.AggresionDistance;
            DrawSliderSetting(ref currentY, settingsRect, "Aggression Distance:", ref aggressionDistance, new FloatRange(AGGRESSION_DISTANCE_MIN, AGGRESSION_DISTANCE_MAX), AGGRESSION_DISTANCE_ROUND_TO);
            if (aggressionDistance != squad.AggresionDistance)
            {
                squad.AggresionDistance = aggressionDistance;
            }

            bool inFormation = squad.InFormation;
            DrawCheckboxSetting(ref currentY, settingsRect, "Maintain Formation", ref inFormation, squad.SetInFormation);

            // Calculate button widths based on available space and desired margin
            float totalButtonWidth = (FORMATION_CHECKBOX_WIDTH * 3) + (BUTTON_MARGIN * 2);
            float startX = settingsRect.x + (settingsRect.width - totalButtonWidth) / 2f;
            if (startX < settingsRect.x + SETTINGS_SECTION_HORIZONTAL_PADDING)
            {
                startX = settingsRect.x + SETTINGS_SECTION_HORIZONTAL_PADDING;
            }


            Rect addSquadMemberRect = new Rect(startX, currentY, 130f, SETTINGS_SECTION_ROW_HEIGHT);
            DrawButtonWithFloatMenu(addSquadMemberRect, "Add To Squad", () =>
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
                if (squad.Leader != null && squad.Leader.Spawned)
                {
                    foreach (var item in squad.Leader.Map.mapPawns.AllPawnsSpawned.Where(x => x.Faction != null && x.Faction == Faction.OfPlayer && x != squad.Leader && !squad.Members.Contains(x)))
                    {
                        menuOptions.Add(new FloatMenuOption($"Add {item.LabelShortCap}", () =>
                        {
                            squad.AddMember(item);
                        }));
                    }
                }
                return menuOptions;
            });

            Rect mergeSquadsRect = new Rect(addSquadMemberRect.xMax + BUTTON_MARGIN, currentY, 130f, SETTINGS_SECTION_ROW_HEIGHT);
            DrawButtonWithFloatMenu(mergeSquadsRect, "Merge with..", () =>
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
                if (squad.Leader != null && squad.Leader.Spawned)
                {
                    foreach (var item in squad.SquadLeader.ActiveSquads.Where(x => x.Value != squad))
                    {
                        menuOptions.Add(new FloatMenuOption($"{item.Value.squadName}", () =>
                        {
                            squad.TryMergeFrom(item.Value);
                        }));
                    }
                }
                return menuOptions;
            });

            Rect disbandSquadRect = new Rect(mergeSquadsRect.xMax + BUTTON_MARGIN, currentY, 130f, SETTINGS_SECTION_ROW_HEIGHT);
            if (Widgets.ButtonText(disbandSquadRect, "Disband Squad"))
            {
                squad.DisbandSquad();
            }
            currentY += SETTINGS_SECTION_ROW_HEIGHT;

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            return SettingsSectionHeight;
        }

        public static void DrawFormationSelector(Rect formationRect, Squad squad)
        {
            if (Widgets.ButtonText(formationRect, "Formation: " + squad.FormationType.ToString()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (FormationDef formation in DefDatabase<FormationDef>.AllDefs)
                {
                    options.Add(new FloatMenuOption(
                        formation.label,
                        delegate { squad.SetFormation(formation); }
                    ));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            if (Mouse.IsOver(formationRect))
            {
                TooltipHandler.TipRegion(formationRect, "Change formation");
            }
        }

        private void ShowStateChangeMenu(Comp_PawnSquadMember squadMember)
        {
            List<FloatMenuOption> stateOptions = new List<FloatMenuOption>
            {
                new FloatMenuOption("Call To Arms", () => squadMember.SetCurrentMemberState(SquadMemberState.CalledToArms)),
                new FloatMenuOption("At Ease", () => squadMember.SetCurrentMemberState(SquadMemberState.AtEase)),
                new FloatMenuOption("Do Nothing", () => squadMember.SetCurrentMemberState(SquadMemberState.DoNothing))
            };
            Find.WindowStack.Add(new FloatMenu(stateOptions));
        }

        public float CalculateTotalHeight(Dictionary<int, Squad> activeSquads)
        {
            float height = 0f;

            foreach (var squadEntry in activeSquads)
            {
                int squadId = squadEntry.Key;
                Squad squad = squadEntry.Value;

                height += SquadRowHeight;

                if (settingsFoldouts.ContainsKey(squadId) && settingsFoldouts[squadId])
                {
                    height += SettingsSectionHeight;
                }

                if (squadFoldouts.ContainsKey(squadId) && squadFoldouts[squadId])
                {
                    int memberCount = 0;
                    if (squad.Leader != null)
                        memberCount++;
                    if (squad.Members != null)
                        memberCount += squad.Members.Count;
                    if (squad.Leader != null && squad.Members.Contains(squad.Leader))
                        memberCount--;

                    height += memberCount * MemberRowHeight;
                }

                height += SQUAD_ENTRY_VERTICAL_SPACING;
            }

            return Math.Max(height, MINIMUM_LIST_HEIGHT);
        }
    }
}