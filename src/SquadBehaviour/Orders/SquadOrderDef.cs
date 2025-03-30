using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class SquadOrderDef : Def
    {
        public string uiIconPath = "";
        private Texture2D _Icon = null;
        public Texture2D Icon
        {
            get
            {
                if (_Icon == null)
                {
                    string path = String.IsNullOrEmpty(uiIconPath) ? "UI/Designators/Cancel" : uiIconPath;

                    _Icon = ContentFinder<Texture2D>.Get(path);
                }

                return _Icon;
            }
        }

        public bool requiresTarget = true;
        public OrderType orderType = OrderType.Unit;
        public TargetingParameters targetingParameters;
        public SquadOrderSettings orderSettings;
        public Type workerClass;

        public SquadOrderWorker CreateWorker(ISquadLeader SquadLeader, ISquadMember SquadMember)
        {
            SquadOrderWorker SquadOrderWorker = (SquadOrderWorker)Activator.CreateInstance(workerClass);
            SquadOrderWorker.SquadMember = SquadMember;
            SquadOrderWorker.SquadLeader = SquadLeader;
            SquadOrderWorker.SquadOrderSettings = orderSettings;
            return SquadOrderWorker;
        }
    }

    [Flags]
    public enum OrderType
    {
        None = 0,
        Global = 2,
        Unit = 4
    }
}
