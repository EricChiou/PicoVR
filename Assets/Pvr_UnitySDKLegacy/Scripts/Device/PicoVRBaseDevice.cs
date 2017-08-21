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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
/// <summary>
/// PicoVRSDK  Platform 
/// </summary>
public abstract class PicoVRBaseDevice
{
    /// <summary>
    /// 异步
    /// </summary>
    public bool Async = false;

    /// <summary>
    /// 抗锯齿
    /// </summary>
    public enum RenderTextureAntiAliasing
    {
        X_1 = 1,
        X_2 = 2,
        X_4 = 4,
        X_8 = 8,
    }

    /// <summary>
    /// bit depth 图像位深
    /// </summary>
    public enum RenderTextureDepth
    {
        BD_0 = 0,
        BD_16 = 16,
        BD_24 = 24,
    }

    /// <summary>
    /// 虚基类PicoVRBaseDevice
    /// </summary>
    private static PicoVRBaseDevice device = null;

    public RtorScreen rtorScren;

    public bool PlantformSupport;

    public struct RtorScreen
    {
        public float Width;
        public float Height;
    }

    public const int eyeTextureCount = 6;

    public RenderTexture[] eyeTextures = new RenderTexture[eyeTextureCount];

    public int[] eyeTextureIds = new int[eyeTextureCount];

    public int currEyeTextureIdx = 0;

    public int nextEyeTextureIdx = 1;

    private bool canConnecttoActivity;


    public virtual bool CanConnecttoActivity
    {
        get;
        set;
    }
   
    protected PicoVRBaseDevice()
    {

    }

    public static PicoVRBaseDevice GetDevice()
    {
        if (device == null)
        {
#if UNITY_EDITOR
         device = new PicoVRUnityDevice();
#elif ANDROID_DEVICE
      device = new PicoVRAndroidDevice();
#elif IOS_DEVICE
      device = new PicoVRIOSDevice();

#else
      throw new InvalidOperationException("Unsupported device.");
#endif
        }
        return device;
    }

    /// <summary>
    /// 初始化方法
    /// </summary>
    public virtual void Init()
    {
    }



    /// <summary>
    /// 设置是否开启VR模式
    /// </summary>
    /// <param name="enabled"></param>
    public virtual void SetVRModeEnabled(bool enabled)
    {
       
    }

    /// <summary>
    /// 设置是否开启畸变
    /// </summary>
    /// <param name="enabled"></param>
    public virtual  void SetDistortionCorrectionEnabled(bool enabled)
    {
    }

    /// <summary>
    /// 创建rendertexture
    /// </summary>
    /// <returns></returns>
    public abstract Vector2 GetStereoScreenSize();

    /// <summary>
    /// 设置rendertexture
    /// </summary>
    /// <param name="stereoScreen"></param>
    public virtual void SetStereoScreen(RenderTexture stereoScreen)
    {
    }
    /// <summary>
    /// 重置姿态数据
    /// </summary>
    public virtual void resetFalconBoxSensor()
    {
    }
    public virtual void ResetFalconBoxSensor()
    {
    }
    /// <summary>
    /// 设置自动漂移校正Auto Drift Correction（？？？）
    /// </summary>
    /// <param name="enabled"></param>
    public virtual  void SetAutoDriftCorrectionEnabled(bool enabled)
    {
    }

    /// <summary>
    /// 在需要更新的地方，更新状态， 更新屏幕数据
    /// </summary>
    public abstract void UpdateState();

    public abstract void UpdateScreenData();

    public virtual  float GetSeparation()
    {
        return 0.0f;
    }

    public virtual  void stopHidService()
    {
    }

    public virtual  void startHidService()
    {
    }

   

    public virtual  void InitForEye(ref Material mat)
    {
    }

