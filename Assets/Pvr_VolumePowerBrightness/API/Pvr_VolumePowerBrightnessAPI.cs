﻿///////////////////////////////////////////////////////////////////////////////
// Copyright 2015-2017  Pico Technology Co., Ltd. All Rights Reserved.
// File: Pvr_VolumePowerBrightnessAPI
// Author: AiLi.Shang
// Date:  2017/03/22
// Discription: The API  funcation
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
    public enum DeviceCommand
    {
        SET_PICO_NEO_HMD_BRIGHTNESS = 12,//操作屏幕亮度，包括设置以及获取，需要PicoNeo头戴固件版B55之上
        SET_PICO_NEO_HMD_SLEEPDELAY = 13//操作熄屏时间，包括设置以及获取，多长时间后休眠，时间单位为秒
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct VolumePowerBrightness
    {
        #region Android
        public const string LibFileName = "Pvr_UnitySDK";
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pvr_SetInitActivity(IntPtr activity, IntPtr vrActivityClass);
#if ANDROID_DEVICE
            /*****************************音量亮度*************************************/
        public AndroidJavaObject activity;
        public static AndroidJavaClass javaSysActivityClass;         
        private static UnityEngine.AndroidJavaClass batteryjavaVrActivityClass;     
        private static UnityEngine.AndroidJavaClass volumejavaVrActivityClass;
#endif
        #endregion

        #region Public Static Funcation
        public static bool UPvr_IsHmdExist()
        {
            return Pvr_IsHmdExist();
        }
        public static int UPvr_GetHmdScreenBrightness()
        {
            return Pvr_GetHmdScreenBrightness();
        }

        public static bool UPvr_SetHmdScreenBrightness(int brightness)
        {
            return Pvr_SetHmdScreenBrightness(brightness);
        }
        public static bool UPvr_SetCommonBrightness(int brightness)
        {
            bool enable = false;
            if (Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_IsHmdExist())
            {
                enable = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetHmdScreenBrightness(brightness);
            }
            else
            {
                enable = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_SetBrightness(brightness);
            }
            return enable;
        }

        public static int UPvr_GetCommonBrightness()
        {
            int lightness = 0;
            if (Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_IsHmdExist())
            {
                lightness = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetHmdScreenBrightness();
            }
            else
            {
                lightness = Pvr_UnitySDKAPI.VolumePowerBrightness.UPvr_GetCurrentBrightness();
            }
            return lightness;
        }

        public static bool UPvr_SetDevicePropForUser(DeviceCommand deviceid, string number)
        {
            return setDevicePropForUser(deviceid, number); ;
        }
        public static string UPvr_GetDevicePropForUser(DeviceCommand deviceid)
        {
            return getDevicePropForUser(deviceid);
        }
        public static bool UPvr_InitBatteryVolClass()
        {
#if ANDROID_DEVICE
            try
            {
               
                javaSysActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.SysActivity");
                if (javaSysActivityClass != null &&Pvr_UnitySDKManager.pvr_UnitySDKRender.activity != null)
                {
                   
                    batteryjavaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.BatteryReceiver");
                    volumejavaVrActivityClass = new AndroidJavaClass("com.psmart.aosoperation.AudioReceiver");
               
                    Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_InitAudioDevice", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity); 
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
#endif
            return true;
        }
        public static bool UPvr_StartBatteryReceiver(string startreceivre)
        {
#if ANDROID_DEVICE
            try
            {
               // string startreceivre = PicoVRManager.SDK.gameObject.name;
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "Pvr_StartReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity, startreceivre);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }
        public static bool UPvr_StopBatteryReceiver()
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "stopReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }
        public static bool UPvr_SetBrightness(int brightness)
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaSysActivityClass, "Pvr_SetScreen_Brightness", brightness, Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(" Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }
        public static int UPvr_GetCurrentBrightness()
        {
#if ANDROID_DEVICE
            int currentlight = 0;
            try
            {
              //  Debug.Log("johnson UPvr_GetCurrentBrightness");
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref currentlight, javaSysActivityClass, "Pvr_GetScreen_Brightness", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
            }
            catch (Exception e)
            {
                Debug.LogError(" Error :" + e.ToString());
            }
            return currentlight;
#endif
            return 0;
        }
        public static bool UPvr_StartAudioReceiver(string startreceivre)
        {
#if ANDROID_DEVICE
            try
            {
               // string startreceivre = PicoVRManager.SDK.gameObject.name;
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(volumejavaVrActivityClass, "Pvr_StartReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity, startreceivre);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }
        public static bool UPvr_StopAudioReceiver()
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(volumejavaVrActivityClass, "Pvr_StopReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("startReceiver Error :" + e.ToString());
                return false;
            }

#endif
            return true;
        }
        public static int UPvr_GetMaxVolumeNumber()
        {
#if ANDROID_DEVICE
            int maxvolm = 0;
            try
            {  
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref maxvolm, javaSysActivityClass, "Pvr_GetMaxAudionumber");
               // Debug.Log("johnson UPvr_GetMaxVolumeNumber = "+maxvolm);
            }
            catch (Exception e)
            {
                Debug.LogError(" Error :" + e.ToString());
            }
            return maxvolm;
#endif
            return 0;
        }
        public static int UPvr_GetCurrentVolumeNumber()
        {
#if ANDROID_DEVICE
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
#endif
            return 0;
        }
        public static bool UPvr_VolumeUp()
        {
#if ANDROID_DEVICE
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
#endif
            return true;
        }
        public static bool UPvr_VolumeDown()
        {
#if ANDROID_DEVICE
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
#endif
            return true;
        }
        public static bool UPvr_SetVolumeNum(int volume)
        {
#if ANDROID_DEVICE
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
#endif
            return true;
        }
        public static bool UPvr_SetAudio(string s)
        {
            return false;
        }          
        public static bool UPvr_SetBattery(string s)
        {
            return false;
        }
        //jar 调用 unity
                             
      
#endregion

        #region DllFuncation

        //Brightness
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_IsHmdExist();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetHmdScreenBrightness();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_SetHmdScreenBrightness(int brightness);

        #endregion

        #region Pravite Static Funcation
        private static string getDevicePropForUser(DeviceCommand deviceid)
        {
            string istrue = "0";
#if  ANDROID_DEVICE
              Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref istrue, Pvr_UnitySDKRender.javaVrActivityClass, "getDevicePropForUser", (int)deviceid);
#endif
            return istrue;
        }
        private static bool setDevicePropForUser(DeviceCommand deviceid, string number)
        {
            bool istrue = false;
#if  ANDROID_DEVICE
             Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref istrue, Pvr_UnitySDKRender.javaVrActivityClass, "setDevicePropForUser", (int)deviceid, number);
#endif
            return istrue;
        } //jar 调用 unity
        #endregion
    }

}