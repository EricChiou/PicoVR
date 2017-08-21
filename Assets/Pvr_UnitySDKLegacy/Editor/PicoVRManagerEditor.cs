using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PicoVRManager))]
public class PicoVRManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        //GUI style 设置
        GUIStyle firstLevelStyle = new GUIStyle(GUI.skin.label);
        firstLevelStyle.alignment = TextAnchor.UpperLeft;
        firstLevelStyle.fontStyle = FontStyle.Bold;
        firstLevelStyle.fontSize = 12;
        firstLevelStyle.wordWrap = true;

        //inspector 所在 target 
        PicoVRManager manager = (PicoVRManager)target;

        //一级分层标题 1
        GUILayout.Space(10);
        EditorGUILayout.LabelField("ConfigFile Setting", firstLevelStyle);
        GUILayout.Space(10);

        manager.DeviceType = (PicoVRConfigProfile.DeviceTypes)EditorGUILayout.EnumPopup("Choose The Glass   ", manager.DeviceType); 

        //一级分层标题 2
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Render Texture Setting", firstLevelStyle);
        GUILayout.Space(10);

        manager.RtAntiAlising = (PicoVRBaseDevice.RenderTextureAntiAliasing)EditorGUILayout.EnumPopup("Render Texture Anti-Aliasing ", manager.RtAntiAlising);
        manager.RtBitDepth = (PicoVRBaseDevice.RenderTextureDepth)EditorGUILayout.EnumPopup("Render Texture Bit Depth   ", manager.RtBitDepth);
        manager.RtFormat = (RenderTextureFormat)EditorGUILayout.EnumPopup("Render Texture Format", manager.RtFormat);
        

        //一级分层标题 1
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Pico Neo Controller Sensor", firstLevelStyle);
        GUILayout.Space(10);
        manager.UseFalconBoxSensor = EditorGUILayout.Toggle("Enable Pico Neo Controller Sensor", manager.UseFalconBoxSensor);
        GUILayout.Space(10);

        //一级分层标题 1
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Show FPS", firstLevelStyle);
        GUILayout.Space(10);
        manager.ShowFPS = EditorGUILayout.Toggle("Show FPS in Scene", manager.ShowFPS);
        GUILayout.Space(10);


        //FalconCV 6DOF
        GUILayout.Space(10);
        EditorGUILayout.LabelField("FalconCV 6DOF", firstLevelStyle);
        GUILayout.Space(10);
        manager.IsFalconCV6DOFEnable = EditorGUILayout.Toggle("Enable Falcon CV Position Tracking ", manager.IsFalconCV6DOFEnable);
        GUILayout.Space(10);




        if (GUI.changed)
        {
            EditorUtility.SetDirty(manager);
        }

    }
    
}
