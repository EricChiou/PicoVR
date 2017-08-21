﻿///////////////////////////////////////////////////////////////////////////////
// Copyright 2015-2017  Pico Technology Co., Ltd. All Rights Reserved.
// File: Pvr_TouchPad
// Author: AiLi.Shang
// Date:  2017/01/11
// Discription: The demo of using TouchPad
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
using UnityEngine.UI;
using Pvr_UnitySDKAPI;

public class Pvr_TouchPad : MonoBehaviour {

    public const int SERVICE_STARTED = 0;
    public const int CONNECTE_SUCCESS = 1;
    public const int DISCONNECTE = 2;
    public const int CONNECTE_FAILED = 3;
    public const int NO_DEVICE = 4;
    public static AndroidJavaObject activity;
    public static AndroidJavaClass javaVrActivityClass;
    int status = DISCONNECTE;
    // Use this for initialization
    void Start () {
        ConnectBleService();

    }
    float scale = 5;
	// Update is called once per frame
	void Update () {

      // if (status == CONNECTE_SUCCESS)     此条件在lark1s 启用
        {
            float x = 0, z = 0;
            if (Input.GetKeyDown(KeyCode.UpArrow) )
            {
                z = scale * Time.deltaTime;  
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                z = -scale * Time.deltaTime;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                x = -scale * Time.deltaTime;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                x = scale * Time.deltaTime;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            this.transform.localPosition = this.transform.localPosition + new Vector3(x, 0, z);

       }

    }
    void ConnectBleService()
    {
        #if ANDROID_DEVICE
        try
        {
            UnityEngine.AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            javaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.link.VrLinkActivity");
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("ConnectBleService------------------------catch" + e.Message);
        }
       #endif
    }
    public void ConnectBleDevice()
    {
        Pvr_UnitySDKAPI.TouchPad.UPvr_StartBLEConnectService(this.gameObject.name);
    }
    public void StopBleDevice()
    {
        Pvr_UnitySDKAPI.TouchPad.UPvr_StopBLEConnectService();
    }
    public void BLEStatusCallback(string s)
    {
        switch (int.Parse(s))
        {
            case (SERVICE_STARTED):
                status = SERVICE_STARTED;
                Debug.Log("BLE_SERVICE_STARTED");
                break;
            case (CONNECTE_SUCCESS):
                status = CONNECTE_SUCCESS;
                Debug.Log("BLE_CONNECTE_SUCCESS");
                break;
            case (DISCONNECTE):
                status = DISCONNECTE;
                Debug.Log("BLE_DISCONNECTE");
                break;
            case (CONNECTE_FAILED):
                status = CONNECTE_FAILED;
                Debug.Log("BLE_CONNECTE_FAILED");
                break;
            case (NO_DEVICE):
                status = NO_DEVICE;
                Debug.Log("BLE_NO_DEVICE");
                break;
        }
    }
}
