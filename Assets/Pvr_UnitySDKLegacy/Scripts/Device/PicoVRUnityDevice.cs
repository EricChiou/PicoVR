using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class PicoVRUnityDevice : PicoVRBaseDevice
{
    #region Properties
    private float mouseX = 0;
    private float mouseY = 0;
    private float mouseZ = 0;
    private float neckModelScale = 0;
    private bool autoUntiltHead = false;
    private bool canConnecttoActivity = false;
    private static readonly Vector3 neckOffset = new Vector3(0, 0.075f, 0.08f);
    #endregion Properties

    public PicoVRUnityDevice()
    {
        Init();
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            try
            {
                Regex r = new Regex(@"(\d+\.\d+)\..*");
                string version = r.Replace(Application.unityVersion, "$1");
                if (new Version(version) >= new Version("4.5"))
                {
                    PlantformSupport = true;
                    return;
                }
            }
            catch
            {
                Debug.LogWarning("Unable to determine Unity version from: "
                       + Application.unityVersion);
            }
        }

        this.PlantformSupport = false;
    }

    #region Inheritance
    public override void Init()
    {
        this.rtorScren.Width = 0.110f;
        this.rtorScren.Height = 0.062f;
    }

    public override bool CanConnecttoActivity
    {
        get { return canConnecttoActivity; }
        set
        {
            if (value != canConnecttoActivity)
                canConnecttoActivity = value;
        }
    }

    public override Vector2 GetStereoScreenSize()
    {
        return new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
    }
    
    public  void SetNeckModelScale(float scale)
    {
        this.neckModelScale = scale;
    }

    public override void UpdateState()
    {
        UpdateSimulatedFrameParams();
    }

    private void UpdateSimulatedFrameParams()
    {
        bool rolled = false;
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            mouseX += Input.GetAxis("Mouse X") * 5;
            if (mouseX <= -180)
            {
                mouseX += 360;
            }
            else if (mouseX > 180)
            {
                mouseX -= 360;
            }
            mouseY -= Input.GetAxis("Mouse Y") * 2.4f;
            mouseY = Mathf.Clamp(mouseY, -90, 90);
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            rolled = true;
            mouseZ += Input.GetAxis("Mouse X") * 5;
            mouseZ = Mathf.Clamp(mouseZ, -90, 90);
        }
        if (!rolled && autoUntiltHead)
        {
            mouseZ = Mathf.Lerp(mouseZ, 0, Time.deltaTime / (Time.deltaTime + 0.1f));
        }
        var rot = Quaternion.Euler(mouseY, mouseX, mouseZ);
        var neck = (rot * neckOffset - neckOffset.y * Vector3.up) * neckModelScale;
        Matrix4x4 Matrix1 = Matrix4x4.TRS(neck, rot, Vector3.one);
        PicoVRManager.SDK.headPose = new PicoVRPose(Matrix1);

    }

    public override void UpdateScreenData()
    {
        ComputeEyesFromProfile();
    }

    public override void InitForEye(ref Material mat)
    {
        if (!SystemInfo.supportsRenderTextures)
        {
            return;
        }
        Shader shader = Shader.Find("PicovrSDK/Undistortion");
        if (shader == null)
        {
            return;
        }
        mat = new Material(shader);
    }

    public override void ResetHeadTrack()
    {
        if (PicoVRManager.SDK.reStartHead)
        {
            mouseX = mouseY = mouseZ = 0;
            PicoVRManager.SDK.reStartHead = false;
        }
    }
    public override void UpdateTextures() { }
    #endregion Inheritance
}
