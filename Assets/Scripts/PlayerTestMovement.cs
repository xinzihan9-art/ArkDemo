
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerTestMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float gravity = -9.81f;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Facing")]
    public bool defaultFacingRight = true;

    private CharacterController controller;
    private float verticalVelocity;

    // true = 向右，false = 向左
    private bool facingRight;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 初始化朝向
        facingRight = defaultFacingRight;
        UpdateFacing();

        // 初始状态为 Idle
        if (animator != null)
            animator.SetBool("IsMoving", false);
    }

    void Update()
    {
        // 1. 读取 WASD 输入
        Vector2 input = Vector2.zero;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
        }

        // 2. 计算移动方向
        Vector3 move = new Vector3(input.x, 0f, input.y);
        move = Vector3.ClampMagnitude(move, 1f);

        // 3. 重力处理
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        // 4. 移动角色
        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // 5. 根据实际水平移动判断动画状态
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        bool isMoving =
            horizontalVelocity.sqrMagnitude > 0.0001f;

        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }

        // 6. 根据左右输入更新朝向
        if (input.x < -0.01f)
        {
            facingRight = false;
        }
        else if (input.x > 0.01f)
        {
            facingRight = true;
        }

        // 每帧应用朝向，避免动画覆盖 Flip X
        UpdateFacing();
    }

    void UpdateFacing()
    {
        if (spriteRenderer == null) return;

        // 原始 PNG 默认朝右
        // 向右：不翻转
        // 向左：水平翻转
        spriteRenderer.flipX = !facingRight;
    }
}
