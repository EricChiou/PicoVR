using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LitJson;
public class BLEListener : MonoBehaviour {

	public GameObject root;
	public GameObject BLEbtn;
	public class BLEDevice{
		public string mac;
		public string name;
	};
	public List<BLEDevice> devicelist = new List<BLEDevice>();
	//------------------------------BLE click----------------
	private bool resetButtonin = false;
	public MoveSphere sphere;
	public MoveCylinder cylinder;
	//------------------------------BLE click----------------
	// Use this for initialization
	void Awake(){
		PicoVRManager.SDK.currentDevice.BLEActionEvent += BLEActionEvent;
		PicoVRManager.SDK.currentDevice.BLEConnectedStateChangedEvent += BLEConnectedStateChangedEvent;
		PicoVRManager.SDK.currentDevice.FindBLEDeviceEvent += FindBLEDeviceEvent;
		PicoVRManager.SDK.currentDevice.NotFindBLEDeviceEvent += NotFindBLEDeviceEvent;
		PicoVRManager.SDK.currentDevice.BluetoothStateChangedEvent += BluetoothStateChangedEvent;
		PicoVRManager.SDK.currentDevice.BLEVersionChangedEvent += BLEVersionChangedEvent;
	}
	void Start () {
		resetButtonin = false;
	}
	public void ResetButtonin()
	{
		resetButtonin = true;
	}
	public void ResetButtonout()
	{
		resetButtonin = false;
	}
	// Update is called once per frame
	// Update is called once per frame
	void Update () {
		bool isBLEOpen = PicoVRManager.SDK.currentDevice.IsBluetoothOpened();
		if(isBLEOpen)
			BLEbtn.transform.FindChild("Text").GetComponent<Text>().text = "Close BLE";
		else 
			BLEbtn.transform.FindChild("Text").GetComponent<Text>().text = "Open BLE";

		int powerState = PicoVRManager.SDK.currentDevice.GetBluetoothState();
		int isConnected = PicoVRManager.SDK.currentDevice.GetBLEConnectState();
		string otaVersion = PicoVRManager.SDK.currentDevice.GetBLEVersion ();
		root.transform.FindChild ("PicoVR/Head/Infor").GetComponent<Text> ().text = "";
		if(powerState == 5)
			root.transform.FindChild ("PicoVR/Head/Infor").GetComponent<Text> ().text += "powerState: On\n";
		else
			root.transform.FindChild ("PicoVR/Head/Infor").GetComponent<Text> ().text += "powerState: Off\n";

		root.transform.FindChild ("PicoVR/Head/Infor").GetComponent<Text> ().text += ("connect state: "+isConnected.ToString()+"\n");
		if(otaVersion != null)
			root.transform.FindChild ("PicoVR/Head/Infor").GetComponent<Text> ().text += ("otaVersion: "+otaVersion+"\n");

	}


