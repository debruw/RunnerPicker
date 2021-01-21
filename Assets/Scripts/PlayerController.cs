using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TapticPlugin;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed = 5;
    [SerializeField] private float m_jumpForce = 8;

    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;

    private float m_currentV = 0;

    public bool m_isGrounded, isInNoJumpZone;
    private Vector3 translation;
    public float Xspeed = 25f;

    public List<GameObject> CollectedObjs;
    public GameObject metarig;
    public Rigidbody[] ragdollRigidbodies;
    public Collider[] ragdollColliders;
    public Camera cam;

    Vector2 firstPressPos, secondPressPos, currentSwipe;
    Touch touch;

    public GameObject JumpTutorialCanvas, CollectTutorial;

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

    Vector3 tempFingerPos;
    void Update()
    {
        if (!GameManager.Instance.isGameStarted)
        {
            return;
        }
        transform.position += transform.forward * m_moveSpeed * Time.deltaTime;

        if (GameManager.Instance.isInSlowMotion)
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (GameManager.Instance.currentLevel == 1)
                {
                    CollectTutorial.SetActive(false);
                }
                StartCoroutine(ScaleTime(.3f, 1, 1));
                GameManager.Instance.isInSlowMotion = false;
                lineRenderer.positionCount = 1;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                tempFingerPos = GetWorldPositionOnPlane();
                lineRenderer.SetPosition(0, tempFingerPos);
                fingerPositions.Add(tempFingerPos);
            }
            else if (Input.GetMouseButton(0))
            {
                tempFingerPos = GetWorldPositionOnPlane();
                if (Vector3.Distance(tempFingerPos, fingerPositions[fingerPositions.Count - 1]) > .1f)
                {
                    UpdateLine(tempFingerPos);
                }
                fingerPositions.Add(tempFingerPos);
            }
            return;
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            translation = new Vector3(Input.GetAxis("Mouse X"), 0, 0) * Time.deltaTime * Xspeed;
            if (GameManager.Instance.currentLevel == 1)
            {
                GameManager.Instance.Tutorial1Canvas.SetActive(false);
            }
            transform.Translate(translation, Space.World);
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -3.25f, 3.25f), transform.position.y, transform.position.z);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && m_isGrounded && !isInNoJumpZone)
        {
            Debug.Log("jump");
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
            m_animator.SetTrigger("Jump");
            if (GameManager.Instance.currentLevel == 1)
            {
                JumpTutorialCanvas.SetActive(false);
            }
            if (CollectedObjs.Count > 0)
            {
                ThrowCollectedObjs();
                StartCoroutine(ScaleTime(1, .3f, 1));
                GameManager.Instance.isInSlowMotion = true;
            }
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                transform.position = new Vector3(Mathf.Clamp(transform.position.x + touch.deltaPosition.x * 0.01f, -4, 4), transform.position.y, transform.position.z);
            }
            else if (touch.phase == TouchPhase.Began)
            {
                //save began touch 2d point
                firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                GameManager.Instance.Tutorial1Canvas.SetActive(false);
            }
            else if (m_isGrounded && touch.phase == TouchPhase.Ended && !isInNoJumpZone)
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
                    Debug.Log("jump");
                    m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
                    m_animator.SetTrigger("Jump");
                    if (GameManager.Instance.currentLevel == 1)
                    {
                        JumpTutorialCanvas.SetActive(false);
                    }
                    if (CollectedObjs.Count > 0)
                    {
                        ThrowCollectedObjs();                        
                        StartCoroutine(ScaleTime(1, .3f, 1));                        
                        GameManager.Instance.isInSlowMotion = true;
                    }
                }
            }
        }
