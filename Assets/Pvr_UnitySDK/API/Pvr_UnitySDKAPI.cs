///////////////////////////////////////////////////////////////////////////////
// Copyright 2015-2017  Pico Technology Co., Ltd. All Rights 
// File: Pvr_UnitySDKAPI
// Author: AiLi.Shang
// Date:  2017/01/11
// Discription: The API Core funcation.Be fully careful of  Code modification
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

    public enum GlobalIntConfigs
    {

        EYE_TEXTURE_RESOLUTION0,
        EYE_TEXTURE_RESOLUTION1,
        SEENSOR_COUNT,
        ABILITY6DOF,
        PLATFORM_TYPE,
    };

    public enum GlobalFloatConfigs
    {
        IPD,
        FOV,
    };
    public enum RenderTextureAntiAliasing
    {
        X_1 = 1,
        X_2 = 2,
        X_4 = 4,
        X_8 = 8,
    }
    public enum PlatForm
    {
        Android = 1,
        IOS = 2,
        Win = 3,
        Notsupport = 4,
    }


    public enum RenderTextureDepth
    {
        BD_0 = 0,
        BD_16 = 16,
        BD_24 = 24,
    }
    public enum Sensorindex
    {
        Default = 0,
        FirstSensor = 1,
        SecondSensor = 2,
    }


    public enum Eye
    {
        LeftEye,
        RightEye
    }

    public enum DeviceMode
    {
        PicoNeoDK,
        PicoNeoDKS,
        PicoNeoCV,
        Goblin,
        Other,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EyeSetting
    {
        public Transform eyelocalPosition;
        public Rect eyeRect;
        public float eyeFov;
        public float eyeAspect;
        public Matrix4x4 eyeProjectionMatrix;
        public Shader eyeShader;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sensor
    {
        #region Android
#if ANDROID_DEVICE
        //---------------------------------------so------------------------------------------------
        public const string LibFileName = "Pvr_UnitySDK";      
              
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_Enable6DofModule(bool enable);
        //---------------------------------------so------------------------------------------------
#endif
        #endregion

        #region IOS
#if IOS_DEVICE
        //---------------------------------------so------------------------------------------------
		public const string LibFileName = "__Internal";
		//---------------------------------------so------------------------------------------------
#endif
        #endregion

        #region UNITY_EDITOR
#if UNITY_EDITOR
        public const string LibFileName = "Pvr_UnitySDK";
#endif
        #endregion


        #region DllFuncation

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_Init(int index);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_StartSensor(int index);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_StopSensor(int index);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_ResetSensor(int index);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_OptionalResetSensor(int index, int resetRot, int resetPos);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetSensorState(int index, ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetMainSensorState(ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz, ref float fov, ref int viewNumber);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetPsensorState();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetSensorAcceleration(int index, ref float x, ref float y, ref float z);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetSensorGyroscope(int index, ref float x, ref float y, ref float z);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetSensorMagnet(int index, ref float x, ref float y, ref float z);

        #endregion

        #region Public Static Funcation
        public static int UPvr_Init(int index)
        {
#if ANDROID_DEVICE
            return Pvr_Init(index);
#endif
            return 0;
        }
        public static int UPvr_GetPsensorState()
        {
#if ANDROID_DEVICE
            return Pvr_GetPsensorState();
#endif
            return 0;
        }
        public static int UPvr_StartSensor(int index)
        {
#if ANDROID_DEVICE
            return Pvr_StartSensor(index);
#endif
            return 0;
        }
        public static int UPvr_StopSensor(int index)
        {
#if ANDROID_DEVICE
            return Pvr_StopSensor(index);
#endif
            return 0;
        }
        public static int UPvr_ResetSensor(int index)
        {
#if ANDROID_DEVICE
            if(Pvr_UnitySDKManager.SDK.Enable6Dof)
            {
                return Pvr_OptionalResetSensor(index, Pvr_UnitySDKManager.SDK.resetRot, Pvr_UnitySDKManager.SDK.resetPos);
            }
            else
            {
                return Pvr_ResetSensor(index);
            }
#endif
            return 0;
        }
        public static int UPvr_GetSensorState(int index, ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz)
        {
#if ANDROID_DEVICE
            return Pvr_GetSensorState(index, ref x, ref y, ref z, ref w, ref px, ref py, ref pz);
#endif
            return 0;
        }
        public static int UPvr_GetMainSensorState(ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz, ref float fov, ref int viewNumber)
        {
#if ANDROID_DEVICE
            return Pvr_GetMainSensorState(ref x, ref y, ref z, ref w, ref px, ref py, ref pz, ref fov, ref viewNumber);
#endif
            return 0;
        }

        public static int UPvr_GetSensorAcceleration(int index, ref float x, ref float y, ref float z)
        {
#if ANDROID_DEVICE
            return Pvr_GetSensorAcceleration(index, ref x, ref y, ref z);
#endif
            return 0;
        }

        public static int UPvr_GetSensorGyroscope(int index, ref float x, ref float y, ref float z)
        {
#if ANDROID_DEVICE
            return Pvr_GetSensorGyroscope(index, ref x, ref y, ref z);
#endif
            return 0;
        }

        public static int UPvr_GetSensorMagnet(int index, ref float x, ref float y, ref float z)
        {
#if ANDROID_DEVICE
            return Pvr_GetSensorMagnet(index, ref x, ref y, ref z);
#endif
            return 0;
        }
        public static int UPvr_Enable6DofModule(bool enable)
        {
#if ANDROID_DEVICE  
            return    Pvr_Enable6DofModule(enable);
#endif
            return 0;
        }

        #endregion

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Render
    {
        #region Android
#if ANDROID_DEVICE
        public const string LibFileName = "Pvr_UnitySDK";
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Pvr_ChangeScreenParameters(string model, int width, int height, double xppi, double yppi, double densityDpi );
		[DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int Pvr_SetRatio(float midH, float midV);

#endif
        #endregion

        #region IOS
#if IOS_DEVICE
		public const string LibFileName = "__Internal";
		[DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void UnityRenderEventIOS(int eventType,int eventData);

		[DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int Pvr_SetRatioIOS(float midH, float midV);

#endif
        #endregion

        #region UNITY_EDITOR
#if UNITY_EDITOR
        public const string LibFileName = "Pvr_UnitySDK";
#endif
        #endregion

        #region DllFuncation
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_SetPupillaryPoint(bool enable);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Pvr_GetSupportHMDTypes();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetCurrentHMDType([MarshalAs(UnmanagedType.LPStr)]string type);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetIntConfig(int configsenum, ref int res);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetFloatConfig(int configsenum, ref float res);



        #endregion

        #region Public Static Funcation
#if ANDROID_DEVICE
		public static void UPvr_ChangeScreenParameters(string model, int width, int height, double xppi, double yppi, double densityDpi)
		{
			Pvr_ChangeScreenParameters(model,  width,  height,  xppi,  yppi, densityDpi );
		}
#endif
        public static int UPvr_SetRatio(float midH, float midV)
        {
#if ANDROID_DEVICE
            return Pvr_SetRatio(midH, midV);
#endif
#if IOS_DEVICE
			return Pvr_SetRatioIOS(midH, midV);
#endif
            return 0;
        }

        public static int UPvr_GetIntConfig(int configsenum, ref int res)
        {
#if ANDROID_DEVICE
            return Pvr_GetIntConfig(configsenum, ref res);
#endif
            return 0;
        }

        public static int UPvr_GetFloatConfig(int configsenum, ref float res)
        {
#if ANDROID_DEVICE
            return Pvr_GetFloatConfig(configsenum, ref res);
#endif
            return 0;
        }
        public static string UPvr_GetSupportHMDTypes()
        {
#if ANDROID_DEVICE
            IntPtr ptr = Pvr_GetSupportHMDTypes();
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
#endif
            return null;

        }
        public static void UPvr_SetCurrentHMDType(string type)
        {
#if ANDROID_DEVICE
            Pvr_SetCurrentHMDType(type);
#endif

        }
        #endregion

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct System
    {
        const string UnitySDKVersion = "V2.1.0.0";

        #region Android

#if ANDROID_DEVICE
         //---------------------------------------so------------------------------------------------
        public const string LibFileName = "Pvr_UnitySDK";
		
		[DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Pvr_SetInitActivity(IntPtr activity, IntPtr vrActivityClass);
       
        /// <summary>
        /// 调用静态方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="jclass"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool UPvr_CallStaticMethod<T>(ref T result, UnityEngine.AndroidJavaClass jclass, string name, params object[] args)
        {
            try
            {
                result = jclass.CallStatic<T>(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception calling static method " + name + ": " + e);
                return false;
            }
        }
        public  static bool UPvr_CallStaticMethod(UnityEngine.AndroidJavaObject jobj, string name, params object[] args)
        {
            try
            {
                jobj.CallStatic(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("CallStaticMethod  Exception calling activity method " + name + ": " + e);
                return false;
            }
        }

        /// <summary>
        ///调用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="jobj"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public  static bool UPvr_CallMethod<T>(ref T result, UnityEngine.AndroidJavaObject jobj, string name, params object[] args)
        {
            try
            {
                result = jobj.Call<T>(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception calling activity method " + name + ": " + e);
                return false;
            }
        }
        /// <summary>
        /// 调用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="jobj"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public  static bool UPvr_CallMethod(UnityEngine.AndroidJavaObject jobj, string name, params object[] args)
        {
            try
            {
                jobj.Call(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError(" Exception calling activity method " + name + ": " + e);
                return false;
            }
        }  
#endif
        #endregion

        #region IOS
#if IOS_DEVICE
         //---------------------------------------so------------------------------------------------
		public const string LibFileName = "__Internal";
		
#endif
        #endregion

        #region UNITY_EDITOR
#if UNITY_EDITOR
        public const string LibFileName = "Pvr_UnitySDK";
#endif
        #endregion

        #region DllFuncation
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Pvr_GetSDKVersion();
        #endregion

        #region Public Static Funcation
        public static string UPvr_GetSDKVersion()
        {
#if ANDROID_DEVICE
            IntPtr ptr = Pvr_GetSDKVersion();
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
#endif
            return null;
        }

        public static string UPvr_GetUnitySDKVersion()
        {
            return UnitySDKVersion;

        }
        public static DeviceMode UPvr_GetDeviceMode()
        {
#if ANDROID_DEVICE
            string devicemode = SystemInfo.deviceModel;
            if (devicemode.Contains("DKS"))
            {
                return DeviceMode.PicoNeoDKS;
            }
            else if (devicemode.Contains("DK"))
            {
                return DeviceMode.PicoNeoDK;
            }
            else if (devicemode.Contains("Goblin"))
            {
                return DeviceMode.Goblin;
            }
            else if (devicemode.Contains("CV"))
            {
                return DeviceMode.PicoNeoCV;
            }
            else
            {
                return DeviceMode.Other;
            }
#endif
            return DeviceMode.Other; ;
        }
        public static string Upvr_GetDeviceSN()
        {
            string serialNum = "UNKONWN";
#if ANDROID_DEVICE
            System.UPvr_CallStaticMethod<string>(ref serialNum, Pvr_UnitySDKRender.javaSysActivityClass, "getDeviceSN");
#endif
            return serialNum;
        }
        public static bool UPvr_StartHomeKeyReceiver(string startreceivre)
        {


#if ANDROID_DEVICE
            try
            {
                if (Pvr_UnitySDKManager.pvr_UnitySDKRender !=null)
                {
					Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaVrActivityLongReceiver, "Pvr_StartReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity, startreceivre);
                    Debug.Log("Start home key   Receiver");
                    return true;
                }
              
            }
            catch (Exception e)
            {
                Debug.LogError("Start home key  Receiver  Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }

#endregion


    }

}