	void BLEActionEvent (object sender, System.EventArgs e)
	{
		PicoVRIOSDevice.BLEAction keyevent = sender as PicoVRIOSDevice.BLEAction;

		root.transform.FindChild ("HeadWearListCanvas").gameObject.SetActive (false);
		root.transform.FindChild ("TTT").gameObject.SetActive (true);

		if (keyevent.keyType == PicoVRIOSDevice.BLEActionType.ACTION_VOLUME_CHANGE) {
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "ACTION_VOLUME_CHANGE";
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = keyevent.keyValue.ToString ();
		} else if (keyevent.keyType == PicoVRIOSDevice.BLEActionType.ACTION_AUDIOJACK_IN)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "ACTION_AUDIOJACK_IN";
		else if (keyevent.keyType == PicoVRIOSDevice.BLEActionType.ACTION_AUDIOJACK_LOSE)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "ACTION_AUDIOJACK_LOSE";
		else if (keyevent.keyType == PicoVRIOSDevice.BLEActionType.ACTION_CAMERA)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "ACTION_CAMERA";
		else if (keyevent.keyType == PicoVRIOSDevice.BLEActionType.ACTION_PICO_BACK)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "ACTION_PICO_BACK";
		else if (keyevent.keyType == PicoVRIOSDevice.BLEActionType.ACTION_PICO_MENU)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "ACTION_PICO_MENU";
		else if (keyevent.keyType == PicoVRIOSDevice.BLEActionType.ACTION_SENSOR_FAR)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "ACTION_SENSOR_FAR";
		else if (keyevent.keyType == PicoVRIOSDevice.BLEActionType.ACTION_SENSOR_NEAR)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "ACTION_SENSOR_NEAR";
		else if (keyevent.keyType == PicoVRIOSDevice.BLEActionType.TOUCH_PAD_CLICK) {
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "TOUCH_PAD_CLICK";
			/*
			if (resetButtonin) {
				if (sphere !=null && cylinder !=null) {
					sphere.Reset ();
					cylinder.Reset ();
				}
			}
			*/
			PicoVRManager.SDK.newPicovrTriggered = true;
		}
		else if(keyevent.keyType == PicoVRIOSDevice.BLEActionType.TOUCH_PAD_DOWN)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "TOUCH_PAD_DOWN";
		else if(keyevent.keyType == PicoVRIOSDevice.BLEActionType.TOUCH_PAD_LEFT)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "TOUCH_PAD_LEFT";
		else if(keyevent.keyType == PicoVRIOSDevice.BLEActionType.TOUCH_PAD_RIGHT)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "TOUCH_PAD_RIGHT";
		else if(keyevent.keyType == PicoVRIOSDevice.BLEActionType.TOUCH_PAD_UP)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "TOUCH_PAD_UP";

	}
	void FindBLEDeviceEvent(object sender, System.EventArgs e)
	{
		root.transform.FindChild ("BLEListCanvas").gameObject.SetActive (true);
		root.transform.FindChild("Canvas").gameObject.SetActive(true);
		root.transform.FindChild ("TTT").gameObject.SetActive (false);


		devicelist.Clear();
		string json = sender as string;
		JsonData ListJson = JsonMapper.ToObject (json);
		for (int i = 0; i < ListJson.Count; i++) {
			BLEDevice deviceModel = new BLEDevice ();
			JsonData item = ListJson [i];
			IDictionary deviceDic = item as IDictionary;
			if(deviceDic.Contains("mac"))
			{	string macString = item ["mac"].ToString();
				deviceModel.mac = macString;
			}
			if (deviceDic.Contains ("name")) {
				string name = item ["name"].ToString();
				deviceModel.name = name;
			}
			devicelist.Add (deviceModel);
		}

		for (int i = 0; i < 10; i++) {
			string sIndex = "Button (" + (i + 1).ToString () + ")";
			string directory = "BLEListCanvas/Panel/" + sIndex;

			GameObject button = root.transform.Find (directory).gameObject;
			if (i < devicelist.Count) {
				button.SetActive (true);
			} else {
				button.SetActive (false);
			}
		}
	}
	void NotFindBLEDeviceEvent(object sender, System.EventArgs e)
	{
		//提示并显示底部面板
		root.transform.FindChild("Canvas").gameObject.SetActive(true);
		root.transform.FindChild ("HeadWearListCanvas").gameObject.SetActive (false);
		root.transform.FindChild ("BLEListCanvas").gameObject.SetActive (false);
		root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "DEVICE NOT FOUND!";

	}
	void BLEVersionChangedEvent(object sender, System.EventArgs e)
	{
		//提示OTA版本号
		root.transform.FindChild ("HeadWearListCanvas").gameObject.SetActive (false);
		root.transform.FindChild ("BLEListCanvas").gameObject.SetActive (false);
		string otaVersion = PicoVRManager.SDK.currentDevice.GetBLEVersion ();
		if(otaVersion != null)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "otaversion"+otaVersion;
	}
	void BluetoothStateChangedEvent(object sender, System.EventArgs e)
	{
		//提示OTA版本号
		root.transform.FindChild ("HeadWearListCanvas").gameObject.SetActive (false);
		root.transform.FindChild ("BLEListCanvas").gameObject.SetActive (false);
		int powerState = PicoVRManager.SDK.currentDevice.GetBluetoothState();
		if(powerState == 5)
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text += "powerState:On";
		else
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text += "powerState:Off";
	}
	void BLEConnectedStateChangedEvent(object sender, System.EventArgs e)
	{
		//提示OTA版本号
		root.transform.FindChild ("HeadWearListCanvas").gameObject.SetActive (false);
		root.transform.FindChild ("BLEListCanvas").gameObject.SetActive (false);
		int isConnected = PicoVRManager.SDK.currentDevice.GetBLEConnectState();
		root.transform.FindChild ("PicoVR/Head/Infor").GetComponent<Text> ().text += isConnected.ToString();
	}


	public void ToggleBLEState(){
		string text  = BLEbtn.transform.FindChild("Text").GetComponent<Text>().text;

		if ("Open BLE".Equals (text)) {
			PicoVRManager.SDK.currentDevice.OpenBLECentral ();
		} else {
			PicoVRManager.SDK.currentDevice.StopBLECentral ();
		}
	}
	void OnDestroy()
	{

	}
}
