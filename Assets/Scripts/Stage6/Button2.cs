using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button2 : MonoBehaviour
{
    public GameObject groundDesTroy;
    public GameObject groundSpawn;
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
            groundSpawn.SetActive(true);
            Destroy(groundDesTroy);
            Destroy(this.gameObject);
            
        }
    }
}
