using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

    Vector3 startingPos;
    private Vector2 directiontoInitialPos;
    public float DirectionalInitialPosForce;
    private bool nukeThrown;
    float TimeSinceLaunch;

    AudioSource source;

    public AudioClip TensionClip;
    public AudioClip LaunchClip;
    
    private void Awake()
    {
        startingPos = transform.position;
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {

        GetComponent<LineRenderer>().SetPosition(1, startingPos);
        GetComponent<LineRenderer>().SetPosition(0, transform.position);

        if (transform.position.x <= -30 || transform.position.x >= 20
            || transform.position.y <= -20 || transform.position.y >=20
            || TimeSinceLaunch >= 2f)
        {
            string currentLoadScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentLoadScene);
        }

        if (nukeThrown == true && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.1f)
        {
            TimeSinceLaunch += Time.deltaTime;
        }

    }



    private void OnMouseDown()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        GetComponent<LineRenderer>().enabled = true;
        source.clip = TensionClip;
        source.Play();
        
    }

    private void OnMouseUp()
    {
        nukeThrown = true;

        GetComponent<SpriteRenderer>().color = Color.white;
        directiontoInitialPos = startingPos - transform.position;
        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);

        GetComponent<Rigidbody2D>().gravityScale = 1;
        GetComponent<LineRenderer>().enabled = false;

        source.clip = LaunchClip;
        source.Play();
    }

    private void OnMouseDrag()
    {
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(newPosition.x, newPosition.y, 0);
    }
}

//// Start is called once before the first execution of Update after the MonoBehaviour is created
//void Start()
//{

//}

//// Update is called once per frame
//void Update()
//{

//    if (Input.GetMouseButtonDown(0))
//    {
//        GetComponent<SpriteRenderer>().color = Color.blue;
//    }

//    if (Input.GetMouseButtonUp(0))
//    {
//        GetComponent<SpriteRenderer>().color = Color.white;
//    }

//    if (Input.GetMouseButtonDown(0))
//    {
//        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        transform.position = new Vector3(newPosition.x, newPosition.y, 0);
//    }
//}