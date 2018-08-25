using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using System.Text.RegularExpressions;

public class ControlProfileSelect : MonoBehaviour, GcodeCreateManager.Listener
{
    //color
    private Color _textBlue = new Color(48 / 255.0f, 149 / 255.0f, 218 / 255.0f);
    private Color _textBlack = new Color(108 / 255.0f, 108 / 255.0f, 108 / 255.0f);
    private Color _textWhite = Color.white;

    public GameObject panel_content;

    //other widget
    public Dropdown dropdown_materials, dropdown_adhesion, dropdown_support;

    //profile
    public Button btn_fastPrint, btn_normalQuality, btn_highQuality;
    public Dropdown dropdown_customProfiles;

    //preview
    public Button btn_preview;

    /*************** dropdown ***************/
    void valueChanged_materials(Dropdown sender)
    {
        switch (sender.value)
        {
            case 0:
                ConfigManager.GetInstance().SetMaterial_PLA();
                break;
            case 1:
                ConfigManager.GetInstance().SetMaterial_ABS();
                break;
                //		case 2:
                //			ConfigManager.GetInstance ().SetMaterial_Custom ();
                //			break;
        }
    }

    void valueChanged_adhesion(Dropdown sender)
    {
        switch (sender.value)
        {
            case 0:
                ConfigManager.GetInstance().SetAdhesion_skirt();
                break;
            case 1:
                ConfigManager.GetInstance().SetAdhesion_brim();
                break;
            case 2:
                ConfigManager.GetInstance().SetAdhesion_raft();
                break;
            case 3:
                ConfigManager.GetInstance().SetAdhesion_none();
                break;
        }
    }

    void valueChanged_support(Dropdown sender)
    {
        switch (sender.value)
        {
            case 0:
                ConfigManager.GetInstance().SetSupport_buildplate();
                break;
            case 1:
                ConfigManager.GetInstance().SetSupport_everywhere();
                break;
            case 2:
                ConfigManager.GetInstance().SetSupport_none();
                break;
        }
    }

    /*************** profile ***************/
    void onClick_quality_fast()
    {
        ConfigManager.GetInstance().SelectProfile_fastPrint();
    }

    void onClick_quality_normal()
    {
        ConfigManager.GetInstance().SelectProfile_normalQuality();
    }

    void onClick_quality_high()
    {
        ConfigManager.GetInstance().SelectProfile_highQuality();
    }

    /*************** preview ***************/
    void onClick_preview()
    {
        if (ModelManager.GetInstance().GetModel() == null)
        {
            AlertMananger.GetInstance().ShowToast("Please open a file to preview.");
            return;
        }

        if (!ModelManager.GetInstance().GetInfo().printable)
        {
            AlertMananger.GetInstance().ShowToast("Please move, scale or rotate the model so that it's within the build volume.");
            return;
        }

        if (!ConfigManager.GetInstance().GetSelectedMyBean().IsValuesAvailable())
        {
            AlertMananger.GetInstance().ShowToast("Invalid configuration. Please check.");
            GameObject.Find("panel_configDisplay").GetComponent<ControlConfigDisplay>().ShowConfigDisplayPanelAnima();
            return;
        }

        Vector3 size = ModelManager.GetInstance().GetModel().GetCurDataSize();
        if (size.x < 2 && size.y < 2 && size.z < 2)
        {
            AlertMananger.GetInstance().ShowToast("Model is too tiny to print.");
            return;
        }

        //model is too thin to print
        //todo change content of alert msg
        if (ModelManager.GetInstance().GetRenderOperateBean().size.z < ConfigManager.GetInstance().GetSelectedMyBean().bean.overrides.layer_height.default_value)
        {
            AlertMananger.GetInstance().ShowAlertMsg("The thickness of model is less than 1 layer. Modify model or decrease the value of ‘Layer Height’.");
            return;
        }

        string stlFilepath = ModelManager.GetInstance().SaveModelAsStlFile();
        string configJsonFilePath = Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Config/" + "output.def.json";

        ConfigManager.GetInstance().GetSelectedMyBean().ArchiveForPrint(configJsonFilePath);

        if (StageManager.GetStageList().Contains(StageManager.Stage_Enum.Gcode_Render))
        {
            if (!GcodeCreateManager.GetInstance().NeedSlice(stlFilepath, configJsonFilePath))
            {
                AlertMananger.GetInstance().ShowToast("You are already in Preview.");
                return;
            }
        }

        GcodeRenderManager.GetInstance().Destroy();
        GcodeCreateManager.GetInstance().StartSubThread_CreateGcode(stlFilepath, configJsonFilePath);
    }

