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
using System;
using System.Runtime.InteropServices;

#if ANDROID_DEVICE



public sealed class PicoVRAndroidDevice : PicoVRBaseDevice
{
#region Properties
    private bool canConnecttoActivity = false;
    public override bool CanConnecttoActivity
    {
        get { return canConnecttoActivity; }
        set
        {
            if (value != canConnecttoActivity)
                canConnecttoActivity = value;
        }
    }
    public UnityEngine.AndroidJavaObject activity;
    private static UnityEngine.AndroidJavaClass javaVrActivityClass;
    private static UnityEngine.AndroidJavaClass javaSysActivityClass; 
    private static UnityEngine.AndroidJavaClass javaAm3dActivityClass; 
    private static UnityEngine.AndroidJavaClass javaLinkActivityClass; 

    private static UnityEngine.AndroidJavaClass batteryjavaVrActivityClass;
    private AndroidJavaObject vrActivityObj;
    private static UnityEngine.AndroidJavaClass volumejavaVrActivityClass;

    private int Headweartype = (int)PicoVRConfigProfile.DeviceTypes.PicoNeo;
    private bool PupillaryPoint = false;
    private Quaternion rot;
    private Vector3 pos;
    private bool usePhoneSensor = true;
    private int inittime = 0;
    private bool useHMD = false;
    private bool isFalcon = false;
    private bool isFalconCV = false;
    private static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));
    private const long NO_DOWNTIME = -1;
    private float w = 0, x = 0, y = 0, z = 0, fov = 90f;
    private int timewarpid = 0;
    private bool isInitrenderThread = false;
    private string UnityVersion = "0.7.6.4";
    private static string model;
    private float px = 0, py = 0, pz = 0;
