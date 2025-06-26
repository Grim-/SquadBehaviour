using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class Window_SquadFormationEditor : Window
    {
        private Comp_PawnSquadLeader squadLeader;
        private Squad selectedSquad;
        private Dictionary<Pawn, Vector2> pawnPositions = new Dictionary<Pawn, Vector2>();
        private Pawn draggingPawn = null;
        private Vector2 dragOffset;
        private float cellSize = 40f;
        private Vector2 gridCenter;
        private int gridRadius = 10;
        private Rect currentGridRect;
        private Color LeaderColor = new Color(0.8f, 0.8f, 0.2f, 0.8f);
        private Color UnitColor = new Color(0.2f, 0.5f, 0.8f, 0.8f);
        private Color gridColor = new Color(1f, 1f, 1f, 0.8f);
        private Color gridFarColor = new Color(1f, 1f, 1f, 0.1f);
        public override Vector2 InitialSize => new Vector2(800f, 600f);

        public Window_SquadFormationEditor(Comp_PawnSquadLeader leader, Squad squad)
        {
            squadLeader = leader;
            selectedSquad = squad;
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;

            gridCenter = new Vector2(400f, 250f);
            InitializePawnPositions();
        }

        private void InitializePawnPositions()
        {
            pawnPositions.Clear();

            pawnPositions[squadLeader.Pawn] = gridCenter;

            foreach (var member in selectedSquad.Members)
            {
                if (member.TryGetComp<Comp_PawnSquadMember>(out var squadMemberComp))
                {
                    IntVec3 offset = squadMemberComp.CustomFormationOffset;
                    if (offset == IntVec3.Invalid)
                    {
                        offset = squadLeader.GetFormationPositionFor(member) - squadLeader.Pawn.Position;
                    }

                    Vector2 pos = gridCenter + new Vector2(offset.x * cellSize, -offset.z * cellSize);
                    pawnPositions[member] = pos;
                }
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "Squad Formation Editor");
            Text.Anchor = TextAnchor.UpperLeft;

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 40f, inRect.width, 25f), $"Squad {selectedSquad.squadName} - Drag pawns to set custom positions");

            currentGridRect = new Rect(0f, 70f, inRect.width, inRect.height - 120f);
            DrawFormationGrid(currentGridRect);

            Rect buttonRect = new Rect(inRect.width / 2f - 100f, inRect.height - 40f, 200f, 35f);
            if (Widgets.ButtonText(buttonRect, "Save Formation"))
            {
                SaveFormation();
                Close();
            }
        }


        private void DrawFormationGrid(Rect rect)
        {
            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int z = -gridRadius; z <= gridRadius; z++)
                {
                    Vector2 cellPos = gridCenter + new Vector2(x * cellSize, z * cellSize);
                    Vector2 screenPos = new Vector2(rect.x + cellPos.x, rect.y + cellPos.y);
                    Rect cellRect = new Rect(screenPos.x - cellSize / 2f, screenPos.y - cellSize / 2f, cellSize, cellSize);
                    float t = Mathf.Clamp01(Vector2.Distance(cellPos, gridCenter) / (gridRadius * cellSize));
                    Widgets.DrawBoxSolidWithOutline(cellRect, Color.clear, Color.Lerp(gridColor, gridFarColor, t));
                }
            }
            HandleDragInput(rect);
            foreach (var kvp in pawnPositions.ToList())
            {
                Pawn pawn = kvp.Key;
                Vector2 pos = kvp.Value;
                DrawPawnIcon(pawn, pos, rect);
            }
        }

        private void HandleDragInput(Rect gridRect)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector2 mousePos = Event.current.mousePosition;

                foreach (var kvp in pawnPositions)
                {
                    if (kvp.Key == squadLeader.Pawn) continue;

                    Vector2 screenPos = new Vector2(gridRect.x + kvp.Value.x, gridRect.y + kvp.Value.y);
                    Rect pawnRect = new Rect(screenPos.x - cellSize / 2f, screenPos.y - cellSize / 2f, cellSize, cellSize);

                    if (pawnRect.Contains(mousePos))
                    {
                        draggingPawn = kvp.Key;
                        dragOffset = mousePos - screenPos;
                        Event.current.Use();
                        break;
                    }
                }
            }
            else if (Event.current.type == EventType.MouseDrag && draggingPawn != null)
            {
                Vector2 mousePos = Event.current.mousePosition;
                Vector2 newScreenPos = mousePos - dragOffset;
                Vector2 newGridPos = new Vector2(newScreenPos.x - gridRect.x, newScreenPos.y - gridRect.y);
                Vector2 snappedPos = SnapToGrid(newGridPos);
                pawnPositions[draggingPawn] = snappedPos;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp && draggingPawn != null)
            {
                draggingPawn = null;
                Event.current.Use();
            }
        }


        private void DrawPawnIcon(Pawn pawn, Vector2 gridPosition, Rect gridRect)
        {
            Vector2 screenPos = new Vector2(gridRect.x + gridPosition.x, gridRect.y + gridPosition.y);
            Rect pawnRect = new Rect(screenPos.x - cellSize / 2f, screenPos.y - cellSize / 2f, cellSize, cellSize);

            bool isLeader = pawn == squadLeader.Pawn;
            Color bgColor = isLeader ? LeaderColor : UnitColor;

            if (draggingPawn == pawn)
            {
                bgColor.a = 0.5f;
            }

            Widgets.DrawBoxSolid(pawnRect, bgColor);
            Widgets.ThingIcon(pawnRect, pawn);

            if (Mouse.IsOver(pawnRect) && !isLeader && draggingPawn == null)
            {
                Widgets.DrawHighlight(pawnRect);
                TooltipHandler.TipRegion(pawnRect, pawn.LabelCap);
            }
        }

        private Vector2 SnapToGrid(Vector2 position)
        {
            float x = Mathf.Round((position.x - gridCenter.x) / cellSize) * cellSize + gridCenter.x;
            float z = Mathf.Round((position.y - gridCenter.y) / cellSize) * cellSize + gridCenter.y;
            return new Vector2(x, z);
        }

        private void SaveFormation()
        {
            Vector2 leaderPos = pawnPositions[squadLeader.Pawn];

            foreach (var member in selectedSquad.Members)
            {
                if (member.TryGetComp<Comp_PawnSquadMember>(out var squadMemberComp))
                {
                    if (pawnPositions.TryGetValue(member, out Vector2 memberPos))
                    {
                        Vector2 relativePos = memberPos - leaderPos;
                        int offsetX = Mathf.RoundToInt(relativePos.x / cellSize);
                        int offsetZ = -Mathf.RoundToInt(relativePos.y / cellSize);

                        squadMemberComp.CustomFormationOffset = new IntVec3(offsetX, 0, offsetZ);
                    }
                }
            }

            Messages.Message($"Custom formation saved for Squad {selectedSquad.squadID}", MessageTypeDefOf.PositiveEvent);
        }

        public static void OpenFormationEditor(Comp_PawnSquadLeader leader, Squad squad)
        {
            Find.WindowStack.Add(new Window_SquadFormationEditor(leader, squad));
        }
    }
}