    public void ComputeEyesFromProfile()
    {
        PicoVRManager.SDK.leftEyeView = Matrix4x4.identity;
        PicoVRManager.SDK.leftEyeView[0, 3] = -PicoVRManager.SDK.picoVRProfile.device.devLenses.separation / 2;
        float[] rect = PicoVRManager.SDK.picoVRProfile.GetLeftEyeVisibleTanAngles(PicoVRManager.SDK.currentDevice.rtorScren.Width, PicoVRManager.SDK.currentDevice.rtorScren.Height);
        PicoVRManager.SDK.leftEyeProj = MakeProjection(rect[0], rect[1], rect[2], rect[3], 1, 1000);
        rect = PicoVRManager.SDK.picoVRProfile.GetLeftEyeNoLensTanAngles(PicoVRManager.SDK.currentDevice.rtorScren.Width, PicoVRManager.SDK.currentDevice.rtorScren.Height);
        PicoVRManager.SDK.leftEyeUndistortedProj = MakeProjection(rect[0], rect[1], rect[2], rect[3], 1, 1000);
        PicoVRManager.SDK.leftEyeRect = PicoVRManager.SDK.picoVRProfile.GetLeftEyeVisibleScreenRect(rect, PicoVRManager.SDK.currentDevice.rtorScren.Width, PicoVRManager.SDK.currentDevice.rtorScren.Height);

        PicoVRManager.SDK.rightEyeView = PicoVRManager.SDK.leftEyeView;
        PicoVRManager.SDK.rightEyeView[0, 3] *= -1;
        PicoVRManager.SDK.rightEyeProj = PicoVRManager.SDK.leftEyeProj;
        PicoVRManager.SDK.rightEyeProj[0, 2] *= -1;
        PicoVRManager.SDK.rightEyeUndistortedProj = PicoVRManager.SDK.leftEyeUndistortedProj;
        PicoVRManager.SDK.rightEyeUndistortedProj[0, 2] *= -1;
        PicoVRManager.SDK.rightEyeRect = PicoVRManager.SDK.leftEyeRect;
        PicoVRManager.SDK.rightEyeRect.x = 1 - PicoVRManager.SDK.rightEyeRect.xMax;
    }

    public static Matrix4x4 MakeProjection(float l, float t, float r, float b, float n, float f)
    {
        Matrix4x4 m = Matrix4x4.zero;
        m[0, 0] = 2 * n / (r - l);
        m[1, 1] = 2 * n / (t - b);
        m[0, 2] = (r + l) / (r - l);
        m[1, 2] = (t + b) / (t - b);
        m[2, 2] = (n + f) / (n - f);
        m[2, 3] = 2 * n * f / (n - f);
        m[3, 2] = -1;
        return m;
    }

    public virtual void Destroy()
    {
        if (device == this)
        {
            device = null;
        }
    }

    public virtual void StartHeadTrack()
    {
    }
    public virtual void StartControllerTrack()
    {
    }
    public virtual void ResetHeadTrack()
    {
    }
    public virtual void ResetControllerTrack()
    {
    }
    public virtual  void CloseHMDSensor() {  }

    public virtual void OpenHMDSensor()
    {
    }

    public virtual  void IsFocus(bool state)
    {
    }

    public virtual void StopControllerTrack()
    {
    }
    public virtual  void StopHeadTrack()
    {
    }

    public virtual  Vector3 GetBoxSensorAcc()
    {
      return  new Vector3(0f,0f,0f);
    }

