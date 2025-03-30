using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class SquadManagerWindow : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private string newSquadName = "New Squad";
        private Dictionary<int, string> editingSquadNames = new Dictionary<int, string>();
        private Dictionary<int, bool> editingSquad = new Dictionary<int, bool>();
        private ISquadLeader currentLeader;
        private float rowHeight = 30f;
        private float memberRowHeight = 24f;
        private float iconSize = 24f;

        public override Vector2 InitialSize => new Vector2(700f, 600f);

        public SquadManagerWindow()
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public SquadManagerWindow(ISquadLeader squadLeader)
        {
            currentLeader = squadLeader;
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }


        private void FindCurrentLeader()
        {
            Pawn selectedPawn = Find.Selector.SingleSelectedThing as Pawn;
            if (selectedPawn != null && selectedPawn.Faction == Faction.OfPlayer)
            {
                currentLeader = selectedPawn as ISquadLeader;

                if (currentLeader == null)
                {
                    var potentialLeaders = Find.CurrentMap.mapPawns.FreeColonists
                        .Where(p => p is ISquadLeader)
                        .Select(p => p as ISquadLeader)
                        .ToList();

                    if (potentialLeaders.Any())
                    {
                        currentLeader = potentialLeaders.First();
                    }
                }
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (currentLeader == null)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(inRect, "No squad leader selected. Select a pawn with squad leadership capabilities.");
                Text.Font = GameFont.Small;
                return;
            }

            Rect toolbarRect = inRect.TopPartPixels(40f);
            DrawToolbar(toolbarRect);

            Rect contentRect = new Rect(inRect.x, toolbarRect.yMax + 10f, inRect.width, inRect.height - toolbarRect.height - 50f);
            DrawSquadsList(contentRect);
        }

        private void DrawToolbar(Rect rect)
        {
            Rect backgroundRect = rect;
            backgroundRect.height += 1f;
            Widgets.DrawMenuSection(backgroundRect);

            RowLayoutManager toolbarLayout = new RowLayoutManager(rect, 10f, 5f);

            Rect leaderLabelRect = toolbarLayout.NextRect(100f, 5f);
            Widgets.Label(leaderLabelRect, "Squad Leader:");


            Rect leaderSelectRect = toolbarLayout.NextRect(200f, 20f);
            if (Widgets.ButtonText(leaderSelectRect, currentLeader.SquadLeaderPawn.LabelShort))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach (var pawn in Find.CurrentMap.mapPawns.FreeColonists)
                {
                    if (pawn is ISquadLeader leader)
                    {
                        options.Add(new FloatMenuOption(pawn.LabelShort, () => currentLeader = leader));
                    }
                }

                if (options.Count > 0)
                {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }

            float addButtonWidth = 120f;
            float remainingSpace = toolbarLayout.RemainingWidth;
            toolbarLayout.NextRect(remainingSpace - addButtonWidth);
            Rect addSquadRect = toolbarLayout.NextRect(addButtonWidth);

            if (Widgets.ButtonText(addSquadRect, "Add New Squad"))
            {
                int newSquadId = currentLeader.ActiveSquads.Count > 0
                    ? currentLeader.ActiveSquads.Keys.Max() + 1
                    : 1;

                if (currentLeader.AddSquad(newSquadId))
                {
                    Squad squad = currentLeader.ActiveSquads[newSquadId];
                    squad.squadName = "Squad " + newSquadId;
                    Messages.Message("New squad created.", MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("Failed to create a new squad.", MessageTypeDefOf.RejectInput);
                }
            }
        }

        private void DrawSquadsList(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            if (currentLeader.ActiveSquads == null || currentLeader.ActiveSquads.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "No squads available. Create a new squad using the button above.");
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            float viewHeight = CalculateTotalHeight();
            Rect viewRect = new Rect(0f, 0f, rect.width - 20f, viewHeight);

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float curY = 0f;

            foreach (var squadEntry in currentLeader.ActiveSquads)
            {
                int squadId = squadEntry.Key;
                Squad squad = squadEntry.Value;

                float squadHeaderHeight = rowHeight;
                Rect squadHeaderRect = new Rect(0f, curY, viewRect.width, squadHeaderHeight);

                if (squadId % 2 == 0)
                {
                    Widgets.DrawHighlight(squadHeaderRect);
                }

                RowLayoutManager headerLayout = new RowLayoutManager(squadHeaderRect, 10f, 5f);

                Rect nameRect = headerLayout.NextRect(150f, 10f);

                if (editingSquad.TryGetValue(squadId, out bool isEditing) && isEditing)
                {
                    if (!editingSquadNames.ContainsKey(squadId))
                    {
                        editingSquadNames[squadId] = squad.squadName;
                    }

                    editingSquadNames[squadId] = Widgets.TextField(nameRect, editingSquadNames[squadId]);

                    // Confirm button
                    Rect confirmRect = headerLayout.NextRect(60f, 10f);
                    if (Widgets.ButtonText(confirmRect, "OK"))
                    {
                        squad.squadName = editingSquadNames[squadId];
                        editingSquad[squadId] = false;
                    }
                }
                else
                {
                    Text.Font = GameFont.Medium;
                    Widgets.Label(nameRect, $"#{squadId}: {squad.squadName}");
                    Text.Font = GameFont.Small;
                }

                headerLayout.NextRect(10f, 10f);


                Rect renameRect = headerLayout.NextRect(80f, 10f);
                if (Widgets.ButtonText(renameRect, "Rename"))
                {
                    editingSquad[squadId] = true;
                    if (!editingSquadNames.ContainsKey(squadId))
                    {
                        editingSquadNames[squadId] = squad.squadName;
                    }
                }

                Rect deleteRect = headerLayout.NextRect(80f, 10f);
                if (Widgets.ButtonText(deleteRect, "Remove"))
                {
                    currentLeader.RemoveSquad(squadId);
                    break;
                }

                // Formation type
                Rect formationRect = headerLayout.NextRect(140f, 10f);
                if (Widgets.ButtonImage(formationRect, currentLeader.FormationType.Icon))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (FormationDef formation in DefDatabase<FormationDef>.AllDefs)
                    {
                        options.Add(new FloatMenuOption(
                            formation.label,
                            delegate { currentLeader.SetFormation(formation); }
                        ));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }


                Rect hostilityRect = headerLayout.NextRect(140f);
                if (Widgets.ButtonText(hostilityRect, $"Hostility: {squad.HostilityResponse}"))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (SquadHostility hostility in Enum.GetValues(typeof(SquadHostility)))
                    {
                        options.Add(new FloatMenuOption(hostility.ToString(), () => {
                            squad.SetHositilityResponse(hostility);
                        }));
                    }

                    Find.WindowStack.Add(new FloatMenu(options));
                }

                curY += squadHeaderHeight;

                if (squad.Members != null)
                {
                    if (squad.Leader != null)
                    {
                        DrawMemberRow(squad.Leader, viewRect.width, curY, true);
                        curY += memberRowHeight;
                    }

                    foreach (Pawn member in squad.Members)
                    {
                        if (member != squad.Leader)
                        {
                            DrawMemberRow(member, viewRect.width, curY);
                            curY += memberRowHeight;
                        }
                    }
                }

                curY += 10f;
            }

            Widgets.EndScrollView();
        }

        private void DrawMemberRow(Pawn pawn, float width, float yPos, bool isLeader = false)
        {
            Rect rowRect = new Rect(10f, yPos, width - 20f, memberRowHeight);

            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2)
                {
                    CameraJumper.TryJumpAndSelect(pawn);
                    Event.current.Use();
                }
            }

            // Use RowLayoutManager for member rows
            RowLayoutManager memberLayout = new RowLayoutManager(rowRect, 5f, 2f);

            // Portrait
            Rect portraitRect = memberLayout.NextRect(iconSize, 5f);
            Widgets.ThingIcon(portraitRect, pawn);

            // Name and role
            string roleLabel = isLeader ? " (Leader)" : "";
            Rect nameRect = memberLayout.NextRect(200f, 10f);
            Widgets.Label(nameRect, pawn.LabelShort + roleLabel);

            // Health status
            float healthPct = pawn.health.summaryHealth.SummaryHealthPercent;
            Rect healthRect = memberLayout.NextRect(150f, 10f);
            Widgets.FillableBar(healthRect, healthPct);

            if (!isLeader)
            {
                float remainingWidth = memberLayout.RemainingWidth;
                memberLayout.NextRect(remainingWidth - 60f);

                Rect removeRect = memberLayout.NextRect(60f);
                if (Widgets.ButtonText(removeRect, "Remove"))
                {
                    foreach (var squad in currentLeader.ActiveSquads.Values)
                    {
                        if (squad.Members.Contains(pawn))
                        {
                            squad.RemoveMember(pawn);
                            break;
                        }
                    }
                }
            }
        }

        private float CalculateTotalHeight()
        {
            float height = 0f;

            foreach (var squad in currentLeader.ActiveSquads.Values)
            {
                height += rowHeight; 

                int memberCount = 0;
                if (squad.Leader != null) memberCount++;
                if (squad.Members != null) memberCount += squad.Members.Count;
                if (squad.Leader != null && squad.Members.Contains(squad.Leader)) memberCount--;

                height += memberCount * memberRowHeight;
                height += 10f;
            }

            return Math.Max(height, 50f);
        }
    }
}
