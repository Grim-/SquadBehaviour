using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class ThinkNode_ConditionalSquadMemberHasDuty : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember))
			{
				return squadMember.CurrentStance != null;
			}

			return false;
		}
	}
}