#endregion Properties

    public PicoVRAndroidDevice()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Application.runInBackground = false;
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        rot = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
        pos = rot * new Vector3(0.0f, 0.0f, 0.0f);
        currEyeTextureIdx = 0;
        nextEyeTextureIdx = 1;
        inittime = 0;


        if (!canConnecttoActivity)
        {
            ConnectToAndriod();
        }
        for (int i = 0; i < eyeTextureCount; i++)
        {
            if (null == eyeTextures[i])
            {
                try
                {
                    ConfigureEyeTexture(i);
                }
                catch (Exception)
                {
                    Debug.LogError("ERROR");
                    throw;
                }
            }
        }

    }

    /// <summary>
    /// 连接安卓
    /// </summary>
    public void ConnectToAndriod()
    {
        try
        {
            Debug.Log("SDK Version = " + GetSDKVersion() + ",UnityVersion=" + UnityVersion);
            UnityEngine.AndroidJavaClass unityPlayer = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
            javaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.VrActivity");
            javaAm3dActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.am3d.Am3d");
            javaSysActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.SysActivity");
            javaLinkActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.link.VrLinkActivity");

            Pvr_UnitySDKAPI.System.Pvr_SetInitActivity(activity.GetRawObject(), javaVrActivityClass.GetRawClass());
            model = javaVrActivityClass.CallStatic<string>("Pvr_GetBuildModel");
            if (model == "Pico Neo DK")
            {
                model = "Falcon";
            }
            if (useHMD)
            {
                usePhoneSensor = false;
            }
            if (model == "Falcon")
            {
                usePhoneSensor = false;
                useHMD = true;
                isFalcon = true;
                Headweartype = (int)PicoVRConfigProfile.DeviceTypes.PicoNeo;
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "initFalconDevice", activity);
            }
            else if (model == "Falcon_CV")
            {
                usePhoneSensor = false;
                useHMD = true;
                isFalconCV = true;
                PicoVRManager.SDK.PVRNeck = !(isFalcon && PicoVRManager.SDK.IsFalconCV6DOFEnable);
                 Headweartype = (int)PicoVRConfigProfile.DeviceTypes.PicoNeo;
                 Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "initFalconDevice", activity);
                if (PicoVRManager.SDK.IsFalconCV6DOFEnable)
                {
                    InitUnitySDK6DofSensor();
                }
            }
            else
            {
                int deviceType = 0;
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref deviceType, javaLinkActivityClass, "Pvr_ReadDeviceTypeFromWing", activity);
                if (deviceType != 0)
                {
                    Headweartype = deviceType;
                }
            } 
            double[] parameters = new double[5];
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref parameters, javaVrActivityClass, "getDPIParameters", activity);
           // Pvr_UnitySDKAPI.Render.UPvr_ChangeScreenParameters(model, (int)parameters[0], (int)parameters[1], parameters[2], parameters[3], parameters[4]);
           // Pvr_UnitySDKAPI.Render.UPvr_ChangeHeadwear(Headweartype);
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("ConnectToAndriod------------------------catch" + e.Message);
        }

    }

    public bool InitUnitySDK6DofSensor()
    {
        bool enable = false;
        try
        {
            int ability6dof = 0;
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.ABILITY6DOF;
            Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref ability6dof);
            if (ability6dof == 1)
            {

                if (Pvr_UnitySDKAPI.Sensor.UPvr_Enable6DofModule(Pvr_UnitySDKManager.SDK.Enable6Dof) == 0)
                {
                    enable = true;
                    Pvr_UnitySDKManager.PVRNeck = false;
                }
            }
            else
            {
                Debug.LogWarning("This platform does NOT support 6 Dof ! ");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("InitUnity6DofSDKSensor ERROR! " + e.Message);
            throw;
        }
        return enable;
    }

    /// <summary>
    /// 调用静态方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="jclass"></param>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
  
    public override void startHidService()
    {
         Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "startPeriodService", activity);
    }

    public override void stopHidService()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "stopPeriodService", activity);
    }

    //shine add
    public void startLarkConnectService()
    {
        Debug.LogError("shine startLarkConnectService ");
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaLinkActivityClass, "Pvr_StartLarkConnectService", activity, PicoVRManager.SDK.gameObject.name);
    }

    public void stopLarkConnectService()
    {
        Debug.LogError("shine stopLarkConnectService ");
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaLinkActivityClass, "stopLarkConnectService", activity);
    }
    //shine add end

    /// <summary>
    /// 
    /// </summary>
    public void UpdateFrameParamsFromActivity()
    {
        float[] frameInfo = null;
        frameInfo = UpdateRenderParams(1.0f, 1000.0f);
        if (frameInfo == null)
        {
            return;
        }
        int j = 0;
        for (int i = 0; i < 16; ++i, ++j)
        {
            PicoVRManager.SDK.headView[i] = frameInfo[j];
        }
        PicoVRManager.SDK.headView = flipZ * PicoVRManager.SDK.headView.inverse * flipZ;

    }
    public void UpdateBOXFrameParamsFromActivity()
    {

        float w = 0, x = 0, y = 0, z = 0, px = 0, py = 0, pz = 0;
        try
        {
            int returns = Pvr_UnitySDKAPI.Sensor.UPvr_GetSensorState(1, ref x, ref y, ref z,ref w, ref px, ref py, ref pz);
            if (returns == 0)
            {
                 PicoVRManager.SDK.boxQuaternion = new Quaternion(-x, -y, z, w);

            }
            if (returns == -1)
                Debug.Log("sesnor update --- GetUnitySDKSensorState     -1    ");
        }
        catch (System.Exception e)
        {
            Debug.LogError("GetUnitySDKSensorState ERROR! " + e.Message);
            throw;
        }


    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>

    public static float[] UpdateRenderParams(float zNear, float zFar)
    {
        float ratation = 90.0f;
        if ("Falcon".Equals(model))
        {
            ratation = 360.0f;
        }
        float[] frameInfo = new float[16];
               //  shangaili  
        //  Native_UpdateRenderParams(frameInfo, ratation, zNear, zFar);
        return frameInfo;
    }
    /// <summary>
    /// 
    /// </summary>
    public override void UpdateTextures()
    {
        if (PicoVRManager.SDK.onResume && inittime < 2401 && !Async)
        {
            for (int i = 0; i < 6; i++)
            {
                if (null == eyeTextures[i])
                {
                    try
                    {
                        ConfigureEyeTexture(i);
                    }
                    catch (Exception)
                    {
                        Debug.LogError("ERROR");
                        throw;
                    }
                }
                if (!eyeTextures[i].IsCreated())
                {
                    eyeTextures[i].Create();
                    eyeTextureIds[i] = eyeTextures[i].GetNativeTexturePtr().ToInt32();
                }
                eyeTextureIds[i] = eyeTextures[i].GetNativeTexturePtr().ToInt32();
            }
            inittime++;
            if (inittime == 2401)
            {
                PicoVRManager.SDK.onResume = false;
                inittime = 1;
            }
        }
        for (int i = 0; i < 6; i++)
        {
            if (null == eyeTextures[i])
            {
                try
                {
                    ConfigureEyeTexture(i);
                }
                catch (Exception)
                {
                    Debug.LogError("ERROR");
                    throw;
                }
            }
            if (!eyeTextures[i].IsCreated())
            {
                eyeTextures[i].Create();
                eyeTextureIds[i] = eyeTextures[i].GetNativeTexturePtr().ToInt32();
            }
        }
        currEyeTextureIdx = nextEyeTextureIdx;
        nextEyeTextureIdx = (nextEyeTextureIdx + 1) % 3;
    }

  

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eyeTextureIndex"></param>
    private void ConfigureEyeTexture(int eyeTextureIndex)
    {
        Vector2 renderTexSize = GetStereoScreenSize();
        int x = (int)renderTexSize.x;
        int y = (int)renderTexSize.y;
        eyeTextures[eyeTextureIndex] = new RenderTexture(x, y, (int)PicoVRManager.SDK.RtBitDepth, PicoVRManager.SDK.RtFormat);
        eyeTextures[eyeTextureIndex].anisoLevel = 0;
        eyeTextures[eyeTextureIndex].antiAliasing = Mathf.Max(QualitySettings.antiAliasing, (int)PicoVRManager.SDK.RtAntiAlising);

        eyeTextures[eyeTextureIndex].Create();
        if (eyeTextures[eyeTextureIndex].IsCreated())
        {
            eyeTextureIds[eyeTextureIndex] = eyeTextures[eyeTextureIndex].GetNativeTexturePtr().ToInt32();
        }

    }

#region Inheritance
    public override void InitForEye(ref Material mat) { mat = null; }

    public override float GetSeparation()
    {
        float separation = 0.0625f;
        int enumindex = (int)Pvr_UnitySDKAPI.GlobalFloatConfigs.IPD;
        if (0 != Pvr_UnitySDKAPI.Render.UPvr_GetFloatConfig(enumindex, ref separation))
        {
            Debug.Log("Cannot get ipd");
            separation = 0.0625f;
        }
        return separation;
    }

    public override void Init()
    {
        if (!isInitrenderThread && PicoVRManager.SDK.VRModeEnabled)
        {

            Pvr_UnitySDKPluginEvent.Issue(RenderEventType.InitRenderThread);
            isInitrenderThread = true;
        }
         GetFOV(ref PicoVRManager.SDK.picoVRProfile.device.devMaxFov.outer);
        PicoVRManager.SDK.picoVRProfile.device.devLenses.separation =GetSeparation() ;

    }

    public override void SetVRModeEnabled(bool enabled) { }

    public override void SetDistortionCorrectionEnabled(bool enabled) { }

    public override Vector2 GetStereoScreenSize()
    {
        Vector2 RendentextureWH;
        int w = 1024;
        int h = 1024;
        try
        {
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.EYE_TEXTURE_RESOLUTION0;
            Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref w);
            enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.EYE_TEXTURE_RESOLUTION1;
            Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref h);
        }
        catch (System.Exception e)
        {
            Debug.LogError("GetRendentextureSize ERROR! " + e.Message);
            throw;
        }
        RendentextureWH = new Vector2(w, h);
        return RendentextureWH;
    }

    public override void SetStereoScreen(RenderTexture stereoScreen) { }

    public override void resetFalconBoxSensor()
    {
        //Native_ResetBoxTrack();
    }
    public override void ResetFalconBoxSensor()
    {
        //Native_ResetBoxTrack();
    }

    public override void SetAutoDriftCorrectionEnabled(bool enabled) { }

    public override void UpdateState()
    {       
        if (inittime == 0)
        {
            inittime++;
            StartHeadTrack();
            if (isFalcon || isFalconCV)
            {
                StartHeadTrack();
                StartControllerTrack();
            }
            else if (usePhoneSensor)
            {
               StartHeadTrack();
            }
        }

        if (PicoVRManager.SDK.PVRNeck)
        {
            pos = (rot * new Vector3(0f, PicoVRManager.NECK_Y, PicoVRManager.NECK_Z) - PicoVRManager.NECK_Y * Vector3.up) * 1.0f;
        }
        else
        {
            pos = rot * new Vector3(0.0f, 0.0f, 0.0f);
        }


        if (useHMD && !usePhoneSensor)
        {
            if (PicoVRManager.SDK.IsFalconCV6DOFEnable && isFalconCV)
            {
                GetRotPos(false, ref w, ref x, ref y, ref z, ref fov, ref timewarpid,
                    ref px, ref py, ref pz);
                PicoVRManager.SDK.timewarpID = timewarpid;
                PicoVRManager.SDK.eyeFov = fov;
                rot = new Quaternion(-x, -y, z, w);
                pos = new Vector3(-px, -py, pz);
            }
            else
            {
                GetSensorState(
                    false,
                    ref w,
                    ref x,
                    ref y,
                    ref z,
                    ref fov,
                    ref timewarpid);
                PicoVRManager.SDK.timewarpID = timewarpid;
                PicoVRManager.SDK.eyeFov = fov;
                rot = new Quaternion(-x, -y, z, w);
                pos = rot * new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        if (!useHMD && usePhoneSensor)
        {
            GetSensorState(
                  false,
                  ref w,
                  ref x,
                  ref y,
                  ref z,
                  ref fov,
                  ref timewarpid);
            PicoVRManager.SDK.timewarpID = timewarpid;
            PicoVRManager.SDK.eyeFov = fov;
            rot = new Quaternion(-x, -y, z, w);
            pos = rot * new Vector3(0.0f, 0.0f, 0.0f);
            PicoVRManager.SDK.eyeFov = fov;
        }
        if (isFalcon && PicoVRManager.SDK.UseFalconBoxSensor)
        {
            UpdateBOXFrameParamsFromActivity();
        }
            
        PicoVRManager.SDK.headPose.Set(pos, rot);

        
    }

    public override void UpdateScreenData()
    {
        ComputeEyesFromProfile();
    }

    public override void Destroy()
    {
        try
        {
            base.Destroy();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    public override void ResetControllerTrack()
    {
        if (isFalcon && PicoVRManager.SDK.UseFalconBoxSensor)
        {
            // shangaili
            //Native_ResetBoxTrack();
        }
    }
    public override void ResetHeadTrack()
    {
        
        if (PicoVRManager.SDK.reStartHead)
        {
            PicoVRManager.SDK.reStartHead = false;
            Pvr_UnitySDKAPI.Sensor.UPvr_ResetSensor((int)Pvr_UnitySDKAPI.Sensorindex.Default);
        }
    }

    public override void CloseHMDSensor()
    {
        // shangaili
        //PVR_CloseSensor();
    }

    public override void OpenHMDSensor()
    {
        // shangaili
        //PVR_OpenSensor();
    }

    public override void IsFocus(bool state)
    {
        // shangaili
        //PVR_SetFocus(state);
        //return true;
    }

    public override void StartControllerTrack()
    {
        if (isFalcon && PicoVRManager.SDK.UseFalconBoxSensor)
        {
            Native_StartHeadTrack();
        }
    }
    bool initSensor = false;
    bool startSensor = false;
    bool SensorCanUse = false;
    public override void StartHeadTrack()
    {
        try
        {
            if (!initSensor)
            {
                if (Pvr_UnitySDKAPI.Sensor.UPvr_Init((int)Pvr_UnitySDKAPI.Sensorindex.Default) == 0)
                {
                    Debug.Log(" InitUnitySDKSensor Sucess! ");
                    initSensor = true;
                }
            }
            if (initSensor && !startSensor)
            {
                if (Pvr_UnitySDKAPI.Sensor.UPvr_StartSensor((int)Pvr_UnitySDKAPI.Sensorindex.Default) == 0)
                {
                    startSensor = true;
                    Debug.Log(" startUnitySDKSensor Sucess! ");
                }
            }
           
        }
        catch (System.Exception e)
        {
            Debug.LogError("InitUnitySDKSensor ERROR! " + e.Message);
            throw;
        }
    }


  

    public bool Native_StartHeadTrack()
    {
        if (isFalcon && PicoVRManager.SDK.UseFalconBoxSensor)
        {
            SensorCanUse = UnitySDKSensorCount();
            if (SensorCanUse && PicoVRManager.SDK.UseFalconBoxSensor)
            {
                try
                {
                    if (Pvr_UnitySDKAPI.Sensor.UPvr_StartSensor(1) == 0)
                        return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("StartUnitySDKSensor ERROR! " + e.Message);
                    throw;
                }
                return false;

            }
            else
                Debug.LogError("There is not extra Sensor!");
          
        }
        else
        {


            try
            {
                if (Pvr_UnitySDKAPI.Sensor.UPvr_StartSensor(1) == 0)
                    return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("StartUnitySDKSensor ERROR! " + e.Message);
                throw;
            }
            return false;
        }
  return false;

    }

    int SensorCount = 1; 
    public bool UnitySDKSensorCount()
    {
        bool enable = false;
        try
        {
            int index = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.SEENSOR_COUNT;
            int temp = Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(index, ref SensorCount);
            Debug.Log("Sesnor Count = " + SensorCount.ToString());
            if (temp == 0)
                if (SensorCount > 1)
                {
                    enable = true;
                }

        }
        catch (System.Exception e)
        {
            Debug.LogError("UnitySDKSensorCount Get ERROR! " + e.Message);
            throw;
        }
        return enable;
    }


    public override void StopControllerTrack()
    {
        if (isFalcon && PicoVRManager.SDK.UseFalconBoxSensor)
        {
            // shangaili
            // Native_StopHeadTrack();
            // shangaili
            // Native_ResetBoxTrack();
        }
    }
    public override void StopHeadTrack()
    {

        if (initSensor && startSensor)
        {
            if (Pvr_UnitySDKAPI.Sensor.UPvr_StopSensor((int)Pvr_UnitySDKAPI.Sensorindex.Default) == 0)
                startSensor = false;
        }

    }

    public override void ChangeHeadwear(int headwear)
    {
        // shangaili
        // PVR_ChangeHeadwear(headwear);
    }

    public override Vector3 GetBoxSensorAcc()
    {
        Vector3 boxSensorDataAcc = new Vector3(0.0f, 0.0f, 0.0f);

        if (model == "Falcon")
        {
            float[] temp = new float[3];
            //temp = headTrack.Call<float[]> ("getBoxSensorDataAcc");//jar
            // shangaili
            //Native_GetBoxSensorAcc(temp);//so
            boxSensorDataAcc.x = temp[0];
            boxSensorDataAcc.y = temp[1];
            boxSensorDataAcc.z = temp[2];
        }
        else
        {
            Debug.LogError("GetBoxSensorAcc: Device model is " + model + ", not Falcon!");
        }

        return boxSensorDataAcc;
    }

    public override Vector3 GetBoxSensorGyr()
    {
        Vector3 boxSensorDataGyr = new Vector3(0.0f, 0.0f, 0.0f);

        if (model == "Falcon")
        {
            float[] temp = new float[3];
            //temp = headTrack.Call<float[]> ("getBoxSensorDataGyr");//jar
            // shangaili
            //Native_GetBoxSensorGyro(temp);//so
            boxSensorDataGyr.x = temp[0];
            boxSensorDataGyr.y = temp[1];
            boxSensorDataGyr.z = temp[2];
        }
        else
        {
            Debug.LogError("GetBoxSensorGyr: Device model is " + model + ", not Falcon!");
        }

        return boxSensorDataGyr;
    }


    /*****************************音量亮度*************************************/
    public override bool initBatteryVolClass()
    {
        try
        {
            if (javaSysActivityClass != null && activity != null)
            {
                batteryjavaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.BatteryReceiver");
                volumejavaVrActivityClass = new AndroidJavaClass("com.psmart.aosoperation.AudioReceiver");

                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_InitAudioDevice", activity); 
                return true;
            }
            else
                return false;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool InitBatteryVolClass()
    {
        try
        {
            if (javaSysActivityClass != null && activity != null)
            {
                batteryjavaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.BatteryReceiver");
                volumejavaVrActivityClass = new AndroidJavaClass("com.psmart.aosoperation.AudioReceiver");

                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_InitAudioDevice", activity);                 
                return true;
            }
            else
                return false;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool startAudioReceiver()
    {
        try
        {
            string startreceivre = PicoVRManager.SDK.gameObject.name;
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(volumejavaVrActivityClass, "Pvr_StartReceiver", activity, startreceivre);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }

    public override bool StartAudioReceiver()
    {
        try
        {
            string startreceivre = PicoVRManager.SDK.gameObject.name;
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(volumejavaVrActivityClass, "Pvr_StartReceiver", activity, startreceivre);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }

    public override bool startBatteryReceiver()
    {
        try
        {
            string startreceivre = PicoVRManager.SDK.gameObject.name;
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "Pvr_StartReceiver", activity, startreceivre);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool StartBatteryReceiver()
    {
        try
        {
            string startreceivre = PicoVRManager.SDK.gameObject.name;
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "Pvr_StartReceiver", activity, startreceivre);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }

    public override bool stopAudioReceiver()
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(volumejavaVrActivityClass, "Pvr_StopReceiver", activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }

    }
    public override bool StopAudioReceiver()
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(volumejavaVrActivityClass, "Pvr_StopReceiver", activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }

    }

    public override bool stopBatteryReceiver()
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "Pvr_StopReceiver", activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool StopBatteryReceiver()
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "Pvr_StopReceiver", activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool setDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid, string number)
    {
        if(deviceid == (PicoVRConfigProfile.DeviceCommand)(12) )
       {
          return Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetHmdScreenBrightness(Convert.ToInt32(number));
       }
       else if(deviceid == (PicoVRConfigProfile.DeviceCommand)(13))
       {
          return false;
       }
       else if(deviceid == (PicoVRConfigProfile.DeviceCommand)(14))
       {
          return false;
       }
       else  
       {
         return false;
       }
    }
    public override bool SetDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid, string number)
    {
        if(deviceid == (PicoVRConfigProfile.DeviceCommand)(12) )
       {
          return Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetHmdScreenBrightness(Convert.ToInt32(number));
       }
       else if(deviceid == (PicoVRConfigProfile.DeviceCommand)(13))
       {
          return false;
       }
       else if(deviceid == (PicoVRConfigProfile.DeviceCommand)(14))
       {
          return false;
       }
       else  
       {
         return false;
       }

    }
    public override string getDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid)
    {
        if(deviceid == (PicoVRConfigProfile.DeviceCommand)(12) )
       {
           int brightness = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetHmdScreenBrightness();
           
           return brightness.ToString();
       }
       else if(deviceid == (PicoVRConfigProfile.DeviceCommand)(13))
       {
          return "0";
       }
       else if(deviceid == (PicoVRConfigProfile.DeviceCommand)(14))
       {
          return "0";
       } 
       else  
       {
         return "0";
       }
      
    }
    public override string GetDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid)
    {
        if(deviceid == (PicoVRConfigProfile.DeviceCommand)(12) )
       {
           int brightness = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetHmdScreenBrightness();
          
           return  brightness.ToString();
       }
       else if(deviceid == (PicoVRConfigProfile.DeviceCommand)(13))
       {
          return "0";
       }
       else if(deviceid == (PicoVRConfigProfile.DeviceCommand)(14))
       {
          return "0";
       }  
       else  
       {
         return "0";
       }
    
    }
    public override string getDeviceSN()
    {
        string serialNum = "UNKONWN";
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref serialNum, javaSysActivityClass, "getDeviceSN");
        return serialNum;
    }
    public override string GetDeviceSN()
    {
        string serialNum = "UNKONWN";
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref serialNum, javaSysActivityClass, "getDeviceSN");
        return serialNum;
    }
    public override string getDeviceModelName()
    {
        if (null != model)
        {
            return model;
        }
        else
        {
            return "unknow";
        }
    }
    public override string GetDeviceModelName()
    {
        if (null != model)
        {
            return model;
        }
        else
        {
            return "unknow";
        }
    }
    public override int getMaxVolumeNumber()
    {
        int maxvolm = 0;
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref maxvolm, javaSysActivityClass, "Pvr_GetMaxAudionumber");
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return maxvolm;
    }

    public override int GetMaxVolumeNumber()
    {
        int maxvolm = 0;
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref maxvolm, javaSysActivityClass, "Pvr_GetMaxAudionumber");
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return maxvolm;
    }
    public override int getCurrentVolumeNumber()
    {
        int currentvolm = 0;
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref currentvolm, javaSysActivityClass, "Pvr_GetAudionumber");
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return currentvolm;
    }
    public override int GetCurrentVolumeNumber()
    {
        int currentvolm = 0;
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref currentvolm, javaSysActivityClass, "Pvr_GetAudionumber");
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return currentvolm;
    }

    public override bool volumeUp()
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_UpAudio");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }
    public override bool VolumeUp()
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_UpAudio");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }

    public override bool volumeDown()
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_DownAudio");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }
    public override bool VolumeDown()
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_DownAudio");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }

    public override bool setVolumeNum(int volume)
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_ChangeAudio", volume);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }
    public override bool SetVolumeNum(int volume)
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_ChangeAudio", volume);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }

    public override bool setBrightness(int brightness)
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_SetScreen_Brightness", brightness, activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }

    public override bool SetBrightness(int brightness)
    {
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_SetScreen_Brightness", brightness, activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }

    public override int getCurrentBrightness()
    {
        int currentlight = 0;
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref currentlight, javaSysActivityClass, "Pvr_GetScreen_Brightness", activity);
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return currentlight;
    }
    public override int GetCurrentBrightness()
    {
        int currentlight = 0;
        try
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref currentlight, javaSysActivityClass, "Pvr_GetScreen_Brightness", activity);
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return currentlight;
    }
    /*******************************音量亮度****************************************/

    /*********************************************************************************/
