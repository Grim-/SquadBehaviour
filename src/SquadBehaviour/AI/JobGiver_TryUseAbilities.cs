using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_TryUseAbilities : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.abilities != null && !pawn.abilities.abilities.NullOrEmpty() && pawn.IsPartOfSquad(out Comp_PawnSquadMember squadMember) && squadMember.AssignedSquad != null && squadMember.AbilitiesAllowed)
			{
				Thing threat = AbilityUtility.FindBestAbilityTarget(pawn);
				if (threat != null)
				{
					Ability ability = AbilityUtility.GetBestAbility(pawn, threat);
					if (ability != null)
					{
						Job abilityJob = AbilityUtility.GetAbilityJob(pawn, threat, ability);
						if (abilityJob != null)
						{
							return abilityJob;
						}
					}
				}
			}

			return null;
		}
	}
}
