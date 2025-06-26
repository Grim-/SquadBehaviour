using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class SingleFileFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            float spacing = FormationUtils.GetPawnSpacing(pawn);
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Column, leaderPos, rotation, index, totalUnits, spacing);
        }
    }
}