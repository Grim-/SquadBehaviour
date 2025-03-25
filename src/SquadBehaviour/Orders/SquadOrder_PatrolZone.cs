using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SquadBehaviour
{
    public class SquadOrder_PatrolZone : SquadOrderWorker
    {
        //public override bool IsSquadOrder => false;

        public override bool CanExecuteOrder(LocalTargetInfo Target)
        {
            return true;
        }

        public override void ExecuteOrder(LocalTargetInfo Target)
        {
            List<FloatMenuOption> option = new List<FloatMenuOption>();

            foreach (var item in SquadLeader.SquadLeaderPawn.Map.zoneManager.AllZones.Where(x => x is Zone_PatrolPath patrolPathZone).ToList())
            {
                option.Add(new FloatMenuOption($"Patrol Zone {item.ID}.", () =>
                {
                    this.SquadMember.SetCurrentMemberState(SquadMemberState.CalledToArms);
                    this.SquadMember.CurrentStance = SquadDefOf.PatrolArea;
                    this.SquadMember.AssignedPatrol = (Zone_PatrolPath)item;
                }));
            }

            Find.WindowStack.Add(new FloatMenu(option));
        }
    }
}
