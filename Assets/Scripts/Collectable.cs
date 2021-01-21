using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public GameObject splash, shadow;
    public bool isThrowed;
    [HideInInspector]
    public GameObject createdShadow;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(!GameManager.Instance.isGameStarted)
        {
            return;
        }
        if (isThrowed)
        {
            // Bit shift the index of the layer (8) to get a bit mask
            int layerMask = 1 << 8;

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, 50f, layerMask))
            {
                if (createdShadow == null)
                {
                    createdShadow = Instantiate(shadow, hit.transform.position, shadow.transform.rotation);
                }
                else
                {
                    createdShadow.transform.position = new Vector3(hit.point.x, hit.point.y + .1f, hit.point.z);
                }
            }
        }
        if (transform.position.y < -3)
        {
            if (SmoothFollow.Instance.targets.Contains(gameObject.transform))
            {
                SmoothFollow.Instance.targets.Remove(gameObject.transform); 
            }
            if (GameManager.Instance.Player.CollectedObjs.Contains(gameObject))
            {
                GameManager.Instance.Player.CollectedObjs.Remove(gameObject);
            }
            Destroy(createdShadow);
            Destroy(gameObject);
        }
    }

    public void ActivateThrowProperties(Collider playerCollider)
    {
        SmoothFollow.Instance.targets.Remove(transform);
        transform.parent = null;
        isThrowed = true;
        GetComponent<Rigidbody>().isKinematic = false;
        StartCoroutine(WaitAndActivateCollision(playerCollider));
    }

    IEnumerator WaitAndActivateCollision(Collider pC)
    {
        yield return new WaitForSeconds(.5f);
        Physics.IgnoreCollision(GetComponent<Collider>(), pC, false);
    }

    public void DeActivateThrowProperties()
    {
        rb.useGravity = false;
        GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        isThrowed = false;
        Destroy(createdShadow);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            if (SmoothFollow.Instance.targets.Contains(gameObject.transform))
            {
                SmoothFollow.Instance.targets.Remove(gameObject.transform);
            }
            if (GameManager.Instance.Player.CollectedObjs.Contains(gameObject))
            {
                GameManager.Instance.Player.CollectedObjs.Remove(gameObject); 
            }
            Destroy(Instantiate(splash, new Vector3(transform.position.x, .6f, transform.position.z), splash.transform.rotation), 2f);
            Destroy(createdShadow);
            if (SmoothFollow.Instance.targets.Contains(gameObject.transform))
            {
                SmoothFollow.Instance.targets.Remove(gameObject.transform); 
            }
            SoundManager.Instance.playSound(SoundManager.GameSounds.Splash);
            Destroy(gameObject);
        }
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance.isInSlowMotion)
        {
            GameManager.Instance.Player.Collect(gameObject);
        }
    }
}
