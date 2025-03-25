using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class SquadDutyDef : Def
    {
        public ThinkNode_JobGiver JobGiver;
        public string uiIconPath;
        public SquadDutyProps props;

        private Texture2D _Tex;
        public Texture2D Tex
        {
            get
            {
                if (_Tex == null)
                {
                    if (string.IsNullOrEmpty(uiIconPath))
                    {
                        _Tex = BaseContent.BadTex;
                    }
                    else
                    {
                        _Tex = ContentFinder<Texture2D>.Get(uiIconPath);
                    }
                }

                return _Tex;
            }
        }
    }

    public class SquadDutyProps
    {
        public bool maintainsFormation = true;
        public bool respondToThreats = true;
        public float maxDistance = 15f;
        public float defensiveRadius = 25f;
        public float patrolLeashRadius = 40f;
        public IntVec3 defendPoint = IntVec3.Invalid;
    }
}
