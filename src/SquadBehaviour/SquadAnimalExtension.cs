using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SquadBehaviour
{
    public class SquadAnimalExtension : DefModExtension
    {
        public bool canEverJoinSquad = false;

        public List<SquadSkillRequirements> skillRequirements;
        public List<TraitDef> traitsRequired;
        public List<HediffDef> hediffRequired;
        public List<GeneDef> genesRequired;

        public bool CanSquadLeaderCommand(Comp_PawnSquadLeader squadLeader)
        {
            if (!canEverJoinSquad)
            {
                return false;
            }

            if (!squadLeader.CanCommandAnimals)
            {
                return false;
            }

            if (skillRequirements.NullOrEmpty() && traitsRequired.NullOrEmpty() && hediffRequired.NullOrEmpty() && genesRequired.NullOrEmpty())
            {
                return true;
            }

            if (!skillRequirements.NullOrEmpty())
            {
                if (squadLeader.Pawn.skills == null) 
                    return false;
                foreach (SquadSkillRequirements req in skillRequirements)
                {
                    SkillRecord skill = squadLeader.Pawn.skills.GetSkill(req.skillDef);
                    if (skill == null || skill.Level < req.minLevel)
                    {
                        return false;
                    }
                }
            }

            if (!traitsRequired.NullOrEmpty())
            {
                if (squadLeader.Pawn.story == null || squadLeader.Pawn.story.traits == null) 
                    return false;
                foreach (TraitDef traitDef in traitsRequired)
                {
                    if (!squadLeader.Pawn.story.traits.HasTrait(traitDef))
                    {
                        return false;
                    }
                }
            }

            if (!hediffRequired.NullOrEmpty())
            {
                if (squadLeader.Pawn.health == null || squadLeader.Pawn.health.hediffSet == null) 
                    return false;
                foreach (HediffDef hediffDef in hediffRequired)
                {
                    if (!squadLeader.Pawn.health.hediffSet.HasHediff(hediffDef))
                    {
                        return false;
                    }
                }
            }

            if (!genesRequired.NullOrEmpty())
            {
                if (!ModLister.BiotechInstalled || squadLeader.Pawn.genes == null) 
                    return false;

                foreach (GeneDef geneDef in genesRequired)
                {
                    if (!squadLeader.Pawn.genes.HasActiveGene(geneDef))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

