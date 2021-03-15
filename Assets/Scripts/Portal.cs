using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public GameObject connectedPortal;
    private Collider2D triggerCollider;

    // Start is called before the first frame update
    void Start()
    {
        triggerCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (connectedPortal != null)
        {
            var script = connectedPortal.gameObject.GetComponent<Portal>();
            if (script != null)
            {
                StartCoroutine(script.TransferGameObject(collision.gameObject));
            }
        }
    }

    public IEnumerator TransferGameObject(GameObject obj)
    {
        triggerCollider.enabled = false;

        obj.transform.position = gameObject.transform.position;

        yield return new WaitForSeconds(0.5f);

        triggerCollider.enabled = true;
    }
}
