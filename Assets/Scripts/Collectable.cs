using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public GameObject splash, shadow;
    public bool isThrowed;
    GameObject createdShadow;

    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position, new Vector3(0, -1, 0), Color.red);
        if (isThrowed)
        {
            // Bit shift the index of the layer (8) to get a bit mask
            int layerMask = 1 << 8;

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, 50f, layerMask))
            {
                Debug.Log("Did Hit");
                Debug.DrawRay(transform.position, new Vector3(0, -1, 0) * hit.distance, Color.red);
                if (createdShadow == null)
                {
                    createdShadow = Instantiate(shadow, hit.transform.position, shadow.transform.rotation);
                }
                else
                {                    
                    createdShadow.transform.position = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);
                }
            }
            else
            {
                Debug.DrawRay(transform.position, new Vector3(0, -1, 0) * 50, Color.white);
                Debug.Log("Did not Hit");
            }
        }
        if (transform.position.y < -3)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Instantiate(splash, collision.transform.position, splash.transform.rotation);
            Destroy(gameObject);
        }
    }
}
