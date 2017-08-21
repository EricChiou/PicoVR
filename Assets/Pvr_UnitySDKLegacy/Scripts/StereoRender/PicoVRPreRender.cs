using UnityEngine;
//using System.Collections;


[RequireComponent(typeof(Camera))]
public class PicoVRPreRender : MonoBehaviour
{


    public Camera cam { get; private set; }

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Use this for initialization
    void Start()
    {

    }


    void Reset()
    {
#if UNITY_EDITOR
        var cam = GetComponent<Camera>();
#endif

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = 0;
        cam.useOcclusionCulling = false;
        cam.depth = -100;
    }
}
