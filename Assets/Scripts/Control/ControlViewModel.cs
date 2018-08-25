using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ControlViewModel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private readonly int PRINT_CUBE_MAX_Z = 120;
    private readonly int PRINT_CUBE_MIN_Z = -330;
    private readonly int PRINT_CUBE_ZOOM_UNIT = 30;

    public Button btn_zoom_in, btn_zoom_out, btn_left, btn_right, btn_top, btn_bottom, btn_center;

    private GameObject _printCube;
    private Transform _tr_printCube, _tr_mainCamera;

    private Vector3 _curMousePos;
    private Vector3 _initialPosition;

    private bool _isDraging = false;

    void Start()
    {
        btn_zoom_in.GetComponent<Button>().onClick.AddListener(onClick_zoom_in);
        btn_zoom_out.GetComponent<Button>().onClick.AddListener(onClick_zoom_out);

        btn_left.GetComponent<Button>().onClick.AddListener(onClick_left);
        btn_right.GetComponent<Button>().onClick.AddListener(onClick_right);
        btn_top.GetComponent<Button>().onClick.AddListener(onClick_top);
        btn_bottom.GetComponent<Button>().onClick.AddListener(onClick_bottom);
        btn_center.GetComponent<Button>().onClick.AddListener(onClick_center);

        _printCube = GameObject.Find("Print_cube");
        _tr_printCube = _printCube.transform;
        _tr_mainCamera = Camera.main.transform;

        _initialPosition = _tr_printCube.position;
    }

    void onClick_zoom_in()
    {
        if (_tr_mainCamera.position.z <= PRINT_CUBE_MAX_Z)
        {
            float z = _tr_mainCamera.position.z + PRINT_CUBE_ZOOM_UNIT;
            iTween.MoveTo(Camera.main.gameObject, iTween.Hash("z", z, "easeType", "easeInOutExpo", "loopType", "none", "delay", 0, "onupdate", "animalOnUpdate", "onupdatetarget", this.gameObject));
        }
    }

    void onClick_zoom_out()
    {
        if (_tr_mainCamera.position.z >= PRINT_CUBE_MIN_Z)
        {
            float z = _tr_mainCamera.position.z - PRINT_CUBE_ZOOM_UNIT;
            iTween.MoveTo(Camera.main.gameObject, iTween.Hash("z", z, "easeType", "easeInOutExpo", "loopType", "none", "delay", 0, "onupdate", "animalOnUpdate", "onupdatetarget", this.gameObject));
        }
    }

    void onClick_left()
    {
        float y = _tr_printCube.localEulerAngles.y;
        RotatePrintCubeTo(0, (Mathf.CeilToInt(y / 90)) * 90 - 90, 0);
    }

    void onClick_right()
    {
        float y = _tr_printCube.localEulerAngles.y;
        RotatePrintCubeTo(0, (Mathf.Floor(y / 90)) * 90 + 90, 0);
    }

    void onClick_top()
    {
        float x = _tr_printCube.localEulerAngles.x;
        if ((0 <= x && x <= 90) || (270 < x && x < 360))
        {
            RotatePrintCubeTo((Mathf.CeilToInt(x / 90)) * 90 - 90, 0, 0);
        }
    }

    void onClick_bottom()
    {
        float x = _tr_printCube.localEulerAngles.x;
        if ((0 <= x && x < 90) || (270 <= x && x < 360))
        {
            RotatePrintCubeTo((Mathf.Floor(x / 90)) * 90 + 90, 0, 0);
        }
    }

    void onClick_center()
    {
        ResetPrintCubeLocalPosistion();
        RotatePrintCubeTo(-30, 0, 0);
    }

    void Update()
    {
        /*************** "zoom in" or "zoom out" print cube ***************/
        //in the range, user could "zoom in" or "zoom out"
        if (_tr_mainCamera.position.z <= PRINT_CUBE_MAX_Z && _tr_mainCamera.position.z >= PRINT_CUBE_MIN_Z)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                _tr_mainCamera.position += new Vector3(0, 0, PRINT_CUBE_ZOOM_UNIT);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                _tr_mainCamera.position -= new Vector3(0, 0, PRINT_CUBE_ZOOM_UNIT);
            }
        }
        else if (_tr_mainCamera.position.z > PRINT_CUBE_MAX_Z)
        {
            //greater than max, only allow zoom in
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                _tr_mainCamera.position -= new Vector3(0, 0, PRINT_CUBE_ZOOM_UNIT);
            }
        }
        else if (_tr_mainCamera.position.z < PRINT_CUBE_MIN_Z)
        {
            //less than max, only allow zoom in
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                _tr_mainCamera.position += new Vector3(0, 0, PRINT_CUBE_ZOOM_UNIT);
            }
        }
    }

    public void RotatePrintCubeTo(float x, float y, float z)
    {
        Hashtable args = new Hashtable();
        args.Add("rotation", new Vector3(x, y, z));
        args.Add("islocal", true);
        args.Add("delay", 0f);
        args.Add("speed", 200f);
        args.Add("easeType", iTween.EaseType.easeInOutExpo);
        args.Add("loopType", iTween.LoopType.none);
        args.Add("onupdate", "animalOnUpdate");
        args.Add("onupdatetarget", this.gameObject);
        iTween.RotateTo(_printCube, args);
    }

    public void ResetPrintCubeLocalPosistion()
    {
        _tr_printCube.position = _initialPosition;
    }

    void animalOnUpdate()
    {
        GcodeRenderManager.GetInstance().DrawLayers();
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        _curMousePos = eventData.position;
        _isDraging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        ControlModelOperate._mode = ControlModelOperate.Mode_Enum.None;
        if (Input.GetKey(KeyCode.Mouse1))
        {
            /*************** move ***************/
            _tr_printCube.transform.position += new Vector3((eventData.position.x - _curMousePos.x) / 4, (eventData.position.y - _curMousePos.y) / 4, 0);
            _curMousePos = eventData.position;
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            /*************** rotate ***************/
            if (_curMousePos.x - eventData.position.x > 0)
            {
                //left
                _tr_printCube.Rotate(new Vector3(0, _curMousePos.x - eventData.position.x, 0), Space.Self);
            }
            else if (_curMousePos.x - eventData.position.x < 0)
            {
                //right
                _tr_printCube.Rotate(new Vector3(0, _curMousePos.x - eventData.position.x, 0), Space.Self);
            }
            if (_curMousePos.y - eventData.position.y < 0)
            {
                //up
                _tr_printCube.Rotate(new Vector3(eventData.position.y - _curMousePos.y, 0, 0), Space.World);
            }
            else if (_curMousePos.y - eventData.position.y > 0)
            {
                //down
                _tr_printCube.Rotate(new Vector3(eventData.position.y - _curMousePos.y, 0, 0), Space.World);
            }
            _curMousePos = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDraging = false;
    }

    void LateUpdate()
    {
        switch (StageManager.GetCurStage())
        {
            case StageManager.Stage_Enum.Gcode_Send:
                GcodeRenderManager.GetInstance().ShowLayerLessThanZ(GcodeSenderManager.GetInstance().GetInfo().completedZ);
                GcodeRenderManager.GetInstance().DrawLayers();
                break;
            case StageManager.Stage_Enum.Gcode_Render:
                if (GcodeRenderManager.GetInstance().GetInfo().isRendered)
                {
                    if (_isDraging || Input.GetAxis("Mouse ScrollWheel") != 0.0f)
                    {
                        GcodeRenderManager.GetInstance().DrawLayers();
                        if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
                        {
                            GcodeRenderManager.GetInstance().AutoSetLineWidth(_tr_mainCamera.position.z + 500);
                        }
                    }
                }
                break;
        }
    }
}
