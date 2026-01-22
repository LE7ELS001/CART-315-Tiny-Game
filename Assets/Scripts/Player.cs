using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    private Rigidbody m_RigidBody;

    public float fMaxForce = 500.0f;
    private float m_CurForce = 0.0f;

    public GameObject Box = null;
    public float fMinDisatance = 1.2f;
    public float fMaxDisatance = 3.0f;
    public float fMinHeight = 0.3f;
    public float fMaxHeight = 2.0f;

    private Vector3 m_Direction = Vector3.forward;
    private float m_Distance = 0.0f;
    private float m_Height = 0.0f;

    private GameObject m_CurCube = null;
    private GameObject m_NextCube = null;
    private GameObject m_Plane = null;

    private Vector3 m_CameraOffset = Vector3.zero;

    private Animator m_Aniator = null;

    private UIManager m_UI = null;

    private AudioSource m_AudioPlayer = null;
    public AudioClip PressAudio = null;
    public AudioClip JumpAudio = null;
    public AudioClip FallAudio = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_RigidBody = GetComponent<Rigidbody>();
        m_Plane = GameObject.FindGameObjectWithTag("Plane");
        m_NextCube = GeneratePlatform();
        m_Aniator = GetComponent<Animator>();
        m_UI = GetComponent<UIManager>();
        m_AudioPlayer = GetComponent<AudioSource>();
        if (m_AudioPlayer == null)
            m_AudioPlayer = gameObject.AddComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject obj = GetHitObject();
        if (obj != null)
        {
            if (obj.tag == "Cube")
            {
                if (m_CurCube == null)
                {
                    //when game starts
                    PlayAudio(FallAudio);
                    m_CurCube = obj;
                    m_CameraOffset = Camera.main.transform.position - m_CurCube.transform.position;
                }
                else if (m_NextCube == obj)
                {
                    PlayAudio(FallAudio);
                    m_UI.AddScore(1);

                    Destroy(m_CurCube);
                    m_CurCube = m_NextCube;
                    m_NextCube = GeneratePlatform();
                    m_RigidBody.Sleep();
                    m_RigidBody.WakeUp();

                    m_Aniator.SetBool("Forward", false);
                    m_Aniator.SetBool("Left", false);
                }

                ProcessInput();
                ShowScale();

                MoveCameraAndPlane();
                //m_UI.SetForceShow(true);
            }
            else if (obj.CompareTag("Plane"))
            {
                //Game over
                if (obj.CompareTag("Plane"))
                    m_UI.SetGameOver(true);
            }



        }


    }

    private void ProcessInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            m_UI.SetForceShow(true);
            m_AudioPlayer.clip = PressAudio;
            m_AudioPlayer.loop = true;
            m_AudioPlayer.Play();
        }

        if (Mouse.current.leftButton.IsPressed())
        {
            m_CurForce += Time.deltaTime * fMaxForce / 2.0f;
            if (m_CurForce > fMaxForce)
            {

                m_CurForce = fMaxForce;
            }
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Jump();
            m_CurForce = 0.0f;
            m_UI.SetForceShow(false);
            m_AudioPlayer.loop = false;
            m_AudioPlayer.Stop();
        }
        m_UI.ShowForce(m_CurForce, fMaxForce);

    }

    //Accumulate force
    private void ShowScale()
    {
        float sc = (fMaxForce - m_CurForce * 0.5f) / fMaxForce;
        Vector3 scale = transform.localScale;
        scale.y = sc * 0.2f;
        transform.localScale = scale;
    }

    private void Jump()
    {
        PlayAudio(JumpAudio);
        m_RigidBody.AddForce(Vector3.up * m_CurForce);
        Vector3 dir = m_NextCube.transform.position - transform.position;
        dir.y = 0;
        m_RigidBody.AddForce(dir.normalized * m_CurForce);
        if (m_Direction == Vector3.forward)
        {
            m_Aniator.SetBool("Forward", true);
        }
        else
        {
            m_Aniator.SetBool("Left", true);
        }
    }



    private GameObject GeneratePlatform()
    {
        GameObject obj = GameObject.Instantiate(Box);

        m_Distance = Random.Range(fMinDisatance, fMaxDisatance);
        m_Height = Random.Range(fMinHeight, fMaxHeight);
        m_Direction = Random.Range(0, 2) == 1 ? Vector3.forward : Vector3.left;

        Vector3 pos = Vector3.zero;
        if (m_CurCube == null)
        {
            pos = m_Direction * m_Distance + transform.position;
        }
        else
        {
            pos = m_Direction * m_Distance + m_CurCube.transform.position;
        }
        pos.y = 2.0f;
        obj.transform.position = pos;

        obj.transform.localScale = new Vector3(1, m_Height, 1);

        obj.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

        return obj;
    }

    private GameObject GetHitObject()
    {
        RaycastHit hit;
        Vector3[] offests = { Vector3.zero, Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (Vector3 offset in offests)
        {
            if (Physics.Raycast(transform.position + offset * 0.1f, Vector3.down, out hit, 0.3f))
            {
                //Debug.Log(hit.collider.tag);
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Vector3[] offests = { Vector3.zero, Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    //     foreach (Vector3 offset in offests)
    //     {
    //         //Debug.Log(hit.collider.tag);
    //         Gizmos.DrawLine(transform.position + offset * 0.1f,
    //         transform.position + offset * 0.1f + Vector3.down * 0.3f);
    //     }
    // }

    private void MoveCameraAndPlane()
    {
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, m_CurCube.transform.position + m_CameraOffset, Time.deltaTime * 2);
        Vector3 pos = m_CurCube.transform.position;
        pos.y = 0;
        m_Plane.transform.position = pos;
    }

    private void PlayAudio(AudioClip clp)
    {
        m_AudioPlayer.Stop();
        m_AudioPlayer.clip = clp;
        m_AudioPlayer.Play();
    }
}