    public virtual  Vector3 GetBoxSensorGyr()
    {
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    public virtual void ChangeHeadwear(int headwear)
    {
    }

    public virtual  void playeffect(int effectID, int whichHaptic)
    {
    }
    public virtual void PlayEffect(int effectID, int whichHaptic)
    {
    }

    public virtual  void playEffectSequence(string sequence, int whichHaptic)
    {
    }
    public virtual void PlayEffectSequence(string sequence, int whichHaptic)
    {
    }

    public virtual  void setAudioHapticEnabled(bool enable, int whichHaptic)
    {
    }
    public virtual void SetAudioHapticEnabled(bool enable, int whichHaptic)
    {
    }
    public virtual void stopPlayingEffect(int whichHaptic)
    {
    }
    public virtual void StopPlayingEffect(int whichHaptic)
    {
    }

    public virtual void playeffectforce(int effectID, int whichHaptic) { }
    public virtual void Playeffectforce(int effectID, int whichHaptic) { }
    public virtual void playTimedEffect(int effectDuration, int whichHaptic) { }
    public virtual void PlayTimedEffect(int effectDuration, int whichHaptic) { }
    public virtual void playPatternRTP(float vibrationDuration, float vibrationStrength,int whichHaptic, bool large, bool small, int repeat_times,float silienceDuration, float HapticsDuration) { }
    public virtual void PlayPatternRTP(float vibrationDuration, float vibrationStrength, int whichHaptic, bool large, bool small, int repeat_times, float silienceDuration, float HapticsDuration) { }
    public virtual void playEffectSeqBuff(byte[] Sequence, int buffSize, int whichHaptic) { }
    public virtual void PlayEffectSeqBuff(byte[] Sequence, int buffSize, int whichHaptic) { }
    public virtual void playRTPSequence(String sequence, int whichHaptic) { }
    public virtual void PlayRTPSequence(String sequence, int whichHaptic) { }
    public virtual void playRTPSeqBuff(byte[] Sequence, int buffSize, int whichHaptic) { }
    public virtual void PlayRTPSeqBuff(byte[] Sequence, int buffSize, int whichHaptic) { }
    //public virtual void playEffectSequenceRepeat(String sequence, int repeat, int whichHaptic) { }
    public virtual void playRingHaptics(int index, int whichHaptic) { }
    public virtual void PlayRingHaptics(int index, int whichHaptic) { }
    public virtual void playRingSeq(int index, int whichHaptic) { }
    public virtual void PlayRingSeq(int index, int whichHaptic) { }
    public virtual string getRingHapticsName() {
        return null;
    }
    public virtual string GetRingHapticsName()
    {
        return null;
    }
    public virtual string getRingHapticsValues() {
        return null;
    }
    public virtual string GetRingHapticsValues()
    {
        return null;
    }
    public virtual string getRingHapticsValue(int index) {
        return null;
    }
    public virtual string GetRingHapticsValue(int index)
    {
        return null;
    }
    public virtual void UpdateTextures() { }
    /*******************************************************************************/

    public virtual void enableTouchPad(bool enable)
    {
    } //触控板是否可用
    public virtual void EnableTouchPad(bool enable)
    {
    }

    public virtual void switchTouchType(int device)
    {
    } //鼠标/触摸板切换
    public virtual void SwitchTouchType(int device)
    {
    }

    public virtual int getTouchPadStatus()
    {
        return 0;
    } //获取触摸板类型
    public virtual int GetTouchPadStatus()
    {
        return 0;
    }

    public virtual bool setDeviceProp(int device_id, string value)
    {
        return true;
    }
    public virtual bool SetDeviceProp(int device_id, string value)
    {
        return true;
    }

    public virtual string getDeviceProp(int device_id)
    {
        return "";
    }
    public virtual string GetDeviceProp(int device_id)
    {
        return "";
    }
	public virtual  string GetSDKVersion()
	{
		return "";
	}
    public virtual bool requestHidSensor(int user)
    {
        return false;
    }
    public virtual bool RequestHidSensor(int user)
    {
        return false;
    }

    public virtual int getHidSensorUser()
    {
        return 1;
    }
    public virtual int GetHidSensorUser()
    {
        return 1;
    }

    public virtual bool setThreadRunCore(int pid, int core_id)
    {
        return true;
    }
    public virtual bool SetThreadRunCore(int pid, int core_id)
    {
        return true;
    }

    public virtual int getThreadRunCore(int pid)
    {
        return 1;
    }
    public virtual int GetThreadRunCore(int pid)
    {
        return 1;
    }

    public virtual bool setSystemRunLevel(int device_id, int level)
    {
        return true;
    }
    public virtual bool SetSystemRunLevel(int device_id, int level)
    {
        return true;
    }

    public virtual int getSystemRunLevel(int device_id)
    {
        return 1;
    }
    public virtual int GetSystemRunLevel(int device_id)
    {
        return 1;
    }

    /*****************************音量亮度*************************************/

    public virtual bool initBatteryVolClass()
    {
        return true;
    }
    public virtual bool InitBatteryVolClass()
    {
        return true;
    }

    public virtual bool startAudioReceiver()
    {
        return true;
    }
    public virtual bool StartAudioReceiver()
    {
        return true;
    }
    public virtual bool startBatteryReceiver()
    {
        return true;
    }
    public virtual bool StartBatteryReceiver()
    {
        return true;
    }

    public virtual bool stopAudioReceiver()
    {
        return true;
    }
    public virtual bool StopAudioReceiver()
    {
        return true;
    }
    public virtual bool stopBatteryReceiver()
    {
        return true;
    }
    public virtual bool StopBatteryReceiver()
    {
        return true;
    }
    public virtual int getMaxVolumeNumber()
    {
        return 1;
    }
    public virtual int GetMaxVolumeNumber()
    {
        return 1;
    }
    public virtual int getCurrentVolumeNumber()
    {
        return 1;
    }
    public virtual int GetCurrentVolumeNumber()
    {
        return 1;
    }
    public virtual bool volumeUp()
    {
        return true;
    }
    public virtual bool VolumeUp()
    {
        return true;
    }

    public virtual bool volumeDown()
    {
        return true;
    }
    public virtual bool VolumeDown()
    {
        return true;
    }

    public virtual bool setVolumeNum(int volume)
    {
        return true;
    }
    public virtual bool SetVolumeNum(int volume)
    {
        return true;
    }

    public virtual bool setBrightness(int brightness)
    {
        return true;
    }
    public virtual bool SetBrightness(int brightness)
    {
        return true;
    }

    public virtual int getCurrentBrightness()
    {
        return 1;
    }
    public virtual int GetCurrentBrightness()
    {
        return 1;
    }


    /****************************音量亮度*********************************************/

    /****************************AM3d*******************************************/
   
    public virtual void openEffects() { }
    public virtual void OpenEffects() { }

    public virtual void closeEffects(){}
    public virtual void CloseEffects() { }
    //Surround room size 1small 2Medium 3Large
    public virtual void setSurroundroomType(int type) { }
    public virtual void SetSurroundroomType(int type) { }
    //Surround room characteristics
    public virtual void openRoomcharacteristics() { }
    public virtual void OpenRoomcharacteristics() { }

    public virtual void closeRoomcharacteristics() { }
    public virtual void CloseRoomcharacteristics() { }
    //Enable surround
    public virtual void EnableSurround() { }

    //Enable reverb
    public virtual void EnableReverb() { }
    public virtual void startAudioEffect(String audioFile, bool isSdcard) { }
    public virtual void StartAudioEffect(String audioFile, bool isSdcard) { }
    public virtual void stopAudioEffect() { }
    public virtual void StopAudioEffect() { }
    public virtual void ReleaseAudioEffect() { }
    /****************************AM3d*******************************************/
    /****************************BLE蓝牙*********************************************/


    public EventHandler FindBLEDeviceEvent;
			public EventHandler NotFindBLEDeviceEvent;
			public EventHandler BLEActionEvent;
			public EventHandler BLEVersionChangedEvent;
			public EventHandler BluetoothStateChangedEvent;
			public EventHandler BLEConnectedStateChangedEvent;



	public virtual bool OpenBLECentral()
	{
			return false;
	}
	public virtual bool StopBLECentral()
	{
			return false;
	}
	public virtual bool ScanBLEDevice()
	{
			return false;
	}
	public virtual bool ConnectBLEDevice(string mac)
	{
			return false;
	}

    public virtual bool setDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid, string number)
    {
        return false;
    }
    public virtual bool SetDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid, string number)
    {
        return false;
    }
    public virtual string getDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid)
    {
        return 1+"";
    }
    public virtual string GetDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid)
    {
        return 1 + "";
    }
    public virtual string getDeviceSN()
    {
        return 1 + "";
    }
    public virtual string GetDeviceSN()
    {
        return 1 + "";
    }
    public virtual string getDeviceModelName()
    {
        return "";
    }
    public virtual string GetDeviceModelName()
    {
        return "";
    }

    public virtual bool IsBluetoothOpened()
	{
		return false;
	}

	public virtual int GetBluetoothState()
	{
		return 0;
	}

	public virtual int GetBLEConnectState(){
		return 0;
	}

	public virtual string GetBLEVersion(){
		return null;
	}

	public virtual void DevicePowerStateChanged (string state){

	}

	public virtual void DeviceConnectedStateChanged (string state){

	}

	public virtual void DeviceFindNotification (string msg){

	}

	public virtual void AcceptDeviceKeycode (string keykode){

	}
    public virtual int GetPsensorState()
    {
        return 0;
    }
    /****************************BLE蓝牙*********************************************/
}
