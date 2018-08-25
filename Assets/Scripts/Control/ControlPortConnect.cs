using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPortConnect : MonoBehaviour, PortConnectManager.Listener, AlertMananger.Listener
{
    public Button btn_connect;
    public Dropdown dropdown_port;

    private SpriteState ss_connect, ss_disconnect;
    private Sprite sp_connect, sp_disconnect;

    void Awake()
    {
        PortConnectManager.GetInstance().AddListener(this);

        ss_connect = new SpriteState();
        ss_disconnect = new SpriteState();


        sp_connect = Resources.Load("Images/cancel", typeof(Sprite)) as Sprite;
        ss_connect.pressedSprite = Resources.Load("Images/cancel_hover", typeof(Sprite)) as Sprite;

        sp_disconnect = Resources.Load("Images/confirm", typeof(Sprite)) as Sprite;
        ss_disconnect.pressedSprite = Resources.Load("Images/confirm_hover", typeof(Sprite)) as Sprite;
    }

    void Start()
    {
        btn_connect.GetComponent<Button>().onClick.AddListener(onClick_connect);
        dropdown_port.onValueChanged.AddListener(valueChanged_dropDown);
    }

    void Update()
    {
        updateDropdown(PortConnectManager.GetAllPortNames());

        if (PortConnectManager.GetInstance().IsConnected())
        {
            btn_connect.spriteState = ss_connect;
            btn_connect.gameObject.GetComponent<Image>().sprite = sp_connect;
            btn_connect.GetComponentInChildren<Text>().text = "Disconnect";
        }
        else
        {
            btn_connect.spriteState = ss_disconnect;
            btn_connect.gameObject.GetComponent<Image>().sprite = sp_disconnect;
            btn_connect.GetComponentInChildren<Text>().text = "Connect";
        }
    }

    private int _loadingMsgId = 0;

    void onClick_connect()
    {
        if (PortConnectManager.GetInstance().IsConnected())
        {
            AlertMananger.GetInstance().ShowConfirmDialog("Disconnect", "Are you sure you want to disconnect? The printing will fail if you disconnect when printing.",this);
            return;
        }

        if (PortConnectManager.GetInstance().IsConnecting())
        {
            AlertMananger.GetInstance().ShowToast("Connecting, wait a minute.");
            return;
        }

        if (PortConnectManager.GetAllPortNames().Count == 0)
        {
            Debug.LogWarning("No port found" + "\n");
            AlertMananger.GetInstance().ShowToast("Please connect the printer to your computer using the provided USB cable.");
            return;
        }

        if (dropdown_port.value >= PortConnectManager.GetAllPortNames().Count)
        {
            AlertMananger.GetInstance().ShowToast("Selelct a port first.");
            return;
        }

        //try to connect
        _loadingMsgId = AlertMananger.GetLoadingMsgId();
        AlertMananger.GetInstance().ShowAlertLoading("Connecting...", _loadingMsgId);

        string portName = PortConnectManager.GetAllPortNames()[dropdown_port.value];
        PortConnectManager.GetInstance().Connect(portName);
    }

    private void updateDropdown(List<string> portNameList)
    {
        dropdown_port.options.Clear();
        Dropdown.OptionData tempData;
        for (int i = 0; i < portNameList.Count; i++)
        {
            tempData = new Dropdown.OptionData();
            //mac port name is too long, split by '.', make name shorter
            // /dev/tty.Makeblock-ELETSPP ---> Makeblock-ELETSPP
            string fullName = portNameList[i];
            string[] nameSplit1 = fullName.Split('/');
            string[] nameSplit2 = nameSplit1[nameSplit1.Length - 1].Split('.');
            tempData.text = nameSplit2[nameSplit2.Length - 1];
            dropdown_port.options.Add(tempData);
        }
        dropdown_port.RefreshShownValue();
    }

    void valueChanged_dropDown(int index)
    {
    }

    /*************** PortConnectManager.Listener call back ***************/
    public void OnPortConnecting(string portName)
    {
    }

    public void OnPortConnectStarted(string portName)
    {

    }

    public void OnPortConnected(string portName)
    {
        AlertMananger.GetInstance().DismissAlertLoading(_loadingMsgId);
        AlertMananger.GetInstance().ShowToast("Connected to " + "\"" + portName + "\"");
    }

    public void OnPortDisconnect(string portName)
    {
        AlertMananger.GetInstance().DismissAlertLoading(_loadingMsgId);
        AlertMananger.GetInstance().ShowToast("Disconnected from " + "\"" + portName + "\"" );
    }

    public void OnPortConnectTimeOut(string portName)
    {
        AlertMananger.GetInstance().DismissAlertLoading(_loadingMsgId);
        AlertMananger.GetInstance().ShowAlertMsg("Connection timeout, try again.");
    }

    public void OnPortInsert(string portName)
    {
        AlertMananger.GetInstance().ShowToast("Cable connected.");
    }

    public void OnPortPullOut(string portName)
    {
        AlertMananger.GetInstance().ShowToast("Cable disconnected.");
    }

    public void OnPortError(string portName, string errorMsg)
    {
        AlertMananger.GetInstance().ShowAlertMsg("Error occurs."+"\n" + errorMsg);
    }

    public void OnPortReceive(string data)
    {

    }

    public void OnPortOpenSucceed(string portName)
    {

    }

    public void OnPortOpenFailed(string portName, string reason)
    {
        AlertMananger.GetInstance().ShowAlertMsg("Failed to connect. The port may be used by another software.");
        AlertMananger.GetInstance().DismissAlertLoading(_loadingMsgId);
    }

    /*************** AlertMananger.Listener call back ***************/
    public void OnCancel()
    {
    }

    public void OnConfirm()
    {
        PortConnectManager.GetInstance().Disconnect();
    }
}
