using System;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class FormationDef : Def
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

        public Type formationWorker;

        public FormationWorker CreateWorker()
        {
            FormationWorker SquadOrderWorker = (FormationWorker)Activator.CreateInstance(formationWorker);
            return SquadOrderWorker;
        }
    }
}