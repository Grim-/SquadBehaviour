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

        public static IntVec3 GetFormationPosition(FormationType formation, Vector3 leaderPos, Rot4 rotation, int index, int totalUnits)
        {
            Vector3 rawPosition;

            switch (formation)
            {
                case FormationType.Circle:
                    rawPosition = CalculateCircleFormation(leaderPos, index, totalUnits, rotation);
                    break;
                case FormationType.Box:
                    rawPosition = CalculateHollowSquareFormation(leaderPos, index, totalUnits, rotation);
                    break;
                case FormationType.Column:
                    rawPosition = CalculateColumnFormation(leaderPos, index, rotation);
                    break;
                case FormationType.Wedge:
                    rawPosition = CalculateWedgeFormation(leaderPos, index, rotation);
                    break;
                case FormationType.Line:
                    rawPosition = CalculateLineFormation(leaderPos, index, totalUnits, rotation);
                    break;
                case FormationType.Phalanx:
                    rawPosition = CalculatePhalanxFormation(leaderPos, index, rotation);
                    break;
                default:
                    rawPosition = CalculateColumnFormation(leaderPos, index, rotation);
                    break;
            }

            return ValidateGridPosition(rawPosition).ToIntVec3();
        }

        private static Vector3 RotateOffset(Vector3 offset, Rot4 rotation)
        {
            switch (rotation.AsInt)
            {
                case 0: // North - default
                    return offset;
                case 1: // East
                    return new Vector3(offset.z, 0, -offset.x);
                case 2: // South
                    return new Vector3(-offset.x, 0, -offset.z);
                case 3: // West
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

        private static Vector3 CalculateColumnFormation(Vector3 leaderPos, int index, Rot4 rotation)
        {
            int column = index / 3;
            int row = index % 3;
            Vector3 offset = new Vector3(row - 1, 0, -column - 1);
            return leaderPos + RotateOffset(offset, rotation);
        }

        private static Vector3 CalculateCircleFormation(Vector3 leaderPos, int index, int totalUnits, Rot4 rotation)
        {
            float radius = 2f;
            float angleStep = 360f / totalUnits;
            float angle = angleStep * index * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.RoundToInt(Mathf.Sin(angle) * radius),
                0,
                Mathf.RoundToInt(Mathf.Cos(angle) * radius)
            );

            return leaderPos + RotateOffset(offset, rotation);
        }

        private static Vector3 CalculateHollowSquareFormation(Vector3 leaderPos, int index, int totalUnits, Rot4 rotation)
        {
            int sideLength = Mathf.CeilToInt((totalUnits + 4) / 4f);

            // Minimum size should be 3 to make a proper hollow square
            sideLength = Mathf.Max(3, sideLength);
            int perimeterPositions = sideLength * 4 - 4;

            // Calculate spacing to distribute units evenly
            float spacing = 1.0f;
            if (totalUnits < perimeterPositions)
            {
                spacing = (float)perimeterPositions / totalUnits;
                index = Mathf.FloorToInt(index * spacing);
            }

            // Ensure we don't exceed available positions
            index = index % perimeterPositions;

            int side = index / (sideLength - 1);
            int posOnSide = index % (sideLength - 1);

            float offset = (sideLength - 1) / 2f;

            Vector3 relativePos;
            switch (side)
            {
                case 0: // Top side
                    relativePos = new Vector3(posOnSide - offset, 0, -offset);
                    break;
                case 1: // Right side
                    relativePos = new Vector3(offset, 0, posOnSide - offset);
                    break;
                case 2: // Bottom side
                    relativePos = new Vector3(offset - posOnSide, 0, offset);
                    break;
                case 3: // Left side
                    relativePos = new Vector3(-offset, 0, offset - posOnSide);
                    break;
                default:
                    relativePos = Vector3.zero;
                    break;
            }

            return leaderPos + RotateOffset(relativePos, rotation);
        }

        private static Vector3 CalculateWedgeFormation(Vector3 leaderPos, int index, Rot4 rotation)
        {
            int row = index / 2;
            int side = index % 2;
            int spread = row + 1;
            int xPos = side == 0 ? -spread : spread;

            Vector3 offset = new Vector3(xPos, 0, -row - 1);
            return leaderPos + RotateOffset(offset, rotation);
        }

        private static Vector3 CalculateLineFormation(Vector3 leaderPos, int index, int totalUnits, Rot4 rotation)
        {
            float offset = (totalUnits - 1) / 2f;
            Vector3 relativePos = new Vector3(index - offset, 0, -1);

            return leaderPos + RotateOffset(relativePos, rotation);
        }

        private static Vector3 CalculatePhalanxFormation(Vector3 leaderPos, int index, Rot4 rotation)
        {
            int unitsPerRow = 6;
            int row = index / unitsPerRow;
            int col = index % unitsPerRow;

            float colOffset = (unitsPerRow - 1) / 2f;
            Vector3 offset = new Vector3(-row - 1, 0, col - colOffset);

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
        public virtual IntVec3 GetFormationPosition(Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            return leaderPos.ToIntVec3();
        }
    }

    public class ColumnFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Column, leaderPos, rotation, index, totalUnits);
        }
    }

    public class CircleFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Circle, leaderPos, rotation, index, totalUnits);
        }
    }

    public class BoxFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Box, leaderPos, rotation, index, totalUnits);
        }
    }

    public class WedgeFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Wedge, leaderPos, rotation, index, totalUnits);
        }
    }

    public class LineFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Line, leaderPos, rotation, index, totalUnits);
        }
    }

    public class PhalanxFormationWorker : FormationWorker
    {
        public override IntVec3 GetFormationPosition(Vector3 leaderPos, int index, Rot4 rotation, int totalUnits)
        {
            return FormationUtils.GetFormationPosition(FormationUtils.FormationType.Phalanx, leaderPos, rotation, index, totalUnits);
        }
    }
}
