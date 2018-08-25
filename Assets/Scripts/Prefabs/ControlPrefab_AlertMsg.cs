using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPrefab_AlertMsg : MonoBehaviour {

    public Button btn_close;
    public Button btn_dismiss;
    public Text text_alertMsg;

	void Start () {
        btn_close.onClick.AddListener(onClick_close);
        btn_dismiss.onClick.AddListener(onClick_dismiss);
	}

    void onClick_close()
    {
        Destroy(gameObject);
    }

    void onClick_dismiss()
    {
        Destroy(gameObject);
    }

    public void SetMsg(string str){
        text_alertMsg.text = str;
    }
}
