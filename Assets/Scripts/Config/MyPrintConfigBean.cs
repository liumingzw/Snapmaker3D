using LitJson;
using System.IO;
using UnityEngine;

public class MyPrintConfigBean
{
    public struct RNG
    {
        public double low;
        public double high;

        public RNG(double low, double high)
        {
            this.low = low;
            this.high = high;
        }
    }

    //value range
    public static RNG RNG_layerHeight = new RNG(0.05, 0.2);
    public static RNG RNG_layerHeight_0 = new RNG(0.05, 0.2);
    public static RNG RNG_initial_layer_line_width_factor = new RNG(0.001, 150);

    public static RNG RNG_wallThickness = new RNG(0.4, 4);
    public static RNG RNG_topThickness = new RNG(0.4, 2);
    public static RNG RNG_bottomThickness = new RNG(0.4, 2);

    public static RNG RNG_infillDesnsity = new RNG(0, 100);

    public static RNG RNG_PrintAndTravelSpeed = new RNG(10, 100);

    public static RNG RNG_bedTemp = new RNG(0, 90);
    public static RNG RNG_bedTempInitialLayer = new RNG(0, 90);

    public static RNG RNG_printTemp = new RNG(170, 250);
    public static RNG RNG_printTempInitialLayer = new RNG(170, 250);
    public static RNG RNG_finalPrintTemp = new RNG(170, 250);
    public static RNG RNG_diameter = new RNG(1.75, 3.5);
    public static RNG RNG_flow = new RNG(50, 150);
    public static RNG RNG_retractDis = new RNG(0, 8);
    public static RNG RNG_retractSpeed = new RNG(0, 1000);

    //travel
    public static RNG RNG_z_hop_height = new RNG(0, 10);

    public PrintConfigBean bean;
    public bool writable;
    public bool selected;

    public string filePath;
    public string name;
    public string extension; //".xxx"

    public MyPrintConfigBean()
    {
    }

    public MyPrintConfigBean(PrintConfigBean bean, bool writable, string filePath)
    {
        this.bean = bean;
        this.writable = writable;
        this.filePath = filePath;
        this.name = Path.GetFileNameWithoutExtension(filePath);
        this.extension = Path.GetExtension(filePath);
    }

    /********** Archive ************/
    public void ArchiveForPrint(string path)
    {
        Debug.Log("ArchiveForPrint:-->" + path + "\n");
        Overrides overrides = bean.overrides;
        overrides.machine_start_gcode.default_value = StartAndEndGcodeUtils.GetStartGcode_Config(bean);

        float modelMaxZ = ModelManager.GetInstance().GetModel().GetCurDataSize().z;
        overrides.machine_end_gcode.default_value = StartAndEndGcodeUtils.GetEndGcode_Config(modelMaxZ);

        overrides.wall_line_count.default_value =
           Mathf.RoundToInt((float)(overrides.wall_thickness.default_value / 0.4));

        //0 if infill_sparse_density == 0 
        //else (infill_line_width * 100) / infill_sparse_density * (2 if infill_pattern == 'grid' else (3 if infill_pattern == 'triangles' or infill_pattern == 'cubic' or infill_pattern == 'cubicsubdiv' else (2 if infill_pattern == 'tetrahedral' else 1)))

        if (overrides.infill_sparse_density.default_value < 1)
        {
            overrides.infill_line_distance.default_value = 0;
        }
        else
        {
            double infill_sparse_density = overrides.infill_sparse_density.default_value;
            double infill_line_width = 0.4;
            //use grid
            overrides.infill_line_distance.default_value = (infill_line_width * 100) / infill_sparse_density * 2;
        }

        //string path = Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Config/" + "output.def.json";
        //string path = PathManager.dirPath_Temp () + "output.def.json";
        string json = JsonMapper.ToJson(bean);
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sr = new StreamWriter(fs);
        sr.Write(json);
        sr.Close();
        fs.Close();
    }

    public void ArchiveForProfile(string path)
    {
        Debug.Log("ArchiveForProfile:-->" + path + "\n");
        string json = JsonMapper.ToJson(bean);
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sr = new StreamWriter(fs);
        sr.Write(json);
        sr.Close();
        fs.Close();
    }

