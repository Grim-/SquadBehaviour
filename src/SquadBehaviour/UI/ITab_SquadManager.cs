using RimWorld;
using System.Linq;
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

        private Comp_PawnSquadLeader _SquadLeader;
        private Comp_PawnSquadLeader SquadLeader
        {
            get
            {
                if (this.SelPawn.TryGetSquadLeader(out Comp_PawnSquadLeader squadLeader))
                {
                    return squadLeader;
                }

                return null;
            }
        }

        public override bool IsVisible
        {
            get
            {
                return this.SelPawn != null && SelPawn.IsColonist && SquadLeader != null && SquadLeader.IsLeaderRoleActive;
            }
        }

        public ITab_SquadManager()
        {
            this.labelKey = "SquadManager";
            this.tutorTag = "SquadManager";
            this.size = new Vector2(500f, 450f);
   
        }

        public override void OnOpen()
        {
            this.squadDisplay = new SquadDisplayUtility();
            base.OnOpen();
        }

        protected override void CloseTab()
        {
            base.CloseTab();
            _SquadLeader = null;
        }

        protected override void FillTab()
        {
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
            float squadButtonWidth = availableWidth * 0.6f;

            Rect squadButtonRect = new Rect(rect.xMax / 2  + buttonMargin - squadButtonWidth /2, rect.y, squadButtonWidth, buttonHeight);


            if (Widgets.ButtonText(squadButtonRect, "Add New Squad"))
            {
                int newSquadId = SquadLeader.ActiveSquads.Count > 0
                    ? SquadLeader.ActiveSquads.Keys.Max() + 1
                    : 1;

                if (SquadLeader.AddSquad(newSquadId))
                {
                    Squad squad = SquadLeader.ActiveSquads[newSquadId];
                    squad.squadName = "Squad " + newSquadId;
                    Messages.Message("New squad created.", MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("Failed to create a new squad.", MessageTypeDefOf.RejectInput);
                }
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
