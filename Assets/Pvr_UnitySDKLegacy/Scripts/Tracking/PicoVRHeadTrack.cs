using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;


/// <summary>
/// 头部跟踪
/// </summary>
public class PicoVRHeadTrack : MonoBehaviour
{
    /// <summary>
    /// 是否跟踪Rotation 
    /// </summary>
    public bool trackRotation = true;

    /// <summary>
    /// 是否跟踪Position
    /// </summary>
    public bool trackPosition = true;

    public Transform target;  // 设定位置

    public bool updateEarly = false;   //在哪处更新 Update  或者LateUpdate

    private bool updated = false;  // 是否更新完成标志

    private Vector3 startPosition;

    private Quaternion startQuaternion;
    static Quaternion absoluteRotation = Quaternion.identity;
    static Quaternion screebRotation = Quaternion.identity;
    static Quaternion centerOfScreenRotation = Quaternion.identity;
   // private Quaternion centerOfScreenRotation = Quaternion.identity;
    public void Awake()
    {
        if (target == null)
        {
            startPosition = transform.localPosition;
            startQuaternion = transform.localRotation;
        }
        else
        {
            startPosition = transform.position;
            startQuaternion = transform.rotation;
        }

    }

    public Ray Gaze
    {
        get
        {
            UpdateHead();
            return new Ray(transform.position, transform.forward);
        }
    }

    void Update()
    {
        updated = false;  // OK to recompute head pose.
        if (updateEarly)
        {
            UpdateHead();
        }
    }

    void LateUpdate()
    {
        if (!updateEarly)
            UpdateHead();
        if (Input.GetKeyUp(KeyCode.H) )
        {
            SetScreenHeading(ref centerOfScreenRotation, absoluteRotation);
        }
    }
    public static void SetScreenHeading(ref Quaternion screenRotation, Quaternion rotationAbsolute)
    {
        Debug.LogError("Setting Screen Heading");
        screenRotation = Quaternion.Inverse(rotationAbsolute);
    }
    private void UpdateHead()
    {
        if (updated)
        {
           return;
        }
        updated = true;
        if (PicoVRManager.SDK ==null)
        {
            return;
        }
        if (trackRotation)
        {
            absoluteRotation = PicoVRManager.SDK.headPose.Orientation;
            screebRotation = centerOfScreenRotation * absoluteRotation;
            if (target == null)
            {
                transform.localRotation = screebRotation;
            }
            else
            {
                transform.rotation = screebRotation * target.rotation;
            }
        }

        else
        {
            var rot = PicoVRManager.SDK.headPose.Orientation;
            if (target == null)
            {
                transform.localRotation = centerOfScreenRotation;
            }
            else
            {
                transform.rotation = rot * target.rotation;
            }
        }
        if (trackPosition)
        {
            Vector3 pos = PicoVRManager.SDK.headPose.Position;
            if (target == null)
            {
                transform.localPosition = pos;
            }
            else
            {
                transform.position = target.position + target.rotation * pos;
            }
        }
    }
}
