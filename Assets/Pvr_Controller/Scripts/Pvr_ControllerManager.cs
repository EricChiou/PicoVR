///////////////////////////////////////////////////////////////////////////////
// Copyright 2015-2017  Pico Technology Co., Ltd. All Rights Reserved.
// File: Pvr_ControllerManager
// Author: Yangel.Yan
// Date:  2017/01/11
// Discription: Be Sure Your controller demo has this script
///////////////////////////////////////////////////////////////////////////////
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
using System;
using System.Collections;
using LitJson;
using Pvr_UnitySDKAPI;

public class Pvr_ControllerManager : MonoBehaviour
{
    /************************************    Properties  *************************************/
    private static Pvr_ControllerManager instance = null;

    public static Pvr_ControllerManager Instance
    {
        get
        {

            if (instance == null)
            {
                instance = UnityEngine.Object.FindObjectOfType<Pvr_ControllerManager>();
            }
            if (instance == null)
            {
                var go = new GameObject("GameObject");
                instance = go.AddComponent<Pvr_ControllerManager>();
                go.transform.localPosition = Vector3.zero;
            }
            return instance;
        }
    }
    #region Properties

#if UNITY_ANDROID
    private string lark2state;
    private string lark2key;
    private float lark2w = 1.0f, lark2x = 0f, lark2y = 0f, lark2z = 0f;
    private int touchXBegin = 0, touchXEnd = 0, touchYBegin = 0, touchYEnd = 0;
    private bool touchClock = false;
#endif
    private bool longpressClock = false;
    #endregion


    public static Pvr_ControllerLink controllerlink;
    public bool ExtendedAPI;
    private float cTime = 0.2f;
    private int touchNum = 0;
    public int slipNum = 43;  //滑动值，0-255，滑动超过此值，则判定为成功，若感觉太灵敏则调高，反之调低

    /*************************************  Unity API ****************************************/
    #region Unity API
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (instance != this)
        {
            Debug.LogError("instance object should be a singleton.");
            return;
        }
        if (controllerlink == null)
        {
            controllerlink = new Pvr_ControllerLink(this.gameObject.name);
        }
    }
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        #region ControllData

