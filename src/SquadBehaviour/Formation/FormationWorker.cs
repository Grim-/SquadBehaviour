using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public abstract class FormationWorker
    {
        public virtual IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            return leaderPos.ToIntVec3();
        }
    }
}