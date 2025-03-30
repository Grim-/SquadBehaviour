using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace SquadBehaviour
{
    // Static class to handle drag and drop operations - not using Unity's Input class
    public static class DragDropManager
    {
        public static Pawn DraggedPawn { get; private set; }
        public static Squad OriginSquad { get; private set; }
        public static bool IsDragging => DraggedPawn != null;
        private static Vector2 dragStartPos;

        private static bool isDragConfirmed;
        private const float MIN_DRAG_DISTANCE = 10f;

        // Start drag operation
        public static void StartDrag(Pawn pawn, Squad originSquad)
        {
            if (pawn == null) return;

            DraggedPawn = pawn;
            OriginSquad = originSquad;
            dragStartPos = Event.current.mousePosition;
            isDragConfirmed = false;
        }

        // Called every frame to update drag state
        public static void UpdateDrag()
        {
            if (!IsDragging) return;

            // Check if we've moved far enough to confirm this is a drag
            if (!isDragConfirmed)
            {
                float distance = Vector2.Distance(dragStartPos, Event.current.mousePosition);
                if (distance >= MIN_DRAG_DISTANCE)
                {
                    isDragConfirmed = true;
                }
            }

            // Handle cancel events (right click or escape)
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                EndDrag();
                Event.current.Use();
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                EndDrag();
                Event.current.Use();
            }
        }

        public static void EndDrag()
        {
            DraggedPawn = null;
            OriginSquad = null;
            isDragConfirmed = false;
        }

        public static bool IsDragConfirmed => IsDragging && isDragConfirmed;

        public static bool TryDropOnSquad(Squad targetSquad, ISquadLeader leader)
        {
            if (!IsDragging || targetSquad == null || leader == null)
                return false;

            if (!isDragConfirmed)
                return false;

            if (OriginSquad != null && OriginSquad.squadID == targetSquad.squadID)
                return false;

            try
            {
                if (OriginSquad != null)
                {
                    OriginSquad.RemoveMember(DraggedPawn);
                }

                // Add to new squad
                targetSquad.AddMember(DraggedPawn);

                Messages.Message($"Transferred {DraggedPawn.LabelShort} to Squad {targetSquad.squadID}", MessageTypeDefOf.TaskCompletion);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Error transferring pawn between squads: {ex}");
                return false;
            }
            finally
            {
                EndDrag();
            }
        }
    }
}