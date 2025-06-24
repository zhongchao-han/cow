using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;

    [Header("Animation Frames (Up/Down: 2 frames; Left/Right: 3 frames)")]
    [Tooltip("Use the '+' button to add exactly 2 sprites for Up direction.")]
    [SerializeField] private List<Sprite> upSprites = new List<Sprite>(2);
    [Tooltip("Use the '+' button to add exactly 2 sprites for Down direction.")]
    [SerializeField] private List<Sprite> downSprites = new List<Sprite>(2);
    [Tooltip("Use the '+' button to add exactly 3 sprites for Left direction.")]
    [SerializeField] private List<Sprite> leftSprites = new List<Sprite>(3);
    [Tooltip("Use the '+' button to add exactly 3 sprites for Right direction.")]
    [SerializeField] private List<Sprite> rightSprites = new List<Sprite>(3);

    [Header("Frame Timing")]
    [SerializeField] private float frameDuration = 0.2f;

    [Header("Sprite World Size (Units)")]
    [Tooltip("Desired width and height of the sprite in world units")]    
    [SerializeField] private Vector2 spriteWorldSize = new Vector2(1f, 2f);

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 movement;
    private float frameTimer;
    private int frameIndex;
    private enum Direction { Up, Down, Left, Right }
    private Direction currentDirection = Direction.Down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ResizeSprite();
    }

    void Update()
    {
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.A)) input.x = -1;
        else if (Input.GetKey(KeyCode.D)) input.x = 1;

        if (input.x == 0)
        {
            if (Input.GetKey(KeyCode.W)) input.y = 1;
            else if (Input.GetKey(KeyCode.S)) input.y = -1;
        }

        movement = input;

        if (movement.x > 0) currentDirection = Direction.Right;
        else if (movement.x < 0) currentDirection = Direction.Left;
        else if (movement.y > 0) currentDirection = Direction.Up;
        else if (movement.y < 0) currentDirection = Direction.Down;

        AnimateWalk();
    }

    void FixedUpdate()
    {
        Vector2 newPosition = rb.position + movement * walkSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void AnimateWalk()
    {
        List<Sprite> sprites = GetSpritesList();
        if (sprites == null || sprites.Count == 0)
            return;

        if (movement == Vector2.zero)
        {
            frameIndex = 0;
            frameTimer = 0;
            sr.sprite = sprites[0];
            return;
        }

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameDuration)
        {
            frameTimer = 0;
            frameIndex = (frameIndex + 1) % sprites.Count;
        }

        sr.sprite = sprites[frameIndex];
    }

    private List<Sprite> GetSpritesList()
    {
        switch (currentDirection)
        {
            case Direction.Up:    return upSprites;
            case Direction.Down:  return downSprites;
            case Direction.Left:  return leftSprites;
            case Direction.Right: return rightSprites;
        }
        return downSprites;
    }

    private void ResizeSprite()
    {
        if (sr.sprite == null) return;
        Vector2 spriteSize = sr.sprite.bounds.size;
        Vector3 newScale = new Vector3(
            spriteWorldSize.x / spriteSize.x,
            spriteWorldSize.y / spriteSize.y,
            1f
        );
        transform.localScale = newScale;
    }
}
