using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public class CustomFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            float spacing = FormationUtils.GetPawnSpacing(pawn);

            if (pawn.IsPartOfSquad(out var pawnSquadMember))
            {
                if (pawnSquadMember.CustomFormationOffset == IntVec3.Invalid)
                {
                    Log.Message("Customformation offset not set");
                    return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Column, leaderPos, rotation, index, totalUnits, spacing);
                }
                return leaderPos.ToIntVec3() + pawnSquadMember.CustomFormationOffset;
            }
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Column, leaderPos, rotation, index, totalUnits, spacing);
        }
    }
}