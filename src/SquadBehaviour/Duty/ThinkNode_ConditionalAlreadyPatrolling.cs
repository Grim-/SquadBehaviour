using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalAlreadyPatrolling : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out ISquadMember squadMember))
            {
                return pawn.CurJob != null &&
                 pawn.CurJob.def == JobDefOf.Goto;
            }

            return false;
        }
    }
}
