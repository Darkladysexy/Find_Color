using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private Animator animator;
    public GameObject door;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = door.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        animator.SetBool("IsPushed", true);
        rb.gravityScale = -0.2F;
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        animator.SetBool("IsPushed", false);
        rb.gravityScale = 0.2F;
    }
}
