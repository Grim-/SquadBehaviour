using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SquadBehaviour
{
    public class SquadOrder_PatrolZone : SquadOrderWorker
    {
        public override bool CanExecuteOrder(LocalTargetInfo Target)
        {
            return true;
        }

        public override void ExecuteOrder(LocalTargetInfo Target)
        {

            List<Zone> availablePatrolZones = this.SquadMember.SquadLeader.SquadLeaderPawn.Map.zoneManager.AllZones.Where(x => x is Zone_PatrolPath patrolPathZone).ToList();

            if (availablePatrolZones.Count == 0)
            {
                return;
            }

            List<FloatMenuOption> option = new List<FloatMenuOption>();

            foreach (var item in availablePatrolZones)
            {
                option.Add(new FloatMenuOption($"Patrol Zone {item.ID}.", () =>
                {
                    this.SquadMember.CurrentStance = SquadDefOf.PatrolArea;
                    this.SquadMember.AssignedPatrol = (Zone_PatrolPath)item;
                }));
            }
            Find.WindowStack.Add(new FloatMenu(option));
        }
    }
}
