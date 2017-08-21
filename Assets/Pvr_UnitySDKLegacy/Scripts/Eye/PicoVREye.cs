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
using System.Linq;
using System.Runtime.InteropServices;


[RequireComponent(typeof(Camera))]
public class PicoVREye : MonoBehaviour
{
    /************************************    Properties  *************************************/
    #region Properties
    public PicoVRManager.Eye eye;

    private int InitCullingMask = 1;

    public int bufferSize = 3;

    private PicoVREyeManager controller;

    public PicoVREyeManager Controller
    {
        get
        {
            if (transform.parent == null)
            {
                return null;
            }
            if ((Application.isEditor && !Application.isPlaying) || controller == null)
            {
                return transform.parent.GetComponentInParent<PicoVREyeManager>();
            }
            return controller;
        }
    }

    private Material material;
    new public Camera camera { get; private set; }

    #endregion

    /************************************ Process Interface  *********************************/
    #region Process Interface

    /// <summary>
    /// fix 投影参数（Unity Editor 模拟）
    /// </summary>
    private void FixProjection(ref Matrix4x4 proj, float near, float far, float ipdScale)
    {

#if !ANDROID_DEVICE
        float aspectFix = camera.rect.height / camera.rect.width / 2;
#endif
#if ANDROID_DEVICE
        float aspectFix = 1;
#endif
        proj[0, 0] *= aspectFix;

        Vector2 dir = transform.localPosition; // ignore Z
        dir = dir.normalized * ipdScale;
        proj[0, 2] *= Mathf.Abs(dir.x);
        proj[1, 2] *= Mathf.Abs(dir.y);


        proj[2, 2] = (near + far) / (near - far);
        proj[2, 3] = 2 * near * far / (near - far);
    }

    /// <summary>
    /// 设置相关参数
    /// </summary>
    private void Setup()
    {
        if (controller == null)
        {
            return;
        }
#if UNITY_EDITOR
       
        var monoCamera = controller.GetComponent<Camera>();

        Matrix4x4 proj = PicoVRManager.SDK.Projection(eye);

        CopyCameraAndMakeSideBySide(controller, proj[0, 2], proj[1, 2]);

        float lerp = Mathf.Clamp01(controller.matchByZoom) * Mathf.Clamp01(controller.matchMonoFOV);

        float monoProj11 = monoCamera.projectionMatrix[1, 1];
        float zoom = 1;
        if (proj[1, 1] != 0 && monoProj11 != 0 && proj[1, 1] != 0)
        {
            zoom = 1 / Mathf.Lerp(1 / proj[1, 1], 1 / monoProj11, lerp) / proj[1, 1];
        }
        proj[0, 0] *= zoom;
        proj[1, 1] *= zoom;

        float ipdScale;
        float eyeOffset;
        controller.ComputeStereoAdjustment(proj[1, 1], transform.lossyScale.z,
                                           out ipdScale, out eyeOffset);

        transform.localPosition = ipdScale * PicoVRManager.SDK.EyeOffset(eye) +
                                  eyeOffset * Vector3.forward;


        // Set up the eye's projection.
        float near = monoCamera.nearClipPlane;
        float far = monoCamera.farClipPlane;
        FixProjection(ref proj, near, far, ipdScale);
        camera.projectionMatrix = proj;

        if (Application.isEditor)
        {
            camera.fieldOfView = 2 * Mathf.Atan(1 / proj[1, 1]) * Mathf.Rad2Deg;
            Matrix4x4 realProj = PicoVRManager.SDK.UndistortedProjection(eye);
            FixProjection(ref realProj, near, far, ipdScale);

            Vector4 projvec = new Vector4(proj[0, 0] / zoom, proj[1, 1] / zoom,
                                          proj[0, 2] - 1, proj[1, 2] - 1) / 2;
            Vector4 unprojvec = new Vector4(realProj[0, 0], realProj[1, 1],
                                            realProj[0, 2] - 1, realProj[1, 2] - 1) / 2;
            Shader.SetGlobalVector("_Projection", projvec);
            Shader.SetGlobalVector("_Unprojection", unprojvec);
            PicoVRConfigProfile p = PicoVRManager.SDK.picoVRProfile;

            float distortionFactor = 0.0241425f;
            Shader.SetGlobalVector("_Distortion1",
                                   new Vector4(p.device.devDistortion.k1, p.device.devDistortion.k2, p.device.devDistortion.k3, distortionFactor));
            Shader.SetGlobalVector("_Distortion2",
                                   new Vector4(p.device.devDistortion.k4, p.device.devDistortion.k5, p.device.devDistortion.k6));
        }

        if (controller.StereoScreen == null)
        {

            Rect rect = camera.rect;

            if (Application.isEditor)
            {
                Rect view = PicoVRManager.SDK.EyeRect(eye);
                if (eye == PicoVRManager.Eye.RightEye)
                {
                    rect.x -= 0.5f;
                }
                rect.width *= 2 * view.width;
                rect.x = view.x + 2 * rect.x * view.width;
                rect.height *= view.height;
                rect.y = view.y + rect.y * view.height;
            }
            camera.rect = rect;
        }


#else
        // NOTE : Android && PC && IOS using the same setting !!!!!
        
		transform.localPosition = PicoVRManager.SDK.EyeOffset(eye);
		camera.aspect = 1.0f;
		Rect rect = new Rect(0,0,1,1); 
		camera.fieldOfView = PicoVRManager.SDK.eyeFov;
		camera.rect = rect;
#endif
    }

