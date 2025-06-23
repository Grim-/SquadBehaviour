using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalIsAggresive : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
			{
				return squadMember.AssignedSquad.HostilityResponse == SquadHostility.Aggressive;
			}

			return false;
		}
	}
}
