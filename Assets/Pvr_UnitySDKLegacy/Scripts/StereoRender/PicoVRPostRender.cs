#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#elif UNITY_IPHONE
#define IOS_DEVICE
#elif UNITY_STANDALONE_WIN
#define WIN_DEVICE
#endif
#endif

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class PicoVRPostRender : MonoBehaviour
{


    public Camera cam { get; private set; }

    void Awake()
    {
        cam = GetComponent<Camera>();
        Reset();
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

        cam.clearFlags = CameraClearFlags.Depth;
        cam.backgroundColor = Color.magenta;

        cam.orthographic = true;
        cam.orthographicSize = 0.5f;
        cam.cullingMask = 0;
        cam.useOcclusionCulling = false;
        cam.depth = 100;
    }



    void OnRenderObject()
    {
        if (Camera.current != cam)
            return;


#if ANDROID_DEVICE


        //int eyeTextureId = PicoVRManager.SDK.currentDevice.eyeTextureIds[PicoVRManager.SDK.currentDevice.currEyeTextureIdx + (int)(PicoVRManager.Eye.LeftEye) * 3];
        //PVRPluginEvent.IssueWithData(RenderEventType.LeftEyeEndFrame, eyeTextureId);
        //eyeTextureId = PicoVRManager.SDK.currentDevice.eyeTextureIds[PicoVRManager.SDK.currentDevice.currEyeTextureIdx + (int)(PicoVRManager.Eye.RightEye) * 3];
        //PVRPluginEvent.IssueWithData(RenderEventType.RightEyeEndFrame, eyeTextureId);

        if (PicoVRManager.SDK.VRModeEnabled)
        {
            Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.TimeWarp, PicoVRManager.SDK.timewarpID);
        }
#endif

#if IOS_DEVICE
         /*       
        PicoVRIOSDevice.PVR_SetRenderTextureID_Native(0, PicoVRManager.SDK.currentDevice.eyeTextureIds[0]);
        PicoVRIOSDevice.PVR_SetRenderTextureID_Native(1, PicoVRManager.SDK.currentDevice.eyeTextureIds[1]);

        if (PicoVRManager.SDK.VRModeEnabled)
        {
            GL.IssuePluginEvent(PicoVRIOSDevice.PVR_GLEventID);
        }

        PicoVRManager.SDK.currentDevice.eyeTextures[0].DiscardContents();
        PicoVRManager.SDK.currentDevice.eyeTextures[1].DiscardContents();
		*/
#endif

    }
}