    /// <summary>
    /// 异步渲染
    /// </summary>
    public void AyscRender()
    {
        Setup();
        int index = (int)eye * bufferSize + PicoVRManager.SDK.currentDevice.currEyeTextureIdx;
        camera.targetTexture =
                     PicoVRManager.SDK.currentDevice.eyeTextures[index];
        camera.enabled = true;
    }

    /// <summary>
    /// 同步渲染
    /// </summary>
    public void Render()
    {
        Setup();
#if ANDROID_DEVICE
         if ( !PicoVRManager.SDK.currentDevice.Async)
            {
            int index = (int) eye* bufferSize + PicoVRManager.SDK.currentDevice.currEyeTextureIdx;
         camera.targetTexture =
                 PicoVRManager.SDK.currentDevice.eyeTextures[index];
        
                //camera.Render();
                //camera.targetTexture = null;
            }
       //if (camera.actualRenderingPath == RenderingPath.DeferredLighting)
       //     QualitySettings.antiAliasing = 0;

#endif
#if UNITY_EDITOR
        camera.Render();
#endif


    }

    /// <summary>
    /// 设置相机参数
    /// Note ：当且仅当 不存在camera时候才会调用
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="parx"></param>
    /// <param name="pary"></param>
    public void CopyCameraAndMakeSideBySide(PicoVREyeManager controller,
                                            float parx, float pary)
    {
        
       
        camera.CopyFrom(controller.GetComponent<Camera>());
        camera.cullingMask = InitCullingMask;
		Camera headCamera = controller.GetComponent<Camera>();
		camera.aspect = headCamera.aspect;
		camera.rect = headCamera.rect;
		camera.fieldOfView = headCamera.fieldOfView;
#if !ANDROID_DEVICE && !IOS_DEVICE
        float ipd = PicoVRManager.SDK.picoVRProfile.device.devLenses.separation * controller.stereoMultiplier;
        transform.localPosition = (eye == PicoVRManager.Eye.LeftEye ? -ipd / 2 : ipd / 2) * Vector3.right;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        Rect rect = camera.rect;
        Vector2 center = rect.center;
        center.x = Mathf.Lerp(center.x, 0.5f, Mathf.Clamp01(controller.stereoPaddingX));
        center.y = Mathf.Lerp(center.y, 0.5f, Mathf.Clamp01(controller.stereoPaddingY));
        rect.center = center;
        float width = Mathf.SmoothStep(-0.5f, 0.5f, (rect.width + 1) / 2);
        rect.x += (rect.width - width) / 2;
        rect.width = width;
        rect.x *= (0.5f - rect.width) / (1 - rect.width);
        if (eye == PicoVRManager.Eye.RightEye)
        {
            rect.x += 0.5f; // Move to right half of the screen.
        }
        float parallax = Mathf.Clamp01(controller.screenParallax);
        if (controller.GetComponent<Camera>().rect.width < 1 && parallax > 0)
        {
            rect.x -= parx / 4 * parallax; // Extra factor of 1/2 because of side-by-side stereo.
            rect.y -= pary / 2 * parallax;
        }
#endif
#if ANDROID_DEVICE || IOS_DEVICE
        Rect  rect = new Rect(0,0,1,1);
#endif
        camera.rect = rect;

    }

