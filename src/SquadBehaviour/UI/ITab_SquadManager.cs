﻿using RimWorld;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{

    //show if the pawn in question is a Squad Leader
    public class ITab_SquadManager : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;
        private const float BUTTON_HEIGHT = 24f;
        private const float SPACING = 10f;
        private int CurrentTabIndex = 0;
        private SquadDisplayUtility squadDisplay;

        private ISquadLeader _SquadLeader;
        private ISquadLeader SquadLeader
        {
            get
            {
                if (_SquadLeader == null)
                {
                    if (this.SelPawn.TryGetSquadLeader(out ISquadLeader squadLeader) && squadLeader.ActiveSquads.Count > 0)
                    {
                        _SquadLeader = squadLeader;
                    }      
                }

                return _SquadLeader;
            }
        }

        public override bool IsVisible
        {
            get
            {
                if (SquadLeader == null)
                {
                    //Log.Message($"Squad leader not found for {SelPawn}");
                    return false;
                }

                return base.IsVisible && this.SelPawn != null && SquadLeader != null;
            }
        }

        public ITab_SquadManager()
        {
            this.labelKey = "SquadManager";
            this.tutorTag = "SquadManager";
            this.size = new Vector2(500f, 450f);
            this.squadDisplay = new SquadDisplayUtility();
        }

        protected override void FillTab()
        {
            // Main container rectangle with padding
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
            Pawn pawn = (Pawn)this.SelPawn;

            if (pawn != null && SquadLeader != null)
            {
                float controlButtonsHeight = BUTTON_HEIGHT + SPACING;
                float dividerHeight = 10f;
                float squadListAreaHeight = rect.height - controlButtonsHeight - dividerHeight;

                Rect controlButtonsRect = new Rect(rect.x, rect.y, rect.width, controlButtonsHeight);
                DrawControlButtons(controlButtonsRect);


                Rect dividerRect = new Rect(rect.x, controlButtonsRect.yMax, rect.width, dividerHeight);
                Widgets.DrawLineHorizontal(dividerRect.x, dividerRect.y + dividerRect.height / 2, dividerRect.width);


                Rect squadListRect = new Rect(rect.x, dividerRect.yMax, rect.width, squadListAreaHeight);
                DrawSquadList(squadListRect);
            }
            else
            {
                Widgets.Label(rect, "No data available");
            }
        }

        private void DrawControlButtons(Rect rect)
        {
            float buttonHeight = BUTTON_HEIGHT;
            float buttonMargin = 10f;
            float availableWidth = rect.width;
            float willBarWidth = availableWidth * 0.4f;
            float squadButtonWidth = availableWidth * 0.2f;


            Rect willBarRect = new Rect(rect.x, rect.y, willBarWidth, buttonHeight);
            Rect squadButtonRect = new Rect(willBarRect.xMax + buttonMargin, rect.y, squadButtonWidth, buttonHeight);
            if (Widgets.ButtonText(squadButtonRect, "Squad"))
            {
                Find.WindowStack.Add(new SquadManagerWindow(this.SquadLeader));
            }
        }

        private void DrawSquadList(Rect rect)
        {
            if (SquadLeader.ActiveSquads == null || SquadLeader.ActiveSquads.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "No active squads. Create squads in the Squad Manager.");
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }


            squadDisplay.DrawSquadsList(rect, ref scrollPosition, SquadLeader.ActiveSquads, SquadLeader);
        }
    }
}