#endregion Inheritance

#region Unity interface
    /*************************** unity interface *************************************/
    public override string GetSDKVersion()
    {
        return Pvr_UnitySDKAPI.System.UPvr_GetSDKVersion();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="xppi"></param>
    /// <param name="yppi"></param>
    /// <param name="densityDpi"></param>
    public void ModifyScreenParameters(string model, int width, int height,
          double xppi, double yppi, double densityDpi)
    {
        // shangaili
        // PVR_ChangeScreenParameters(model, width, height, xppi, yppi, densityDpi);
    }

   
   
    /// <summary>
    /// 是否需开启Pupillary point
    /// </summary>
    private void SetPupillaryPoint(bool enable)
    {
        // shangaili
        // PVR_SetPupillaryPoint(enable);
    }

   

    private void GetSensorState(bool monoscopic, ref float w, ref float x, ref float y, ref float z, ref float fov, ref int timewarpid)
    {
        //PVR_GetSensorState(
        //        monoscopic,
        //        ref w,
        //        ref x,
        //        ref y,
        //        ref z,
        //        ref fov,
        //        ref timewarpid);
        float px = 0, py = 0, pz = 0;
        int returns = Pvr_UnitySDKAPI.Sensor.UPvr_GetMainSensorState(ref x, ref y, ref z, ref w, ref px, ref py, ref pz, ref fov, ref timewarpid);
    }

    private void GetRotPos(bool monoscopic, ref float rw, ref float rx, ref float ry, ref float rz, ref float fov,
        ref int timewarpid, ref float px, ref float py, ref float pz)
    {
        //PVR_GetRotPos(
        //     monoscopic,
        //    ref rw,
        //    ref rx,
        //    ref ry,
        //    ref rz,
        //    ref fov,
        //    ref timewarpid,
        //    ref px,
        //    ref py,
        //    ref pz);

        int returns = Pvr_UnitySDKAPI.Sensor.UPvr_GetMainSensorState(ref rx, ref ry, ref rz, ref rw, ref px, ref py, ref pz, ref fov, ref timewarpid);
    }

    private void GetFOV(ref float fov)
    {
        //PVR_GetFOV(ref fov);
        int enumindex = (int)Pvr_UnitySDKAPI.GlobalFloatConfigs.FOV;
        Pvr_UnitySDKAPI.Render.UPvr_GetFloatConfig(enumindex, ref fov);
    }
    public override int GetPsensorState()
    {
        int state = Pvr_UnitySDKAPI.Sensor.UPvr_GetPsensorState();
        return state;
    }
    private bool GetUsePredictedMatrix()
    {
        bool premat = false;
        // shangaili
        //premat = PVR_GetUsePredictedMatrix();
        return premat;
    }

    private float GetBatteryLevel()
    {
        float batterylevel = 100.0f;
        // shangaili
        // batterylevel = PVR_GetBatteryLevel();
        return batterylevel;
    }

    /*************************** unity interface *************************************/
#endregion unity interface

#region Haptics

    /***************************Haptics*************************************/
    public static int HAPTICS_LEFT = 0x01;
    public static int HAPTICS_RIGHT = 0x02;
    public static int HAPTICS_ALL = 0x03;
    public static int HAPTICS_HAPTICTHEME_SIP = 1;
    public static int HAPTICS_HAPTICTHEME_DIALPAD = 2;
    public static int HAPTICS_HAPTICTHEME_LAUNCHER = 3;
    public static int HAPTICS_HAPTICTHEME_LONGPRESS = 4;
    public static int HAPTICS_HAPTICTHEME_VIRTUALKEY = 5;
    public static int HAPTICS_HAPTICTHEME_ROTATE = 7;
    public static int HAPTICS_HAPTICTHEME_GALLERY = 8;
    public static int HAPTICS_HAPTICTHEME_LOCKSCREEN = 9;
    public static int HAPTICS_HAPTICTHEME_TRY_UNLOCK = 10;
    public static int HAPTICS_HAPTICTHEME_MULTITOUCH = 11;
    public static int HAPTICS_HAPTICTHEME_SCROLLING = 12;
    public static String DATA_HAPTICTHEME_VIRTUALKEY = "data_haptictheme_virtualkey";
    public static String DATA_HAPTICTHEME_LONGPRESS = "data_haptictheme_longpress";
    public static String DATA_HAPTICTHEME_LAUNCHER = "data_haptictheme_launcher";
    public static String DATA_HAPTICTHEME_DIALPAD = "data_haptictheme_dialpad";
    public static String DATA_HAPTICTHEME_SIP = "data_haptictheme_SIP";
    public static String DATA_HAPTICTHEME_ROTATE = "data_haptictheme_rotate";
    public static String DATA_HAPTICTHEME_GALLERY = "data_haptictheme_gallery";
    public static String DATA_HAPTICTHEME_SCROLL = "data_haptictheme_scroll";
    public static String DATA_HAPTICTHEME_MULTI_TOUCH = "data_haptictheme_multi_touch";
    public static String DATA_HAPTIC_VIBRATE = "haptic_vibrate_data";
    public static String DATA_HAPTIC_A2H = "haptic_A2H_data";

    public override void playeffect(int effectID, int whichHaptic)
    {
        // Log.e("berton", "========playeffectandroid================");
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playeffect", effectID, whichHaptic);
    }
    public override void PlayEffect(int effectID, int whichHaptic)
    {
        // Log.e("berton", "========playeffectandroid================");
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playeffect", effectID, whichHaptic);
    }

    public override void playEffectSequence(string sequence, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playEffectSequence", sequence, whichHaptic);
    }
    public override void PlayEffectSequence(string sequence, int whichHaptic)
    {
       Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playEffectSequence", sequence, whichHaptic);
    }

    public override void setAudioHapticEnabled(bool enable, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "setAudioHapticEnabled", enable, whichHaptic);
    }
    public override void SetAudioHapticEnabled(bool enable, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "setAudioHapticEnabled", enable, whichHaptic);
    }

    public override void stopPlayingEffect(int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "stopPlayingEffect", whichHaptic);
    }
    public override void StopPlayingEffect(int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "stopPlayingEffect", whichHaptic);
    }
    public override void playeffectforce(int effectID, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playeffectforce", effectID, whichHaptic);
    }
    public override void Playeffectforce(int effectID, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playeffectforce", effectID, whichHaptic);
    }
    public override void playTimedEffect(int effectDuration, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playTimedEffect", effectDuration, whichHaptic);
    }
    public override void PlayTimedEffect(int effectDuration, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playTimedEffect", effectDuration, whichHaptic);
    }
    public override void playPatternRTP(float vibrationDuration, float vibrationStrength, int whichHaptic, bool large, bool small, int repeat_times, float silienceDuration, float HapticsDuration)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playPatternRTP", vibrationDuration, vibrationStrength, whichHaptic, large, small, repeat_times, silienceDuration, HapticsDuration);
    }
    public override void PlayPatternRTP(float vibrationDuration, float vibrationStrength, int whichHaptic, bool large, bool small, int repeat_times, float silienceDuration, float HapticsDuration)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playPatternRTP", vibrationDuration, vibrationStrength, whichHaptic, large, small, repeat_times, silienceDuration, HapticsDuration);
    }
    public override void playEffectSeqBuff(byte[] Sequence, int buffSize, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playEffectSeqBuff", Sequence, buffSize, whichHaptic);
    }
    public override void PlayEffectSeqBuff(byte[] Sequence, int buffSize, int whichHaptic)
    {
       Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playEffectSeqBuff", Sequence, buffSize, whichHaptic);
    }
    public override void playRTPSequence(String sequence, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playRTPSequence", sequence, whichHaptic);
    }
    public override void PlayRTPSequence(String sequence, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playRTPSequence", sequence, whichHaptic);
    }
    public override void playRTPSeqBuff(byte[] Sequence, int buffSize, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playRTPSeqBuff", Sequence, buffSize, whichHaptic);
    }
    public override void PlayRTPSeqBuff(byte[] Sequence, int buffSize, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playRTPSeqBuff", Sequence, buffSize, whichHaptic);
    }
    public override void playRingHaptics(int index, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playRingHaptics", index, whichHaptic);
    }
    public override void PlayRingHaptics(int index, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playRingHaptics", index, whichHaptic);
    }
    public override void playRingSeq(int index, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playRingSeq", index, whichHaptic);
    }
    public override void PlayRingSeq(int index, int whichHaptic)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "playRingSeq", index, whichHaptic);
    }
    public override string getRingHapticsName()
    {
        string value = null;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsName");
        return value;
    }
    public override string GetRingHapticsName()
    {
        string value = null;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsName");
        return value;
    }
    public override string getRingHapticsValues()
    {
        string value = null;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsValues");
        return value;
    }
    public override string GetRingHapticsValues()
    {
        string value = null;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsValues");
        return value;
    }
    public override string getRingHapticsValue(int index)
    {
        string value = null;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsValue", index);
        return value;
    }
    public override string GetRingHapticsValue(int index)
    {
        string value = null;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsValue", index);
        return value;
    }
    /***************************Haptics*************************************/

    /****************************AM3d*******************************************/
    public override void openEffects()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_OpenEffects");
    }
    public override void OpenEffects()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_OpenEffects");
    }
    public override void closeEffects()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_CloseEffects");
    }
    public override void CloseEffects()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_CloseEffects");
    }
    public override void setSurroundroomType(int type)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_SetSurroundroomType", type);
    }
    public override void SetSurroundroomType(int type)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_SetSurroundroomType", type);
    }
    public override void openRoomcharacteristics()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_OpenRoomcharacteristics");
    }
    public override void OpenRoomcharacteristics()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_OpenRoomcharacteristics");
    }
    public override void closeRoomcharacteristics()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_CloseRoomcharacteristics");
    }
    public override void CloseRoomcharacteristics()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_CloseRoomcharacteristics");
    }
    public override void EnableSurround()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_EnableSurround");
    }
    public override void EnableReverb()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_EnableReverb");
    }
    public override void startAudioEffect(String audioFile, bool isSdcard)
    {
     
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_StartAudioEffect", activity, audioFile, isSdcard);
    }
    public override void StartAudioEffect(String audioFile, bool isSdcard)
    {
       
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_StartAudioEffect", activity, audioFile, isSdcard);
    }
    public override void stopAudioEffect()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_StopAudioEffect");
    }
    public override void StopAudioEffect()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_StopAudioEffect");
    }
    public override void ReleaseAudioEffect()
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaAm3dActivityClass, "Pvr_ReleaseAudio");
    }
    /****************************AM3d*******************************************/

    /***************************Touch*************************************/
    public override void enableTouchPad(bool enable)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "enableTouchPad", enable);
    }
    public override void EnableTouchPad(bool enable)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "enableTouchPad", enable);
    }

    public override void switchTouchType(int device)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "switchTouchType", device);
    }
    public override void SwitchTouchType(int device)
    {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "switchTouchType", device);
    }

    public override int getTouchPadStatus()
    {
        int i = 0;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref i, javaVrActivityClass, "getTouchPadStatus");
        return i;
    }
    public override int GetTouchPadStatus()
    {
        int i = 0;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref i, javaVrActivityClass, "getTouchPadStatus");
        return i;
    }

    public bool setDeviceCpuFreqDefault()
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDeviceCpuFreqDefault");
        return istrue;
    }
    public bool SetDeviceCpuFreqDefault()
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDeviceCpuFreqDefault");
        return istrue;
    }

    public override bool setDeviceProp(int device_id, string value)
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDeviceProp", device_id, value);
        return istrue;
    }
    public override bool SetDeviceProp(int device_id, string value)
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDeviceProp", device_id, value);
        return istrue;
    }

    public override string getDeviceProp(int device_id)
    {
        string str = "";
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref str, javaVrActivityClass, "getDeviceProp", device_id);
        return str;
    }
    public override string GetDeviceProp(int device_id)
    {
        string str = "";
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref str, javaVrActivityClass, "getDeviceProp", device_id);
        return str;
    }

    public override bool requestHidSensor(int user)
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "requestHidSensor", user);
        return istrue;
    }
    public override bool RequestHidSensor(int user)
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "requestHidSensor", user);
        return istrue;
    }

    public override int getHidSensorUser()
    {
        int i = 0;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref i, javaVrActivityClass, "getHidSensorUser");
        return i;
    }
    public override int GetHidSensorUser()
    {
        int i = 0;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref i, javaVrActivityClass, "getHidSensorUser");
        return i;
    }

    public override bool setThreadRunCore(int pid, int core_id)
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setThreadRunCore", pid, core_id);
        return istrue;

    }
    public override bool SetThreadRunCore(int pid, int core_id)
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setThreadRunCore", pid, core_id);
        return istrue;

    }

    public override int getThreadRunCore(int pid)
    {
        int i = 0;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref i, javaVrActivityClass, "getThreadRunCore", pid);
        return i;
    }
    public override int GetThreadRunCore(int pid)
    {
        int i = 0;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref i, javaVrActivityClass, "getThreadRunCore", pid);
        return i;
    }
    public override bool setSystemRunLevel(int device_id, int level)
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setSystemRunLevel", device_id, level);
        return istrue;
    }
    public override bool SetSystemRunLevel(int device_id, int level)
    {
        bool istrue = false;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setSystemRunLevel", device_id, level);
        return istrue;
    }

    public override int getSystemRunLevel(int device_id)
    {
        int i = 0;
       Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref i, javaVrActivityClass, "getSystemRunLevel", device_id);
        return i;
    }
    public override int GetSystemRunLevel(int device_id)
    {
        int i = 0;
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref i, javaVrActivityClass, "getSystemRunLevel", device_id);
        return i;
    }
    /***************************Touch*************************************/
#endregion Haptics

}
#endif