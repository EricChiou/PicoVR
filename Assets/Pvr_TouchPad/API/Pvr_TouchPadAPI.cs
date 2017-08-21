///////////////////////////////////////////////////////////////////////////////
// Copyright 2015-2017  Pico Technology Co., Ltd. All Rights 
// File: Pvr_HapticsAPI
// Author: AiLi.Shang
// Date:  2017/03/22
// Discription: The Haptics API 
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
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace Pvr_UnitySDKAPI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TouchPad
    {
       
        /**************************** Private Static Funcations *******************************************/
        #region Private Static Funcation
        private static void startBLEConnectService(string name)
        {
            Debug.LogError("shine startLarkConnectService ");
#if ANDROID_DEVICE
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_TouchPad.javaVrActivityClass, "Pvr_StartLarkConnectService", Pvr_TouchPad.activity, name);
#endif
        }

        private static void stopBLEConnectService()
        {
            Debug.LogError("shine stopLarkConnectService ");
#if ANDROID_DEVICE
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_TouchPad.javaVrActivityClass, "Pvr_StopLarkConnectService", Pvr_TouchPad.activity);
#endif
        }

        #endregion

        /**************************** Public Static Funcations *******************************************/
        #region Public Static Funcation  
        //  public static void  UPvr_

        public static void UPvr_StartBLEConnectService(string name)
        {
            startBLEConnectService(name);
        }

        public static void UPvr_StopBLEConnectService()
        {
            stopBLEConnectService();
        }
#endregion

    }

}