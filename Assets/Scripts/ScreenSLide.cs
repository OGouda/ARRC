using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenSLide : MonoBehaviour
{
    public Vector3 first;
    public Vector3 last;
    public Vector3 swipe;
    public Vector3 camstartpoint;
    float distance;
    public float nearstplane;
    public float speed;
    public GameObject maincam;
    public Transform[] Screens;

    public void StartBt()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    // Start is called before the first frame update
    void Start()
    {
        //Screens = GameObject.Find("Screens").GetComponentsInChildren<Transform>();
        distance = Screen.height * 15 / 100;
        maincam = Camera.main.gameObject;
        camstartpoint = maincam.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButton(0))
        {
            foreach (Transform p in Screens)
            {
                nearstplane = (p.position - maincam.transform.position).x;
                if (Mathf.Abs(nearstplane) > 0 && Mathf.Abs(nearstplane) < 10)
                {
                    Vector3 newposition = new Vector3(p.position.x, maincam.transform.position.y, maincam.transform.position.z);
                    maincam.transform.position = Vector3.Lerp(maincam.transform.position, newposition, .05f);
                }
                else if (maincam.transform.position.x < 0)
                {
                    maincam.transform.position = new Vector3(Screens[0].position.x, Screens[0].position.y, -10);
                }
                else if (maincam.transform.position.x > 100) // Change this everytime you add another slide to prevent it from going after last slide// add 20 for each slide 
                {
                    maincam.transform.position = new Vector3(Screens[5].position.x, Screens[5].position.y, -10);
                }


            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            first = Input.mousePosition;
        }
        if(Input.GetMouseButton(0)||Input.GetMouseButtonUp(0))
        {
            last = Input.mousePosition;
            swipe = new Vector3(last.x - first.x, 0, 0);
        }
        {
            maincam.transform.Translate(-swipe * speed * Time.deltaTime *1.5f);
            swipe = new Vector3(0, 0, 0);
            first = last;
        }
        if (Input.touchCount == 1)
        {
            Touch touch1 = Input.GetTouch(0);
            if(touch1.phase == TouchPhase.Began)
            {
                first = touch1.position;
            }
            if (touch1.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Ended)
            {
                last = touch1.position;
                swipe = new Vector3(last.x - first.x, 0, 0);
            }

            {
                maincam.transform.Translate(-swipe * speed * Time.deltaTime *1.5f);
                swipe = new Vector3(0, 0, 0);
                first = last;
            }
        }
    }
}
