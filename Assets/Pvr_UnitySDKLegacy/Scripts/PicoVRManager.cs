#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#elif UNITY_IPHONE
#define IOS_DEVICE
#elif UNITY_STANDALONE_WIN
#define WIN_DEVICE
#endif
#endif
using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
/// <summary>
/// 总控Manager
/// </summary>
public class PicoVRManager : MonoBehaviour
{
    /************************************    Properties  *************************************/
    #region Properties
    /// <summary>
    /// resume 标志位
    /// </summary>
    public bool onResume = false;

	public const int SERVICE_STARTED = 0;
	public const int CONNECTE_SUCCESS = 1;
	public const int DISCONNECTE = 2;
	public const int CONNECTE_FAILED = 3;
	public const int NO_DEVICE = 4;

    public const float NECK_Y = 0.075f;
    public const float NECK_Z = 0.08f;

    private static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));

    /// <summary>
    /// SDK 单例
    /// </summary>
    private static PicoVRManager sdk = null;
    public static PicoVRManager SDK
    {
        get
        {
            if (sdk == null)
            {
                sdk = UnityEngine.Object.FindObjectOfType<PicoVRManager>();
            }
            if (sdk == null)
            {
                var go = new GameObject("PicoVRManager");
                sdk = go.AddComponent<PicoVRManager>();
                go.transform.localPosition = Vector3.zero;
            }
            return sdk;
        }
    }

    /// <summary>
    /// 是否使用Falcon box sensor
    /// </summary>
    [SerializeField]
    private bool useFalconBoxSensor = false;
    public bool UseFalconBoxSensor
    {
        get
        {
            return useFalconBoxSensor;
        }
        set
        {
            if (value != useFalconBoxSensor)
            {
                useFalconBoxSensor = value;
            }
        }
    }

    /// <summary>
    /// 是否使用 Falcon CV Position Tracking
    /// </summary>
    [SerializeField]
    private bool isFalconCV6DOFEnable = false;
    public bool IsFalconCV6DOFEnable
    {
        get
        {
            return isFalconCV6DOFEnable;
        }
        set
        {
            if (value != isFalconCV6DOFEnable)
            {
                isFalconCV6DOFEnable = value;
            }
        }
    }

    public Quaternion boxQuaternion;
    public Matrix4x4 boxView;
    /// <summary>
    /// 显示FPS
    /// </summary>
    [SerializeField]
    private bool showFPS = false;

   
    public bool ShowFPS
    {
        get
        {            
            return showFPS;
        }
        set
        {
            if (value != showFPS)
            {
                showFPS = value;               
            }
        }
    }
    
    /// <summary>
    /// 配置文件
    /// </summary>
    public PicoVRConfigProfile picoVRProfile;

    /// <summary>
    /// Head Pose
    /// </summary>
    public PicoVRPose headPose;
    /// <summary>
    /// Hand Pose
    /// </summary>
    public PicoVRPose leftHandPose;
    public PicoVRPose rightHandPose;
    /// <summary>
    /// 当前设备
    /// </summary>
    public PicoVRBaseDevice currentDevice;

    /// <summary>
    /// 适配HMD类型
    /// </summary>
    [SerializeField]
    private PicoVRConfigProfile.DeviceTypes deviceType = PicoVRConfigProfile.DeviceTypes.PicoNeo;
    public PicoVRConfigProfile.DeviceTypes DeviceType
    {
        get
        {
            return deviceType;
        }
        set
        {
            if (value != deviceType)
            {
                deviceType = value;
            }
        }
    }

    /// <summary>
    /// 抗锯齿倍数
    /// </summary>
    [SerializeField]
    private PicoVRBaseDevice.RenderTextureAntiAliasing rtAntiAlising = PicoVRBaseDevice.RenderTextureAntiAliasing.X_2;
    public PicoVRBaseDevice.RenderTextureAntiAliasing RtAntiAlising
    {
        get
        {
            return rtAntiAlising;
        }
        set
        {
            if (value != rtAntiAlising)
            {
                rtAntiAlising = value;

            }
        }
    }

    /// <summary>
    /// RT 位深
    /// </summary>
    [SerializeField]
    private PicoVRBaseDevice.RenderTextureDepth rtBitDepth = PicoVRBaseDevice.RenderTextureDepth.BD_24;
    public PicoVRBaseDevice.RenderTextureDepth RtBitDepth
    {
        get
        {
            return rtBitDepth;
        }
        set
        {
            if (value != rtBitDepth)
                rtBitDepth = value;

        }
    }

    /// <summary>
    /// RT 类型
    /// </summary>
    [SerializeField]
    private RenderTextureFormat rtFormat = RenderTextureFormat.Default;
    public RenderTextureFormat RtFormat
    {
        get
        {
            return rtFormat;
        }
        set
        {
            if (value != rtFormat)
                rtFormat = value;

        }
    }

    /// <summary>
    /// VR （Unity Editor 模拟）
    /// 其他默认为true
    /// </summary>
    [SerializeField]
    private bool vrModeEnabled = true;
    public bool VRModeEnabled
    {

        get
        {
            return vrModeEnabled;
        }
        set
        {
            if (value != vrModeEnabled)
                vrModeEnabled = value;

        }
    }



    [SerializeField]
    private bool pvrNeck = true;

    public bool PVRNeck
    {
        get
        {
            return pvrNeck;
        }
        set
        {
            if (value != pvrNeck)
            {
                pvrNeck = value;
            }
        }
    }
    /// <summary>
    /// 消息传递参数WarpID
    /// </summary>
    public int timewarpID = 0;

    /// <summary>
    /// 更新标志位
    /// </summary>
    private bool upDated = false;

    /// <summary>
    /// 状态更新标志位
    /// </summary>
    public bool upDateState = false;

    /// <summary>
    /// RenderTexture （Unity Editor 模拟）
    /// </summary>
    [HideInInspector]
    public RenderTexture stereoScreen;
    public RenderTexture StereoScreen
    {
        get
        {
            if (!vrModeEnabled)
            {
                return null;
            }
            if (stereoScreen == null)
            {
                stereoScreen = CreateStereoScreen();
            }

            return stereoScreen;
        }
        set
        {
            try
            {
                if (value != stereoScreen)
                {
                    stereoScreen = value;
                }
            }
            catch (Exception)
            {

                Debug.LogError("StereoScreen ERROR!");
            }
        }
    }     
    public RenderTexture CreateStereoScreen()
    {
        Vector2 renderTexSize = currentDevice.GetStereoScreenSize();
        int x = (int)renderTexSize.x;
        int y = (int)renderTexSize.y;

        if (currentDevice.CanConnecttoActivity && SystemInfo.supportsRenderTextures)
        {
            var steroscreen = new RenderTexture(x, y, (int)SDK.RtBitDepth, SDK.RtFormat);
            steroscreen.anisoLevel = 0;
            steroscreen.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, (int)SDK.RtAntiAlising);
            Debug.Log("steroscreen ok");
            return steroscreen;
        }
        else
           return null;
    }

    /// <summary>
    /// 左右眼标志位
    /// </summary>
    public enum Eye
    {
        LeftEye,
        RightEye
    }

    /// <summary>
    /// 左右眼投影矩阵（Unity Editor 模拟）
    /// </summary>
    public Matrix4x4 Projection(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeProj : rightEyeProj;
    }
    [HideInInspector]
    public Matrix4x4 leftEyeProj;
    [HideInInspector]
    public Matrix4x4 rightEyeProj;

    /// <summary>
    /// 左右眼无畸变矩阵（Unity Editor 模拟）
    /// </summary>
    public Matrix4x4 UndistortedProjection(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeUndistortedProj : rightEyeUndistortedProj;
    }
    [HideInInspector]
    public Matrix4x4 leftEyeUndistortedProj;
    [HideInInspector]
    public Matrix4x4 rightEyeUndistortedProj;

    /// <summary>
    /// 凝视事件标志位
    /// </summary>
    public bool picovrTriggered { get; private set; }
    public bool newPicovrTriggered = false;
    public bool inPicovr { get; set; }
    private bool newInPicovr;

    /// <summary>
    /// 左右眼偏移量
    /// </summary>
    public Vector3 EyeOffset(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeOffset : rightEyeOffset;
    }
    public Vector3 leftEyeOffset;
    public Vector3 rightEyeOffset;

    /// <summary>
    /// 左右眼view rect（Unity Editor 模拟）
    /// </summary>
    public Rect EyeRect(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeRect : rightEyeRect;
    }
    [HideInInspector]
    public Rect leftEyeRect;
    [HideInInspector]
    public Rect rightEyeRect;

    /// <summary>
    /// headView 矩阵
    /// </summary>
    [HideInInspector]
    public Matrix4x4 headView;


    /// <summary>
    /// 左右眼view 矩阵
    /// </summary>
    [HideInInspector]
    public Matrix4x4 leftEyeView;
    [HideInInspector]
    public Matrix4x4 rightEyeView;

    /// <summary>
    /// FOV
    /// </summary>
    [HideInInspector]
    public float eyeFov = 90.0f;


    /// <summary>
    /// SimulateInput 参数
    /// </summary>
    private const float TOUCH_TIME_LIMIT = 0.2f;
    private float touchStartTime = 0;

    /// <summary>
    /// reset head 标志
    /// </summary>
    public bool reStartHead = false;
    private Vector2 prefinger1 = new Vector2(0.0f, 0.0f);
    private Vector2 prefinger2 = new Vector2(0.0f, 0.0f);
    #endregion

    /************************************ Process Interface  *********************************/
    #region Process Interface
    /// <summary>
    /// 初始化设备接口
    /// </summary>
    private void InitDevice()
    {
        if (currentDevice != null)
        {
            currentDevice.Destroy();
        }
        currentDevice = PicoVRBaseDevice.GetDevice();
    }

    /// <summary>
    /// 更新状态
    /// </summary>
    public bool UpdateState()
    {
        if (upDated)
        {
            return true;
        }
        currentDevice.UpdateState();
        leftEyeOffset = leftEyeView.GetColumn(3);
        rightEyeOffset = rightEyeView.GetColumn(3);
        upDated = true;
        return upDated;
    }
   
    public void StartHeadTrack()
    {
        currentDevice.StartHeadTrack();
    }
    public void StartControllerTrack()
    {
        currentDevice.StartControllerTrack();
    }
    /// <summary>
    /// reset head tracking
    /// </summary>
    public void ResetHead()
    {
        reStartHead = true;
        currentDevice.ResetHeadTrack();
        
        //currentDevice.ResetControllerTrack();
    }
    public void ResetHeadTrack()
    {
        reStartHead = true;
        currentDevice.ResetHeadTrack();
        //currentDevice.ResetControllerTrack();
    }
    public void StopHeadTrack()
    {
        currentDevice.StopHeadTrack();
    }
    public void StopControllerTrack()
    {
        currentDevice.StopControllerTrack();
    }
    #endregion

    /*************************************Unity Editor ***************************************/
    #region UnityEditor
    /// <summary>
    /// 模拟输入（Unity Editor 模拟）
    /// </summary>
    private void SimulateInput()
    {

        if (Input.GetMouseButtonDown(0)
            && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
        {
            if (inPicovr)
            {
                OnRemovedFromPicovrInternal();
            }
            else
            {
                OnInsertedIntoPicovrInternal();
            }
            VRModeEnabled = !VRModeEnabled;
            return;
        }
        if (!inPicovr)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            touchStartTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - touchStartTime <= TOUCH_TIME_LIMIT)
            {
                newPicovrTriggered = true;
            }
            touchStartTime = 0;
        }
    }

    /// <summary>
    ///  模拟输入调用（Unity Editor 模拟）
    /// </summary>
    void OnRemovedFromPicovrInternal()
    {
        newInPicovr = false;
    }

    /// <summary>
    ///  模拟输入调用（Unity Editor 模拟）
    /// </summary>
    void OnInsertedIntoPicovrInternal()
    {
        newInPicovr = true;
    }

    /// <summary>
    /// 切换VR模型（Unity Editor 模拟）
    /// </summary>
    public void ToggleVRMode()
    {
        vrModeEnabled = !vrModeEnabled;
    }
    #endregion

    /*************************** Public Interfaces  (android ok) *****************************/
    #region Interfaces

    /// <summary>
    /// 电池电量发生变化获取当前电量值
    /// </summary>
    /// 
    public void setBattery(string s)
    {
        Debug.Log(s.ToString() + "Battery");

    }
    public void SetBattery(string s)
    {
        Debug.Log(s.ToString() + "Battery");

    }

    /// <summary>
    /// 音量发生变化获取当前音量值
    /// </summary>
    public void setAudio(string s)
    {
        Debug.Log(s.ToString() + "Audio");
    }
    public void SetAudio(string s)
    {
        Debug.Log(s.ToString() + "Audio");
    }

	/// <summary>
	/// called from android to set BLEStatus
	/// </summary>
	public void BLEStatusCallback(string s)
	{
		switch(int.Parse(s)){
		case(SERVICE_STARTED):
			Debug.Log("BLE_SERVICE_STARTED");
			break;
		case(CONNECTE_SUCCESS):
			Debug.Log("BLE_CONNECTE_SUCCESS");
			break;
		case(DISCONNECTE):
			Debug.Log("BLE_DISCONNECTE");
			break;
		case(CONNECTE_FAILED):
			Debug.Log("BLE_CONNECTE_FAILED");
			break;
		case(NO_DEVICE):
			Debug.Log("BLE_NO_DEVICE");
			break;
		}
	}
    
    /// <summary>
    /// 获取音量最大值
    /// </summary>
    public int getMaxVolumeNumber()
    {
        int maxVolumeNumber = currentDevice.getMaxVolumeNumber();
        Debug.Log("maxVolumeNumber" + maxVolumeNumber);
        return maxVolumeNumber;
    }
    public int GetMaxVolumeNumber()
    {
        int maxVolumeNumber = currentDevice.GetMaxVolumeNumber();
        Debug.Log("maxVolumeNumber" + maxVolumeNumber);
        return maxVolumeNumber;
    }

    /// <summary>
    /// 获取当前音量
    /// </summary>
    public int getCurrentVolumeNumber()
    {
        int currentVolumeNumber = currentDevice.getCurrentVolumeNumber();
        Debug.Log("currentVolumeNumber" + currentVolumeNumber);
        return currentVolumeNumber;
    }
    public int GetCurrentVolumeNumber()
    {
        int currentVolumeNumber = currentDevice.GetCurrentVolumeNumber();
        Debug.Log("currentVolumeNumber" + currentVolumeNumber);
        return currentVolumeNumber;
    }

    /// <summary>
    /// 获取当前亮度
    /// </summary>
    public int getCurrentBrightness()
    {
        int currentBrightness = currentDevice.getCurrentBrightness();
        Debug.Log("currentBrightness" + currentBrightness);
        return currentBrightness;
    }
    public int GetCurrentBrightness()
    {
        int currentBrightness = currentDevice.GetCurrentBrightness();
        Debug.Log("currentBrightness" + currentBrightness);
        return currentBrightness;
    }

    /// <summary>
    /// 设置亮度
    /// </summary>
    public void setBrightness(int brightness)
    {
        currentDevice.setBrightness(brightness);
    }
    public void SetBrightness(int brightness)
    {
        currentDevice.SetBrightness(brightness);
    }

    /// <summary>
    /// 操作falcon系统接口
    /// </summary>
    public  bool setDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid, string value)
    {
        return currentDevice.setDevicePropForUser( deviceid, value);
    }
    public bool SetDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid, string value)
    {
        return currentDevice.SetDevicePropForUser(deviceid, value);
    }


    /// <summary>
    /// 操作falcon系统接口
    /// </summary>
    public string getDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid)
    {
        return currentDevice.getDevicePropForUser(deviceid);
    }
    public string GetDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid)
    {
        return currentDevice.GetDevicePropForUser(deviceid);
    }
    /// <summary>
    /// 升高音量
    /// </summary>
    public void volumeUp()
    {
        currentDevice.volumeUp();
    }
    public void VolumeUp()
    {
        currentDevice.VolumeUp();
    }

    /// <summary>
    /// 降低音量
    /// </summary>
    public void volumeDown()
    {
        currentDevice.volumeDown();
    }
    public void VolumeDown()
    {
        currentDevice.VolumeDown();
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    public void setVolumeNum(int volume)
    {
        currentDevice.setVolumeNum(volume);
    }
    public void SetVolumeNum(int volume)
    {
        currentDevice.SetVolumeNum(volume);
    }
    /// <summary>
    /// 得到Box的数据
    /// </summary>
    /// <returns></returns>
    public Quaternion getBoxQuaternion()
    {
        if (useFalconBoxSensor)
        {
            return boxQuaternion;
        }
        else
            return Quaternion.identity;
    }
    public Quaternion GetBoxQuaternion()
    {
        if (useFalconBoxSensor)
        {
            return boxQuaternion;
        }
        else
            return Quaternion.identity;
    }
    public Quaternion GetControllerQuaternion()
    {
        if (useFalconBoxSensor)
        {
            return boxQuaternion;
        }
        else
            return Quaternion.identity;
    }
    public void resetFalconBoxSensor()
    {
#if ANDROID_DEVICE
        if (useFalconBoxSensor)
        {
            SDK.currentDevice.resetFalconBoxSensor();
        }
        else
            return;
#endif
    }
    public void ResetFalconBoxSensor()
    {
#if ANDROID_DEVICE
        if (useFalconBoxSensor)
        {
            SDK.currentDevice.ResetFalconBoxSensor();
        }
        else
            return;
#endif
    }
    public void ResetControllerSensor()
    {
#if ANDROID_DEVICE
        if (useFalconBoxSensor)
        {
            SDK.currentDevice.ResetFalconBoxSensor();
        }
        else
            return;
#endif
    }
    #endregion

    #region IOS Special
    void CheckStereoRender()
    {
        GameObject stereo = this.transform.FindChild("StereoRender").gameObject;
        if (stereo != null)
            stereo.SetActive( true );
    }

    void AddPrePostRenderStages()
    {
        var preRender = UnityEngine.Object.FindObjectOfType<PicoVRPreRender>();
        if (preRender == null)
        {
            var go = new GameObject("PreRender", typeof(PicoVRPreRender));
            go.SendMessage("Reset");
            go.transform.parent = transform;
        }

        var postRender = UnityEngine.Object.FindObjectOfType<PicoVRPostRender>();
        if (postRender == null)
        {
            var go = new GameObject("PostRender", typeof(PicoVRPostRender));
            go.SendMessage("Reset");
            go.transform.parent = transform;
        }
    }
    #endregion
    /*************************************  Unity API ****************************************/
    #region Application EVENT

    void Awake()
    {
#if ANDROID_DEVICE || IOS_DEVICE
        Application.targetFrameRate = 60;
#endif
        if (sdk == null)
        {
            sdk = this;
        }
        if (sdk != this)
        {
            Debug.LogWarning("SDK object should be a singleton.");
            enabled = false;
            return;
        }
        string FPSname = "PicoVR/Head/FPS";
        GameObject FPS = GameObject.Find(FPSname);
        FPS.SetActive(showFPS);
        InitDevice();
        picoVRProfile = PicoVRConfigProfile.GetPicoProfile(DeviceType);
        inPicovr = false;
        newInPicovr = true;
        eyeFov = 90.0f;
        boxQuaternion = Quaternion.identity;
        headPose = new PicoVRPose(Matrix4x4.identity);
        //CheckStereoRender();
#if !UNITY_EDITOR
        AddPrePostRenderStages();
#endif
		leftHandPose = new PicoVRPose(Matrix4x4.identity);
        rightHandPose= new PicoVRPose(Matrix4x4.identity);

    }

    void Start()
    {
        if (currentDevice == null)
        {
            Application.Quit();
            Debug.Log("start  Device == null ");
        }
        currentDevice.Init();
        currentDevice.UpdateScreenData();
       
    }

    void Update()
    {
#if UNITY_EDITOR || WIN_DEVICE
        SimulateInput();
#else
       if (Input.touchCount == 1)//一个手指触摸屏幕
        {
            if (Input.touches[0].phase == TouchPhase.Began)//开始触屏
            {
                newPicovrTriggered = true;
            }
        }
        else 
        if(Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            newPicovrTriggered = true;

        } 

#endif

        upDateState = UpdateState();
        if (inPicovr != newInPicovr)
        {
            currentDevice.UpdateScreenData();
        }
        inPicovr = newInPicovr;
        picovrTriggered = newPicovrTriggered;
        newPicovrTriggered = false;
        upDated = false;
       
    }

    void OnDestroy()
    {
        if (sdk == this)
        {
            sdk = null;
        } 		
        RenderTexture.active = null;
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

    }

    public void OnApplicationQuit()
    {
        //currentDevice.stopHidService();
        StopHeadTrack();
        currentDevice.StopBLECentral ();
        //currentDevice.requestHidSensor(0);

#if ANDROID_DEVICE
/*
		try{
			Debug.Log("OnApplicationQuit  1  -------------------------");
			Pvr_UnitySDKPluginEvent.Issue( RenderEventType.ShutdownRenderThread );
		}
		catch (Exception e)
        {
            Debug.Log("ShutdownRenderThread Error" + e.Message);
        }
*/
#elif WIN_DEVICE
        currentDevice.Destroy();
#endif
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnPause()
    {
        //currentDevice.stopHidService();
        //currentDevice.StopHeadTrack();
        //currentDevice.StopControllerTrack();
        //currentDevice.requestHidSensor (0); 
        StopHeadTrack();
        //if (PicoVRManager.SDK.currentDevice.Async)
        {
            LeaveVRMode();
        }
    }       

    private void OnApplicationPause(bool pause)
    {
        Debug.Log("OnApplicationPause-------------------------" + (pause ? "true" : "false"));
#if ANDROID_DEVICE
        if (pause)
        {
            OnPause();
        }
        else
        {
            onResume = true;
            //currentDevice.StartHeadTrack(); 
                GL.InvalidateState();
                StartCoroutine(OnResume()); 
        }
#endif
#if IOS_DEVICE
		if (pause)
		{
			currentDevice.StopHeadTrack();
		}
		else
		{
			currentDevice.StartHeadTrack();  
		}
#endif
    }

    private IEnumerator OnResume()
    { 
		//currentDevice.requestHidSensor (1);  
		//currentDevice.OpenHMDSensor();
        StartHeadTrack();
        StartControllerTrack();
        //currentDevice.startHidService();
        for (int i = 0; i < 20; i++)
        {
            yield return null;
        }
        //if (PicoVRManager.SDK.currentDevice.Async)
        {
            EnterVRMode();
        }
    }

    void OnApplicationFocus(bool focus)
    {
        Debug.Log("OnApplicationFocus-------------------------" + (focus ? "true" : "false"));
        if (currentDevice != null)
        {
            currentDevice.IsFocus(focus);
        }
		
    }

    public static void EnterVRMode()
    {
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.Resume);
    }

    public static void LeaveVRMode()
    {
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.Pause);
    }

