using UnityEngine;
using UnityEditor;
[InitializeOnLoad]


public class Startup
{
    static Startup()
    {          
        QualitySettings.vSyncCount = 0;
        int  currentLevel = QualitySettings.GetQualityLevel();      
        for (int i = currentLevel; i >= 1; i--)
        {
            QualitySettings.DecreaseLevel(true);
            QualitySettings.vSyncCount = 0;   
        }
        QualitySettings.SetQualityLevel(currentLevel, true);    
        for (int i = currentLevel; i < 10; i++)
        {
            QualitySettings.IncreaseLevel(true);
            QualitySettings.vSyncCount = 0;               
        }         
    }
}

[InitializeOnLoad]
public class AndriodBuilSetting : Editor
{

    // Use this for initialization
    void Start()
    {
        PlayerSettings.MTRendering = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.companyName = "Pico";
        //PlayerSettings.gpuSkinning = true;
        PlayerSettings.mobileMTRendering = false;
        PlayerSettings.productName = "PicoVRSDK";
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;//Disabled;
        PlayerSettings.defaultIsFullScreen = true;
        EditorUserBuildSettings.activeBuildTargetChanged += OnChangePlatform;
        SetvSyncCount();

    }

    // Update is called once per frame
    static AndriodBuilSetting()
    {
        EditorUserBuildSettings.activeBuildTargetChanged += OnChangePlatform;
    }
    static void OnChangePlatform()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            PlayerSettings.MTRendering = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.companyName = "Pico";
            //PlayerSettings.gpuSkinning = true;
            PlayerSettings.mobileMTRendering = false;
            PlayerSettings.productName = "PicoVRSDK";
            SetvSyncCount(); 

        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
        {           
            PlayerSettings.companyName = "Pico";
            PlayerSettings.productName = "PicoVRSDK";
            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;//Disabled;
            PlayerSettings.defaultIsFullScreen = true;
            //EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;
            SetvSyncCount();
        }


    }

    [MenuItem("PicoVR/APK Setting")]
    static void PerformAndroidAPKBuild()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
        PlayerSettings.MTRendering = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.companyName = "Pico";
        // PlayerSettings.gpuSkinning = true;
        PlayerSettings.mobileMTRendering = false;
        PlayerSettings.productName = "PicoVRSDK";
        QualitySettings.vSyncCount = 0;
    }
    [MenuItem("PicoVR/WinPC Setting")]
    static void PerformPCSDKBuild()
    {
        PlayerSettings.companyName = "Pico";
        PlayerSettings.productName = "PicoVRSDK";
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.defaultIsFullScreen = false;
        // EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;
        QualitySettings.vSyncCount = 0;

    }
    static void SetvSyncCount()
    {
        QualitySettings.vSyncCount = 0;
        int currentLevel = QualitySettings.GetQualityLevel();
        for (int i = currentLevel; i >= 1; i--)
        {
            QualitySettings.DecreaseLevel(true);
            QualitySettings.vSyncCount = 0;
        }
        QualitySettings.SetQualityLevel(currentLevel, true);
        for (int i = currentLevel; i < 10; i++)
        {
            QualitySettings.IncreaseLevel(true);
            QualitySettings.vSyncCount = 0;
        }
    }
} 