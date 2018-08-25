using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class AlertMananger
{
    private const string _panelName_alert = "panel_alert";
    private const string _panelName_confirmDialog = "panel_confirmDialog";
    private const string _panelName_toast = "panel_toast";

    /*************** single ***************/
    private static AlertMananger _INSTANCE;

    private AlertMananger()
    {
    }

    public static AlertMananger GetInstance()
    {
        if (_INSTANCE == null)
        {
            _INSTANCE = new AlertMananger();
        }
        return _INSTANCE;
    }

    /*************** listener ***************/
    public interface Listener
    {
        void OnCancel();

        void OnConfirm();
    }

    /*************** Alert Msg ***************/
    public void ShowAlertMsg(string msg)
    {
        Loom.RunAsync(
            () =>
            {
                Thread t = new Thread(new ParameterizedThreadStart(doShowAlertMsg));
                t.Start(msg);
            }
        );
    }

    private void doShowAlertMsg(object obj)
    {
        string msg = (string)obj;
        Loom.QueueOnMainThread((param) =>
        {
            activeAlertPanel();
            GameObject.Find(_panelName_alert).GetComponent<ControlAlertPanel>().ShowAlertMsg(msg);
        }, null);
    }

    public void DismissAllAlertMsg()
    {
        Loom.RunAsync(
            () =>
            {
                Thread t = new Thread(doDismissAllAlertMsg);
                t.Start();
            }
        );
    }

    private void doDismissAllAlertMsg()
    {
        Loom.QueueOnMainThread((param) =>
        {
            activeAlertPanel();
            GameObject.Find(_panelName_alert).GetComponent<ControlAlertPanel>().RemoveAllAlertMsg();
        }, null);
    }

    /*************** Alert Loading ***************/
    public void ShowAlertLoading(string msg, int id)
    {
        Loom.RunAsync(
            () =>
            {
                Thread t = new Thread(new ParameterizedThreadStart(doShowAlertLoading));
                List<System.Object> paramsList = new List<System.Object>();
                paramsList.Add((System.Object)msg);
                paramsList.Add((System.Object)id);
                t.Start(paramsList);
            }
        );
    }

    private void doShowAlertLoading(object obj)
    {
        List<System.Object> paramsList = (List<System.Object>)obj;
        string msg = (string)paramsList[0];
        int id = (int)paramsList[1];

        Loom.QueueOnMainThread((param) =>
        {
            activeAlertPanel();
            GameObject.Find(_panelName_alert).GetComponent<ControlAlertPanel>().ShowLoadingtMsg(msg, id);
        }, null);
    }

    public void DismissAlertLoading(int id)
    {
        Loom.RunAsync(
            () =>
            {
                Thread t = new Thread(new ParameterizedThreadStart(doDismissAlertLoading));
                t.Start(id);
            }
        );
    }

    private void doDismissAlertLoading(object obj)
    {
        int id = (int)obj;

        Loom.QueueOnMainThread((param) =>
        {
            activeAlertPanel();
            GameObject.Find(_panelName_alert).GetComponent<ControlAlertPanel>().RemoveLoading(id);
        }, null);
    }

    /*************** Confirm Dialog ***************/
    public void ShowConfirmDialog(string title, string msg, Listener listener)
    {
        List<System.Object> paramList = new List<System.Object>();
        paramList.Add(title);
        paramList.Add(msg);
        paramList.Add(listener);
        Loom.RunAsync(
            () =>
            {
                Thread t = new Thread(new ParameterizedThreadStart(doShowConfirmDialog));
                t.Start(paramList);
            }
        );
    }

    private void doShowConfirmDialog(object obj)
    {
        List<System.Object> paramList = (List<System.Object>)obj;
        string title = (string)paramList[0];
        string msg = (string)paramList[1];
        Listener listener = (Listener)paramList[2];
        Loom.QueueOnMainThread((param) =>
        {
            activeConfirmDialogPanel();
            GameObject.Find(_panelName_confirmDialog).GetComponent<ControlConfirmDialog>().ShowConfirmDialog(title, msg, listener);
        }, null);
    }

    /*************** Toast ***************/
    public void ShowToast(string msg)
    {
        Loom.RunAsync(
            () =>
            {
                Thread t = new Thread(new ParameterizedThreadStart(doShowToast));
                t.Start(msg);
            }
        );
    }

    private void doShowToast(object obj)
    {
        string msg = (string)obj;
        Loom.QueueOnMainThread((param) =>
        {
            activeToastPanel();
            GameObject.Find(_panelName_toast).GetComponent<ControlToastPanel>().ShowToast(msg);
        }, null);
    }

    /*************** others ***************/
    private void activeConfirmDialogPanel()
    {
        GameObject.Find("Canvas").transform.Find(_panelName_confirmDialog).gameObject.SetActive(true);
    }

    private void activeAlertPanel()
    {
        GameObject.Find("Canvas").transform.Find(_panelName_alert).gameObject.SetActive(true);
    }

    private void activeToastPanel()
    {
        GameObject.Find("Canvas").transform.Find(_panelName_toast).gameObject.SetActive(true);
    }

    private static int _loadingMsgId = 0;
    public static int GetLoadingMsgId()
    {
        return ++_loadingMsgId;
    }
}
