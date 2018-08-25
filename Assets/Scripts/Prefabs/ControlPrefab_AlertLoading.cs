using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPrefab_AlertLoading : MonoBehaviour
{
    public Scrollbar scrollbar_loading;
    public Text text_loadingMsg;

    private int _loadingStepFactoy = 0;

    void Update()
    {
        scrollbar_loading.value = Mathf.Abs(Mathf.Sin(Mathf.PI/ 180.0f * (++_loadingStepFactoy*0.5f))) ;
    }

    public void SetMsg(string str)
    {
        text_loadingMsg.text = str;
    }
}
