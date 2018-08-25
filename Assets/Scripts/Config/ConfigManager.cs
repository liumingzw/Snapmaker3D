using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System;

public class ConfigManager
{
    private const string KEY_Adhesion = "KEY_Adhesion";
    private const string KEY_Support_Type = "KEY_Support_Type";
    private const string KEY_Material = "KEY_Material";

    private const string KEY_Custom_bed_temperature = "KEY_Custom_bed_temperature";
    private const string KEY_Custom_bed_temperature_Layer0 = "KEY_Custom_bed_temperature_Layer0";
    private const string KEY_Custom_print_temperature = "KEY_Custom_print_temperature";
    private const string KEY_Custom_print_temperature_Layer0 = "KEY_Custom_print_temperature_Layer0";
    private const string KEY_Custom_print_temperature_Final = "KEY_Custom_print_temperature_Final";

    //bed
    private const int value_bed_temperature_PLA = 50;
    private const int value_bed_temperature_ABS = 80;
    private int value_bed_temperature_Custom = 0;

    //bed layer0
    private const int value_bed_temperature_Layer0_PLA = 50;
    private const int value_bed_temperature_Layer0_ABS = 80;
    private int value_bed_temperature_Layer0_Custom = 0;

    //print temp
    private const int value_print_temperature_PLA = 198;
    private const int value_print_temperature_ABS = 235;
    private int value_print_temperature_Custom = 0;

    //print temp layer0
    private const int value_print_temperature_Layer0_PLA = 200;
    private const int value_print_temperature_Layer0_ABS = 238;
    private int value_print_temperature_Layer0_Custom = 0;

    //print temp final
    private const int value_print_temperature_Final_PLA = 198;
    private const int value_print_temperature_Final_ABS = 235;
    private int value_print_temperature_Final_Custom = 0;

    /********** single mode ************/
    private static ConfigManager _INSTANCE;

    private ConfigManager()
    {
    }

    public static ConfigManager GetInstance()
    {
        if (_INSTANCE == null)
        {
            _INSTANCE = new ConfigManager();
        }
        return _INSTANCE;
    }

    /********** MyPrintConfigBean ************/
    private List<MyPrintConfigBean> _officialMyBeanList = new List<MyPrintConfigBean>();
    private List<MyPrintConfigBean> _customMyBeanList = new List<MyPrintConfigBean>();
    private List<MyPrintConfigBean> _allMyBeanList = new List<MyPrintConfigBean>();

    public MyPrintConfigBean myBean_fastPrint;
    public MyPrintConfigBean myBean_normalQuality;
    public MyPrintConfigBean myBean_highQuality;

    public List<MyPrintConfigBean> GetCustomMyBeanList()
    {
        return _customMyBeanList;
    }

