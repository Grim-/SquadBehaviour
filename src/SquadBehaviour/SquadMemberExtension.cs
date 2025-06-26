using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SquadBehaviour
{
    public class SquadMemberExtension : DefModExtension
    {
        public bool canEverJoinSquad = true;
        public bool overrideDefaultChecks = false;
        public List<SquadSkillRequirements> skillRequirements;
        public List<TraitDef> traitsRequired;
        public List<HediffDef> hediffRequired;
        public List<GeneDef> genesRequired;

        public bool CanJoinSquads(Pawn memberPawn, Comp_PawnSquadLeader squadLeader, out string reason)
        {
            reason = "";

            if (!canEverJoinSquad)
            {
                reason = "cannot ever join a squad.";
                return false;
            }

            if (!overrideDefaultChecks)
            {
                if (!DefaultCanJoinChecks(memberPawn, squadLeader, out reason))
                {
                    return false;
                }
            }

            if (!CheckRequirements(squadLeader, out reason))
            {
                return false;
            }

            return true;
        }

        private bool DefaultCanJoinChecks(Pawn memberPawn, Comp_PawnSquadLeader squadLeader, out string reason)
        {
            reason = "";

            if (memberPawn.RaceProps.IsMechanoid && !squadLeader.CanCommandMechs)
            {
                reason = "Cannot command mechs";
                return false;
            }

            if (memberPawn.RaceProps.Animal)
            {
                if (memberPawn.RaceProps.trainability == TrainabilityDefOf.None)
                {
                    reason = "Not trainable";
                    return false;
                }

                if (!squadLeader.CanCommandAnimals(out string cantCommandReason))
                {
                    reason = cantCommandReason;
                    return false;
                }
            }

            if (memberPawn.Faction != squadLeader.Pawn.Faction)
            {
                reason = "Different faction";
                return false;
            }

            return true;
        }

        private bool CheckRequirements(Comp_PawnSquadLeader squadLeader, out string reason)
        {
            reason = "";

            if (skillRequirements.NullOrEmpty() && traitsRequired.NullOrEmpty() &&
                hediffRequired.NullOrEmpty() && genesRequired.NullOrEmpty())
            {
                return true;
            }

            if (!skillRequirements.NullOrEmpty())
            {
                if (squadLeader.Pawn.skills == null)
                {
                    reason = "Leader has no skills";
                    return false;
                }

                foreach (SquadSkillRequirements req in skillRequirements)
                {
                    SkillRecord skill = squadLeader.Pawn.skills.GetSkill(req.skillDef);
                    if (skill == null || skill.Level < req.minLevel)
                    {
                        reason = $"Leader requires {req.skillDef.skillLabel} level {req.minLevel}, has {skill?.Level ?? 0}";
                        return false;
                    }
                }
            }

            if (!traitsRequired.NullOrEmpty())
            {
                if (squadLeader.Pawn.story?.traits == null)
                {
                    reason = "Leader has no traits";
                    return false;
                }

                foreach (TraitDef traitDef in traitsRequired)
                {
                    if (!squadLeader.Pawn.story.traits.HasTrait(traitDef))
                    {
                        reason = $"Leader requires {traitDef.LabelCap} trait";
                        return false;
                    }
                }
            }

            if (!hediffRequired.NullOrEmpty())
            {
                if (squadLeader.Pawn.health?.hediffSet == null)
                {
                    reason = "Leader has no health data";
                    return false;
                }

                foreach (HediffDef hediffDef in hediffRequired)
                {
                    if (!squadLeader.Pawn.health.hediffSet.HasHediff(hediffDef))
                    {
                        reason = $"Leader requires {hediffDef.LabelCap}";
                        return false;
                    }
                }
            }

            if (!genesRequired.NullOrEmpty())
            {
                if (!ModLister.BiotechInstalled || squadLeader.Pawn.genes == null)
                {
                    reason = "Leader has no genes or Biotech not installed";
                    return false;
                }

                foreach (GeneDef geneDef in genesRequired)
                {
                    if (!squadLeader.Pawn.genes.HasActiveGene(geneDef))
                    {
                        reason = $"Leader requires {geneDef.LabelCap} gene";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