    /*************** life cycle ***************/
    void Awake()
    {
        GcodeCreateManager.GetInstance().AddListener(this);
    }

    void Start()
    {
        //dropdown
        dropdown_materials.onValueChanged.AddListener(delegate
        {
            valueChanged_materials(dropdown_materials);
        });
        dropdown_adhesion.onValueChanged.AddListener(delegate
        {
            valueChanged_adhesion(dropdown_adhesion);
        });
        dropdown_support.onValueChanged.AddListener(delegate
        {
            valueChanged_support(dropdown_support);
        });

        //profile
        btn_fastPrint.GetComponent<Button>().onClick.AddListener(onClick_quality_fast);
        btn_normalQuality.GetComponent<Button>().onClick.AddListener(onClick_quality_normal);
        btn_highQuality.GetComponent<Button>().onClick.AddListener(onClick_quality_high);

        /* 
         * tips written Walker.
         * 
         * if alwaysCallback is 'true', then invoke 'onValueChanged' when click the same item(value not changed)
         * if you want to use the function,
         * you must change source code of UGUI and compile to get a new .dll file,
         * then replace the .dll file in your Unity installed
         * 
         * key source code: seen at UGUI_Modify/Dropdown.cs
         * doc-1: https://bitbucket.org/Unity-Technologies/ui
         */
        //dropdown_customProfiles.alwaysCallback = true;
            
        updateDropdown();

        dropdown_customProfiles.onValueChanged.AddListener(delegate
        {
            valueChanged_customProfiles(dropdown_customProfiles);
        });

        //preview 
        btn_preview.GetComponent<Button>().onClick.AddListener(onClick_preview);
        btn_preview.GetComponentInChildren<Text>().color = this._textWhite;
    }

