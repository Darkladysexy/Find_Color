using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public GameObject ground;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Clone")
        {
            ground.SetActive(true);
            // Rigidbody2D rb = clone.GetComponent<Rigidbody2D>();
            // rb.bodyType = RigidbodyType2D.Static;
            Destroy(collision.gameObject);
            PlayerCollision.instant.onGround = true;
        }
    }
}
