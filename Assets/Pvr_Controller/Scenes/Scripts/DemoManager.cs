using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using Pvr_UnitySDKAPI;

public class DemoManager : MonoBehaviour {

    public Transform m_point;
    public Ray ray;
    public Transform direction;
    public Transform m_dot;

    private SystemLanguage localLanguage;
    private float scanTime = 15;  //搜索手柄倒计时
    public GameObject checkPanel;
    public Text toast;

    private bool needStopScan = true;
    private bool isSendConnectToJar = false;
    private bool isRealPhone = false;
    // Use this for initialization
    void Start () {

        int platformType = -1;
        int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.PLATFORM_TYPE;
        Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref platformType);
        if (platformType == 0)
        {
            isRealPhone = true;
        }

        ray = new Ray();
        ray.origin = transform.position;

        localLanguage = Application.systemLanguage;
	    if(Pvr_ControllerManager.controllerlink != null)
        {
            if(!Pvr_ControllerManager.controllerlink.notPhone && Pvr_ControllerManager.controllerlink.isAutoConnect && isRealPhone )
            {
                StartCoroutine(CheckIsConnected());
                Invoke("StartScan", 1.0f);
                if(toast != null)
                {
                    toast.gameObject.SetActive(true);   
                }
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        ray.direction = direction.position - transform.position;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 200))
        {
            m_point.gameObject.SetActive(true);
            m_point.transform.position = hit.point + new Vector3(0, 0, -0.1f);
            m_point.DOKill();
            m_point.DOScale(0.025f, 0.5f);
            m_dot.gameObject.SetActive(false);

        }
        else
        {
            m_point.DOScale(0.0f, 0.2f);
            m_point.gameObject.SetActive(false);
            m_dot.gameObject.SetActive(true);
        }


        if(!Pvr_ControllerManager.controllerlink.notPhone && Pvr_ControllerManager.controllerlink.isAutoConnect && isRealPhone)
        {
            if(Controller.UPvr_GetControllerState() == ControllerState.DisConnected && !isSendConnectToJar && Pvr_ControllerManager.controllerlink.hummingBirdMac != "" )
            {
                Pvr_ControllerManager.controllerlink.ConnectBLE();
                isSendConnectToJar = true;
            }

            if(Controller.UPvr_GetControllerState() == ControllerState.Connected)
            {
                if(checkPanel.activeSelf == true && checkPanel != null)
                {
                    checkPanel.SetActive(false);
                }
                if(needStopScan)
                {
                    if(toast != null)
                    {
                        if(localLanguage == SystemLanguage.ChineseSimplified || localLanguage == SystemLanguage.Chinese)
                        {
                            toast.text = "连接成功！";
                        }
                        else
                        {
                            toast.text = "Connection Success!";
                        }
                        Invoke("HideToast", 1.5f);
                        Pvr_ControllerManager.controllerlink.hummingBirdMac = "";
                        Pvr_ControllerManager.controllerlink.StopScan();
                        needStopScan = false;
                    }
                }
            }
        }
    }

    void OnApplicationPause(bool ispause)
    {
        if (Pvr_ControllerManager.controllerlink.notPhone)
        {
            if (ispause)
            {
                Pvr_ControllerManager.controllerlink.StopLark2Service();
            }
            else
            {
                Pvr_ControllerManager.controllerlink.StartLark2Service();
            }
        }
        else
        {
            if (ispause)
            {
                Pvr_ControllerManager.controllerlink.DisConnectBLE();
                Pvr_ControllerManager.controllerlink.StopLark2Receiver();
            }
            else
            {
                Pvr_ControllerManager.controllerlink.StartLark2Receiver();
                StartScan();
            }
        }
    }

    public void StartScan()
    {
        needStopScan = true;
        isSendConnectToJar = false;
        if(toast != null && !Pvr_ControllerManager.controllerlink.notPhone && Controller.UPvr_GetControllerState() == ControllerState.DisConnected && isRealPhone)
        {
            toast.gameObject.SetActive(true);
            if (localLanguage == SystemLanguage.ChineseSimplified || localLanguage == SystemLanguage.Chinese)
            {
                toast.text = "正在搜索Pico Controller...";
            }
            else
            {
                toast.text = "Scanning Pico Controller...";
            }
        }
        Pvr_ControllerManager.controllerlink.StartScan();
    }
    public void StartScanAndClosePanel()
    {

        needStopScan = true;
        isSendConnectToJar = false;
        Pvr_ControllerManager.controllerlink.StartScan();
        StartCoroutine(CheckIsConnected());

        if (checkPanel.activeSelf && checkPanel != null)
        {
            checkPanel.SetActive(false);
        }
    }
    public void StopScanAndClosePanel()
    {
        if (checkPanel.activeSelf && checkPanel != null)
        {
            checkPanel.SetActive(false);
        }
    }

    private void HideToast()
    {
        if (toast.gameObject.activeSelf == true && toast != null)
        {
            toast.gameObject.SetActive(false);
        }
    }
    IEnumerator CheckIsConnected()
    {
        yield return new WaitForSeconds(scanTime);
        if(Controller.UPvr_GetControllerState() != ControllerState.Connected)
        {
            Pvr_ControllerManager.controllerlink.StopScan();
            if (checkPanel != null)
            {
                checkPanel.SetActive(true);

                var text = checkPanel.transform.FindChild("Panel/Text").gameObject.GetComponent<Text>();
                if (localLanguage == SystemLanguage.ChineseSimplified || localLanguage == SystemLanguage.Chinese)
                {
                    text.text = "自动搜索Pico控制器超时，是否重新搜索？";
                }
                else
                {
                    text.text = "Automatic search for Pico controller timeout. Do you want to Scan again?";
                }
            }
        }
        
    }
}
