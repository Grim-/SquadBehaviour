using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class ITab_PatrolPathDebug : InspectTabBase
    {
        private Vector2 scrollPosition;
        private const float LineHeight = 22f;

        public ITab_PatrolPathDebug()
        {
            size = new Vector2(400f, 480f);
            labelKey = "TabPatrolDebug";
            tutorTag = "PatrolDebug";
        }

        private Zone_PatrolPath PatrolPath => (Zone_PatrolPath)Find.Selector.SingleSelectedObject;

        protected override bool StillValid => Find.Selector.SingleSelectedObject is Zone_PatrolPath;

        protected override float PaneTopY
        {
            get
            {
                return (float)UI.screenHeight - 165f - 35f;
            }
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Widgets.BeginGroup(rect);

            float curY = 0f;

            // Header info
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, curY, rect.width, 30f), "Patrol Path Debug");
            curY += 35f;

            Text.Font = GameFont.Small;

            // Basic stats
            Widgets.Label(new Rect(0f, curY, rect.width, LineHeight),
                $"Total cells: {PatrolPath.cells.Count}");
            curY += LineHeight;

            Widgets.Label(new Rect(0f, curY, rect.width, LineHeight),
                $"Ordered cells: {PatrolPath.orderedCells.Count}");
            curY += LineHeight;

            if (PatrolPath.orderedCells.Count > 1)
            {
                float distance = CalculatePathDistance(PatrolPath.orderedCells);
                Widgets.Label(new Rect(0f, curY, rect.width, LineHeight),
                    $"Path distance: {distance:F1} cells");
                curY += LineHeight;
            }

            Widgets.Label(new Rect(0f, curY, rect.width, LineHeight),
                $"Auto patrol: {(PatrolPath.allowAutoPatrol ? "Enabled" : "Disabled")}");
            curY += LineHeight;

            // Bounds info
            if (PatrolPath.orderedCells.Count > 0)
            {
                CellRect bounds = CellRect.FromCellList(PatrolPath.orderedCells);
                Widgets.Label(new Rect(0f, curY, rect.width, LineHeight),
                    $"Bounds: {bounds.Width}x{bounds.Height} ({bounds})");
                curY += LineHeight;
            }

            curY += 10f;

            // Reorder button
            if (Widgets.ButtonText(new Rect(0f, curY, 120f, 30f), "Reorder Cells"))
            {
                PatrolPath.ReorderCells();
            }
            curY += 35f;

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, curY, rect.width, LineHeight), "Ordered cell list:");
            curY += LineHeight;

            float listHeight = rect.height - curY;
            Rect viewRect = new Rect(0f, 0f, rect.width - 20f, PatrolPath.orderedCells.Count * LineHeight);

            Widgets.BeginScrollView(new Rect(0f, curY, rect.width, listHeight),
                ref scrollPosition, viewRect);

            float listY = 0f;
            for (int i = 0; i < PatrolPath.orderedCells.Count; i++)
            {
                IntVec3 cell = PatrolPath.orderedCells[i];
                Rect rowRect = new Rect(0f, listY, viewRect.width, LineHeight);

                if (i % 2 == 1)
                {
                    Widgets.DrawLightHighlight(rowRect);
                }

                Widgets.Label(new Rect(5f, listY, 200f, LineHeight),
                    $"{i}: {cell}");

                if (i < PatrolPath.orderedCells.Count - 1)
                {
                    float dist = cell.DistanceTo(PatrolPath.orderedCells[i + 1]);
                    Widgets.Label(new Rect(210f, listY, 100f, LineHeight),
                        $"→ {dist:F1}");
                }
                else if (PatrolPath.orderedCells.Count > 1)
                {
                    float dist = cell.DistanceTo(PatrolPath.orderedCells[0]);
                    Widgets.Label(new Rect(210f, listY, 100f, LineHeight),
                        $"→ {dist:F1}");
                }

                listY += LineHeight;
            }

            Widgets.EndScrollView();
            Widgets.EndGroup();
        }

        private float CalculatePathDistance(List<IntVec3> path)
        {
            if (path.Count <= 1)
                return 0f;

            float totalDistance = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                totalDistance += path[i].DistanceTo(path[i + 1]);
            }

            totalDistance += path[path.Count - 1].DistanceTo(path[0]);
            return totalDistance;
        }

        protected override void CloseTab()
        {
        }
    }
}