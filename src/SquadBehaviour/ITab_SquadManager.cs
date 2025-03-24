using RimWorld;
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

        private ISquadLeader _UndeadMaster;
        private ISquadLeader UndeadMaster
        {
            get
            {
                if (_UndeadMaster == null)
                {
                    if (this.SelPawn.TryGetSquadLeader(out ISquadLeader squadLeader))
                    {
                        _UndeadMaster = squadLeader;
                    }      
                }

                return _UndeadMaster;
            }
        }

        public override bool IsVisible => base.IsVisible && this.SelPawn != null && this.SelPawn.TryGetSquadLeader(out ISquadLeader squadLeader);

        public ITab_SquadManager()
        {
            this.labelKey = "Undead";
            this.tutorTag = "Undead";
            this.size = new Vector2(500f, 450f);
            this.squadDisplay = new SquadDisplayUtility();
        }

        protected override void FillTab()
        {
            // Main container rectangle with padding
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
            Pawn pawn = (Pawn)this.SelPawn;

            if (pawn != null && UndeadMaster != null)
            {
                // Calculate heights for different sections
                float controlButtonsHeight = BUTTON_HEIGHT + SPACING;
                float dividerHeight = 10f;
                float squadListAreaHeight = rect.height - controlButtonsHeight - dividerHeight;

                // Draw control buttons at the top
                Rect controlButtonsRect = new Rect(rect.x, rect.y, rect.width, controlButtonsHeight);
                DrawControlButtons(controlButtonsRect);

                // Add divider
                Rect dividerRect = new Rect(rect.x, controlButtonsRect.yMax, rect.width, dividerHeight);
                Widgets.DrawLineHorizontal(dividerRect.x, dividerRect.y + dividerRect.height / 2, dividerRect.width);

                // Squad list area below
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

            // Will capacity bar
            Rect willBarRect = new Rect(rect.x, rect.y, willBarWidth, buttonHeight);
            //Widgets.FillableBar(willBarRect, this.UndeadMaster.WillCapacityAsPercent);
            //Text.Anchor = TextAnchor.MiddleCenter;
            //Widgets.Label(willBarRect, $"Will: {this.UndeadMaster.WillRequiredForUndead} / {this.UndeadMaster.WillStat}");
            //Text.Anchor = TextAnchor.UpperLeft;

            // Squad manager button
            Rect squadButtonRect = new Rect(willBarRect.xMax + buttonMargin, rect.y, squadButtonWidth, buttonHeight);
            if (Widgets.ButtonText(squadButtonRect, "Squad"))
            {
                Find.WindowStack.Add(new SquadManagerWindow(this.UndeadMaster));
            }
        }

        private void DrawSquadList(Rect rect)
        {
            if (UndeadMaster.ActiveSquads == null || UndeadMaster.ActiveSquads.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "No active squads. Create squads in the Squad Manager.");
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            // Use the reusable squad display utility
            squadDisplay.DrawSquadsList(rect, ref scrollPosition, UndeadMaster.ActiveSquads, UndeadMaster);
        }
    }
}