    /// <summary>
    /// andriod 描黑边
    /// </summary>
    private Material mat_Vignette;

    /// <summary>
    /// andriod 描黑边
    /// </summary>
    void DrawVignetteLine()
    {
        if (null == mat_Vignette)
        {
            mat_Vignette = new Material(Shader.Find("Diffuse"));//Mobile/
            if (null == mat_Vignette)
            {
                return;
            }
        }
        GL.PushMatrix();
        mat_Vignette.SetPass(0);
        GL.LoadOrtho();
        vignette();
        GL.PopMatrix();
    }
    /// <summary>
    /// andriod 描黑边
    /// </summary>
    void vignette()
    {
        //GL.Begin(GL.LINES);//can not to set line width
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);
        //top
        GL.Vertex3(0.0f, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.995f, 0.0f);
        GL.Vertex3(0.0f, 0.995f, 0.0f);
        //bottom
        GL.Vertex3(0.0f, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.005f, 0.0f);
        GL.Vertex3(1.0f, 0.005f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 0.0f);
        //left
        GL.Vertex(new Vector3(0.0f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(0.005f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(0.005f, 0.0f, 0.0f));
        GL.Vertex(new Vector3(0.0f, 0.0f, 0.0f));
        //right
        GL.Vertex(new Vector3(0.995f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(1.0f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(1.0f, 0.0f, 0.0f));
        GL.Vertex(new Vector3(0.995f, 0.0f, 0.0f));
        GL.End();
    }
    #endregion

    /*************************************  Unity API ****************************************/
    #region Application EVENT
    void Awake()
    {
        camera = this.GetComponent<Camera>();
    }

    void Start()
    {
        var ctlr = Controller;
        if (ctlr == null)
        {
            enabled = false;
        }
        controller = ctlr;
        InitCullingMask = camera.cullingMask;
#if UNITY_EDITOR
        PicoVRManager.SDK.currentDevice.InitForEye(ref material);
        Setup();
#endif
#if ANDROID_DEVICE
        if (PicoVRManager.SDK.currentDevice.Async)
        {
            camera.enabled = true;
        }
#endif

    }

#if ANDROID_DEVICE

    void OnPreCull()
    {
        if (!PicoVRManager.SDK.currentDevice.Async)
        {
            Setup();

            int index = (int)eye * bufferSize + PicoVRManager.SDK.currentDevice.currEyeTextureIdx;
            camera.targetTexture =
                    PicoVRManager.SDK.currentDevice.eyeTextures[index];

        }
    }

    void OnPostRender()
    {
        DrawVignetteLine();
        RenderEventType eventType = (eye == PicoVRManager.Eye.LeftEye) ?
        RenderEventType.LeftEyeEndFrame :
        RenderEventType.RightEyeEndFrame;
        int eyeTextureId = PicoVRManager.SDK.currentDevice.eyeTextureIds[PicoVRManager.SDK.currentDevice.currEyeTextureIdx + (int)eye * bufferSize];
        Pvr_UnitySDKPluginEvent.IssueWithData(eventType, eyeTextureId);
    }
#endif

#if IOS_DEVICE
    
//    void OnPreCull()
//    {
//		Debug.Log ("OnPreCull() eye");
//
//        Setup();
//
//        if (eye == PicoVRManager.Eye.LeftEye)
//        {
//            camera.targetTexture = PicoVRManager.SDK.currentDevice.eyeTextures[0];
//        }
//        else
//        {
//            camera.targetTexture = PicoVRManager.SDK.currentDevice.eyeTextures[1];
//        }
//
//        camera.depth = (eye == PicoVRManager.Eye.LeftEye) ?
//            (int)RenderEventType.LeftEyeEndFrame :
//                (int)RenderEventType.RightEyeEndFrame;
//    }
#endif

#if UNITY_EDITOR
    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        bool disabled = material == null;

        bool mainCamera = controller != null && controller.GetComponent<Camera>().tag == "MainCamera";

        disabled &= !mainCamera;

        if (disabled)
        {
            Graphics.Blit(source, dest);
        }
        else
        {
            Graphics.Blit(source, dest, material);
        }
    }
#endif
    #endregion
}
