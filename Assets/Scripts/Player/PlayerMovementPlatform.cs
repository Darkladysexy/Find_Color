using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 1f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public float jumpForce = 3f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        Jump();
    }
    void FixedUpdate()
    {


    }
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && PlayerCollision.playerCollision.onGround)
        {
            Debug.Log("Jump");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
    // void MovePlayer()
    // {
    //     float moveX = Input.GetAxis("Horizontal");
    //     Vector2 playerInput = new Vector2(moveX,rb.velocity.y).normalized * speed;
    //     rb.velocity = playerInput;
    //     if (playerInput.x > 0)
    //     {
    //         spriteRenderer.flipX = false; // Face right
    //     }
    //     else if (playerInput.x < 0)
    //     {
    //         spriteRenderer.flipX = true; // Face left
    //     }
    //     if (playerInput != Vector2.zero || playerInput != Vector2.zero)
    //     {
    //         animator.SetBool("isRun", true);
    //     }
    //     else
    //     {
    //         animator.SetBool("isRun", false);
    //     }
    // }
    void MovePlayer()
{
    float moveX = Input.GetAxisRaw("Horizontal"); // Dùng GetAxisRaw để có giá trị -1, 0, 1

    // Gán tốc độ cố định theo X, giữ nguyên tốc độ Y (để không phá lực nhảy)
    rb.velocity = new Vector2(moveX * speed, rb.velocity.y);

    // Lật sprite theo hướng
    if (moveX > 0)
        spriteRenderer.flipX = false;
    else if (moveX < 0)
        spriteRenderer.flipX = true;

    // Bật/tắt animation chạy
    animator.SetBool("isRun", moveX != 0);
}


}