#endregion
    
    public void ChangeHeadwear(int headwear)
    {
        currentDevice.ChangeHeadwear(headwear);
    }
    public void PlayEffect(int effectID, int whichHaptic)
    {
        currentDevice.PlayEffect(effectID, whichHaptic);
    }
    public void PlayEffectSequence(string sequence, int whichHaptic)
    {
        currentDevice.PlayEffectSequence(sequence, whichHaptic);
    }
    public void SetAudioHapticEnabled(bool enable, int whichHaptic)
    {
        currentDevice.SetAudioHapticEnabled(enable, whichHaptic);
    }
    public Vector3 GetBoxSensorAcc()
    {
        return currentDevice.GetBoxSensorAcc();
    }
    public Vector3 GetBoxSensorGyr()
    {
        return currentDevice.GetBoxSensorGyr();
    }
    public void StopPlayingEffect(int whichHaptic)
    {
        currentDevice.StopPlayingEffect(whichHaptic);
    }
    public void EnableTouchPad(bool enable)
    {
        currentDevice.EnableTouchPad(enable);
    }
    public void SwitchTouchType(int device)
    {
        currentDevice.SwitchTouchType(device);
    }
    public int GetTouchPadStatus()
    {
        return currentDevice.GetTouchPadStatus();
    }
    public bool SetDeviceProp(int device_id, string value)
    {
        return currentDevice.SetDeviceProp(device_id, value);
    }
    public string GetDeviceProp(int device_id)
    {
        return currentDevice.GetDeviceProp(device_id);
    }
    public bool RequestHidSensor(int user)
    {
        return currentDevice.RequestHidSensor(user);
    }
    public int GetHidSensorUser()
    {
        return currentDevice.GetHidSensorUser();
    }
    public bool SetThreadRunCore(int pid, int core_id)
    {
        return currentDevice.SetThreadRunCore(pid, core_id);
    }
    public int GetThreadRunCore(int pid)
    {
        return currentDevice.GetThreadRunCore(pid);
    }
    public bool SetSystemRunLevel(int device_id,int level)
    {
        return currentDevice.SetSystemRunLevel(device_id, level);
    }
    public int GetSystemRunLevel(int device_id)
    {
        return currentDevice.GetSystemRunLevel(device_id);
    }
    public bool InitBatteryVolClass()
    {
        return currentDevice.InitBatteryVolClass();
    }
    public bool StartAudioReceiver()
    {
        return currentDevice.StartAudioReceiver();
    }
    public bool StartBatteryReceiver()
    {
        return currentDevice.StartBatteryReceiver();
    }
    public bool StopAudioReceiver()
    {
        return currentDevice.StopAudioReceiver();
    }
    public bool StopBatteryReceiver()
    {
        return currentDevice.StopBatteryReceiver();
    }
    public void SetSurroundroomType(int type)
    {
        currentDevice.SetSurroundroomType(type);
    }
    public void OpenRoomcharacteristics()
    {
        currentDevice.OpenRoomcharacteristics();
    }
    public void CloseRoomcharacteristics()
    {
        currentDevice.CloseRoomcharacteristics();
    }
    public void EnableSurround()
    {
        currentDevice.EnableSurround();
    }
    public void EnableReverb()
    {
        currentDevice.EnableReverb();
    }
    public void StartAudioEffect(String audioFile, bool isSdcard)
    {
        currentDevice.StartAudioEffect(audioFile,isSdcard);
    }
    public void StopAudioEffect()
    {
        currentDevice.StopAudioEffect();
    }
    public void ReleaseAudioEffect()
    {
        currentDevice.ReleaseAudioEffect();
    }
    public bool ConnectBLEDevice(string mac )
    {
        return currentDevice.ConnectBLEDevice(mac);
    }
    public string GetDeviceModelName()
    {
        return currentDevice.GetDeviceModelName();
    }
    public bool IsBluetoothOpened()
    {
        return currentDevice.IsBluetoothOpened();
    }
    public int GetBluetoothState()
    {
        return currentDevice.GetBluetoothState();
    }
    public int GetBLEConnectState()
    {
        return currentDevice.GetBLEConnectState();
    }
    public string GetBLEVersion()
    {
        return currentDevice.GetBLEVersion();
    }
	public void devicePowerStateChanged (string state){
		currentDevice.DevicePowerStateChanged (state);
	}
    public void DevicePowerStateChanged(string state)
    {
        currentDevice.DevicePowerStateChanged(state);
    }

	public void deviceConnectedStateChanged (string state){
		currentDevice.DeviceConnectedStateChanged (state);
	}
    public void DeviceConnectedStateChanged(string state)
    {
        currentDevice.DeviceConnectedStateChanged(state);
    }
	public void deviceFindNotification (string msg){
		currentDevice.DeviceFindNotification (msg);
	}
    public void DeviceFindNotification(string msg)
    {
        currentDevice.DeviceFindNotification(msg);
    }
	public void acceptDeviceKeycode (string keykode){
		currentDevice.AcceptDeviceKeycode (keykode);
	}
    public void AcceptDeviceKeycode(string keykode)
    {
        currentDevice.AcceptDeviceKeycode(keykode);
    }
    public string GetSDKVersion()
    {
		return currentDevice.GetSDKVersion ();
    }
    public int GetPsensorState()
    {
        return currentDevice.GetPsensorState();
    }
    public string GetDeviceSN()
    {
        return currentDevice.GetDeviceSN();
    }
   
}