#if UNITY_ANDROID
        if (controllerlink.isBind)
        {
            //手柄转动四元数
            lark2state = controllerlink.GetHBSensorState();
            JsonData Jstate = JsonMapper.ToObject(lark2state);
            lark2w = Convert.ToSingle(Jstate[0].ToString());
            lark2x = Convert.ToSingle(Jstate[1].ToString());
            lark2y = Convert.ToSingle(Jstate[2].ToString());
            lark2z = Convert.ToSingle(Jstate[3].ToString());
            Controller.ControllerQua = new Quaternion(lark2x, lark2y, lark2z, lark2w);

            //lark2手柄键值TouchpadX,TouchpadY,HomeKeyPress,AppKeyPress,ClickKeyPress,VolumeUpKeyPress,VolumeDownKeyPress,BatteryLevel
            lark2key = controllerlink.javaHummingbirdClass.CallStatic<string>("getHBKeyEvent");

            JsonData JKey = JsonMapper.ToObject(lark2key);
            if (Convert.ToInt16(JKey[0].ToString()) > 0 || Convert.ToInt16(JKey[1].ToString()) > 0)
            {
                if (Convert.ToInt16(JKey[0].ToString()) == 0)
                {
                    TouchPadPosition.x = 1;
                }
                if (Convert.ToInt16(JKey[1].ToString()) == 0)
                {
                    TouchPadPosition.y = 1;
                }
                TouchPadPosition.x = Convert.ToInt16(JKey[0].ToString());
                TouchPadPosition.y = Convert.ToInt16(JKey[1].ToString());
            }
            else
            {
                touchNum++;
                if (touchNum >= 1)
                {
                    TouchPadPosition.x = 0;
                    TouchPadPosition.y = 0;
                    touchNum = 0;
                }
            }
            Controller.BatteryLevel = Convert.ToInt16(JKey[7].ToString());
            #region base api 
            //键值状态
            //Home Key
            if (Convert.ToInt16(JKey[2].ToString()) == 1)
            {
                if (!HomeKey.state)
                {
                    HomeKey.pressedDown = true;
                }
                else
                {
                    HomeKey.pressedDown = false;
                }
                HomeKey.state = true;
            }
            else
            {
                if (HomeKey.state)
                {
                    HomeKey.pressedUp = true;
                }
                else
                {
                    HomeKey.pressedUp = false;
                }
                HomeKey.state = false;
                HomeKey.pressedDown = false;
            }
            //APP Key
            if (Convert.ToInt16(JKey[3].ToString()) == 1)
            {
                if (!APPKey.state)
                {
                    APPKey.pressedDown = true;
                }
                else
                {
                    APPKey.pressedDown = false;
                }
                APPKey.state = true;
            }
            else
            {
                if (APPKey.state)
                {
                    APPKey.pressedUp = true;
                }
                else
                {
                    APPKey.pressedUp = false;
                }
                APPKey.state = false;
                APPKey.pressedDown = false;
            }
            //Touchpad Key
            if (Convert.ToInt16(JKey[4].ToString()) == 1)
            {
                if (!TouchPadKey.state)
                {
                    TouchPadKey.pressedDown = true;
                }
                else
                {
                    TouchPadKey.pressedDown = false;
                }
                TouchPadKey.state = true;
            }
            else
            {
                if (TouchPadKey.state)
                {
                    TouchPadKey.pressedUp = true;
                }
                else
                {
                    TouchPadKey.pressedUp = false;
                }
                TouchPadKey.state = false;
                TouchPadKey.pressedDown = false;
            }
            //VolumeUP Key
            if (Convert.ToInt16(JKey[5].ToString()) == 1)
            {
                if (!VolumeUpKey.state)
                {
                    VolumeUpKey.pressedDown = true;
                }
                else
                {
                    VolumeUpKey.pressedDown = false;
                }
                VolumeUpKey.state = true;
            }
            else
            {
                if (VolumeUpKey.state)
                {
                    VolumeUpKey.pressedUp = true;
                }
                else
                {
                    VolumeUpKey.pressedUp = false;
                }
                VolumeUpKey.state = false;
                VolumeUpKey.pressedDown = false;
            }
            //VolumeDown Key
            if (Convert.ToInt16(JKey[6].ToString()) == 1)
            {
                if (!VolumeDownKey.state)
                {
                    VolumeDownKey.pressedDown = true;
                }
                else
                {
                    VolumeDownKey.pressedDown = false;
                }
                VolumeDownKey.state = true;
            }
            else
            {
                if (VolumeDownKey.state)
                {
                    VolumeDownKey.pressedUp = true;
                }
                else
                {
                    VolumeDownKey.pressedUp = false;
                }
                VolumeDownKey.state = false;
                VolumeDownKey.pressedDown = false;
            }

        }
        #endregion

        #region extended api
        //打开扩展API后，提供长按和滑动功能
        if (ExtendedAPI)
        {
            //slip
            if (TouchPadPosition.x > 0 || TouchPadPosition.y > 0)
            {
                if (!touchClock)
                {
                    touchXBegin = TouchPadPosition.x;
                    touchYBegin = TouchPadPosition.y;
                    touchClock = true;
                }
                touchXEnd = TouchPadPosition.x;
                touchYEnd = TouchPadPosition.y;
            }
            else
            {
                if (touchXEnd > touchXBegin)
                {
                    if (touchYEnd > touchYBegin)
                    {
                        if (touchXEnd - touchXBegin > slipNum && ((touchXEnd - touchXBegin) > (touchYEnd - touchYBegin)))
                        {
                            //slide up
                            TouchPadKey.slideup = true;
                        }
                        if (touchYEnd - touchYBegin > slipNum && ((touchYEnd - touchYBegin) > (touchXEnd - touchXBegin)))
                        {
                            //slide right
                            TouchPadKey.slideright = true;
                        }
                    }
                    else if (touchYEnd < touchYBegin)
                    {
                        if (touchXEnd - touchXBegin > slipNum && ((touchXEnd - touchXBegin) > (touchYBegin - touchYEnd)))
                        {
                            //slide up
                            TouchPadKey.slideup = true;
                        }
                        if (touchYBegin - touchYEnd > slipNum && ((touchYBegin - touchYEnd) > (touchXEnd - touchXBegin)))
                        {
                            //slide left
                            TouchPadKey.slideleft = true;
                        }
                    }
                    else
                    {

                    }

                }
                else if (touchXEnd < touchXBegin)
                {
                    if (touchYEnd > touchYBegin)
                    {
                        if (touchXBegin - touchXEnd > slipNum && ((touchXBegin - touchXEnd) > (touchYEnd - touchYBegin)))
                        {
                            //slide down
                            TouchPadKey.slidedown = true;
                        }
                        if (touchYEnd - touchYBegin > slipNum && ((touchYEnd - touchYBegin) > (touchXBegin - touchXEnd)))
                        {
                            //slide right
                            TouchPadKey.slideright = true;
                        }
                    }
                    else if (touchYEnd < touchYBegin)
                    {
                        if (touchXBegin - touchXEnd > slipNum && ((touchXBegin - touchXEnd) > (touchYBegin - touchYEnd)))
                        {
                            //slide down 
                            TouchPadKey.slidedown = true;
                        }
                        if (touchYBegin - touchYEnd > slipNum && ((touchYBegin - touchYEnd) > (touchXBegin - touchXEnd)))
                        {
                            //slide left
                            TouchPadKey.slideleft = true;
                        }
                    }
                    else
                    {

                    }
                }
                else
                {
                    TouchPadKey.slideright = false;
                    TouchPadKey.slideleft = false;
                    TouchPadKey.slidedown = false;
                    TouchPadKey.slideup = false;
                }
                touchXBegin = 0;
                touchXEnd = 0;
                touchYBegin = 0;
                touchYEnd = 0;
                touchClock = false;
            }

            //longpress
            if (HomeKey.state)
            {
                HomeKey.count++;
                if (HomeKey.count >= 20)
                {
                    HomeKey.longPressed = true;
                }
                else
                {
                    HomeKey.longPressed = false;
                }
            }
            else
            {
                if (HomeKey.count >= 20)
                {
                    HomeKey.longPressed = true;
                }
                else
                {
                    HomeKey.longPressed = false;
                }
                longpressClock = false;
                HomeKey.count = 0;
            }
            if (APPKey.state)
            {
                APPKey.count++;
                if (APPKey.count >= 20)
                {
                    APPKey.longPressed = true;
                }
                else
                {
                    APPKey.longPressed = false;
                }
            }
            else
            {
                if (APPKey.count >= 20)
                {
                    APPKey.longPressed = true;
                }
                else
                {
                    APPKey.longPressed = false;
                }
                APPKey.count = 0;
            }
            if (TouchPadKey.state)
            {
                TouchPadKey.count++;
                if (TouchPadKey.count >= 20)
                {
                    TouchPadKey.longPressed = true;
                }
                else
                {
                    TouchPadKey.longPressed = false;
                }
            }
            else
            {
                if (TouchPadKey.count >= 20)
                {
                    TouchPadKey.longPressed = true;
                }
                else
                {
                    TouchPadKey.longPressed = false;
                }
                TouchPadKey.count = 0;
            }
            if (VolumeUpKey.state)
            {
                VolumeUpKey.count++;
                if (VolumeUpKey.count >= 20)
                {
                    VolumeUpKey.longPressed = true;
                }
                else
                {
                    VolumeUpKey.longPressed = false;
                }
            }
            else
            {
                if (VolumeUpKey.count >= 20)
                {
                    VolumeUpKey.longPressed = true;
                }
                else
                {
                    VolumeUpKey.longPressed = false;
                }
                VolumeUpKey.count = 0;
            }
            if (VolumeDownKey.state)
            {
                VolumeDownKey.count++;
                if (VolumeDownKey.count >= 20)
                {
                    VolumeDownKey.longPressed = true;
                }
                else
                {
                    VolumeDownKey.longPressed = false;
                }
            }
            else
            {
                if (VolumeDownKey.count >= 20)
                {
                    VolumeDownKey.longPressed = true;
                }
                else
                {
                    VolumeDownKey.longPressed = false;
                }
                VolumeDownKey.count = 0;
            }

        }
        #endregion
