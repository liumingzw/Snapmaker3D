using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PrintOtherSideTrigger : MonoBehaviour, ModelManager.Listener
{
    private int side_top, side_left, side_right, side_front, side_back;
    private Dictionary<GameObject, int> _triggerCount = new Dictionary<GameObject, int>();

    private Sprite normalSprite, triggerSprite;

    private List<GameObject> _triggeredGameObjectList = new List<GameObject>();

    void Awake()
    {
        ModelManager.GetInstance().AddListener(this);
    }

    private void Start()
    {
        normalSprite =  Resources.Load("Images/printCuber_side", typeof(Sprite)) as Sprite; ;
        triggerSprite =  Resources.Load("Images/printCube_side_trigger", typeof(Sprite)) as Sprite; ;
    }

    void OnTriggerEnter(Collider e)
    {
        ModelManager.GetInstance().OnTriggerEnter(e);

        //e.transform.gameObject.name : child*
        if (e.transform.gameObject.name.StartsWith("child"))
        {
            if(name.StartsWith("side_")){
                if (!_triggeredGameObjectList.Contains(gameObject))
                {
                    _triggeredGameObjectList.Add(gameObject);
                }
            }
            if (name == "side_top")
            {
                ++side_top;
                gameObject.GetComponent<SpriteRenderer>().sprite = triggerSprite;
            }
            if (name == "side_left")
            {
                ++side_left;
                gameObject.GetComponent<SpriteRenderer>().sprite = triggerSprite;
            }
            if (name == "side_right")
            {
                ++side_right;
                gameObject.GetComponent<SpriteRenderer>().sprite = triggerSprite;
            }
            if (name == "side_front")
            {
                ++side_front;
                gameObject.GetComponent<SpriteRenderer>().sprite = triggerSprite;
            }
            if (name == "side_back")
            {
                ++side_back;
                gameObject.GetComponent<SpriteRenderer>().sprite = triggerSprite;
            }
        }
    }

    void OnTriggerExit(Collider e)
    {
        ModelManager.GetInstance().OnTriggerExit(e);

        //e.transform.gameObject.name : child*
        if (e.transform.gameObject.name.StartsWith("child"))
        {
            if (name == "side_top")
            {
                --side_top;
                if (side_top <= 0)
                {
                    side_top = 0;
                    gameObject.GetComponent<SpriteRenderer>().sprite = normalSprite;
                }
            }
            if (name == "side_left")
            {
                --side_left;
                if (side_left <= 0)
                {
                    side_left = 0;
                    gameObject.GetComponent<SpriteRenderer>().sprite = normalSprite;
                }
            }
            if (name == "side_right")
            {
                --side_right;
                if (side_right <= 0)
                {
                    side_right = 0;
                    gameObject.GetComponent<SpriteRenderer>().sprite = normalSprite;
                }
            }
            if (name == "side_front")
            {
                --side_front;
                if (side_front <= 0)
                {
                    side_front = 0;
                    gameObject.GetComponent<SpriteRenderer>().sprite = normalSprite;
                }
            }
            if (name == "side_back")
            {
                --side_back;
                if (side_back <= 0)
                {
                    side_back = 0;
                    gameObject.GetComponent<SpriteRenderer>().sprite = normalSprite;
                }
            }
        }
    }

    private void resetSideSprite()
    {
        Loom.RunAsync(
            () =>
            {
            Thread t = new Thread(doResetSideSprite);
                t.Start();
            }
        );
    }

    private void doResetSideSprite(object obj)
    {
        Loom.QueueOnMainThread((param) =>
        {
            side_top = side_left = side_right = side_front = side_back = 0;
            foreach (GameObject mGameObject in _triggeredGameObjectList)
            {
                mGameObject.GetComponent<SpriteRenderer>().sprite = normalSprite;
            }
            _triggeredGameObjectList.Clear();
        }, null);
    }

    /*************** ModelManager call back ***************/
    public void OnModelManager_ParseModelStarted()
    {
        resetSideSprite();
    }
    public void OnModelManager_ParseModelSucceed()
    {
    }
    public void OnModelManager_RenderModelSucceed(){
    }
    public void OnModelManager_ParseModelError()
    {
    }
    public void OnModelManager_ParseModelProgress(float progress)
    {
    }
}
