using System;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    public static class FormationUtils
    {
        public enum FormationType
        {
            Circle,
            Box,
            Column,
            Wedge,
            Line,
            Phalanx
        }

        public static IntVec3 GetFormationPosition(FormationType formation, Vector3 leaderPos, Rot4 rotation, int index, int totalUnits, float spacing = 1f)
        {
            Vector3 rawPosition;

            switch (formation)
            {
                case FormationType.Circle:
                    rawPosition = CalculateCircleFormation(leaderPos, index, totalUnits, rotation, spacing);
                    break;
                case FormationType.Box:
                    rawPosition = CalculateHollowSquareFormation(leaderPos, index, totalUnits, rotation, spacing);
                    break;
                case FormationType.Column:
                    rawPosition = CalculateColumnFormation(leaderPos, index, rotation, spacing);
                    break;
                case FormationType.Wedge:
                    rawPosition = CalculateWedgeFormation(leaderPos, index, rotation, spacing);
                    break;
                case FormationType.Line:
                    rawPosition = CalculateLineFormation(leaderPos, index, totalUnits, rotation, spacing);
                    break;
                case FormationType.Phalanx:
                    rawPosition = CalculatePhalanxFormation(leaderPos, index, rotation, spacing);
                    break;
                default:
                    rawPosition = CalculateColumnFormation(leaderPos, index, rotation, spacing);
                    break;
            }

            return ValidateGridPosition(rawPosition).ToIntVec3();
        }

        public static float GetPawnSpacing(Pawn pawn)
        {
            return pawn.def.Size.x;
        }

        private static Vector3 RotateOffset(Vector3 offset, Rot4 rotation)
        {
            switch (rotation.AsInt)
            {
                case 0:
                    return offset;
                case 1:
                    return new Vector3(offset.z, 0, -offset.x);
                case 2:
                    return new Vector3(-offset.x, 0, -offset.z);
                case 3:
                    return new Vector3(-offset.z, 0, offset.x);
                default:
                    return offset;
            }
        }

        private static Vector3 ValidateGridPosition(Vector3 position)
        {
            return new Vector3(
                Mathf.RoundToInt(position.x),
                0,
                Mathf.RoundToInt(position.z)
            );
        }

        private static Vector3 CalculateColumnFormation(Vector3 leaderPos, int index, Rot4 rotation, float spacing)
        {
            int column = index / 3;
            int row = index % 3;
            Vector3 offset = new Vector3((row - 1) * spacing, 0, (-column - 1) * spacing);
            return leaderPos + RotateOffset(offset, rotation);
        }

        private static Vector3 CalculateCircleFormation(Vector3 leaderPos, int index, int totalUnits, Rot4 rotation, float spacing)
        {
            float radius = 2f * spacing;
            float angleStep = 360f / totalUnits;
            float angle = angleStep * index * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.RoundToInt(Mathf.Sin(angle) * radius),
                0,
                Mathf.RoundToInt(Mathf.Cos(angle) * radius)
            );

            return leaderPos + RotateOffset(offset, rotation);
        }

        private static Vector3 CalculateHollowSquareFormation(Vector3 leaderPos, int index, int totalUnits, Rot4 rotation, float spacing)
        {
            int sideLength = Mathf.CeilToInt((totalUnits + 4) / 4f);
            sideLength = Mathf.Max(3, sideLength);
            int perimeterPositions = sideLength * 4 - 4;

            float unitSpacing = 1.0f;
            if (totalUnits < perimeterPositions)
            {
                unitSpacing = (float)perimeterPositions / totalUnits;
                index = Mathf.FloorToInt(index * unitSpacing);
            }

            index = index % perimeterPositions;

            int side = index / (sideLength - 1);
            int posOnSide = index % (sideLength - 1);

            float offset = (sideLength - 1) / 2f * spacing;

            Vector3 relativePos;
            switch (side)
            {
                case 0:
                    relativePos = new Vector3((posOnSide - (sideLength - 1) / 2f) * spacing, 0, -offset);
                    break;
                case 1:
                    relativePos = new Vector3(offset, 0, (posOnSide - (sideLength - 1) / 2f) * spacing);
                    break;
                case 2:
                    relativePos = new Vector3((((sideLength - 1) / 2f) - posOnSide) * spacing, 0, offset);
                    break;
                case 3:
                    relativePos = new Vector3(-offset, 0, (((sideLength - 1) / 2f) - posOnSide) * spacing);
                    break;
                default:
                    relativePos = Vector3.zero;
                    break;
            }

            return leaderPos + RotateOffset(relativePos, rotation);
        }

        private static Vector3 CalculateWedgeFormation(Vector3 leaderPos, int index, Rot4 rotation, float spacing)
        {
            int row = index / 2;
            int side = index % 2;
            int spread = row + 1;
            int xPos = side == 0 ? -spread : spread;

            Vector3 offset = new Vector3(xPos * spacing, 0, (-row - 1) * spacing);
            return leaderPos + RotateOffset(offset, rotation);
        }

        private static Vector3 CalculateLineFormation(Vector3 leaderPos, int index, int totalUnits, Rot4 rotation, float spacing)
        {
            float offset = (totalUnits - 1) / 2f;
            Vector3 relativePos = new Vector3((index - offset) * spacing, 0, -spacing);

            return leaderPos + RotateOffset(relativePos, rotation);
        }

        private static Vector3 CalculatePhalanxFormation(Vector3 leaderPos, int index, Rot4 rotation, float spacing)
        {
            int unitsPerRow = 6;
            int row = index / unitsPerRow;
            int col = index % unitsPerRow;

            float colOffset = (unitsPerRow - 1) / 2f;
            Vector3 offset = new Vector3((-row - 1) * spacing, 0, (col - colOffset) * spacing);

            return leaderPos + RotateOffset(offset, rotation);
        }
    }

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

    public abstract class FormationWorker
    {
        public virtual IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            return leaderPos.ToIntVec3();
        }
    }

    public class ColumnFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            float spacing = FormationUtils.GetPawnSpacing(pawn);
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Column, leaderPos, rotation, index, totalUnits, spacing);
        }
    }

    public class CircleFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            float spacing = FormationUtils.GetPawnSpacing(pawn);
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Circle, leaderPos, rotation, index, totalUnits, spacing);
        }
    }

    public class BoxFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            float spacing = FormationUtils.GetPawnSpacing(pawn);
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Box, leaderPos, rotation, index, totalUnits, spacing);
        }
    }

    public class WedgeFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            float spacing = FormationUtils.GetPawnSpacing(pawn);
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Wedge, leaderPos, rotation, index, totalUnits, spacing);
        }
    }

    public class LineFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            float spacing = FormationUtils.GetPawnSpacing(pawn);
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Line, leaderPos, rotation, index, totalUnits, spacing);
        }
    }

    public class PhalanxFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Pawn pawn, Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            float spacing = FormationUtils.GetPawnSpacing(pawn);
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Phalanx, leaderPos, rotation, index, totalUnits, spacing);
        }
    }
}