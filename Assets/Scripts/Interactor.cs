using UnityEngine;

// Attach this to the player, alongside GridPlayerController.
// Pressing E interacts with whatever InteractableTask occupies the cell
// the player is currently facing.
public class Interactor : MonoBehaviour
{
    public GridPlayerController player;

    void Update()
    {
        if (player.InputLocked) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        Vector3 targetCell = player.FacingCellWorldPosition;

        foreach (var task in InteractableTask.All)
        {
            if (task.OccupiesCell(targetCell))
            {
                task.Interact();
                return; // one interaction per keypress
            }
        }
    }
}
