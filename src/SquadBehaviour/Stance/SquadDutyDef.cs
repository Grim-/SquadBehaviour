using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class SquadDutyDef : Def
    {
        public bool maintainsFormation = true;
        public bool respondToThreats = true;
        public Type dutyJobGiver;
        public string uiIconPath = "";

        public SquadDutyProps props;

        private Texture2D _Tex;

        public Texture2D Tex
        {
            get
            {
                if (_Tex == null && !string.IsNullOrEmpty(uiIconPath))
                {
                    _Tex = ContentFinder<Texture2D>.Get(uiIconPath);
                }

                return _Tex;
            }
        }

        public ThinkNode_JobGiver CreateJobGiver()
        {
            if (dutyJobGiver != null)
            {
                return (ThinkNode_JobGiver)Activator.CreateInstance(dutyJobGiver);
            }
            return null;
        }
    }


    public class SquadDutyProps
    {
        public bool maintainsFormation = true;
        public bool respondToThreats = true;
        public float maxDistance = 15f;

        // Duty-specific properties
        public float defensiveRadius = 25f;
        public float patrolLeashRadius = 40f;
    }


    //SquadDuy
    //Called To Arms
    //Params, In formation, hostility response, 

    //Patrol
    //Patrol Zone
    //hostility response
    //patrol type, perimeter,area

    //Defend Point
    //point to defend
    //hostiltiy repsonse
    //defend radius
    //wander radius

    //Off Duty
    //does not interfere with normal think tree


    //Squad Orders - Can be given per unit or per squad, or everyone.
    //Attack target
    //Defend point
    //Start Patrol


}
