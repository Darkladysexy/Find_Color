using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosControlClone : MonoBehaviour
{
    public GameObject clonePrefab;
    public GameObject PosClone;
    private GameObject spawnedClone;
    private int flag = 0;
    
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
        if (collision.gameObject.tag == "Player" && flag == 0)
        {
            if (spawnedClone == null)
            {
                spawnedClone = Instantiate(clonePrefab, PosClone.transform.position, Quaternion.identity);
                spawnedClone.tag = "Clone";
            }
            flag = 1;
        }
    }
}
