using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed = 5;
    [SerializeField] private float m_jumpForce = 8;

    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;

    private float m_currentV = 0;

    private bool m_isGrounded;
    private Vector3 translation;
    public float Xspeed = 25f;

    public List<GameObject> CollectedObjs;
    public GameObject metarig;
    public Rigidbody[] ragdollRigidbodies;
    public Collider[] ragdollColliders;
    public Camera cam;

    Vector2 firstPressPos, secondPressPos, currentSwipe;

    void Awake()
    {
        m_animator = gameObject.GetComponent<Animator>();
        m_rigidBody = gameObject.GetComponent<Rigidbody>();
        ragdollRigidbodies = metarig.GetComponentsInChildren<Rigidbody>();
        ragdollColliders = metarig.GetComponentsInChildren<Collider>();
        foreach (Rigidbody item in ragdollRigidbodies)
        {
            item.useGravity = false;
            item.isKinematic = true;
        }
        foreach (Collider item in ragdollColliders)
        {
            item.enabled = false;
        }
        m_rigidBody.useGravity = true;
        m_rigidBody.isKinematic = false;
    }

    void Update()
    {
        transform.position += transform.forward * m_moveSpeed * Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            translation = new Vector3(Input.GetAxis("Mouse X"), 0, 0) * Time.deltaTime * Xspeed;

            transform.Translate(translation, Space.World);
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -2.5f, 2.5f), transform.position.y, transform.position.z);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            //save began touch 2d point
            firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        else if (m_isGrounded && Input.GetMouseButtonUp(0))
        {
            //save ended touch 2d point
            secondPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            //create vector from the two points
            currentSwipe = new Vector2(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

            //normalize the 2d vector
            currentSwipe.Normalize();

            //swipe upwards
            if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
                m_animator.SetTrigger("Jump");
                ThrowCollectedObjs();
            }
        }
        m_animator.SetFloat("MoveSpeed", m_currentV);
    }

    public Transform CarryObject;
    public int CollectableCount;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            m_isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            m_moveSpeed = 0;
            m_animator.SetTrigger("Fall");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            if (!CollectedObjs.Contains(other.gameObject))
            {
                Debug.Log("apple");
                other.transform.rotation = Quaternion.identity;
                if (CollectedObjs.Count > 0)
                {
                    other.transform.position = new Vector3(CollectedObjs[CollectedObjs.Count - 1].transform.position.x, CollectedObjs[CollectedObjs.Count - 1].transform.position.y + (other.transform.localScale.y / 3), CollectedObjs[CollectedObjs.Count - 1].transform.position.z);
                    cam.GetComponent<SmoothFollow>().targets.Add(other.transform);
                }
                else
                {
                    other.transform.position = CarryObject.transform.position;
                }
                CollectableCount++;
                CollectedObjs.Add(other.gameObject);
                other.transform.parent = CarryObject;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            m_isGrounded = false;
        }
    }

    public void ActivateRagdoll()
    {
        m_animator.enabled = false;
        m_rigidBody.isKinematic = true;
        m_rigidBody.useGravity = false;
        GetComponent<BoxCollider>().enabled = false;
        foreach (Rigidbody item in ragdollRigidbodies)
        {
            item.useGravity = true;
            item.isKinematic = false;
        }
        foreach (Collider item in ragdollColliders)
        {
            item.enabled = true;
        }
        metarig.SetActive(true);
    }

    public void ThrowCollectedObjs()
    {
        Debug.Log("Throw Objs");
        foreach (GameObject item in CollectedObjs)
        {
            item.transform.parent = null;
            cam.GetComponent<SmoothFollow>().targets.Remove(item.transform);
            item.GetComponent<Collider>().isTrigger = false;
            item.GetComponent<Rigidbody>().useGravity = true;
            item.GetComponent<Rigidbody>().AddForce(new Vector3(0, 60, 85));
        }
        CollectedObjs.Clear();
    }
}
