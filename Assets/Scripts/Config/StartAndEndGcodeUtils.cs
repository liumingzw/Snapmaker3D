using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAndEndGcodeUtils
{

    public static string GetEndGcode_Config(float modelMaxZ)
    {
        float targetX = 0;
        float targetY = Global.GetInstance().GetPrinterParamsStruct().size.y;
        float targetZ =
            (modelMaxZ + 10 <= Global.GetInstance().GetPrinterParamsStruct().size.z) ?
            (modelMaxZ + 10) :
            (125);

        return
        "\n" +
        ";End GCode begin" +
        "\n" +
        "M104 S0 ;extruder heater off" +
        "\n" +
        "M140 S0 ;heated bed heater off (if you have it)" +
        "\n" +
        "G90 ;absolute positioning" +
        "\n" +
        "G92 E0" +
        "\n" +
        "G1 E-1 F300 ;retract the filament a bit before lifting the nozzle, to release some of the pressure" +
        "\n" +
        "G1 Z" + targetZ + " E-1 F{speed_travel} ;move Z up a bit and retract filament even more" +
        "\n" +
        "G1 X" + targetX + " F3000 ;move X to min endstops, so the head is out of the way" +
        "\n" +
        "G1 Y" + targetY + " F3000 ;so the head is out of the way and Plate is moved forward" +
        "\n" +
        "M84 ;steppers off" +
        "\n" +
        ";End GCode end" +
        "\n";
    }

    public static string GetStartGcode_Config(PrintConfigBean bean)
    {
        bool bedEnable = bean.overrides.machine_heated_bed.default_value;

        double hotendTemp_Layer0 = bean.overrides.material_print_temperature_layer_0.default_value;

        double bedTemp_Layer0 = bean.overrides.material_bed_temperature_layer_0.default_value;

        string setTempCode = "";

        /***** 1.set bed temperature and not wait to reach the target temperature
			 * 2.set hotend temperature and wait to reach the target temperature
			 * 3.set bed temperature and wait to reach the target temperature
			 * bed:
			 * M190 wait
			 * M140 not wait
			 * hotend:
			 * M109 wait
			 * M104 not wait
			 * example:
			 * M140 S60
			 * M109 S200
			 * M190 S60
			*/
        if (bedEnable)
        {
            setTempCode =
            "M140 S" + bedTemp_Layer0 +
            "\n" +
            "M109 S" + hotendTemp_Layer0 +
            "\n" +
            "M190 S" + bedTemp_Layer0;
        }
        else
        {
            setTempCode = "M109 S" + hotendTemp_Layer0;
        }

        return
        "\n" +
        ";Start GCode begin" +
        "\n" +
        setTempCode +
        "\n" +
        "G28 ;Home" +
        "\n" +
        "G90 ;absolute positioning" +
        "\n" +
        "G1 X-4 Y-4" +
        "\n" +
        "G1 Z0 F3000" +
        "\n" +
        "G92 E0" +
        "\n" +
        "G1 F200 E20" +
        "\n" +
        "G92 E0" +
        "\n" +
        ";Start GCode end" +
        "\n";
    }

    public static List<string> GetEndGcode_StopPrint(float modelMaxZ)
    {
        string endgcode = GetEndGcode_Config(modelMaxZ);
        string[] array = endgcode.Split('\n');
        List<string> gcodeList = new List<string>(array);
        return gcodeList;
    }

}
