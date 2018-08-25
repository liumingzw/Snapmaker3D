using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlAlertPanel : MonoBehaviour
{
    private const float _duration = 1.0f;
    private const int _visiableAlertLimit = 3;

    private string gameObjectName_msg = "msg";
    private string gameObjectName_loading = "loading";

    public GameObject panel_content;

    public GameObject prefab_AlertMsg, prefab_AlertLoading;

    private List<GameObject> _msgGameObjectList = new List<GameObject>();

    /********** msg ************/
    public void ShowAlertMsg(string msg)
    {
        addAlertMsg(msg);
        gameObject.SetActive(true);
    }

    public void RemoveAllAlertMsg()
    {
        foreach (Transform child in panel_content.transform)
        {
            if (child.name == (gameObjectName_msg))
            {
                Destroy(child.gameObject);
            }
        }
    }

    void addAlertMsg(string msg)
    {
        GameObject alertMsg = (GameObject)Instantiate(prefab_AlertMsg);

        alertMsg.transform.SetParent(panel_content.transform);
        alertMsg.name = gameObjectName_msg;

        //set height, width will be expand to equal parent
        alertMsg.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 115);
        //must set localScale
        alertMsg.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        alertMsg.GetComponent<ControlPrefab_AlertMsg>().SetMsg(msg);

        _msgGameObjectList.Add(alertMsg);

        //dismiss one GameObject if count is more than 3
        if (_msgGameObjectList.Count >= 3)
        {
            GameObject selectedGameObject = _msgGameObjectList[0];
            _msgGameObjectList.RemoveAt(0);

            //CrossFadeAlpha
            if(selectedGameObject==null){
                return;
            }
            foreach (Transform child in selectedGameObject.transform)
            {
                if (child.name.Contains("image"))
                {
                    child.GetComponent<Image>().CrossFadeAlpha(0f, _duration, false);
                }
                if (child.name.Contains("text"))
                {
                    child.GetComponent<Text>().CrossFadeAlpha(0f, _duration, false);
                }
                if (child.name.Contains("btn"))
                {
                    child.GetComponent<Image>().CrossFadeAlpha(0f, _duration, false);
                }
            }
            StartCoroutine(destroyGameObject(selectedGameObject));
        }
    }

    IEnumerator destroyGameObject(GameObject mGameObject)
    {
        yield return new WaitForSeconds(_duration);
        Destroy(mGameObject);
    }

    /********** loading ************/
    public void ShowLoadingtMsg(string msg, int id)
    {
        addAlertLoading(msg, id);
        gameObject.SetActive(true);
    }

    public void RemoveLoading(int id)
    {
        foreach (Transform child in panel_content.transform)
        {
            if (child.name == (gameObjectName_loading + id))
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    void addAlertLoading(string msg, int id)
    {
        GameObject alertLoading = (GameObject)Instantiate(prefab_AlertLoading);
        alertLoading.transform.SetParent(panel_content.transform);
        alertLoading.name = gameObjectName_loading + id;

        //set height, width will be expand to equal parent
        alertLoading.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);
        //must set localScale
        alertLoading.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        alertLoading.GetComponent<ControlPrefab_AlertLoading>().SetMsg(msg);
    }
}
