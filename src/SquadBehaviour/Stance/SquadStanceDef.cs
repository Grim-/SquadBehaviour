using System;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class SquadStanceDef : Def
    {
        public bool maintainsFormation = true;
        public bool respondToThreats = true;
        public bool requiresDefendPoint = false;
        public bool allowNormalBehavior = false;

        public float aggressionLevel = 0.5f;
        public float formationTightness = 0.8f; 
        public float maxEngagementDistance = 40f;


        public bool shouldWander = false;
        public float wanderRadius = 10f;

        public Type stanceJobGiverClass;

        public ThinkNode_JobGiver CreateJobGiver()
        {
            if (stanceJobGiverClass != null)
            {
                return (ThinkNode_JobGiver)Activator.CreateInstance(stanceJobGiverClass);
            }
            return null;
        }
    }
}
