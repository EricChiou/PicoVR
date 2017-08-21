using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public float gazeTime = 3f;

    private IEnumerator countDown;
    private Material red;
    private Material blue;

    // Use this for initialization
    void Start()
    {
        red = Resources.Load("Material/Red", typeof(Material)) as Material;
        blue = Resources.Load("Material/Blue", typeof(Material)) as Material;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnGazeEnter()
    {
        gameObject.GetComponent<Renderer>().material = red;
        countDown = startCountDown();
        StartCoroutine(countDown);
    }

    public void OnGazeLeave()
    {
        if (countDown != null)
        {
            StopCoroutine(countDown);
        }
        gameObject.GetComponent<Renderer>().material = blue;
    }

    IEnumerator startCountDown()
    {
        float startTime = Time.time;
        while ((Time.time - startTime) < gazeTime)
        {
            yield return null;
        }
        StartCoroutine(moveCamera());
    }

    IEnumerator moveCamera()
    {
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        while (Vector3.Distance(mainCamera.transform.position, gameObject.transform.position) > 0.5f)
        {
            mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, gameObject.transform.position, 0.05f);
            yield return null;
        }
    }
}
