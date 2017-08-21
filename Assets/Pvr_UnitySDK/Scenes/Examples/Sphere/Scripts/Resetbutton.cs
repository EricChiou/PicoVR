using UnityEngine;
using System.Collections;

public class Resetbutton : MonoBehaviour {

    public void DemoResetTracking()
    {
        if (Pvr_UnitySDKManager.SDK!= null)
        {
          //  Pvr_UnitySDKManager.SDK.pvr_UnitySDKSensor.ResetUnitySDKSensor();
            Pvr_UnitySDKManager.pvr_UnitySDKSensor.ResetUnitySDKSensor();
        }

    }   

}
