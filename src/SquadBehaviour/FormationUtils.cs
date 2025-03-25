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
                    rawPosition = CalculateBoxFormation(leaderPos, index, totalUnits, rotation);
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

        private static Vector3 CalculateBoxFormation(Vector3 leaderPos, int index, int totalUnits, Rot4 rotation)
        {
            int sideLength = Mathf.CeilToInt(Mathf.Sqrt(totalUnits));
            int row = index / sideLength;
            int col = index % sideLength;

            float offset = (sideLength - 1) / 2f;
            Vector3 relativePos = new Vector3(col - offset, 0, row - offset);

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
}
