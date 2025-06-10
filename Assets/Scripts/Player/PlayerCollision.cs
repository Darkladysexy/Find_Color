using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public bool onGround = false;
    public static PlayerCollision playerCollision;
    void Awake()
    {
        playerCollision = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            onGround = true;
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        onGround = false;
    }
}
