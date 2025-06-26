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

        private List<Zone_PatrolPath> GetPatrolZones()
        {
            return this.SquadMember.SquadLeader.Pawn.Map.zoneManager.AllZones.Where(x => x is Zone_PatrolPath patrolPathZone).Cast<Zone_PatrolPath>().ToList();
        }

        public override void ExecuteOrder(LocalTargetInfo Target)
        {
            List<Zone_PatrolPath> availablePatrolZones = GetPatrolZones();
            if (availablePatrolZones.Count == 0)
            {
                return;
            }

            List<FloatMenuOption> option = new List<FloatMenuOption>();

            foreach (var item in availablePatrolZones)
            {
                option.Add(new FloatMenuOption($"Patrol Zone [{item.ID}].", () =>
                {
                    this.SquadMember.StartPatrolling(item);
                }));
            }
            Find.WindowStack.Add(new FloatMenu(option));
        }

        public override void ExecuteOrderGlobal(LocalTargetInfo Target)
        {
            List<Zone_PatrolPath> availablePatrolZones = GetPatrolZones();

            if (availablePatrolZones.Count == 0)
            {
                return;
            }

            List<FloatMenuOption> option = new List<FloatMenuOption>();
            foreach (var item in availablePatrolZones)
            {
                option.Add(new FloatMenuOption($"Patrol Zone [{item.ID}].", () =>
                {                   
                    foreach (var member in this.SquadMember.AssignedSquad.Members)
                    {
                        if (member.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
                        {
                            squadMember.StartPatrolling(item);
                        }                   
                    }
                }));
            }

            Find.WindowStack.Add(new FloatMenu(option));
        }
    }
}
