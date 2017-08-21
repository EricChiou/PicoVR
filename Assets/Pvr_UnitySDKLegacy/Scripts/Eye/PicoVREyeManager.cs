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

public class PicoVREyeManager : MonoBehaviour
{ 
    /************************************    Properties  *************************************/
    #region Properties

    /// <summary>
    /// 瞳距比例  默认值为1（Unity Editor 模拟）
    /// </summary>
    [Tooltip("Set the stereo level for this camera.")]
    [Range(0, 1)]
    public float stereoMultiplier = 1.0f;

    /// <summary>
    /// 与 matchByZoom 共同调节fov（Unity Editor 模拟）
    /// </summary>
    [Tooltip("How much to adjust the stereo field of view to match this camera.")]
    [Range(0, 1)]
    public float matchMonoFOV = 0;

    /// <summary>
    /// 与 matchMonoFOV 共同调节fov（Unity Editor 模拟）
    /// </summary>
    [Tooltip("Whether to adjust FOV by moving the eyes (0) or simply zooming (1).")]
    [Range(0, 1)]
    public float matchByZoom = 0;

    /// <summary>
    /// 设置焦点（兴趣点）物体（Unity Editor 模拟）
    /// </summary>
    [Tooltip("Object or point where field of view matching is done.")]
    public Transform centerOfInterest;

    /// <summary>
    /// 焦点（兴趣点）半径（Unity Editor 模拟）
    /// </summary>
    [Tooltip("If COI is an object, its approximate size.")]
    public float radiusOfInterest = 0;

    /// <summary>
    /// 焦点（兴趣点）距离调节（Unity Editor 模拟）
    /// </summary>
    [Tooltip("Adjust stereo level when COI gets too close or too far.")]
    public bool checkStereoComfort = true;

    /// <summary>
    /// 焦点（兴趣点）距离调节  最近距离（Unity Editor 模拟）
    /// </summary>
    public float MinimumComfortDistance
    {
        get
        {
            return 1.0f;
        }
    }

    /// <summary>
    /// 焦点（兴趣点）距离调节  最远距离（Unity Editor 模拟）
    /// </summary>
    public float MaximumComfortDistance
    {
        get
        {
            return 100000f;  // i.e. really really big.
        }
    }

    /// <summary>
    /// PIP调节（Unity Editor 模拟）
    /// </summary>
    [Tooltip("Adjust the virtual depth of this camera's window (picture-in-picture only).")]
    [Range(0, 1)]
    public float screenParallax = 0;

    /// <summary>
    /// PIP调节（Unity Editor 模拟）
    /// </summary>
    [Tooltip("Move the camera window horizontally towards the center of the screen (PIP only).")]
    [Range(0, 1)]
    public float stereoPaddingX = 0;

    /// <summary>
    /// PIP调节（Unity Editor 模拟）
    /// </summary>
    [Tooltip("Move the camera window vertically towards the center of the screen (PIP only).")]
    [Range(0, 1)]
    public float stereoPaddingY = 0;

    /// <summary>
    /// 渲染标志位
    /// </summary>
    private bool renderedStereo = false;

    /// <summary>
    /// 背景shader（Unity Editor 模拟）
    /// </summary>
    public Material material;

    /// <summary>
    /// 填充高度（Unity Editor 模拟）
    /// </summary>
    private int ScreenHeight
    {
        get
        {
            return Screen.height - (Application.isEditor && StereoScreen == null ? 36 : 0);
        }
    }

    /// <summary>
    /// Eyes
    /// </summary>
#if !UNITY_EDITOR
    private PicoVREye[] eyes;
#endif
    public PicoVREye[] Eyes
    {
        get
        {
#if UNITY_EDITOR
            PicoVREye[] eyes = null;
#endif
            if (eyes == null)
            {
                eyes = GetComponentsInChildren<PicoVREye>(true)
                       .Where(eye => eye.Controller == this)
                       .ToArray();
            }
            return eyes;
        }
    }

    /// <summary>
    /// RenderTexture（Unity Editor 模拟）
    /// </summary>
    public RenderTexture StereoScreen
    {
        get
        {
            return GetComponent<Camera>().targetTexture ?? PicoVRManager.SDK.StereoScreen;
        }
    }
    #endregion

    /************************************ Process Interface  *********************************/
    #region Process Interface

    /// <summary>
    /// 通过参数设置计算参数
    /// </summary>
    /// <param name="proj11"></param>
    /// <param name="zScale"></param>
    /// <param name="ipdScale"></param>
    /// <param name="eyeOffset"></param>
    public void ComputeStereoAdjustment(float proj11, float zScale,
                                      out float ipdScale, out float eyeOffset)
    {
        ipdScale = stereoMultiplier;
        eyeOffset = 0;
        if (centerOfInterest == null || !centerOfInterest.gameObject.activeInHierarchy)
        {
            return;
        }
        float distance = (centerOfInterest.position - transform.position).magnitude;
        float radius = Mathf.Clamp(radiusOfInterest, 0, distance);
        float scale = proj11 / GetComponent<Camera>().projectionMatrix[1, 1];  // vertical FOV
        float offset =
            Mathf.Sqrt(radius * radius + (distance * distance - radius * radius) * scale * scale);
        eyeOffset = (distance - offset) * Mathf.Clamp01(matchMonoFOV) / zScale;

        if (checkStereoComfort)
        {
            float minComfort = MinimumComfortDistance;
            float maxComfort = MaximumComfortDistance;
            if (minComfort < maxComfort)
            {
                float minDistance = (distance - radius) / zScale - eyeOffset;
                ipdScale *= minDistance / Mathf.Clamp(minDistance, minComfort, maxComfort);
            }
        }

    }