    public void UnarchiveProfiles()
    {
        _officialMyBeanList.Clear();
        _customMyBeanList.Clear();
        _allMyBeanList.Clear();

        //official profiles, extension is .json
        string path_fastPrint = PathManager.configPath_fastPrint();
        string path_normalQuality = PathManager.configPath_normalQuality();
        string path_highQuality = PathManager.configPath_highQuality();

        PrintConfigBean bean_fastPrint = unarchive(path_fastPrint);
        PrintConfigBean bean_normalQuality = unarchive(path_normalQuality);
        PrintConfigBean bean_highQuality = unarchive(path_highQuality);

        myBean_fastPrint = new MyPrintConfigBean(bean_fastPrint, false, path_fastPrint);
        myBean_fastPrint.name = "Fast Print";
        myBean_normalQuality = new MyPrintConfigBean(bean_normalQuality, false, path_normalQuality);
        myBean_normalQuality.name = "Normal Quality";
        myBean_highQuality = new MyPrintConfigBean(bean_highQuality, false, path_highQuality);
        myBean_highQuality.name = "High Quality";

        _officialMyBeanList.Add(myBean_fastPrint);
        _officialMyBeanList.Add(myBean_normalQuality);
        _officialMyBeanList.Add(myBean_highQuality);

        //custom profiles, extension is .snapmaker3dProfile
        string customProfilesDir = PathManager.CustomPrintProfilesArchiveDir();
        DirectoryInfo dirInfo = new DirectoryInfo(customProfilesDir);
        foreach (FileInfo fileInfo in dirInfo.GetFiles())
        {
            string path = fileInfo.FullName;
            //GetExtension return ".xxx"
            if (Path.GetExtension(path) == ("." + Global.extension_Profile))
            {
                try
                {
                    PrintConfigBean bean = unarchive(path);
                    MyPrintConfigBean myBean = new MyPrintConfigBean(bean, true, path);
                    _customMyBeanList.Add(myBean);
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception occur : UnarchiveProfiles e:" + e.ToString() + "\n" + " file path" + path + "\n");
                }
            }
        }

        _allMyBeanList.AddRange(_officialMyBeanList);
        _allMyBeanList.AddRange(_customMyBeanList);

        if (IsMaterial_PLA())
        {
            SetMaterial_PLA();
        }
        else if (IsMaterial_ABS())
        {
            SetMaterial_ABS();
        }
        else if (IsMaterial_Custom())
        {
            SetMaterial_Custom();
        }
        else
        {
            SetMaterial_PLA();
        }

        if (IsAdhesion_brim())
        {
            SetAdhesion_brim();
        }
        else if (IsAdhesion_none())
        {
            SetAdhesion_none();
        }
        else if (IsAdhesion_raft())
        {
            SetAdhesion_raft();
        }
        else if (IsAdhesion_skirt())
        {
            SetAdhesion_skirt();
        }
        else
        {
            SetAdhesion_none();
        }

        if (IsSupport_buildplate())
        {
            SetSupport_buildplate();
        }
        else if (IsSupport_everywhere())
        {
            SetSupport_everywhere();
        }
        else if (IsSupport_none())
        {
            SetSupport_none();
        }
        else
        {
            SetSupport_none();
        }

        value_bed_temperature_Custom = PlayerPrefs.GetInt(KEY_Custom_bed_temperature);
        value_bed_temperature_Layer0_Custom = PlayerPrefs.GetInt(KEY_Custom_bed_temperature_Layer0);

        value_print_temperature_Custom = PlayerPrefs.GetInt(KEY_Custom_print_temperature);
        value_print_temperature_Layer0_Custom = PlayerPrefs.GetInt(KEY_Custom_print_temperature_Layer0);
        value_print_temperature_Final_Custom = PlayerPrefs.GetInt(KEY_Custom_print_temperature_Final);

        //no record, use params of PLA
        if (value_bed_temperature_Custom == 0)
        {
            value_bed_temperature_Custom = value_bed_temperature_PLA;
            PlayerPrefs.SetInt(KEY_Custom_bed_temperature, value_bed_temperature_Custom);
        }

        if (value_bed_temperature_Layer0_Custom == 0)
        {
            value_bed_temperature_Layer0_Custom = value_bed_temperature_Layer0_PLA;
            PlayerPrefs.SetInt(KEY_Custom_bed_temperature_Layer0, value_bed_temperature_Layer0_Custom);
        }

        if (value_print_temperature_Custom == 0)
        {
            value_print_temperature_Custom = value_print_temperature_PLA;
            PlayerPrefs.SetInt(KEY_Custom_print_temperature, value_print_temperature_Custom);
        }

        if (value_print_temperature_Layer0_Custom == 0)
        {
            value_print_temperature_Layer0_Custom = value_print_temperature_Layer0_PLA;
            PlayerPrefs.SetInt(KEY_Custom_print_temperature_Layer0, value_print_temperature_Layer0_Custom);
        }

        if (value_print_temperature_Final_Custom == 0)
        {
            value_print_temperature_Final_Custom = value_print_temperature_Final_PLA;
            PlayerPrefs.SetInt(KEY_Custom_print_temperature_Final, value_print_temperature_Final_Custom);
        }
    }

    private PrintConfigBean unarchive(string path)
    {
        Debug.Log("unarchive:-->" + path + "\n");
        string json = File.ReadAllText(path);
        PrintConfigBean bean = JsonMapper.ToObject<PrintConfigBean>(json);
        return bean;
    }