#endif

    }

    IEnumerator ScaleTime(float start, float end, float time)
    {
        float lastTime = Time.realtimeSinceStartup;
        float timer = 0.0f;

        while (timer < time)
        {
            Time.timeScale = Mathf.Lerp(start, end, timer / time);
            timer += (Time.realtimeSinceStartup - lastTime);
            lastTime = Time.realtimeSinceStartup;
            yield return null;
        }
        if (start == 1 && GameManager.Instance.currentLevel == 1)
        {
            CollectTutorial.SetActive(true);
        }
        Time.timeScale = end;
    }

    public Vector3 GetWorldPositionOnPlane()
    {
        // Bit shift the index of the layer (9) to get a bit mask
        int layerMask = 1 << 9;

        // This would cast rays only against colliders in layer 9
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (hit.collider.CompareTag("Plane"))
            {
                return hit.point;
            }
        }
        return Vector3.zero;
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
            GameManager.Instance.isGameStarted = false;
            m_moveSpeed = 0;
            m_animator.SetTrigger("Fall");
            if (PlayerPrefs.GetInt("VIBRATION") == 1)
                TapticManager.Impact(ImpactFeedback.Medium);
            StartCoroutine(GameManager.Instance.WaitAndGameLose());
        }
    }

    public GameObject PickUpParticle;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            Collect2(other.gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            GameManager.Instance.isGameStarted = false;
            m_moveSpeed = 0;
            m_animator.SetTrigger("Fall");
            if (PlayerPrefs.GetInt("VIBRATION") == 1)
                TapticManager.Impact(ImpactFeedback.Medium);
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
        else if (other.CompareTag("NoJumpZone"))
        {
            isInNoJumpZone = true;
        }
        else if (other.CompareTag("JumpTrigger") && GameManager.Instance.currentLevel == 1)
        {
            JumpTutorialCanvas.SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            m_isGrounded = false;
        }
    }

    public GameObject catchAnimation3d;
    public void Collect(GameObject collectable)
    {
        StartCoroutine(WaitAndCollect(collectable, 1));
    }

    public void Collect2(GameObject collectable)
    {
        StartCoroutine(WaitAndCollect(collectable, 0));
    }

    IEnumerator WaitAndCollect(GameObject collectable, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (PlayerPrefs.GetInt("VIBRATION") == 1)
            TapticManager.Impact(ImpactFeedback.Light);
        SoundManager.Instance.playSound(SoundManager.GameSounds.Collect);
        if (!CollectedObjs.Contains(collectable))
        {
            Destroy(Instantiate(PickUpParticle, collectable.transform.position, Quaternion.identity), 2f);
            collectable.GetComponent<Collectable>().DeActivateThrowProperties();
            collectable.transform.parent = CarryObject;

            if (waitTime == 1)
            {
                Instantiate(catchAnimation3d, collectable.transform.position, Quaternion.identity);
            }

            if (CollectedObjs.Count > 0)
            {
                collectable.transform.localPosition = new Vector3(CollectedObjs[CollectedObjs.Count - 1].transform.localPosition.x, CollectedObjs[CollectedObjs.Count - 1].transform.localPosition.y + (collectable.transform.localScale.y / 2), CollectedObjs[CollectedObjs.Count - 1].transform.localPosition.z);
            }
            else
            {
                collectable.transform.localPosition = Vector3.zero;
            }

            CollectableCount++;
            CollectedObjs.Add(collectable.gameObject);
            if (CollectedObjs.Count < 8)
            {
                cam.GetComponent<SmoothFollow>().targets.Add(collectable.transform);
            }

            collectable.transform.localPosition = new Vector3(0, collectable.transform.localPosition.y, 0);
            collectable.transform.localRotation = Quaternion.identity;
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
        Debug.Log("Throw");
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

    public void ThrowCollectedObjs2()
    {
        Debug.Log("Throw2");
        for (int i = CollectedObjs.Count - 1; i >= 0; i--)
        {
            if (CollectedObjs[i] != null)
            {
                Physics.IgnoreCollision(CollectedObjs[i].GetComponent<Collider>(), GetComponent<Collider>(), true);
                CollectedObjs[i].GetComponent<Collectable>().ActivateThrowProperties(GetComponent<Collider>());
                CollectedObjs[i].GetComponent<Launcher>().Launch(new Vector3(transform.position.x + (Random.Range(-3f, 3f)), transform.position.y, transform.position.z + 5), m_moveSpeed);
            }
        }
        CollectedObjs.Clear();
    }

    public void LoadObjsWithTime()
    {
        foreach (GameObject item in CollectedObjs)
        {
            item.SetActive(false);
        }
    }

    public LineRenderer lineRenderer;
    public List<Vector3> fingerPositions;
    public void UpdateLine(Vector3 newFingerPos)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newFingerPos);
    }
}
