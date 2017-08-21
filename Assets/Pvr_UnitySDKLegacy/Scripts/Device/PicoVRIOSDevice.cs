using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Runtime.InteropServices;



public class PicoVRIOSDevice : PicoVRBaseDevice
{
	public static int PVR_GLEventID = 7001;

	[DllImport("__Internal")]
	public static extern void PVR_Init_Native ( );

	[DllImport("__Internal")]
	public static extern float PVR_FOV_Native();

	[DllImport("__Internal")]
	public static extern float PVR_Separation_Native();

	[DllImport("__Internal")]
	public static extern void PVR_RenderTexturenSize_Native (ref int width, ref int height);

	[DllImport("__Internal")]
	public static extern void PVR_UpdateRenderParams_Native(float[] renderParams,float zNear, float zFar);

	[DllImport("__Internal")]
	public static extern int PVR_HeadWearType_Native ();

	[DllImport("__Internal")]
	public static extern void PVR_ChangeHeadWearType_Native (int type);

	[DllImport("__Internal")]
	public static extern void PVR_SetRenderTextureID_Native (int eye, int texID);

	[DllImport("__Internal")]
	public static extern void PVR_ResetHeadTrack_Native ();
	[DllImport("__Internal")]
	public static extern void PVR_StartHeadTrack_Native ();
	[DllImport("__Internal")]
	public static extern void PVR_StopHeadTrack_Native ();
	[DllImport("__Internal")]
	public static extern int PVR_OpenBLECentral ();
	[DllImport("__Internal")]
	public static extern int PVR_StopBLECentral ();
	[DllImport("__Internal")]
	public static extern int PVR_ConnectBLEDevice (string mac);
	[DllImport("__Internal")]
	public static extern int PVR_ScanBLEDevice ();
	[DllImport("__Internal")]
	public static extern void PVR_SDKVersion (ref int high,ref int mid,ref int low);
    

    [DllImport("__Internal")]
    private static extern void PVR_SetRatio(float midH, float midV);

    private string model;
	private float fov = 90f;
	private Quaternion rot;
	private Vector3 pos;
	private static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));
	private int deviceType = -1;
    

    public PicoVRIOSDevice()
	{
	    Async = true;
		Debug.Log (GetSDKVersion ());
		PVR_Init_Native ();
		CreateRenderTextureArray ();
	}
    public override string GetSDKVersion()
    {
		int high = 0;
		int mid = 0;
		int low = 0;
		PVR_SDKVersion (ref high, ref mid, ref low);
		return ("" + high.ToString () + "." + mid.ToString () + "." + low.ToString ());
	}
#region 创建贴图
	private void CreateRenderTextureArray()
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
		}
	}
	
	
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
			Debug.Log("eyeTextureIndex : " + eyeTextureIndex.ToString());
		}
		
	}
#endregion

#region 更新每一帧的状态
	public void UpdateFrameParamsFromActivity()
	{
		float[] frameInfo = UpdateRenderParams(1.0f, 1000.0f);
		int j = 0;
		for (int i = 0; i < 16; ++i, ++j)
		{
			PicoVRManager.SDK.headView[i] = frameInfo[j];
		}
		PicoVRManager.SDK.headView = flipZ * PicoVRManager.SDK.headView.inverse * flipZ;
	}

	public float[] UpdateRenderParams(float zNear, float zFar)
	{
		float[] frameInfo = new float[16];
		PVR_UpdateRenderParams_Native(frameInfo, zNear, zFar);
		return frameInfo;
	}
		
	public override void UpdateState()
	{	int device = PVR_HeadWearType_Native();
		if (device != this.deviceType) {
			this.deviceType = device;
			UpdateScreenData ();
		}
		UpdateFrameParamsFromActivity();
		this.fov = PVR_FOV_Native();
		rot = Quaternion.LookRotation(PicoVRManager.SDK.headView.GetColumn(2), PicoVRManager.SDK.headView.GetColumn(1));
		pos = PicoVRManager.SDK.headView.GetColumn(3);
		PicoVRManager.SDK.eyeFov = fov;
		PicoVRManager.SDK.headPose.Set(pos, rot);
        
    }
