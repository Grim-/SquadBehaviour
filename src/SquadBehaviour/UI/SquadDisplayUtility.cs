using RimWorld;
using System;
using System.Collections.Generic;
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
        public Dictionary<int, bool> squadFoldouts = new Dictionary<int, bool>();
        public Dictionary<int, bool> settingsFoldouts = new Dictionary<int, bool>();

        private const float DRAG_HIGHLIGHT_ALPHA = 0.3f;

        private Dictionary<int, Rect> squadHeaderRects = new Dictionary<int, Rect>();

        private Pawn interactingPawn = null;
        private Squad interactingSquad = null;

        public SquadDisplayUtility()
        {
            squadFoldouts = new Dictionary<int, bool>();
            settingsFoldouts = new Dictionary<int, bool>();
        }

        /// <summary>
        /// Draws all squads and their members with drag and drop support
        /// </summary>
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
            Rect viewRect = new Rect(0f, 0f, contentRect.width - 20f, viewHeight);

            DragDropManager.UpdateDrag();

            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);

            float curY = 0f;

            // Draw each squad
            foreach (var squadEntry in activeSquads)
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
                curY += 10f;
            }

            Widgets.EndScrollView();

            DrawDraggedPawn();
        }

        /// <summary>
        /// Draws the dragged pawn following the mouse cursor
        /// </summary>
        private void DrawDraggedPawn()
        {
            if (DragDropManager.IsDragConfirmed)
            {
                Vector2 mousePos = Event.current.mousePosition;
                Rect dragIconRect = new Rect(mousePos.x - IconSize / 2, mousePos.y - IconSize / 2, IconSize, IconSize);
                GUI.DrawTexture(dragIconRect, PortraitsCache.Get(DragDropManager.DraggedPawn, new Vector2(IconSize, IconSize), Rot4.South));

                // Draw label
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Tiny;
                Rect labelRect = new Rect(mousePos.x - 50f, mousePos.y + IconSize / 2 + 2f, 100f, 20f);
                GUI.color = Color.white;
                Widgets.Label(labelRect, "Drop on squad");
                Text.Font = GameFont.Small;

                GUI.changed = true;
            }
        }

        /// <summary>
        /// Draws a single squad header row
        /// </summary>
        public float DrawSquadHeader(float width, float yPos, Squad squad, int squadId, Comp_PawnSquadLeader leader,
                                    ref bool expanded, ref bool settingsExpanded)
        {
            float squadHeaderHeight = SquadRowHeight;
            Rect squadHeaderRect = new Rect(0f, yPos, width, squadHeaderHeight);

            if (squadId % 2 == 0)
            {
                Widgets.DrawHighlight(squadHeaderRect);
            }

            RowLayoutManager headerLayout = new RowLayoutManager(squadHeaderRect, 5f);

            Rect foldoutRect = headerLayout.NextRect(30f, 0f);
            if (Widgets.ButtonImage(foldoutRect, expanded ? TexButton.Collapse : TexButton.Reveal))
            {
                expanded = !expanded;
            }

            Rect nameRect = headerLayout.NextRect(120f, 5f);
            Widgets.DrawBoxSolidWithOutline(nameRect, Color.clear, Color.grey);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(nameRect, $"{squad.squadName}");
            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(nameRect))
            {
                Widgets.DrawHighlightIfMouseover(nameRect);
            }

            Rect formationRect = headerLayout.NextRect(30f);
            SquadWidgets.DrawSquadFormationSelector(squad, formationRect);


            Rect hostilityRect = headerLayout.NextRect(30f, 5f);
            SquadWidgets.DrawSquadHostilitySelector(squad, hostilityRect);

            SquadWidgets.DrawSquadStateSelector(squad, headerLayout.NextRect(30f, 5f));

            SquadWidgets.DrawSquadOrderFloatGrid(squad, headerLayout.NextRect(30f, 5f));


            Rect disbandSquadRect = headerLayout.NextRect(30f);
            if (Widgets.ButtonImage(disbandSquadRect, TexCommand.DropCarriedPawn, true, "Disband Squad"))
            {
                squad.DisbandSquad();
            }




            Rect settingsRect = headerLayout.NextRect(30f, 5f);
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

        /// <summary>
        /// Draws a single member row with drag and drop support
        /// </summary>
        public float DrawMemberRow(Pawn pawn, float width, float yPos, Comp_PawnSquadLeader leader, Squad currentSquad, bool isLeader = false)
        {
            Rect rowRect = new Rect(20f, yPos, width - 20f, MemberRowHeight);

            bool isBeingDragged = DragDropManager.DraggedPawn == pawn && DragDropManager.IsDragConfirmed;
            if (isBeingDragged)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.3f);
            }

            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }

            RowLayoutManager memberLayout = new RowLayoutManager(rowRect, 5f, 2f);

            Rect portraitRect = memberLayout.NextRect(IconSize, 5f);
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
            Rect nameRect = memberLayout.NextRect(150f, 5f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(nameRect, $"{pawn.LabelShort}{roleLabel}");
            Text.Anchor = TextAnchor.UpperLeft;

            float healthPct = pawn.health.summaryHealth.SummaryHealthPercent;
            Rect healthRect = memberLayout.NextRect(150f, 5f);
            Widgets.FillableBar(healthRect, healthPct);

            Rect removeButtonRect = memberLayout.NextRect(60f);
            if (Widgets.ButtonText(removeButtonRect, "Remove"))
            {
                if (leader != null)
                {
                    leader.RemoveFromSquad(pawn);
                }
            }

            if (isBeingDragged)
            {
                GUI.color = Color.white;
            }

            return MemberRowHeight;
        }

        /// <summary>
        /// Draws additional squad settings when expanded
        /// </summary>
        private float DrawSquadSettings(float width, float yPos, Squad squad)
        {
            float height = 120f;
            Rect settingsRect = new Rect(20f, yPos, width - 20f, height);
            Widgets.DrawLightHighlight(settingsRect);

            float currentY = settingsRect.y + 10f;
            float rowHeight = 30f;

            Rect followDistanceRect = new Rect(settingsRect.x + 10f, currentY, settingsRect.width - 20f, rowHeight);
            Rect labelRect = followDistanceRect.LeftPart(0.4f);
            Rect sliderRect = followDistanceRect.RightPart(0.55f);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, "Follow Distance:");
            Text.Anchor = TextAnchor.UpperLeft;
            float oldFollowDistance = squad.FollowDistance;
            float newFollowDistance = squad.FollowDistance;
            Widgets.HorizontalSlider(
                sliderRect,
                ref newFollowDistance,
                new FloatRange(1f, 30f),
                $"{newFollowDistance:F1}",
                roundTo: 0.5f
            );
            if (newFollowDistance != oldFollowDistance)
            {
                squad.SetFollowDistance(newFollowDistance);
            }

            currentY += rowHeight;
            Rect aggressionDistanceRect = new Rect(settingsRect.x + 10f, currentY, settingsRect.width - 20f, rowHeight);
            labelRect = aggressionDistanceRect.LeftPart(0.4f);
            sliderRect = aggressionDistanceRect.RightPart(0.55f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, "Aggression Distance:");
            Text.Anchor = TextAnchor.UpperLeft;
            float newAggressionDistance = squad.AggresionDistance;
            Widgets.HorizontalSlider(
                sliderRect,
                ref newAggressionDistance,
                new FloatRange(1f, 30f),
                $"{newAggressionDistance:F1}",
                roundTo: 0.5f
            );
            if (newAggressionDistance != squad.AggresionDistance)
            {
                squad.AggresionDistance = newAggressionDistance;
            }

            currentY += rowHeight;
            Rect formationCheckboxRect = new Rect(settingsRect.x + 10f, currentY, 200f, rowHeight);
            bool inFormation = squad.InFormation;
            Widgets.CheckboxLabeled(formationCheckboxRect, "Maintain Formation", ref inFormation);
            if (inFormation != squad.InFormation)
            {
                squad.SetInFormation(inFormation);
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            return height;
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



        /// <summary>
        /// Shows the state change menu for undead pawns
        /// </summary>
        private void ShowStateChangeMenu(Comp_PawnSquadMember squadMember)
        {
            List<FloatMenuOption> stateOptions = new List<FloatMenuOption>();
            stateOptions.Add(new FloatMenuOption("Call To Arms", () =>
            {
                squadMember.SetCurrentMemberState(SquadMemberState.CalledToArms);
            }));
            stateOptions.Add(new FloatMenuOption("At Ease", () =>
            {
                squadMember.SetCurrentMemberState(SquadMemberState.AtEase);
            }));
            stateOptions.Add(new FloatMenuOption("Do Nothing", () =>
            {
                squadMember.SetCurrentMemberState(SquadMemberState.DoNothing);
            }));

            Find.WindowStack.Add(new FloatMenu(stateOptions));
        }

        /// <summary>
        /// Calculates the total height needed for all squads
        /// </summary>
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
                    height += 70f;
                }

                if (squadFoldouts.ContainsKey(squadId) && squadFoldouts[squadId])
                {
                    int memberCount = 0;
                    if (squad.Leader != null) memberCount++;
                    if (squad.Members != null) memberCount += squad.Members.Count;
                    if (squad.Leader != null && squad.Members.Contains(squad.Leader)) memberCount--;

                    height += memberCount * MemberRowHeight;
                }

                height += 10f;
            }

            return Math.Max(height, 50f);
        }
    }
}