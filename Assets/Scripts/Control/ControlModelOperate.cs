using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class ControlModelOperate : MonoBehaviour, ModelManager.Listener
{
    private Sprite _sp_export_normal, _sp_export_hover, _sp_export_disable;
    private SpriteState _ss_exportEnable, _ss_exportDisable;

    private Sprite _sp_move_normal, _sp_move_hover;
    private Sprite _sp_scale_normal, _sp_scale_hover;
    private Sprite _sp_rotate_normal, _sp_rotate_hover;

    /*************** mode ***************/
    public enum Mode_Enum
    {
        Move,
        Scale,
        Rotate,
        None
    }

    public static Mode_Enum _mode = Mode_Enum.Move;

    private int _clickTimes_openBtn = 0;

    public Button btn_open, btn_move, btn_scale, btn_rotate, btn_export, btn_print;
    public Image img_scaleContainer, img_moveContainer, img_rotateContainer;
    public Slider slider_scale;
    public Slider slider_moveX, slider_moveY;
    public Slider slider_rotateX, slider_rotateY, slider_rotateZ;
    public InputField input_moveX, input_moveY;
    public InputField input_scale;
    public InputField input_rotateX, input_rotateY, input_rotateZ;
    public Button btn_left_scale, btn_right_scale;
    public Button btn_left_moveX, btn_right_moveX, btn_left_moveY, btn_right_moveY;
    public Button btn_left_rotateX, btn_right_rotateX, btn_right_rotateY, btn_left_rotateY, btn_right_rotateZ, btn_left_rotateZ;

    /***************** button listener *****************/
    private void onClick_open()
    {
        string defaultDir = "";
        //open Snapmaker3DExampleDir
        int launchTimes_curVersion = PlayerPrefsManager.GetInstance().GetLaunchTimes_CurVersion();
        if (++_clickTimes_openBtn == 1 && launchTimes_curVersion <= 1)
        {
            Debug.Log("times_curVersionLaunched :" + launchTimes_curVersion + "\n");
        }

        string modelPath = FileDialogManager.GetInstance().ShowFileSelectDialog_STL_OBJ(defaultDir);

        if (string.IsNullOrEmpty(modelPath) || modelPath.Length == 0)
        {
            Debug.LogWarning("Model path is empty");
        }
        else
        {
            GcodeRenderManager.GetInstance().Destroy();
            ModelManager.GetInstance().StartSubThread_parseModel(modelPath);
        }
    }

    private void onClick_move()
    {
        _mode = Mode_Enum.Move;
        StageManager.SetStage_Load_Model();
    }

    private void onClick_scale()
    {
        _mode = Mode_Enum.Scale;
        StageManager.SetStage_Load_Model();
    }

    private void onClick_rotate()
    {
        _mode = Mode_Enum.Rotate;
        StageManager.SetStage_Load_Model();
    }

    private void onClick_export()
    {
        if (StageManager.GetStageList().Contains(StageManager.Stage_Enum.Gcode_Render) &&
            GcodeRenderManager.GetInstance().GetInfo().isRendered &&
            StageManager.GetCurStage() != StageManager.Stage_Enum.Gcode_Send)
        {

            if (GcodeRenderManager.GetInstance().IsInBounds())
            {
                string path = ModelManager.GetInstance().GetInfo().modelPath;
                string defaultName = System.IO.Path.GetFileNameWithoutExtension(path);
                string exportPath = FileDialogManager.GetInstance().ShowFileSaveDialog_Gcode(defaultName);

                if (string.IsNullOrEmpty(exportPath) || exportPath.Trim().Length == 0)
                {
                    Debug.LogWarning("Gcode export is empty");
                }
                else
                {
                    string originPath = GcodeCreateManager.GetInstance().curGcodeBean.gcodePath;
                    startSubThreadToExportGcode(originPath, exportPath);
                }
            }
            else
            {
                AlertMananger.GetInstance().ShowAlertMsg("Unable to print. The model goes beyond the work area.");
            }
        }
    }

    private void startSubThreadToExportGcode(string originPath, string exportPath)
    {
        Thread t = new Thread(new ParameterizedThreadStart(doExportGcode));
        List<string> list = new List<string>();
        list.Add(originPath);
        list.Add(exportPath);
        t.Start(list);
    }

    private int _loadingMsgId = 0;
    private void doExportGcode(object obj)
    {
        List<string> list = (List<string>)obj;
        string originPath = list[0];
        string exportPath = list[1];

        _loadingMsgId = AlertMananger.GetLoadingMsgId();
        AlertMananger.GetInstance().ShowAlertLoading("Exporting...", _loadingMsgId);
        //todo: may exception occured
        Debug.Log("Gcode export: " + originPath + "--->" + exportPath + "\n");
        File.Copy(originPath, exportPath, true);

        AlertMananger.GetInstance().DismissAlertLoading(_loadingMsgId);
        AlertMananger.GetInstance().ShowToast("Successfully export gcode.");

    }

    void onClick_print()
    {
        string gcodePath = GcodeCreateManager.GetInstance().curGcodeBean.gcodePath;
        GcodeSenderManager.GetInstance().StartSend(gcodePath);
    }

    /***************** slider listener *****************/
    //Cling to bottom when release slider
    private void valueChanged_move(Slider slider)
    {
        if (slider == slider_moveX)
        {
            ModelManager.GetInstance().MoveRenderXTo(slider.value);
        }
        else if (slider == slider_moveY)
        {
            ModelManager.GetInstance().MoveRenderYTo(slider.value);
        }
    }

    private void valueChanged_scale(Slider slider)
    {
        ModelManager.GetInstance().ScaleRenderTo(slider.value);
    }

    private void valueChanged_rotate(Slider slider)
    {
        float value = slider.value;
        if (slider == slider_rotateX)
        {
            ModelManager.GetInstance().RotateRenderXTo(value);
        }
        else if (slider == slider_rotateY)
        {
            ModelManager.GetInstance().RotateRenderYTo(value);
        }
        else if (slider == slider_rotateZ)
        {
            ModelManager.GetInstance().RotateRenderZTo(value);
        }
    }

    /***************** input listener *****************/
    private void inputChanged(InputField input)
    {
        if (string.IsNullOrEmpty(input.text))
        {
            return;
        }

        //less than min: set to min
        //greater than max: set to max

        float value = float.Parse(input.text);

        if (input == input_scale)
        {

            ModelManager.GetInstance().ScaleRenderTo(value / 100);

        }
        else if (input == input_moveX)
        {

            ModelManager.GetInstance().MoveRenderXTo(value);

        }
        else if (input == input_moveY)
        {

            ModelManager.GetInstance().MoveRenderYTo(value);

        }
        else if (input == input_rotateX)
        {

            ModelManager.GetInstance().RotateRenderXTo(value);

        }
        else if (input == input_rotateY)
        {

            ModelManager.GetInstance().RotateRenderYTo(value);

        }
        else if (input == input_rotateZ)
        {

            ModelManager.GetInstance().RotateRenderZTo(value);
        }

        ModelManager.GetInstance().OnOperateEnd();
    }

    /*************** left&right button listener ***************/
    void onClick_left_right(Button sender)
    {
        //change slider value, will 
        if (sender == btn_left_scale)
        {

            ModelManager.GetInstance().ScaleRenderTo(ModelManager.GetInstance().GetRenderOperateBean().scale - 0.01f);

        }
        else if (sender == btn_right_scale)
        {

            ModelManager.GetInstance().ScaleRenderTo(ModelManager.GetInstance().GetRenderOperateBean().scale + 0.01f);

        }
        else if (sender == btn_left_moveX)
        {

            ModelManager.GetInstance().MoveRenderXTo(ModelManager.GetInstance().GetRenderOperateBean().move.x - 0.1f);

        }
        else if (sender == btn_right_moveX)
        {

            ModelManager.GetInstance().MoveRenderXTo(ModelManager.GetInstance().GetRenderOperateBean().move.x + 0.1f);

        }
        else if (sender == btn_left_moveY)
        {

            ModelManager.GetInstance().MoveRenderYTo(ModelManager.GetInstance().GetRenderOperateBean().move.y - 0.1f);

        }
        else if (sender == btn_right_moveY)
        {

            ModelManager.GetInstance().MoveRenderYTo(ModelManager.GetInstance().GetRenderOperateBean().move.y + 0.1f);

        }
        else if (sender == btn_left_rotateX)
        {

            ModelManager.GetInstance().RotateRenderXTo(ModelManager.GetInstance().GetRenderOperateBean().rotate.x - 1);

        }
        else if (sender == btn_right_rotateX)
        {

            ModelManager.GetInstance().RotateRenderXTo(ModelManager.GetInstance().GetRenderOperateBean().rotate.x + 1);

        }
        else if (sender == btn_left_rotateY)
        {

            ModelManager.GetInstance().RotateRenderYTo(ModelManager.GetInstance().GetRenderOperateBean().rotate.y - 1);

        }
        else if (sender == btn_right_rotateY)
        {

            ModelManager.GetInstance().RotateRenderYTo(ModelManager.GetInstance().GetRenderOperateBean().rotate.y + 1);

        }
        else if (sender == btn_left_rotateZ)
        {

            ModelManager.GetInstance().RotateRenderZTo(ModelManager.GetInstance().GetRenderOperateBean().rotate.z - 1);

        }
        else if (sender == btn_right_rotateZ)
        {

            ModelManager.GetInstance().RotateRenderZTo(ModelManager.GetInstance().GetRenderOperateBean().rotate.z + 1);
        }

        ModelManager.GetInstance().OnOperateEnd();
    }

    /*************** life cycle ***************/
    void Awake()
    {
        ModelManager.GetInstance().AddListener(this);
    }

    void Start()
    {
        _sp_export_normal = Resources.Load("Images/save-gcode", typeof(Sprite)) as Sprite;
        _sp_export_hover = Resources.Load("Images/save-gcode-hover", typeof(Sprite)) as Sprite;
        _sp_export_disable = Resources.Load("Images/save-gcode-disable", typeof(Sprite)) as Sprite;

        _ss_exportEnable = new SpriteState();
        _ss_exportDisable = new SpriteState();
        _ss_exportEnable.pressedSprite = _sp_export_hover;
        _ss_exportDisable.pressedSprite = _sp_export_disable;

        _sp_move_normal = Resources.Load("Images/Move_normal", typeof(Sprite)) as Sprite;
        _sp_move_hover = Resources.Load("Images/Move_clicked", typeof(Sprite)) as Sprite;

        _sp_scale_normal = Resources.Load("Images/Scale_normal", typeof(Sprite)) as Sprite;
        _sp_scale_hover = Resources.Load("Images/Scale_clicked", typeof(Sprite)) as Sprite;

        _sp_rotate_normal = Resources.Load("Images/Rotate_normal", typeof(Sprite)) as Sprite;
        _sp_rotate_hover = Resources.Load("Images/Rotate_clicked", typeof(Sprite)) as Sprite;

        //left panel
        btn_open.GetComponent<Button>().onClick.AddListener(onClick_open);
        btn_move.GetComponent<Button>().onClick.AddListener(onClick_move);
        btn_scale.GetComponent<Button>().onClick.AddListener(onClick_scale);
        btn_rotate.GetComponent<Button>().onClick.AddListener(onClick_rotate);
        btn_export.GetComponent<Button>().onClick.AddListener(onClick_export);
        btn_print.GetComponent<Button>().onClick.AddListener(onClick_print);

        //rotate
        slider_rotateX.minValue = -180.0f;
        slider_rotateX.maxValue = 180.0f;
        slider_rotateY.minValue = -180.0f;
        slider_rotateY.maxValue = 180.0f;
        slider_rotateZ.minValue = -180.0f;
        slider_rotateZ.maxValue = 180.0f;

        slider_rotateX.onValueChanged.AddListener(delegate
        {
            valueChanged_rotate(slider_rotateX);
        });
        slider_rotateY.onValueChanged.AddListener(delegate
        {
            valueChanged_rotate(slider_rotateY);
        });
        slider_rotateZ.onValueChanged.AddListener(delegate
        {
            valueChanged_rotate(slider_rotateZ);
        });

        //move
        Vector3 deviceSize = Global.GetInstance().GetPrinterParamsStruct().size;

        slider_moveX.minValue = -deviceSize.x / 2;
        slider_moveX.maxValue = deviceSize.x / 2;
        slider_moveX.value = 0;
        slider_moveX.onValueChanged.AddListener(delegate
        {
            valueChanged_move(slider_moveX);
        });

        slider_moveY.minValue = -deviceSize.y / 2;
        slider_moveY.maxValue = deviceSize.x / 2;
        slider_moveY.value = 0;
        slider_moveY.onValueChanged.AddListener(delegate
        {
            valueChanged_move(slider_moveY);
        });

        slider_scale.onValueChanged.AddListener(delegate
        {
            valueChanged_scale(slider_scale);
        });

        //input filed
        input_scale.onEndEdit.AddListener(delegate
        {
            inputChanged(input_scale);
        });
        input_moveX.onEndEdit.AddListener(delegate
        {
            inputChanged(input_moveX);
        });
        input_moveY.onEndEdit.AddListener(delegate
        {
            inputChanged(input_moveY);
        });
        input_rotateX.onEndEdit.AddListener(delegate
        {
            inputChanged(input_rotateX);
        });
        input_rotateY.onEndEdit.AddListener(delegate
        {
            inputChanged(input_rotateY);
        });
        input_rotateZ.onEndEdit.AddListener(delegate
        {
            inputChanged(input_rotateZ);
        });

        //left&right button
        btn_left_scale.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_left_scale);
        });
        btn_right_scale.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_right_scale);
        });

        btn_left_moveX.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_left_moveX);
        });
        btn_right_moveX.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_right_moveX);
        });
        btn_left_moveY.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_left_moveY);
        });
        btn_right_moveY.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_right_moveY);
        });

        btn_left_rotateX.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_left_rotateX);
        });
        btn_right_rotateX.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_right_rotateX);
        });

        btn_right_rotateY.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_right_rotateY);
        });
        btn_left_rotateY.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_left_rotateY);
        });
        btn_right_rotateZ.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_right_rotateZ);
        });
        btn_left_rotateZ.GetComponent<Button>().onClick.AddListener(delegate
        {
            onClick_left_right(btn_left_rotateZ);
        });
    }

    void Update()
    {
        //hide operate container when click empty place. check update_mode
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                _mode = Mode_Enum.None;
            }
        }

        /*************** btn_export ***************/
        bool exportable =
            StageManager.GetStageList().Contains(StageManager.Stage_Enum.Gcode_Render) &&
            GcodeRenderManager.GetInstance().GetInfo().isRendered &&
            StageManager.GetCurStage() != StageManager.Stage_Enum.Gcode_Send &&
            GcodeRenderManager.GetInstance().IsInBounds();
        
        if (exportable)
        {
            btn_export.spriteState = _ss_exportEnable;
            btn_export.gameObject.GetComponent<Image>().sprite = _sp_export_normal;
        }
        else
        {
            btn_export.spriteState = _ss_exportDisable;
            btn_export.gameObject.GetComponent<Image>().sprite = _sp_export_disable;
        }

        /*************** btn_print ***************/
        bool active_print =
            StageManager.GetStageList().Contains(StageManager.Stage_Enum.Gcode_Render) &&
            !GcodeRenderManager.GetInstance().GetInfo().isParsing &&
            StageManager.GetCurStage() != StageManager.Stage_Enum.Gcode_Send &&
            PortConnectManager.GetInstance().IsConnected() &&
            GcodeRenderManager.GetInstance().IsInBounds();

        btn_print.interactable = active_print;

        /*************** other widget ***************/
        switch (StageManager.GetCurStage())
        {
            case StageManager.Stage_Enum.Idle:

                btn_open.interactable = true;
                btn_move.interactable = false;
                btn_scale.interactable = false;
                btn_rotate.interactable = false;

                img_moveContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_scaleContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_rotateContainer.GetComponent<Transform>().gameObject.SetActive(false);

                break;

            case StageManager.Stage_Enum.Load_Model:

                if (ModelManager.GetInstance().GetInfo().isParsing)
                {

                    btn_open.interactable = false;
                    btn_move.interactable = false;
                    btn_scale.interactable = false;
                    btn_rotate.interactable = false;

                    img_moveContainer.GetComponent<Transform>().gameObject.SetActive(false);
                    img_scaleContainer.GetComponent<Transform>().gameObject.SetActive(false);
                    img_rotateContainer.GetComponent<Transform>().gameObject.SetActive(false);

                }
                else
                {
                    btn_open.interactable = true;
                    btn_move.interactable = true;
                    btn_scale.interactable = true;
                    btn_rotate.interactable = true;

                    update_mode();
                }

                break;

            case StageManager.Stage_Enum.Gcode_Create:

                btn_open.interactable = false;
                btn_move.interactable = false;
                btn_scale.interactable = false;
                btn_rotate.interactable = false;

                img_moveContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_scaleContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_rotateContainer.GetComponent<Transform>().gameObject.SetActive(false);

                break;

            case StageManager.Stage_Enum.Gcode_Render:
                //Sigle thread so far
                //todo: Rendering/Rendered
                if (GcodeRenderManager.GetInstance().GetInfo().isParsing)
                {

                    btn_open.interactable = false;
                    btn_move.interactable = false;
                    btn_scale.interactable = false;
                    btn_rotate.interactable = false;

                    img_moveContainer.GetComponent<Transform>().gameObject.SetActive(false);
                    img_scaleContainer.GetComponent<Transform>().gameObject.SetActive(false);
                    img_rotateContainer.GetComponent<Transform>().gameObject.SetActive(false);

                }
                else
                {

                    btn_open.interactable = true;
                    btn_move.interactable = true;
                    btn_scale.interactable = true;
                    btn_rotate.interactable = true;

                    update_mode();
                }
                break;

            case StageManager.Stage_Enum.Gcode_Send:

                btn_open.interactable = false;
                btn_move.interactable = false;
                btn_scale.interactable = false;
                btn_rotate.interactable = false;

                img_moveContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_scaleContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_rotateContainer.GetComponent<Transform>().gameObject.SetActive(false);

                break;
        }

        if (ModelManager.GetInstance().GetRenderOperateBean() != null)
        {
            if (!input_moveX.isFocused)
            {
                input_moveX.text = (ModelManager.GetInstance().GetRenderOperateBean().move.x).ToString("0.0");
            }

            if (!input_moveY.isFocused)
            {
                input_moveY.text = (ModelManager.GetInstance().GetRenderOperateBean().move.y).ToString("0.0");
            }

            if (!input_scale.isFocused)
            {
                input_scale.text = (ModelManager.GetInstance().GetRenderOperateBean().scale * 100).ToString("0.0");
            }

            if (!input_rotateX.isFocused)
            {
                input_rotateX.text = (ModelManager.GetInstance().GetRenderOperateBean().rotate.x).ToString("0.0");
            }

            if (!input_rotateY.isFocused)
            {
                input_rotateY.text = (ModelManager.GetInstance().GetRenderOperateBean().rotate.y).ToString("0.0");
            }

            if (!input_rotateZ.isFocused)
            {
                input_rotateZ.text = (ModelManager.GetInstance().GetRenderOperateBean().rotate.z).ToString("0.0");
            }

            slider_moveX.value = ModelManager.GetInstance().GetRenderOperateBean().move.x;
            slider_moveY.value = ModelManager.GetInstance().GetRenderOperateBean().move.y;

            slider_scale.value = ModelManager.GetInstance().GetRenderOperateBean().scale;

            slider_rotateX.value = ModelManager.GetInstance().GetRenderOperateBean().rotate.x;
            slider_rotateY.value = ModelManager.GetInstance().GetRenderOperateBean().rotate.y;
            slider_rotateZ.value = ModelManager.GetInstance().GetRenderOperateBean().rotate.z;
        }
    }

    void update_mode()
    {
        switch (_mode)
        {
            case Mode_Enum.None:
                btn_move.gameObject.GetComponent<Image>().sprite = _sp_move_normal;
                btn_scale.gameObject.GetComponent<Image>().sprite = _sp_scale_normal;
                btn_rotate.gameObject.GetComponent<Image>().sprite = _sp_rotate_normal;

                img_moveContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_scaleContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_rotateContainer.GetComponent<Transform>().gameObject.SetActive(false);
                break;
            case Mode_Enum.Move:

                btn_move.gameObject.GetComponent<Image>().sprite = _sp_move_hover;
                btn_scale.gameObject.GetComponent<Image>().sprite = _sp_scale_normal;
                btn_rotate.gameObject.GetComponent<Image>().sprite = _sp_rotate_normal;

                img_moveContainer.GetComponent<Transform>().gameObject.SetActive(true);
                img_scaleContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_rotateContainer.GetComponent<Transform>().gameObject.SetActive(false);
                break;
            case Mode_Enum.Scale:

                btn_move.gameObject.GetComponent<Image>().sprite = _sp_move_normal;
                btn_scale.gameObject.GetComponent<Image>().sprite = _sp_scale_hover;
                btn_rotate.gameObject.GetComponent<Image>().sprite = _sp_rotate_normal;

                img_moveContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_scaleContainer.GetComponent<Transform>().gameObject.SetActive(true);
                img_rotateContainer.GetComponent<Transform>().gameObject.SetActive(false);
                break;
            case Mode_Enum.Rotate:

                btn_move.gameObject.GetComponent<Image>().sprite = _sp_move_normal;
                btn_scale.gameObject.GetComponent<Image>().sprite = _sp_scale_normal;
                btn_rotate.gameObject.GetComponent<Image>().sprite = _sp_rotate_hover;

                img_moveContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_scaleContainer.GetComponent<Transform>().gameObject.SetActive(false);
                img_rotateContainer.GetComponent<Transform>().gameObject.SetActive(true);
                break;
        }
    }

    /*************** ModelManager call back ***************/
    public void OnModelManager_ParseModelStarted()
    {
        Debug.Log("OnModelManager_ParseModelStarted" + "\n");
        StageManager.SetStage_Load_Model();
        AlertMananger.GetInstance().ShowToast("Start loading.");
    }

    public void OnModelManager_ParseModelSucceed()
    {
        Debug.Log("OnModelManager_ParseModelSucceed" + "\n");
        AlertMananger.GetInstance().ShowToast("Loading file succeed.");
    }

    public void OnModelManager_ParseModelError()
    {
        Debug.LogError("OnModelManager_ParseModelError" + "\n");
        StageManager.SetStage_Idle();

        AlertMananger.GetInstance().ShowAlertMsg("Unable to open the file.");
    }

    public void OnModelManager_ParseModelProgress(float progress)
    {
    }

    //on main thread
    public void OnModelManager_RenderModelSucceed()
    {
        Debug.Log("OnModelManager_RenderModelSucceed" + "\n");

        if (ModelManager.GetInstance().GetModel() == null)
        {
            //todo: parse exception, then alert user
            Debug.LogError("Model3d is null");
            return;
        }

        //3.set material of shellCube children 
        ModelManager.GetInstance().SetPrintable(true);

        //4.add model to scence
        //rotate print space to front
        Transform transform = GameObject.Find("Print_cube").transform;
        transform.localEulerAngles = Vector3.zero;
        ModelManager.GetInstance().GetModel().renderedModel.shellCube.transform.parent = transform;
        ModelManager.GetInstance().SetModelInitialLocalPosition();

        //5.update shellCubeOriginLocalScale
        //Attention: must do it, use to scale stl model. must do it after parentCube be added into go_printCube
        ModelManager.GetInstance().GetModel().renderedModel.shellCubeOriginLocalScale = ModelManager.GetInstance().GetModel().renderedModel.shellCube.transform.localScale;

        if (ModelManager.GetInstance().GetModel() != null)
        {
            //slider
            slider_scale.value = ModelManager.GetInstance().GetRenderOperateBean().scale;
            slider_scale.minValue = ModelManager.GetInstance().GetScaleRange().x;
            slider_scale.maxValue = ModelManager.GetInstance().GetScaleRange().y;
        }

        Vector3 size = ModelManager.GetInstance().GetCurDataSize();
        float min = 10.0f;
        if (Mathf.Max(size.x, size.y, size.z) < min)
        {
            AlertMananger.GetInstance().ShowAlertMsg("Model is too tiny to see, scale first");
        }
    }

}
