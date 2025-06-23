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

            List<Zone_PatrolPath> availablePatrolZones = this.SquadMember.SquadLeader.SquadLeaderPawn.Map.zoneManager.AllZones.Where(x => x is Zone_PatrolPath patrolPathZone).Cast<Zone_PatrolPath>().ToList();

            if (availablePatrolZones.Count == 0)
            {
                return;
            }

            List<FloatMenuOption> option = new List<FloatMenuOption>();

            foreach (var item in availablePatrolZones)
            {
                option.Add(new FloatMenuOption($"Patrol Zone [{item.ID}].", () =>
                {
                    if (IsGlobalOrder)
                    {
                        foreach (var member in this.SquadMember.AssignedSquad.Members)
                        {
                            if (member.IsPartOfSquad(out Comp_PawnSquadMember pawnSquadMember))
                            {
                                pawnSquadMember.CurrentStance = SquadDefOf.PatrolArea;
                                pawnSquadMember.PatrolTracker.SetPatrolZone(item);
                            }
                        }
                    }
                    else
                    {
                        this.SquadMember.CurrentStance = SquadDefOf.PatrolArea;
                        this.SquadMember.PatrolTracker.SetPatrolZone(item);
                    }

                }));
            }
            Find.WindowStack.Add(new FloatMenu(option));
        }
    }
}