    public void ArchiveParamsOfCustomMaterial()
    {
        PlayerPrefs.SetInt(KEY_Custom_bed_temperature, value_bed_temperature_Custom);
        PlayerPrefs.SetInt(KEY_Custom_bed_temperature_Layer0, value_bed_temperature_Layer0_Custom);

        PlayerPrefs.SetInt(KEY_Custom_print_temperature, value_print_temperature_Custom);
        PlayerPrefs.SetInt(KEY_Custom_print_temperature_Layer0, value_print_temperature_Layer0_Custom);
        PlayerPrefs.SetInt(KEY_Custom_print_temperature_Final, value_print_temperature_Final_Custom);
    }

    public MyPrintConfigBean GetSelectedMyBean()
    {
        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            if (temp.selected)
            {
                return temp;
            }
        }
        return null;
    }

    /********** adhesion ************/
    public void SetAdhesion_skirt()
    {
        PlayerPrefs.SetString(KEY_Adhesion, "skirt");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.adhesion_type.default_value = "skirt";
        }
    }

    public void SetAdhesion_brim()
    {
        PlayerPrefs.SetString(KEY_Adhesion, "brim");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.adhesion_type.default_value = "brim";
        }
    }

    public void SetAdhesion_raft()
    {
        PlayerPrefs.SetString(KEY_Adhesion, "raft");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.adhesion_type.default_value = "raft";
        }
    }

    public void SetAdhesion_none()
    {
        PlayerPrefs.SetString(KEY_Adhesion, "none");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.adhesion_type.default_value = "none";
        }
    }

    public bool IsAdhesion_skirt()
    {
        return PlayerPrefs.GetString(KEY_Adhesion, "") == "skirt";
    }

    public bool IsAdhesion_brim()
    {
        return PlayerPrefs.GetString(KEY_Adhesion, "") == "brim";
    }

    public bool IsAdhesion_raft()
    {
        return PlayerPrefs.GetString(KEY_Adhesion, "") == "raft";
    }

    public bool IsAdhesion_none()
    {
        return PlayerPrefs.GetString(KEY_Adhesion, "") == "none";
    }

    /********** support ************/
    public void SetSupport_buildplate()
    {
        PlayerPrefs.SetString(KEY_Support_Type, "buildplate");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.support_enable.default_value = true;
            temp.bean.overrides.support_type.default_value = "buildplate";
        }
    }

    public void SetSupport_everywhere()
    {
        PlayerPrefs.SetString(KEY_Support_Type, "everywhere");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.support_enable.default_value = true;
            temp.bean.overrides.support_type.default_value = "everywhere";
        }
    }

    public void SetSupport_none()
    {
        PlayerPrefs.SetString(KEY_Support_Type, "none");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.support_enable.default_value = false;
        }
    }

    public bool IsSupport_buildplate()
    {
        return PlayerPrefs.GetString(KEY_Support_Type, "") == "buildplate";
    }

    public bool IsSupport_everywhere()
    {
        return PlayerPrefs.GetString(KEY_Support_Type, "") == "everywhere";
    }

    public bool IsSupport_none()
    {
        return PlayerPrefs.GetString(KEY_Support_Type, "") == "none";
    }

    /********** material ************/
    public void SetMaterial_ABS()
    {
        PlayerPrefs.SetString(KEY_Material, "ABS");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.material_print_temperature.default_value = value_print_temperature_ABS;
            temp.bean.overrides.material_print_temperature_layer_0.default_value = value_print_temperature_Layer0_ABS;
            temp.bean.overrides.material_final_print_temperature.default_value = value_print_temperature_Final_ABS;

            temp.bean.overrides.material_bed_temperature.default_value = value_bed_temperature_ABS;
            temp.bean.overrides.material_bed_temperature_layer_0.default_value = value_bed_temperature_Layer0_ABS;
        }
    }

    public void SetMaterial_PLA()
    {
        PlayerPrefs.SetString(KEY_Material, "PLA");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.material_print_temperature.default_value = value_print_temperature_PLA;
            temp.bean.overrides.material_print_temperature_layer_0.default_value = value_print_temperature_Layer0_PLA;
            temp.bean.overrides.material_final_print_temperature.default_value = value_print_temperature_Final_PLA;

            temp.bean.overrides.material_bed_temperature.default_value = value_bed_temperature_PLA;
            temp.bean.overrides.material_bed_temperature_layer_0.default_value = value_bed_temperature_Layer0_PLA;
        }
    }

    public void SetMaterial_Custom()
    {
        PlayerPrefs.SetString(KEY_Material, "Custom");

        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.bean.overrides.material_print_temperature.default_value = value_print_temperature_Custom;
            temp.bean.overrides.material_print_temperature_layer_0.default_value = value_print_temperature_Layer0_Custom;
            temp.bean.overrides.material_final_print_temperature.default_value = value_print_temperature_Final_Custom;

            temp.bean.overrides.material_bed_temperature.default_value = value_bed_temperature_Custom;
            temp.bean.overrides.material_bed_temperature_layer_0.default_value = value_bed_temperature_Layer0_Custom;
        }
    }

    public bool IsMaterial_ABS()
    {
        return PlayerPrefs.GetString(KEY_Material, "") == "ABS";
    }

    public bool IsMaterial_PLA()
    {
        return PlayerPrefs.GetString(KEY_Material, "") == "PLA";
    }

    public bool IsMaterial_Custom()
    {
        return PlayerPrefs.GetString(KEY_Material, "") == "Custom";
    }

    /********** set profile ************/
    public void SelectProfile_fastPrint()
    {
        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.selected = false;
        }
        myBean_fastPrint.selected = true;
    }

    public void SelectProfile_normalQuality()
    {
        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.selected = false;
        }
        myBean_normalQuality.selected = true;
    }

    public void SelectProfile_highQuality()
    {
        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.selected = false;
        }
        myBean_highQuality.selected = true;
    }

    public void SelectProfile_customize(int index)
    {
        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            temp.selected = false;
        }
        _customMyBeanList[index].selected = true;
    }

    public void ArchiveCustomProfiles()
    {
        foreach (MyPrintConfigBean temp in _customMyBeanList)
        {
            string newPath = new FileInfo(temp.filePath).DirectoryName + "/" + temp.name + "." + Global.extension_Profile;
            temp.ArchiveForProfile(newPath);
        }
    }

    public void RemoveCustomProfile(MyPrintConfigBean myBean)
    {
        myBean.RemoveSelf();
        _customMyBeanList.Remove(myBean);
        _allMyBeanList.Remove(myBean);
        myBean = null;

        SelectProfile_fastPrint();
    }

    public bool RenameCustomProfile(MyPrintConfigBean myBean, string newName)
    {
        return myBean.Rename(newName.Trim());
    }

    public void DuplicateProfile(MyPrintConfigBean myBean)
    {
        //1.get a avaiable name
        //string oldName = myBean.name;
        string oldName = "Duplicate";
        int index = 1;
        string newName = oldName + " #" + (index);
        while (ConfigManager.GetInstance().IsProfileNameExist(newName))
        {
            newName = oldName + " #" + (++index);
        }

        //2.
        string path = PathManager.CustomPrintProfilesArchiveDir() + "/" + newName + "." + Global.extension_Profile;

        myBean.ArchiveForProfile(path);

        PrintConfigBean newBean = unarchive(path);

        MyPrintConfigBean newMyBean = new MyPrintConfigBean(newBean, true, path);

        _customMyBeanList.Add(newMyBean);
        _allMyBeanList.Add(newMyBean);
    }

    public bool IsProfileNameExist(string name)
    {
        foreach (MyPrintConfigBean temp in _allMyBeanList)
        {
            if (temp.name == name)
            {
                return true;
            }
        }
        return false;
    }

    public MyPrintConfigBean ImportProfile(string importPath)
    {
        try
        {
            //1.get a avaiable name
            //string oldName = myBean.name;
            string oldName = "Import";
            int index = 1;
            string newName = oldName + " #" + (index);
            while (ConfigManager.GetInstance().IsProfileNameExist(newName))
            {
                newName = oldName + " #" + (++index);
            }

            //2.parse profile
            PrintConfigBean newBean = unarchive(importPath);
            string filePath = PathManager.CustomPrintProfilesArchiveDir() + "/" + newName + "." + Global.extension_Profile;
            MyPrintConfigBean newMyBean = new MyPrintConfigBean(newBean, true, filePath);
            newMyBean.ArchiveForProfile(filePath);

            _customMyBeanList.Add(newMyBean);
            _allMyBeanList.Add(newMyBean);

            return newMyBean;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception occured : " + e.ToString() + "\n");
            return null;
        }

        return null;
    }
}
