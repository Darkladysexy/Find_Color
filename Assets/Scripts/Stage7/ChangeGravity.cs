using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeGravity : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject box;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (this.gameObject.tag == "GravityUp")
        {
            if (box != null)
            {
                Rigidbody2D boxRB = box.GetComponent<Rigidbody2D>();
                boxRB.gravityScale = -1;
            }
            Rigidbody2D rb = PlayerMovementPlatform.instant.gameObject.GetComponent<Rigidbody2D>();
            rb.gravityScale = -1;
            Quaternion rotation = PlayerMovementPlatform.instant.gameObject.transform.rotation;
            PlayerMovementPlatform.instant.gameObject.transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, 180f);
        }
        else if (this.gameObject.tag == "GravityDown")
        {
            if (box != null)
            {
                Rigidbody2D boxRB = box.GetComponent<Rigidbody2D>();
                boxRB.gravityScale = 1;
            }
            Rigidbody2D rb = PlayerMovementPlatform.instant.gameObject.GetComponent<Rigidbody2D>();
            rb.gravityScale = 1;
            Quaternion rotation = PlayerMovementPlatform.instant.gameObject.transform.rotation;
            PlayerMovementPlatform.instant.gameObject.transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
        }
    }
}