#endregion

	public override Vector2 GetStereoScreenSize()
	{
		int width = 0, height = 0;
		PVR_RenderTexturenSize_Native (ref width, ref height);
		return new Vector2(width,height);
	}
	public override void UpdateScreenData()
	{
		ComputeEyesFromProfile();
	}

    public override float GetSeparation()
    {
		float separation = PVR_Separation_Native ();
		return separation;
    }
		
    public override void ChangeHeadwear(int headwear) { 
		PVR_ChangeHeadWearType_Native ( headwear );
	}

	public override void StartHeadTrack ()
	{
		PVR_StartHeadTrack_Native ();
	}
	public override void StopHeadTrack ()
	{
		PVR_StopHeadTrack_Native ();
	}

	public override void ResetHeadTrack ()
	{
		PVR_ResetHeadTrack_Native ();
	}

	public override void UpdateTextures() {     
	
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


		currEyeTextureIdx = nextEyeTextureIdx;
		nextEyeTextureIdx = (nextEyeTextureIdx + 1) % 3;
}

	
	public override void Destroy ()
	{
		base.Destroy ();
	}



	#region BLE
	private int devicePowerState = 0;
	private string otaVersion = null;
	private bool deviceConnected = false;
	private bool bleOpened = false;
	public int DevicePowerState{
		get{ return this.devicePowerState;}
		set{
			if (this.devicePowerState != value) {
				this.devicePowerState = value;
				if (null != this.BluetoothStateChangedEvent) {
					this.BluetoothStateChangedEvent (this, new EventArgs ());
				}
			}
		}
	}
	public bool DeviceConnected{
		get{ return this.deviceConnected;}
		set{
			if (this.deviceConnected != value) {
				this.deviceConnected = value;
				if (null != this.BLEConnectedStateChangedEvent) {
					this.BLEConnectedStateChangedEvent (this, new EventArgs ());
				}
			}
		}
	}

	public string OTAVersion{
		get{ return this.otaVersion;}
		set{
			if (this.otaVersion != value) {
				this.otaVersion = value;
				if (null != this.BLEVersionChangedEvent) {
					this.BLEVersionChangedEvent (this, new EventArgs ());
				}
			}
		}
	}



	public enum BLEActionType{
		TOUCH_PAD_CLICK = 0,
		TOUCH_PAD_UP = 1,
		TOUCH_PAD_DOWN =2,
		TOUCH_PAD_LEFT=3,
		TOUCH_PAD_RIGHT=4,
		ACTION_PICO_BACK=5,
		ACTION_PICO_MENU=6,
		ACTION_VOLUME_CHANGE=7,
		ACTION_CAMERA=8,
		ACTION_AUDIOJACK_IN=9,
		ACTION_AUDIOJACK_LOSE=10,
		ACTION_SENSOR_NEAR=11,
		ACTION_SENSOR_FAR=12
	};

	public class BLEAction{
		public BLEActionType keyType;
		public int keyValue;
	};
		
	public override bool IsBluetoothOpened()
	{
		return bleOpened;
	}
	public override bool OpenBLECentral()
	{
		int result = PVR_OpenBLECentral();
		if (result == 0) {
			bleOpened = false;
		} else {
			bleOpened = true;
		}
		return bleOpened;
	}
	public override bool StopBLECentral()
	{
		int result = PVR_StopBLECentral();
		if (result == 0) {
			bleOpened = true;
			return false;
		} else {
			this.devicePowerState = 4;
			this.deviceConnected = false;
			this.otaVersion = null;
			bleOpened = false;
			return true;
		}
	}
	public override bool ScanBLEDevice()
	{
		int result = PVR_ScanBLEDevice();
		if (result == 0) {
			return false;
		} else {
			return true;
		}
	}
	public override bool ConnectBLEDevice(string mac)
	{

		int result = PVR_ConnectBLEDevice (mac);
		if (result == 0) {
			return false;
		} else {
			return true;
		}
	}
	public override int GetBluetoothState()
	{
		return devicePowerState;
	}

	public override int GetBLEConnectState(){
		if (deviceConnected) {
			return 1;
		}
		return 0;
	}

	public override string GetBLEVersion(){
		return otaVersion;
	}

	public override void DevicePowerStateChanged (string state){
		this.devicePowerState = int.Parse (state);
		if (this.devicePowerState != 5) {
			this.deviceConnected = false;
			this.otaVersion = null;
		}
	}
	public override void DeviceConnectedStateChanged (string state){
		if ("1".Equals (state)) {
			this.deviceConnected = true;
		} else {
			this.deviceConnected = false;
			this.otaVersion = null;
		}
	}
	public override void DeviceFindNotification (string msg){
		if (null == msg || "".Equals(msg)) {
			if (null != this.NotFindBLEDeviceEvent) {
				this.NotFindBLEDeviceEvent (this, new EventArgs ());
			}
		} else {
			if (null != this.FindBLEDeviceEvent) {
				this.FindBLEDeviceEvent (msg, new EventArgs ());
			}
		}
	}
	public override void AcceptDeviceKeycode (string keycode){
		if (null != keycode) {
			string[] list = keycode.Split ( '|');
			BLEAction deviceKey = new BLEAction ();
			int keytype = int.Parse (list [0]);
			if (keytype == 13) {
				otaVersion = list [1];
			} else {
				deviceKey.keyType = (BLEActionType)int.Parse (list [0]);
				if (deviceKey.keyType == BLEActionType.ACTION_VOLUME_CHANGE) {
					deviceKey.keyValue = int.Parse (list [1]);
				}
				if (null != this.BLEActionEvent) {
					BLEActionEvent (deviceKey, new EventArgs ());
				}
			}
		}
	}
	#endregion
}
