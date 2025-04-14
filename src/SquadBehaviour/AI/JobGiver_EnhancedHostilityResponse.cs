using RimWorld;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class JobGiver_EnhancedHostilityResponse : JobGiver_ConfigurableHostilityResponse
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.IsPartOfSquad(out ISquadMember squadMember) || squadMember.AssignedSquad == null || !squadMember.AbilitiesAllowed)
			{
				return base.TryGiveJob(pawn);
			}

			if (pawn.abilities != null && !pawn.abilities.abilities.NullOrEmpty())
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
			return base.TryGiveJob(pawn);
		}
	}
}