#endif
        #endregion

        if (Controller.UPvr_GetKeyLongPressed(Pvr_KeyCode.HOME) && !longpressClock)
        {
            Pvr_UnitySDKManager.pvr_UnitySDKSensor.ResetUnitySDKSensor();
            ResetController();
            longpressClock = true;
        }

        if (controllerlink.notPhone)
        {
            if (!HomeKey.longPressed && Controller.UPvr_GetKeyUp(Pvr_KeyCode.HOME))
            {
                controllerlink.RebackToLauncher();
            }
            if (!VolumeUpKey.longPressed && Controller.UPvr_GetKeyUp(Pvr_KeyCode.VOLUMEUP))
            {
                controllerlink.TurnUpVolume();
            }
            if (!VolumeDownKey.longPressed && Controller.UPvr_GetKeyUp(Pvr_KeyCode.VOLUMEDOWN))
            {
                controllerlink.TurnDownVolume();
            }
            if (Controller.UPvr_GetKeyLongPressed(Pvr_KeyCode.VOLUMEUP))
            {
                cTime -= Time.deltaTime;
                if (cTime <= 0)
                {
                    cTime = 0.2f;
                    controllerlink.TurnUpVolume();
                }
            }
            if (Controller.UPvr_GetKeyLongPressed(Pvr_KeyCode.VOLUMEDOWN))
            {
                cTime -= Time.deltaTime;
                if (cTime <= 0)
                {
                    cTime = 0.2f;
                    controllerlink.TurnDownVolume();
                }
            }
        }
    }


    #endregion

    /************************************ Public Interfaces  *********************************/
    #region Public Interfaces


    public void StopLark2Service()
    {
        if (controllerlink != null)
        {
            controllerlink.StopLark2Service();
        }
    }


    public Vector3 GetAngularVelocity()
    {
        if (controllerlink != null)
        {
            return controllerlink.GetAngularVelocity();
        }
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    public Vector3 GetAcceleration()
    {
        if (controllerlink != null)
        {
            return controllerlink.GetAcceleration();
        }
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    public void StartLark2Service()
    {
        if (controllerlink != null)
        {
            controllerlink.StartLark2Service();
        }
    }
    public void BindHBService()
    {
        if (controllerlink != null)
        {
            controllerlink.BindHBService();
        }
    }
    public void StartScan()
    {
        if (controllerlink != null)
        {

            Debug.Log("Yangel scan");
            controllerlink.StartScan();

            Invoke("ConnectBLE", 2.0f);
        }
    }
    public void StopScan()
    {
        if (controllerlink != null)
        {
            controllerlink.StopScan();
        }
    }
    public void ResetController()
    {
        if (controllerlink != null)
        {
            controllerlink.ResetController();
        }
    }
    public static int GetHBConnectionState()
    {
        int sta;
        sta = controllerlink.GetHBConnectionState();
        return sta;
    }
    public void ConnectBLE()
    {
        if (controllerlink != null)
        {
            controllerlink.ConnectBLE();
        }

    }
    public void DisConnectBLE()
    {
        if (controllerlink != null)
        {
            controllerlink.DisConnectBLE();
        }
    }

    public void SetBinPath(string path, bool isAsset)
    {
        if (controllerlink != null)
        {
            controllerlink.setBinPath(path, isAsset);
        }
    }
    public void StartUpgrade()
    {
        if (controllerlink != null)
        {
            controllerlink.StartUpgrade();
        }
    }
    public static string GetBLEImageType()
    {
        string type;
        type = controllerlink.GetBLEImageType();
        return type;
    }
    public static long GetBLEVersion()
    {
        long version;
        version = controllerlink.GetBLEVersion();
        return version;
    }
    public static string GetFileImageType()
    {
        string type;
        type = controllerlink.GetFileImageType();
        return type;
    }
    public static long GetFileVersion()
    {
        long version;
        version = controllerlink.GetFileVersion();
        return version;
    }
    public static void AutoConnectHbController(int scans)
    {
        if (controllerlink != null)
        {
            controllerlink.AutoConnectHbController(scans);

        }
    }
    //--------------
    public void setHbControllerMac(string mac)
    {
        controllerlink.hummingBirdMac = mac.Substring(0, 17);
        controllerlink.hummingBirdRSSI = Convert.ToInt16(mac.Remove(0, 18));
    }
    public int GetControllerRSSI()
    {
        return controllerlink.hummingBirdRSSI;
    }

    public void setHbServiceBindState(string state)
    {
        //state：0-已解绑，1-已绑定，2-未知
        if (Convert.ToInt16(state) == 0)
        {
            Invoke("BindHBService", 0.5f);
            controllerlink.isBind = false;
        }
        else if (Convert.ToInt16(state) == 1)
        {
            controllerlink.isBind = true;
        }
        else
        {

        }

    }

    public void setHbControllerConnectState(string isconnect)
    {
        controllerlink.controllerState = Convert.ToInt16(isconnect);
        //state：0-断开，1-已连接，2-未知
    }


    public void setupdateFailed()
    {
        //回调方法
    }

    public void setupdateSuccess()
    {
        //回调方法
    }

    public void setupdateProgress(string progress)
    {
        //升级进度 0-100 
    }

    public void setHbAutoConnectState(string state)
    {
        //UNKNOW = -1; //默认值
        //NO_DEVICE = 0;//没有扫描到HB手柄
        //ONLY_ONE = 1;//只扫描到一只HB手柄
        //MORE_THAN_ONE = 2;// 扫描到多只HB手柄
        //LAST_CONNECTED = 3;//扫描到上一次连接过的HB手柄
        //FACTORY_DEFAULT = 4;//扫描到工厂绑定的HB手柄（暂时未启用）
    }
    #endregion

}
