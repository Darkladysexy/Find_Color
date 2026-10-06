using UnityEngine;

public class Button2 : MonoBehaviour
{
    [Tooltip("Ground to destroy when a clone steps on this button.")]
    public GameObject groundDesTroy;
    [Tooltip("Ground to enable when a clone steps on this button.")]
    public GameObject groundSpawn;

    private const string k_CloneTag = "Clone";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(k_CloneTag))
        {
            return;
        }

        if (groundSpawn != null)
        {
            groundSpawn.SetActive(true);
        }

        if (groundDesTroy != null)
        {
            Destroy(groundDesTroy);
        }

        Destroy(gameObject);
    }
}
