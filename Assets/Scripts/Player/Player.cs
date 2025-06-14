using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        
    }


    void Update()
    {
        if (Menu.instant.isPaused)
        {
            MovePlayer();
        }
    }
    void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector2 playerInput = new Vector2(moveX, moveY).normalized * speed;
        rb.velocity = playerInput;
        if (playerInput.x > 0)
        {
            spriteRenderer.flipX = false; // Face right
        }
        else if (playerInput.x < 0)
        {
            spriteRenderer.flipX = true; // Face left
        }
        if (playerInput != Vector2.zero || playerInput != Vector2.zero)
        {
            animator.SetBool("isRun", true);
        }
        else
        {
            animator.SetBool("isRun", false);
        }
    }
}
