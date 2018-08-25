using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlToastPanel : MonoBehaviour
{
    public Text text_parent, text_toast;
    public Image image_bg;
    private float _duration = 3.5f;

    private int _indexShow = 0;
    private int _indexInvoke = 0;

    public void ShowToast(string msg)
    {
        ++_indexShow;
        text_parent.text = msg;
        text_toast.text = msg;

        image_bg.CrossFadeAlpha(1f, 0, false);
        text_toast.CrossFadeAlpha(1f, 0, false);

        image_bg.CrossFadeAlpha(0f, _duration, false);
        text_toast.CrossFadeAlpha(0f, _duration, false);

        gameObject.SetActive(true);

        Invoke("disactive", _duration * 0.65f);
    }

    void disactive()
    {
        ++_indexInvoke;
        if (_indexInvoke == _indexShow)
        {
            gameObject.SetActive(false);
        }
    }

}
