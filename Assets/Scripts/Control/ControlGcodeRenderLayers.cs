using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class ControlGcodeRenderLayers : MonoBehaviour, GcodeRenderManager.Listener, GcodeSenderManager.Listener
{
    public GameObject panel_content;
    public Slider slider;
    public Button btn_layerIndex;
    private bool _isGcodeRendered;

    void Awake()
    {
        GcodeRenderManager.GetInstance().AddListener(this);
        GcodeSenderManager.GetInstance().AddListener(this);
    }

    void Start()
    {
        slider.onValueChanged.AddListener(delegate
        {
            valueChanged_slider(slider);
        });
    }

    void Update()
    {
        switch (StageManager.GetCurStage())
        {
            case StageManager.Stage_Enum.Idle:
            case StageManager.Stage_Enum.Gcode_Create:
            case StageManager.Stage_Enum.Gcode_Send:
                panel_content.SetActive(false);
                break;

            case StageManager.Stage_Enum.Load_Model:
                if (StageManager.GetStageList().Contains(StageManager.Stage_Enum.Gcode_Render)
                    && GcodeRenderManager.GetInstance().GetInfo().isRendered)
                {
                    panel_content.SetActive(true);

                    btn_layerIndex.GetComponent<Transform>().gameObject.SetActive(false);
                    slider.value = slider.maxValue;
                }
                else
                {
                    panel_content.SetActive(false);
                }
                break;

            case StageManager.Stage_Enum.Gcode_Render:

                if (GcodeRenderManager.GetInstance().GetInfo().isRendered)
                {
                    panel_content.SetActive(true);

                    btn_layerIndex.GetComponentInChildren<Text>().text = ((int)slider.value).ToString();
                    btn_layerIndex.GetComponent<Transform>().gameObject.SetActive(true);
                }
                else
                {
                    panel_content.SetActive(false);
                }
                break;
        }
    }

    void valueChanged_slider(Slider sender)
    {
        if (!StageManager.GetStageList().Contains(StageManager.Stage_Enum.Gcode_Render))
        {
            return;
        }

        if (slider.value > Mathf.Floor(slider.maxValue / 1.1f))
        {
            StageManager.SetStage_Load_Model();
            ModelManager.GetInstance().SetPrintable(true);
            GcodeRenderManager.GetInstance().SetActive_gcodeRender(false);
        }
        else
        {
            //printing online
            if (StageManager.GetCurStage() == StageManager.Stage_Enum.Gcode_Send)
            {

            }
            else
            {
                //Load_Model--->Gcode_Render
                //turn on gcode render
                StageManager.SetStage_Gcode_Render();
            }

            ModelManager.GetInstance().SetTranslucent();
            GcodeRenderManager.GetInstance().SetActive_gcodeRender(true);
            GcodeRenderManager.GetInstance().ShowFirstLayers((int)slider.value);
            GcodeRenderManager.GetInstance().DrawLayers();
        }
    }

    /*************** GcodeRenderManager.Listener call back ***************/
    public void OnGcodeParseSucceed()
    {
        Debug.Log("OnGcodeParsed" + "\n");
        Loom.RunAsync(
            () =>
            {
                Thread t = new Thread(renderGcode);
                t.Start();
            }
        );
    }

    public void OnGcodeParseFailed()
    {
        Debug.Log("OnGcodeParseFailed" + "\n");
        AlertMananger.GetInstance().ShowAlertMsg("Unable to generate G-code for this model.");
    }

    private void renderGcode()
    {
        Loom.QueueOnMainThread((param) =>
        {
            GameObject go = GcodeRenderManager.GetInstance().RenderGcode();
            if (go == null)
            {
                return;
            }

            GameObject print_cube = GameObject.Find("Print_cube");
            go.transform.parent = print_cube.transform;
            go.transform.localPosition = new Vector3(-0.5f, -0.5f, -0.5f);
            go.transform.localScale = new Vector3(1, 1, 1) / 125.0f;
            go.transform.localEulerAngles = Vector3.zero;

            slider.maxValue = Mathf.CeilToInt(GcodeRenderManager.GetInstance().GetLayerCount() * 1.1f);
            slider.minValue = 1;

            //show half of model rendered
            slider.value = GcodeRenderManager.GetInstance().GetLayerCount();

            //			VectorLineManager.GetInstance ().SetGcodeRenderTypeColors (null);

            /**fix a bug : load the same stl second time, 
			 * slider.value = GcodeRenderManager.GetInstance ().GetLayerCount () 
			 * will not invoke valueChanged_slider
			 * so cannot "show half of model rendered" 
			 * call GcodeRenderManager.GetInstance ().ShowFirstLayers((int)slider.value) to fix
			 **/
            GcodeRenderManager.GetInstance().ShowFirstLayers((int)slider.value);
        }, null);
    }

    /*************** GcodeSenderManager.Listener call back ***************/
    public void OnSendStarted()
    {
    }

    public void OnSendCompleted()
    {
    }

    public void OnSendStoped()
    {
    }
}
