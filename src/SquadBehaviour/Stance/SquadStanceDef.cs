using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SquadBehaviour
{
    public class SquadStanceDef : Def
    {
        public bool maintainsFormation = true;
        public bool respondToThreats = true;
        public Type stanceJobGiverClass;
        public string uiIconPath = "";

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
            if (stanceJobGiverClass != null)
            {
                return (ThinkNode_JobGiver)Activator.CreateInstance(stanceJobGiverClass);
            }
            return null;
        }
    }
}