    /********** value check ************/
    public bool IsValuesAvailable()
    {
        Overrides overrides = bean.overrides;
        bool b1 =
             overrides.layer_height.default_value >= RNG_layerHeight.low &&
             overrides.layer_height.default_value <= RNG_layerHeight.high;

        bool b2 =
             overrides.wall_thickness.default_value >= RNG_wallThickness.low &&
             overrides.wall_thickness.default_value <= RNG_wallThickness.high;

        bool b3 =
             overrides.top_thickness.default_value >= RNG_topThickness.low &&
             overrides.top_thickness.default_value <= RNG_topThickness.high;

        bool b4 =
             overrides.bottom_thickness.default_value >= RNG_bottomThickness.low &&
             overrides.bottom_thickness.default_value <= RNG_bottomThickness.high;

        bool b5 =
             overrides.infill_sparse_density.default_value >= RNG_infillDesnsity.low &&
             overrides.infill_sparse_density.default_value <= RNG_infillDesnsity.high;

        bool b6 =
             overrides.speed_print.default_value >= RNG_PrintAndTravelSpeed.low &&
             overrides.speed_print.default_value <= RNG_PrintAndTravelSpeed.high;

        bool b7 =
             overrides.speed_infill.default_value >= RNG_PrintAndTravelSpeed.low &&
             overrides.speed_infill.default_value <= RNG_PrintAndTravelSpeed.high;

        bool b8 =
             overrides.speed_wall_0.default_value >= RNG_PrintAndTravelSpeed.low &&
             overrides.speed_wall_0.default_value <= RNG_PrintAndTravelSpeed.high;

        bool b9 =
             overrides.speed_wall_x.default_value >= RNG_PrintAndTravelSpeed.low &&
             overrides.speed_wall_x.default_value <= RNG_PrintAndTravelSpeed.high;

        bool b10 =
             overrides.speed_topbottom.default_value >= RNG_PrintAndTravelSpeed.low &&
             overrides.speed_topbottom.default_value <= RNG_PrintAndTravelSpeed.high;

        bool b11 =
             overrides.speed_travel.default_value >= RNG_PrintAndTravelSpeed.low &&
             overrides.speed_travel.default_value <= RNG_PrintAndTravelSpeed.high;

        bool b22 =
             overrides.speed_print_layer_0.default_value >= RNG_PrintAndTravelSpeed.low &&
             overrides.speed_print_layer_0.default_value <= RNG_PrintAndTravelSpeed.high;

        bool b23 =
             overrides.speed_travel_layer_0.default_value >= RNG_PrintAndTravelSpeed.low &&
             overrides.speed_travel_layer_0.default_value <= RNG_PrintAndTravelSpeed.high;

        bool b12 =
             overrides.material_print_temperature.default_value >= RNG_printTemp.low &&
             overrides.material_print_temperature.default_value <= RNG_printTemp.high;

        bool b13 =
             overrides.material_print_temperature_layer_0.default_value >= RNG_printTempInitialLayer.low &&
             overrides.material_print_temperature_layer_0.default_value <= RNG_printTempInitialLayer.high;

        bool b14 =
             overrides.material_final_print_temperature.default_value >= RNG_finalPrintTemp.low &&
             overrides.material_final_print_temperature.default_value <= RNG_finalPrintTemp.high;

        bool b15 =
             overrides.material_diameter.default_value >= RNG_diameter.low &&
             overrides.material_diameter.default_value <= RNG_diameter.high;

        bool b16 =
             overrides.material_flow.default_value >= RNG_flow.low &&
             overrides.material_flow.default_value <= RNG_flow.high;

        bool b17 = true;
        bool b18 = true;
        //if retraction_enable, then check range
        if (overrides.retraction_enable.default_value)
        {
            b17 =
            overrides.retraction_amount.default_value >= RNG_retractDis.low &&
            overrides.retraction_amount.default_value <= RNG_retractDis.high;

            b18 =
            overrides.retraction_speed.default_value >= RNG_retractSpeed.low &&
            overrides.retraction_speed.default_value <= RNG_retractSpeed.high;
        }

        bool b19 =
             overrides.layer_height_0.default_value >= RNG_layerHeight_0.low &&
             overrides.layer_height_0.default_value <= RNG_layerHeight_0.high;

        bool b20 =
             overrides.initial_layer_line_width_factor.default_value >= RNG_initial_layer_line_width_factor.low &&
             overrides.initial_layer_line_width_factor.default_value <= RNG_initial_layer_line_width_factor.high;

        bool b21 = true;
        //if retraction_enable and retraction_hop_enabled, then check range
        if (overrides.retraction_enable.default_value && overrides.retraction_hop_enabled.default_value)
        {
            b21 =
            overrides.retraction_hop.default_value >= RNG_z_hop_height.low &&
            overrides.retraction_hop.default_value <= RNG_z_hop_height.high;
        }

        return b1 && b2 && b3 && b4 && b5 && b6 && b7 && b8 && b9 && b10
            && b11 && b12 && b13 && b14 && b15 && b16 && b17 && b18 && b19 && b20 && b21
            && b22 && b23;
    }

    public override string ToString()
    {
        string info = "path=" + filePath + "\n" +
            "name=" + name + "\n" +
            "bean=" + bean;
        return info;
    }

    public bool Rename(string newName)
    {
        string oldPath = filePath;
        string newPath = new FileInfo(filePath).DirectoryName + "/" + newName + "." + Global.extension_Profile;
        if (File.Exists(oldPath))
        {
            Debug.Log("Move file:" + oldPath + "-->" + newPath);
            File.Move(oldPath, newPath);
            name = newName;
            filePath = newPath;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemoveSelf()
    {
        File.Delete(filePath);
    }

    public MyPrintConfigBean DeepCopy(){
        //todo
        return Utils.DeepCopy(this);
    }

    //public void DuplicateSelf(string newName)
    //{
    //    MyPrintConfigBean deepCopy = this.DeepCopy();
    //    string newPath = new FileInfo(filePath).DirectoryName + "/" + newName + "." + Global.extension_Profile;
    //    deepCopy.name = newName;
    //    deepCopy.filePath = newPath;
    //    deepCopy.ArchiveForProfile(newPath);

    //}
}
