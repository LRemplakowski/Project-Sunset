using Sirenix.OdinInspector;
using UnityEngine;

namespace SunsetSystems.Entities
{
    [CreateAssetMenu(fileName = "New Faction Interaction Data", menuName = "Sunset Entities/Faction Interactions Data")]
    public class FactionInteractionData : SerializedScriptableObject
    {
        // Matrix to hold the FactionInteraction values
        [TableMatrix(
            HorizontalTitle = "Factions",
            VerticalTitle = "Factions",
            DrawElementMethod = "DrawCell",
            ResizableColumns = false,
            RowHeight = 30,
            Labels = nameof(GetFactionLabels))]
        public FactionInteraction[,] collisionMatrix = new FactionInteraction[5, 5];

#if UNITY_EDITOR
        // DrawCell method to handle how each cell is displayed
        private static FactionInteraction DrawCell(Rect rect, FactionInteraction value, FactionInteraction[,] grid, int x, int y)
        {
            int reversedX = grid.GetLength(0) - 1 - x; // Reverse the column index

            if (reversedX < y)
            {
                return value; // Skip drawing cells in the lower triangle and diagonal
            }

            return (FactionInteraction)UnityEditor.EditorGUI.EnumPopup(rect, value);
        }
#endif

        // Method to provide faction labels
        private static (string label, LabelDirection direction) GetFactionLabels(int index, bool isRow)
        {
            var labels = new string[]
            {
                "None",
                "Hostile",
                "Neutral",
                "Friendly",
                "PlayerControlled"
            };
            return (labels[index], isRow ? LabelDirection.LeftToRight : LabelDirection.TopToBottom);
        }
    }
}

public enum FactionInteraction
{
    Neutral, Friendly, Hostile
}
