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
        if (!GameManager.Instance.isGameStarted)
        {
            return;
        }
        transform.position += transform.forward * m_moveSpeed * Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            translation = new Vector3(Input.GetAxis("Mouse X"), 0, 0) * Time.deltaTime * Xspeed;

            transform.Translate(translation, Space.World);
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -3.5f, 3.5f), transform.position.y, transform.position.z);
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
        if (Input.GetKeyDown(KeyCode.Space) && m_isGrounded)
        {
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
            m_animator.SetTrigger("Jump");
            ThrowCollectedObjs();
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
            StartCoroutine(GameManager.Instance.WaitAndGameLose());
        }
        else if (collision.gameObject.CompareTag("Collectable"))
        {
            if (collision.gameObject.GetComponent<Collectable>().isThrowed)
            {
                collision.gameObject.GetComponent<Collectable>().DeActivateThrowProperties();
                if (CollectedObjs.Count > 0)
                {
                    collision.gameObject.transform.position = new Vector3(CollectedObjs[CollectedObjs.Count - 1].transform.position.x, CollectedObjs[CollectedObjs.Count - 1].transform.position.y + (collision.gameObject.transform.localScale.y / 2), CollectedObjs[CollectedObjs.Count - 1].transform.position.z);
                }
                else
                {
                    collision.gameObject.transform.position = CarryObject.transform.position;
                }
                if (CollectedObjs.Count < 6)
                {
                    cam.GetComponent<SmoothFollow>().targets.Add(collision.gameObject.transform);
                }
                CollectableCount++;
                CollectedObjs.Add(collision.gameObject.gameObject);
                collision.gameObject.transform.parent = CarryObject;
                collision.gameObject.transform.localPosition = new Vector3(0, collision.gameObject.transform.localPosition.y, 0);
            }
        }
    }

    public GameObject PickUpParticle;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            Debug.Log("Trigger : " + !CollectedObjs.Contains(other.gameObject));
            if (!CollectedObjs.Contains(other.gameObject))
            {
                Destroy(Instantiate(PickUpParticle, other.transform.position, Quaternion.identity), 2f);
                other.GetComponent<Collectable>().DeActivateThrowProperties();
                other.transform.rotation = Quaternion.identity;
                if (CollectedObjs.Count > 0)
                {
                    other.transform.position = new Vector3(CollectedObjs[CollectedObjs.Count - 1].transform.position.x, CollectedObjs[CollectedObjs.Count - 1].transform.position.y + (other.transform.localScale.y / 2), CollectedObjs[CollectedObjs.Count - 1].transform.position.z);
                }
                else
                {
                    other.transform.position = CarryObject.transform.position;
                }
                cam.GetComponent<SmoothFollow>().targets.Add(other.transform);
                CollectableCount++;
                CollectedObjs.Add(other.gameObject);
                other.transform.parent = CarryObject;
                other.transform.localPosition = new Vector3(0, other.transform.localPosition.y, 0);
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            m_moveSpeed = 0;
            m_animator.SetTrigger("Fall");
            StartCoroutine(GameManager.Instance.WaitAndGameLose());
        }
        else if (other.CompareTag("FinishLine"))
        {
            m_moveSpeed = 0;
            GameManager.Instance.isGameStarted = false;
            GetComponent<Animator>().SetTrigger("Idle");
            SmoothFollow.Instance.isOnFinish = true;
            GameManager.Instance.TapToLoadButton.SetActive(true);
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

    public Vector3 ThrowForce;
    public void ThrowCollectedObjs()
    {
        for (int i = CollectedObjs.Count - 1; i >= 0; i--)
        {
            if (CollectedObjs[i] != null)
            {
                Physics.IgnoreCollision(CollectedObjs[i].GetComponent<Collider>(), GetComponent<Collider>(), true);
                CollectedObjs[i].GetComponent<Collectable>().ActivateThrowProperties(GetComponent<Collider>());
                CollectedObjs[i].GetComponent<Launcher>().Launch(new Vector3(transform.position.x + (Random.Range(-3f, 3f)), transform.position.y, transform.position.z), m_moveSpeed);
            }
        }
        CollectedObjs.Clear();
    }

    public void LoadObjs()
    {
        foreach (GameObject item in CollectedObjs)
        {
            item.SetActive(false);
        }
    }
}
