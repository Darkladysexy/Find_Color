using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRender;
    private Animator animator;
    public float jumpForce = 7f;
    public static CloneMovement instant;
    void Awake()
    {
        instant = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();
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
        Debug.Log(FootClone.instant.onGround);
        if (Input.GetKeyDown(KeyCode.Space) && FootClone.instant.onGround)
        {

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void MovePlayer()
    {
        
        float moveX = Input.GetAxisRaw("Horizontal"); // Dùng GetAxisRaw để có giá trị -1, 0, 1
        // Debug.Log(BodyPlayer.instant.onGround);
        // Gán tốc độ cố định theo X, giữ nguyên tốc độ Y (để không phá lực nhảy)
        rb.velocity = new Vector2(moveX * speed * (-1), rb.velocity.y);

        // Lật sprite theo hướng
        if (moveX < 0)
            spriteRender.flipX = false;
        else if (moveX > 0)
            spriteRender.flipX = true;

        // Bật/tắt animation chạy
        animator.SetBool("isRun", moveX != 0);
    }


}
