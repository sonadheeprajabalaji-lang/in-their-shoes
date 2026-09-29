using UnityEngine;

// Tile-by-tile movement, Pokemon-style: one cell per keypress, no diagonal
// movement, blocked by anything on the Solid layer. Pressing into a wall
// just turns the player to face that direction without moving.
[RequireComponent(typeof(SpriteRenderer))]
public class GridPlayerController : MonoBehaviour
{
    // Shared by InteractableTask/Interactor so every script agrees on what
    // "one cell" means. Change this in one place if the grid size changes.
    public const float CellSize = 1f;

    public enum Direction { Down, Up, Left, Right }

    [Header("Movement")]
    public float moveDuration = 0.12f;
    [Tooltip("Put walls and furniture that should block movement on this layer.")]
    public LayerMask solidLayer;

    [Header("Placeholder look (ignored once a real sprite is assigned)")]
    public Color placeholderColor = new Color(0.35f, 0.55f, 0.75f);

    public Direction Facing { get; private set; } = Direction.Down;

    // GameManager sets this true while a choice/response popup is on
    // screen so the player can't wander off mid-decision.
    public bool InputLocked { get; set; }

    // True while the player is sliding between two cells. Interactor uses
    // this to hide the prompt mid-step.
    public bool IsMoving => moving;

    SpriteRenderer sr;
    bool moving;
    Vector3 moveStart;
    Vector3 moveTarget;
    float moveT;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null)
        {
            sr.sprite = PlaceholderSprite.Square();
            sr.color = placeholderColor;
        }

        transform.position = Snap(transform.position);
    }

    void Update()
    {
        if (moving)
        {
            moveT += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(moveStart, moveTarget, Mathf.Clamp01(moveT));
            if (moveT >= 1f) moving = false;
            return;
        }

        if (InputLocked) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) TryStep(Direction.Up);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) TryStep(Direction.Down);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) TryStep(Direction.Left);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) TryStep(Direction.Right);
    }

    void TryStep(Direction dir)
    {
        Facing = dir; // turning to face a direction always happens, even if blocked

        Vector3 target = transform.position + Offset(dir) * CellSize;

        if (Physics2D.OverlapBox(target, Vector2.one * (CellSize * 0.8f), 0f, solidLayer))
        {
            return; // blocked: player turned but did not move
        }

        moveStart = transform.position;
        moveTarget = target;
        moveT = 0f;
        moving = true;
    }

    // The world position of the cell directly in front of the player.
    // Used by Interactor to find whatever the player is facing.
    public Vector3 FacingCellWorldPosition => transform.position + Offset(Facing) * CellSize;

    static Vector3 Offset(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector3.up;
            case Direction.Down: return Vector3.down;
            case Direction.Left: return Vector3.left;
            default: return Vector3.right;
        }
    }

    static Vector3 Snap(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / CellSize) * CellSize,
            Mathf.Round(pos.y / CellSize) * CellSize,
            pos.z);
    }
}