    void Update()
    {
        switch (StageManager.GetCurStage())
        {
            case StageManager.Stage_Enum.Idle:
            case StageManager.Stage_Enum.Load_Model:
                panel_content.SetActive(true);
                break;

            case StageManager.Stage_Enum.Gcode_Create:
                //render gcode is following, so do not set apart ‘creating’ from ‘created’
                panel_content.SetActive(false);
                break;

            case StageManager.Stage_Enum.Gcode_Render:
                if (GcodeRenderManager.GetInstance().GetInfo().isParsing)
                {
                    //rendering
                    panel_content.SetActive(false);
                }
                else
                {
                    //rendered
                    panel_content.SetActive(true);
                }
                break;

            case StageManager.Stage_Enum.Gcode_Send:
                panel_content.SetActive(false);
                break;
        }

        //material
        if (ConfigManager.GetInstance().IsMaterial_PLA())
        {
            dropdown_materials.value = 0;
        }
        else if (ConfigManager.GetInstance().IsMaterial_ABS())
        {
            dropdown_materials.value = 1;
        }
        //		else if (ConfigManager.GetInstance ().IsMaterial_Custom ()) {
        //			dropdown_materials.value = 2;
        //		}

        //adhesion
        if (ConfigManager.GetInstance().IsAdhesion_skirt())
        {
            dropdown_adhesion.value = 0;
        }
        else if (ConfigManager.GetInstance().IsAdhesion_brim())
        {
            dropdown_adhesion.value = 1;
        }
        else if (ConfigManager.GetInstance().IsAdhesion_raft())
        {
            dropdown_adhesion.value = 2;
        }
        else if (ConfigManager.GetInstance().IsAdhesion_none())
        {
            dropdown_adhesion.value = 3;
        }

        //support
        if (ConfigManager.GetInstance().IsSupport_buildplate())
        {
            dropdown_support.value = 0;
        }
        else if (ConfigManager.GetInstance().IsSupport_everywhere())
        {
            dropdown_support.value = 1;
        }
        else if (ConfigManager.GetInstance().IsSupport_none())
        {
            dropdown_support.value = 2;
        }

        //profile
        if (ConfigManager.GetInstance().myBean_fastPrint.selected)
        {
            btn_fastPrint.interactable = false;
            btn_fastPrint.GetComponentInChildren<Text>().color = _textBlue;

            btn_normalQuality.interactable = true;
            btn_normalQuality.GetComponentInChildren<Text>().color = _textBlack;

            btn_highQuality.interactable = true;
            btn_highQuality.GetComponentInChildren<Text>().color = _textBlack;

            dropdown_customProfiles.GetComponentInChildren<Text>().color = _textBlack;

        }
        else if (ConfigManager.GetInstance().myBean_normalQuality.selected)
        {
            btn_fastPrint.interactable = true;
            btn_fastPrint.GetComponentInChildren<Text>().color = _textBlack;

            btn_normalQuality.interactable = false;
            btn_normalQuality.GetComponentInChildren<Text>().color = _textBlue;

            btn_highQuality.interactable = true;
            btn_highQuality.GetComponentInChildren<Text>().color = _textBlack;

            dropdown_customProfiles.GetComponentInChildren<Text>().color = _textBlack;

        }
        else if (ConfigManager.GetInstance().myBean_highQuality.selected)
        {
            btn_fastPrint.interactable = true;
            btn_fastPrint.GetComponentInChildren<Text>().color = _textBlack;

            btn_normalQuality.interactable = true;
            btn_normalQuality.GetComponentInChildren<Text>().color = _textBlack;

            btn_highQuality.interactable = false;
            btn_highQuality.GetComponentInChildren<Text>().color = _textBlue;

            dropdown_customProfiles.GetComponentInChildren<Text>().color = _textBlack;
        }
        else
        {
            //select custom profile
            btn_fastPrint.interactable = true;
            btn_fastPrint.GetComponentInChildren<Text>().color = _textBlack;

            btn_normalQuality.interactable = true;
            btn_normalQuality.GetComponentInChildren<Text>().color = _textBlack;

            btn_highQuality.interactable = true;
            btn_highQuality.GetComponentInChildren<Text>().color = _textBlack;

            dropdown_customProfiles.GetComponentInChildren<Text>().color = _textBlue;

            for (int i = 0; i < ConfigManager.GetInstance().GetCustomMyBeanList().Count; i++)
            {
                if (ConfigManager.GetInstance().GetCustomMyBeanList()[i].selected)
                {
                    dropdown_customProfiles.value = i;
                    break;
                }
            }
        }

        updateDropdown();
    }

    /*************** GcodeCreateManager.Listener: callback ***************/
    //on sub thread
    public void OnGcodeCreated(GcodeCreateBean gcodeBean)
    {
        Debug.Log("@@ OnGcodeCreated" + "\n");

        GcodeRenderManager.GetInstance().StartParseGcodeFile(gcodeBean.gcodePath);
    }

    public void OnGcodeExist(GcodeCreateBean gcodeBean)
    {
        Debug.Log("@@ OnGcodeExist" + "\n");
    }


    private void updateDropdown()
    {
        List<string> list = new List<string>();
        foreach (MyPrintConfigBean myBean in ConfigManager.GetInstance().GetCustomMyBeanList())
        {
            list.Add(myBean.name);
        }

        dropdown_customProfiles.options.Clear();
        Dropdown.OptionData tempData = null;
        for (int i = 0; i < list.Count; i++)
        {
            tempData = new Dropdown.OptionData();
            tempData.text = list[i];
            dropdown_customProfiles.options.Add(tempData);
        }
        dropdown_customProfiles.RefreshShownValue();
    }

    void valueChanged_customProfiles(Dropdown sender)
    {
        ConfigManager.GetInstance().SelectProfile_customize(sender.value);
    }
}