    /// <summary>
    /// 创建eye
    /// </summary>
    /// <param name="eye"></param>
    private void CreateEye(PicoVRManager.Eye eye)
    {
        string nm = name + (eye == PicoVRManager.Eye.LeftEye ? " LeftEye" : " RightEye");
        GameObject go = new GameObject(nm);
        go.transform.parent = transform;
        go.AddComponent<Camera>().enabled = false;
#if !UNITY_5
    if (GetComponent<GUILayer>() != null) {
      go.AddComponent<GUILayer>();
    }
    if (GetComponent("FlareLayer") != null) {
      go.AddComponent("FlareLayer");
    }
#endif
        var picovrEye = go.AddComponent<PicoVREye>();
        picovrEye.eye = eye;
        picovrEye.CopyCameraAndMakeSideBySide(this, 0, 0);
    }

    /// <summary>
    /// 填充颜色
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="color"></param>
    private void FillScreenRect(int width, int height, Color color)
    {
        int x = Screen.width / 2;
        int y = Screen.height / 2;
        if (Application.isEditor && StereoScreen == null)
        {
            y -= 15;
        }
        width /= 2;
        height /= 2;
        material.color = color;
        material.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix();
        GL.Color(Color.white);
        GL.Begin(GL.QUADS);
        GL.Vertex3(x - width, y - height, 0);
        GL.Vertex3(x - width, y + height, 0);
        GL.Vertex3(x + width, y + height, 0);
        GL.Vertex3(x + width, y - height, 0);
        GL.End();
        GL.PopMatrix();

    }

    /// <summary>
    /// 添加eye
    /// </summary>
    public void AddStereoRig()
    {
        if (Eyes.Length > 0)
        {
            return;
        }
        CreateEye(PicoVRManager.Eye.LeftEye);
        CreateEye(PicoVRManager.Eye.RightEye);
    }
    #endregion

    /*************************************  Unity API ****************************************/
    #region Application EVENT
    void Awake()
    {
        AddStereoRig();
        if (null == material)
        {
            Shader shader = Shader.Find("PicovrSDK/FillColor");
            if (shader == null)
            {
                Debug.LogWarning("Undistortion disabled: shader not found.");
                return;
            }
            material = new Material(shader);
        }

    }

    void OnEnable()
    {
        StartCoroutine("EndOfFrame");
    }

    void Update()
    {
        if (PicoVRManager.SDK.currentDevice.Async)
        {
            for (int i = 0; i < Eyes.Length; i++)
            {
                Eyes[i].AyscRender();
            }
            
        }
    }

    void OnPreCull()
    {
        if (!PicoVRManager.SDK.VRModeEnabled)
        {
            return;
        }
#if UNITY_EDITOR|| ANDROID_DEVICE || IOS_DEVICE     
       GetComponent<Camera>().enabled = false;
#endif
        bool mainCamera = (tag == "MainCamera");
        if (mainCamera)
        {
            if (Application.isEditor)
            {
                GL.Clear(true, false, Color.black);
                FillScreenRect(Screen.width, ScreenHeight, Color.black);
                for (int i = 0; i < Eyes.Length; i++)
                {
                    Eyes[i].Render();
                }
            }
            else
            {
#if IOS_DEVICE || ANDROID_DEVICE

                if (!PicoVRManager.SDK.currentDevice.Async)
                {
                    for (int i = 0; i < Eyes.Length; i++)
                    {
                        Eyes[i].camera.enabled = true;
                    }

                    //Debug.Log("kkk main OnPreCull depth : " + GetComponent<Camera>().depth);
                }
#else                
                if (!PicoVRManager.SDK.currentDevice.Async)
                {
                    GL.Clear(true, true, Color.black);
                    for (int i = 0; i < Eyes.Length; i++)
                    {
                        Eyes[i].Render();
                    }
                }
#endif
            }
        }
     
        PicoVRManager.SDK.upDateState = false;
        renderedStereo = true;
    }



    void OnDisable()
    {
        StopAllCoroutines();
    }
    #endregion

    /************************************    IEnumerator  *************************************/
    IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
           
            if (renderedStereo)
            {
                GetComponent<Camera>().enabled = true;
                renderedStereo = false;
            }

		
		
			#if IOS_DEVICE
			int LeyeTextureId = PicoVRManager.SDK.currentDevice.eyeTextureIds[PicoVRManager.SDK.currentDevice.currEyeTextureIdx + 0];
			int ReyeTextureId = PicoVRManager.SDK.currentDevice.eyeTextureIds[PicoVRManager.SDK.currentDevice.currEyeTextureIdx + 3];

			PicoVRIOSDevice.PVR_SetRenderTextureID_Native(0, LeyeTextureId);
			PicoVRIOSDevice.PVR_SetRenderTextureID_Native(1, ReyeTextureId);
			
			if (PicoVRManager.SDK.VRModeEnabled)
			{
				GL.IssuePluginEvent(PicoVRIOSDevice.PVR_GLEventID);
			}
			
			PicoVRManager.SDK.currentDevice.eyeTextures[PicoVRManager.SDK.currentDevice.currEyeTextureIdx + 0].DiscardContents();
			PicoVRManager.SDK.currentDevice.eyeTextures[PicoVRManager.SDK.currentDevice.currEyeTextureIdx + 3].DiscardContents();
			#endif
            PicoVRManager.SDK.currentDevice.UpdateTextures();
        }
    }
}