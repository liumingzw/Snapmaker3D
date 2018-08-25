using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class ControlConfigDisplay : MonoBehaviour, AlertMananger.Listener
{
    private const int infill_value_hollow = 8;
    private const int infill_value_light = 15;
    private const int infill_value_dense = 25;
    private const int infill_value_solid = 80;

    private Sprite sp_hollow, sp_hollow_hover, sp_light, sp_light_hover, sp_dense, sp_dense_hover, sp_solid, sp_solid_hover;

    private enum AnimationState_Enum
    {
        Changing,
        Shown,
        Hidden,
        None,
    }

    private AnimationState_Enum _animationState = AnimationState_Enum.None;

    /********** left ************/
    //quality
    public InputField input_layerHeight, input_layerHeight_0, input_initial_layer_line_width_factor;

    //shell
    public InputField input_wallThickness, input_topThickness, input_bottomThickness;

    //infill
    public InputField input_infillDesnsity;
    public Button btn_infill_hollow, btn_infill_light, btn_infill_dense, btn_infill_solid;

    //speed
    public InputField input_printSpeed, input_infillSpeed, input_outerWallSpeed, input_innerWallSpeed,
    input_topBottomSpeed, input_travelSpeed, input_initialLayerPrintSpeed, input_initialLayerTravelSpeed;

    //edit
    public InputField input_name;
    public Button btn_duplicate, btn_remove, btn_import, btn_export, btn_edit;

    /********** right ************/
    //material
    public Toggle toggle_enableHeatedBed;
    public InputField input_bedTemp, input_bedTempInitialLayer;
    public InputField input_printTemp, input_printTempInitialLayer, input_finalPrintTemp, input_diameter, input_flow;
    public InputField input_retractDis, input_retractSpeed;
    public Toggle toggle_enableRetract, toggle_retractLayerChange;

    //travel
    public Toggle toggle_z_hop_when_retract;
    public InputField input_z_hop_height;

    //surface mode
    public Toggle toggle_spiralize, toggle_flowMeshSurface;

    //param description panel
    public GameObject panel_paramDescription;
    public Text text_title, text_msg;

    //other widget
    public Button btn_arrow;

    //others
    private Color _color_error = new Color(223 / 255.0f, 57 / 255.0f, 0);
    private Color _color_ok = Color.white;

    private Color _color_text_disable = new Color(176 / 255.0f, 176 / 255.0f, 176 / 255.0f);
    private Color _color_text_able = new Color(50 / 255.0f, 50 / 255.0f, 50 / 255.0f);

    private SpriteState ss_arrowDown, ss_arrowUp;
    private Sprite sp_arrowDown, sp_arrowUp;

    /********** left widget ************/
    private void onEndEdit_left(InputField input)
    {
        double value = string.IsNullOrEmpty(input.text) ? 0 : double.Parse(input.text);

        PrintConfigBean bean = ConfigManager.GetInstance().GetSelectedMyBean().bean;

        if (input == input_layerHeight)
        {
            //quality
            bean.overrides.layer_height.default_value = value;

        }
        else if (input == input_layerHeight_0)
        {

            bean.overrides.layer_height_0.default_value = value;

        }
        else if (input == input_initial_layer_line_width_factor)
        {

            bean.overrides.initial_layer_line_width_factor.default_value = value;

        }
        else if (input == input_wallThickness)
        {
            //shell
            bean.overrides.wall_thickness.default_value = value;

        }
        else if (input == input_topThickness)
        {

            bean.overrides.top_thickness.default_value = value;

        }
        else if (input == input_bottomThickness)
        {

            bean.overrides.bottom_thickness.default_value = value;

        }
        else if (input == input_infillDesnsity)
        {
            //infill
            bean.overrides.infill_sparse_density.default_value = value;

        }
        else if (input == input_printSpeed)
        {
            //speed
            bean.overrides.speed_print.default_value = value;

        }
        else if (input == input_infillSpeed)
        {

            bean.overrides.speed_infill.default_value = value;

        }
        else if (input == input_outerWallSpeed)
        {

            bean.overrides.speed_wall_0.default_value = value;

        }
        else if (input == input_innerWallSpeed)
        {

            bean.overrides.speed_wall_x.default_value = value;

        }
        else if (input == input_topBottomSpeed)
        {

            bean.overrides.speed_topbottom.default_value = value;

        }
        else if (input == input_travelSpeed)
        {

            bean.overrides.speed_travel.default_value = value;

        }
        else

            if (input == input_initialLayerPrintSpeed)
        {

            bean.overrides.speed_print_layer_0.default_value = value;

        }
        else if (input == input_initialLayerTravelSpeed)
        {

            bean.overrides.speed_travel_layer_0.default_value = value;

        }
    }

    /********** right widget ************/
    private void onEndEdit_right(InputField input)
    {
        double value = string.IsNullOrEmpty(input.text) ? 0 : double.Parse(input.text);

        PrintConfigBean bean = ConfigManager.GetInstance().GetSelectedMyBean().bean;

        if (input == input_bedTemp)
        {
            //material
            bean.overrides.material_bed_temperature.default_value = value;

        }
        else if (input == input_bedTempInitialLayer)
        {

            bean.overrides.material_bed_temperature_layer_0.default_value = value;

        }
        else if (input == input_printTemp)
        {

            bean.overrides.material_print_temperature.default_value = value;

        }
        else if (input == input_printTempInitialLayer)
        {

            bean.overrides.material_print_temperature_layer_0.default_value = value;

        }
        else if (input == input_finalPrintTemp)
        {

            bean.overrides.material_final_print_temperature.default_value = value;

        }
        else if (input == input_diameter)
        {

            bean.overrides.material_diameter.default_value = value;

        }
        else if (input == input_flow)
        {

            bean.overrides.material_flow.default_value = value;

        }
        else if (input == input_retractDis)
        {

            bean.overrides.retraction_amount.default_value = value;

        }
        else if (input == input_retractSpeed)
        {

            bean.overrides.retraction_speed.default_value = value;

        }
        else if (input == input_z_hop_height)
        {
            //travel
            bean.overrides.retraction_hop.default_value = value;

        }
    }

    private void toggleChanged(Toggle sender)
    {
        PrintConfigBean _bean = ConfigManager.GetInstance().GetSelectedMyBean().bean;
        if (sender == toggle_enableHeatedBed)
        {
            _bean.overrides.machine_heated_bed.default_value = sender.isOn;
        }
        else if (sender == toggle_enableRetract)
        {
            //material
            _bean.overrides.retraction_enable.default_value = sender.isOn;

        }
        else if (sender == toggle_retractLayerChange)
        {

            _bean.overrides.retract_at_layer_change.default_value = sender.isOn;

        }
        else if (sender == toggle_spiralize)
        {

            _bean.overrides.magic_spiralize.default_value = sender.isOn;

        }
        else if (sender == toggle_flowMeshSurface)
        {

            _bean.overrides.magic_mesh_surface_mode.default_value =
                    sender.isOn ? "surface" : "normal";
        }
        else if (sender == toggle_z_hop_when_retract)
        {
            //travel
            _bean.overrides.retraction_hop_enabled.default_value = sender.isOn;
        }
    }

    /********** other widget ************/
    void onClick_arrow()
    {
        switch (_animationState)
        {
            case AnimationState_Enum.Changing:
                break;

            case AnimationState_Enum.Shown:
                HideConfigDisplayPanelAnima();
                break;
            case AnimationState_Enum.Hidden:
                ShowConfigDisplayPanelAnima();
                break;
        }
    }

    /********** param description panel ************/
    void showParamDescriptionPanel(Selectable widget)
    {
        List<string> list = getDescription(widget);
        if (list == null)
        {
            return;
        }
        panel_paramDescription.GetComponent<Transform>().gameObject.SetActive(true);

        string title = list[0];
        string msg = list[1];
        text_title.text = title;
        text_msg.text = msg;
        panel_paramDescription.GetComponent<Transform>().position = widget.GetComponent<Transform>().position;
    }

    void hideParamDescriptionPanel()
    {
        panel_paramDescription.GetComponent<Transform>().gameObject.SetActive(false);
    }

    /********** others ************/
    string range = "Range: ";
    string recommand = "Recommand: ";

    private List<string> getDescription(Selectable widget)
    {
        string title = null;
        string msg = null;

        //left
        if (widget == input_layerHeight)
        {
            title = "Layer Height";
            msg = "The height of each layer in mm. The higher the layer is, the lower resolution you will get and the less time it will take." +
            "\n" +
            recommand + "0.15" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_layerHeight.low, MyPrintConfigBean.RNG_layerHeight.high);

        }
        else if (widget == input_layerHeight_0)
        {
            title = "Initial Layer Height";
            msg = "The height of the initial layer in mm. A thicker initial layer makes the print easier to stick to the heated bed." +
            "\n" +
            recommand + "0.1" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_layerHeight_0.low, MyPrintConfigBean.RNG_layerHeight_0.high);
        }
        else if (widget == input_initial_layer_line_width_factor)
        {
            title = "Initial Layer Line Width";
            msg = "Multiplier of the line width on the first layer. Increasing this could improve bed adhesion." +
            "\n" +
            recommand + "100" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_initial_layer_line_width_factor.low, MyPrintConfigBean.RNG_initial_layer_line_width_factor.high);
        }
        else if (widget == input_wallThickness)
        {
            title = "Wall Thickness";
            msg = "The thickness of the wall horizontally. The bigger this value is, the more solid the print will be." +
            "\n" +
            recommand + "0.8" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_wallThickness.low, MyPrintConfigBean.RNG_wallThickness.high);

        }
        else if (widget == input_topThickness)
        {
            title = "Top Thickness";
            msg = "The thickness of the top layers. The bigger this value is, the more solid the top will be." +
            "\n" +
            recommand + "0.8" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_topThickness.low, MyPrintConfigBean.RNG_topThickness.high);

        }
        else if (widget == input_bottomThickness)
        {
            title = "Bottom Thickness";
            msg = "The thickness of the bottom layers. The bigger this value is, the more solid the bottom will be." +
            "\n" +
            recommand + "0.6" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_bottomThickness.low, MyPrintConfigBean.RNG_bottomThickness.high);

        }
        else if (widget == input_infillDesnsity)
        {
            title = "Infill Density";
            msg = "Adjusts the infill density of the print. The bigger this value is, the more solid the print will be." +
            "\n" +
            recommand + "20" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_infillDesnsity.low, MyPrintConfigBean.RNG_infillDesnsity.high);

        }
        else if (widget == input_printSpeed)
        {
            title = "Print Speed";
            msg = "Specifies all the speeds except the following ones." +
            "\n" +
            recommand + "40" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_PrintAndTravelSpeed.low, MyPrintConfigBean.RNG_PrintAndTravelSpeed.high);

        }
        else if (widget == input_infillSpeed)
        {
            title = "Infill Speed";
            msg = "The speed at which the infill is printed. The higher this speed is, the less time it will take." +
            "\n" +
            recommand + "40" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_PrintAndTravelSpeed.low, MyPrintConfigBean.RNG_PrintAndTravelSpeed.high);

        }
        else if (widget == input_outerWallSpeed)
        {
            title = "Outer Wall Speed";
            msg = "The speed at which the outer wall is printed. The lower this speed is, the higher quality of the skin you will get." +
            "\n" +
            recommand + "30" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_PrintAndTravelSpeed.low, MyPrintConfigBean.RNG_PrintAndTravelSpeed.high);

        }
        else if (widget == input_innerWallSpeed)
        {
            title = "Inner Wall Speed";
            msg = "The speed at which the inner wall is printed. The higher this speed is, the less time it will take. It is recommended to set this value between the outer wall speed and infilll speed." +
            "\n" +
            recommand + "45" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_PrintAndTravelSpeed.low, MyPrintConfigBean.RNG_PrintAndTravelSpeed.high);

        }
        else if (widget == input_topBottomSpeed)
        {
            title = "Top / Bottom Speed";
            msg = "The speed at which the top/bottom wall is printed." +
            "\n" +
            recommand + "15" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_PrintAndTravelSpeed.low, MyPrintConfigBean.RNG_PrintAndTravelSpeed.high);

        }
        else if (widget == input_travelSpeed)
        {
            title = "Travel Speed";
            msg = "The speed at which the printer travels through a non-printed area." +
            "\n" +
            recommand + "80" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_PrintAndTravelSpeed.low, MyPrintConfigBean.RNG_PrintAndTravelSpeed.high);

        }


        else if (widget == input_initialLayerPrintSpeed)
        {
            title = "Initial Layer Print Speed";
            msg = "The speed of printing for the initial layer. A lower value is advised to improve adhesion to the build plate." +
            "\n" +
            recommand + "15" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_PrintAndTravelSpeed.low, MyPrintConfigBean.RNG_PrintAndTravelSpeed.high);

        }
        else if (widget == input_initialLayerTravelSpeed)
        {
            title = "Initial Layer Travel Speed";
            msg = "The speed of travel moves in the initial layer. A lower value is advised to prevent pulling previously printed parts away from the build plate. The value of this setting can automatically be calculated from the ratio between the Travel Speed and the Print Speed." +
            "\n" +
            recommand + "40" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_PrintAndTravelSpeed.low, MyPrintConfigBean.RNG_PrintAndTravelSpeed.high);

        }

        //right
        if (widget == toggle_enableHeatedBed)
        {
            title = "Enable Heated Bed";
            msg = "Enable the heated bed.";

        }
        else if (widget == input_bedTemp)
        {
            title = "Build Plate Temperature";
            msg = "The temperature of the heated bed when the printer is printing." +
            "\n" +
            recommand + "50" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_bedTemp.low, MyPrintConfigBean.RNG_bedTemp.high);

        }
        else if (widget == input_bedTempInitialLayer)
        {
            title = "Build Plate Temperature Initial Layer";
            msg = "The temperature of the heated bed when the printer is printing the first layer. It is recommended to set it a little higher than the Heated Bed Temperature. It helps the print stick to the heated bed." +
            "\n" +
            recommand + "60" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_bedTempInitialLayer.low, MyPrintConfigBean.RNG_bedTempInitialLayer.high);

        }
        else if (widget == input_printTemp)
        {
            title = "Printing Temperature";
            msg = "The temperature of the extruder when it is printing." +
            "\n" +
            recommand + "210" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_printTemp.low, MyPrintConfigBean.RNG_printTemp.high);

        }
        else if (widget == input_printTempInitialLayer)
        {
            title = "Printing Temperature Initial Layer";
            msg = "The temperature of the extruder when it is printing the first layer. It is recommended to set it a little higher than the Printing Temperature." +
            "\n" +
            recommand + "210" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_printTempInitialLayer.low, MyPrintConfigBean.RNG_printTempInitialLayer.high);

        }
        else if (widget == input_finalPrintTemp)
        {
            title = "Final Printing Temperature";
            msg = "The temperature of the extruder when the printing is almost finished. It is recommended to set it a little lower than the Printing Temperature." +
            "\n" +
            recommand + "210" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_finalPrintTemp.low, MyPrintConfigBean.RNG_finalPrintTemp.high);

        }
        else if (widget == input_diameter)
        {
            title = "Diameter";
            msg = "Enter the diameter of the used filament." +
            "\n" +
            recommand + "1.75" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_diameter.low, MyPrintConfigBean.RNG_diameter.high);

        }
        else if (widget == input_flow)
        {
            title = "Flow";
            msg = "The percentage of the filament is extruded. If the extruded filament is too little, enter a bigger value." +
            "\n" +
            recommand + "100" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_flow.low, MyPrintConfigBean.RNG_flow.high);

        }
        else if (widget == input_retractDis)
        {
            title = "Retraction Distance";
            msg = "The retracted length of the filament during one retraction. " +
            "\n" +
            recommand + "4.5" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_retractDis.low, MyPrintConfigBean.RNG_retractDis.high);

        }
        else if (widget == input_retractSpeed)
        {
            title = "Retraction Speed";
            msg = "The speed at which the filament is retracted and primed during one retraction." +
            "\n" +
            recommand + "40" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_retractSpeed.low, MyPrintConfigBean.RNG_retractSpeed.high);

        }
        else if (widget == toggle_enableRetract)
        {
            title = "Enable Retraction";
            msg = "Enable to retract the filament when the nozzle is travelling over a non-printed area to mitigate stringing.";

        }
        else if (widget == toggle_retractLayerChange)
        {
            title = "Retract at Layer Change";
            msg = "Enable to retract the filament when the nozzle is moving to the next layer. It is recommended to enable this function when there is a thin wall in your print.";

        }
        else if (widget == toggle_spiralize)
        {
            title = "Spiralize Outer Contour";
            msg = "Enable to smooth the spiralized surface of the print. Please note that surface detailes will get blurry when this function is enabled.";

        }
        else if (widget == toggle_flowMeshSurface)
        {
            title = "Surface Mode";
            msg = "Enable to turn a solid model into single wall tracing the mesh surface with no infill and no top/bottom skin. If this function and Spiralize the Outer Contour are both enabled, the printer will print a single wall with no infill and no top/bottom skin but with better quality.";

        }
        //travel
        else if (widget == input_z_hop_height)
        {
            title = "Z Hop Height";
            msg = "The height difference when performing a Z Hop." +
            "\n" +
            recommand + "1" +
            "\n" +
            string.Format(range + "[{0}, {1}]", MyPrintConfigBean.RNG_z_hop_height.low, MyPrintConfigBean.RNG_z_hop_height.high);

        }
        else if (widget == toggle_z_hop_when_retract)
        {
            title = "Z Hop During Retraction";
            msg = "When enabled, the 3D Printing Module is raised to create clearance between the nozzle and the print during a retraction. It prevents the nozzle from hitting the print when the nozzle is travelling over a non-printed area." +
            "\n" +
            "Note: This can be enabled only when Retraction is enabled.";
        }
        //smooth_spiralized_contours
        //Smooth Spiralized Contours
        //Smooth the spiralized contours to reduce the visibility of the Z seam (the Z-seam should be barely visible on the print but will still be visible in the layer view). Note that smoothing will tend to blur fine surface details.

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(msg))
        {
            return null;
        }

        List<string> list = new List<string>();
        list.Add(title);
        list.Add(msg);
        return list;
    }

    private bool checkValueRange(InputField input)
    {
        bool avaiable = false;

        //1.empty
        if (string.IsNullOrEmpty(input.text))
        {
            input.text = "0";
        }
        //2.minus sign
        if (input.text == "-")
        {
            return false;
        }
        //3.negative
        double value = double.Parse(input.text);
        if (value < 0)
        {
            return avaiable;
        }
        //4.check range
        if (input == input_layerHeight)
        {
            //left widget
            avaiable = value >= MyPrintConfigBean.RNG_layerHeight.low && value <= MyPrintConfigBean.RNG_layerHeight.high;

        }
        else if (input == input_layerHeight_0)
        {
            avaiable = value >= MyPrintConfigBean.RNG_layerHeight_0.low && value <= MyPrintConfigBean.RNG_layerHeight_0.high;

        }
        else if (input == input_initial_layer_line_width_factor)
        {
            avaiable = value >= MyPrintConfigBean.RNG_initial_layer_line_width_factor.low && value <= MyPrintConfigBean.RNG_initial_layer_line_width_factor.high;

        }
        else if (input == input_wallThickness)
        {
            avaiable = value >= MyPrintConfigBean.RNG_wallThickness.low && value <= MyPrintConfigBean.RNG_wallThickness.high;

        }
        else if (input == input_topThickness)
        {
            avaiable = value >= MyPrintConfigBean.RNG_topThickness.low && value <= MyPrintConfigBean.RNG_topThickness.high;

        }
        else if (input == input_bottomThickness)
        {
            avaiable = value >= MyPrintConfigBean.RNG_bottomThickness.low && value <= MyPrintConfigBean.RNG_bottomThickness.high;

        }
        else if (input == input_infillDesnsity)
        {
            avaiable = value >= MyPrintConfigBean.RNG_infillDesnsity.low && value <= MyPrintConfigBean.RNG_infillDesnsity.high;

        }
        else if (input == input_printSpeed)
        {
            avaiable = value >= MyPrintConfigBean.RNG_PrintAndTravelSpeed.low && value <= MyPrintConfigBean.RNG_PrintAndTravelSpeed.high;

        }
        else if (input == input_infillSpeed)
        {
            avaiable = value >= MyPrintConfigBean.RNG_PrintAndTravelSpeed.low && value <= MyPrintConfigBean.RNG_PrintAndTravelSpeed.high;

        }
        else if (input == input_outerWallSpeed)
        {
            avaiable = value >= MyPrintConfigBean.RNG_PrintAndTravelSpeed.low && value <= MyPrintConfigBean.RNG_PrintAndTravelSpeed.high;

        }
        else if (input == input_innerWallSpeed)
        {
            avaiable = value >= MyPrintConfigBean.RNG_PrintAndTravelSpeed.low && value <= MyPrintConfigBean.RNG_PrintAndTravelSpeed.high;

        }
        else if (input == input_topBottomSpeed)
        {
            avaiable = value >= MyPrintConfigBean.RNG_PrintAndTravelSpeed.low && value <= MyPrintConfigBean.RNG_PrintAndTravelSpeed.high;

        }
        else if (input == input_travelSpeed)
        {
            avaiable = value >= MyPrintConfigBean.RNG_PrintAndTravelSpeed.low && value <= MyPrintConfigBean.RNG_PrintAndTravelSpeed.high;

        }
        else if (input == input_initialLayerPrintSpeed)
        {
            avaiable = value >= MyPrintConfigBean.RNG_PrintAndTravelSpeed.low && value <= MyPrintConfigBean.RNG_PrintAndTravelSpeed.high;

        }
        else if (input == input_initialLayerTravelSpeed)
        {
            avaiable = value >= MyPrintConfigBean.RNG_PrintAndTravelSpeed.low && value <= MyPrintConfigBean.RNG_PrintAndTravelSpeed.high;

        }
        else if (input == input_bedTemp)
        {
            //right widget
            avaiable = value >= MyPrintConfigBean.RNG_bedTemp.low && value <= MyPrintConfigBean.RNG_bedTemp.high;

        }
        else if (input == input_bedTempInitialLayer)
        {
            avaiable = value >= MyPrintConfigBean.RNG_bedTempInitialLayer.low && value <= MyPrintConfigBean.RNG_bedTempInitialLayer.high;

        }
        else if (input == input_printTemp)
        {

            avaiable = value >= MyPrintConfigBean.RNG_printTemp.low && value <= MyPrintConfigBean.RNG_printTemp.high;

        }
        else if (input == input_printTempInitialLayer)
        {
            avaiable = value >= MyPrintConfigBean.RNG_printTempInitialLayer.low && value <= MyPrintConfigBean.RNG_printTempInitialLayer.high;

        }
        else if (input == input_finalPrintTemp)
        {
            avaiable = value >= MyPrintConfigBean.RNG_finalPrintTemp.low && value <= MyPrintConfigBean.RNG_finalPrintTemp.high;

        }
        else if (input == input_diameter)
        {
            avaiable = value >= MyPrintConfigBean.RNG_diameter.low && value <= MyPrintConfigBean.RNG_diameter.high;

        }
        else if (input == input_flow)
        {
            avaiable = value >= MyPrintConfigBean.RNG_flow.low && value <= MyPrintConfigBean.RNG_flow.high;

        }
        else if (input == input_retractDis)
        {
            avaiable = value >= MyPrintConfigBean.RNG_retractDis.low && value <= MyPrintConfigBean.RNG_retractDis.high;

        }
        else if (input == input_retractSpeed)
        {
            avaiable = value >= MyPrintConfigBean.RNG_retractSpeed.low && value <= MyPrintConfigBean.RNG_retractSpeed.high;

        }
        else if (input == input_z_hop_height)
        {
            //travel
            avaiable = value >= MyPrintConfigBean.RNG_z_hop_height.low && value <= MyPrintConfigBean.RNG_z_hop_height.high;

        }

        return avaiable;
    }
    //to fix a bug : the position.x is not experted. Reason : when execute "ShowConfigDisplayPanelAnima/HideConfigDisplayPanelAnima",
    //at the same time, animation is executing and resize the window. So the value of x should change. 
    //So reset position.x when execute "ShowConfigDisplayPanelAnima/HideConfigDisplayPanelAnima" and when animaition complete.
    //Because animation time is short, so the bug is hard to reappear. Make easy to reappear : set animation time to long such as 6 seconds.

    /********** show&hide panel ************/
    public void ShowConfigDisplayPanelAnima()
    {
        switch (_animationState)
        {
            case AnimationState_Enum.Changing:
            case AnimationState_Enum.Shown:
                break;
            case AnimationState_Enum.Hidden:
            case AnimationState_Enum.None:
                _animationState = AnimationState_Enum.Changing;

                gameObject.transform.position = new Vector3(Screen.width / 2, gameObject.transform.position.y, gameObject.transform.position.z);

                int screenHeight = Screen.height;
                Vector2 referenceResolution = GameObject.Find("Canvas").GetComponent<CanvasScaler>().referenceResolution;
                float y = GameObject.Find("panel_profileSelect").transform.position.y;
                float dy = y + gameObject.GetComponent<RectTransform>().sizeDelta.y * screenHeight / referenceResolution.y;
                iTween.MoveTo(gameObject, iTween.Hash("y", dy, "easeType", "easeInOutExpo", "loopType", "none", "delay", 0, "oncomplete", "oncomplete_show"));

                break;
        }
    }

    public void HideConfigDisplayPanelAnima()
    {
        switch (_animationState)
        {
            case AnimationState_Enum.Changing:
            case AnimationState_Enum.Hidden:
                break;
            case AnimationState_Enum.Shown:
            case AnimationState_Enum.None:
                _animationState = AnimationState_Enum.Changing;

                gameObject.transform.position = new Vector3(Screen.width / 2, gameObject.transform.position.y, gameObject.transform.position.z);

                float y = GameObject.Find("panel_profileSelect").transform.position.y;
                iTween.MoveTo(gameObject, iTween.Hash("y", y, "easeType", "easeInOutExpo", "loopType", "none", "delay", 0, "oncomplete", "oncomplete_hide"));

                break;
        }
    }

    void oncomplete_show()
    {
        gameObject.transform.position = new Vector3(Screen.width / 2, gameObject.transform.position.y, gameObject.transform.position.z);
        _animationState = AnimationState_Enum.Shown;
        btn_arrow.spriteState = ss_arrowDown;
        btn_arrow.gameObject.GetComponent<Image>().sprite = sp_arrowDown;
    }

    void oncomplete_hide()
    {
        gameObject.transform.position = new Vector3(Screen.width / 2, gameObject.transform.position.y, gameObject.transform.position.z);
        _animationState = AnimationState_Enum.Hidden;
        btn_arrow.spriteState = ss_arrowUp;
        btn_arrow.gameObject.GetComponent<Image>().sprite = sp_arrowUp;
    }

    /********** life cycle ************/
    void Start()
    {
        sp_hollow = Resources.Load("Images/hollow-normal", typeof(Sprite)) as Sprite;
        sp_hollow_hover = Resources.Load("Images/hollow-hover", typeof(Sprite)) as Sprite;

        sp_light = Resources.Load("Images/light-normal", typeof(Sprite)) as Sprite;
        sp_light_hover = Resources.Load("Images/light-hover", typeof(Sprite)) as Sprite;

        sp_dense = Resources.Load("Images/dense-normal", typeof(Sprite)) as Sprite;
        sp_dense_hover = Resources.Load("Images/dense-hover", typeof(Sprite)) as Sprite;

        sp_solid = Resources.Load("Images/solid-normal", typeof(Sprite)) as Sprite;
        sp_solid_hover = Resources.Load("Images/solid-hover", typeof(Sprite)) as Sprite;

        //hide panel
        Vector3 newPos = gameObject.GetComponent<Transform>().position;
        newPos.y = -150;
        gameObject.GetComponent<Transform>().position = newPos;

        //left widget
        input_layerHeight.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_layerHeight);
        });

        input_layerHeight_0.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_layerHeight_0);
        });

        input_initial_layer_line_width_factor.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_initial_layer_line_width_factor);
        });

        input_wallThickness.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_wallThickness);
        });
        input_topThickness.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_topThickness);
        });
        input_bottomThickness.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_bottomThickness);
        });

        input_infillDesnsity.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_infillDesnsity);
        });

        input_printSpeed.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_printSpeed);
        });
        input_infillSpeed.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_infillSpeed);
        });
        input_outerWallSpeed.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_outerWallSpeed);
        });
        input_innerWallSpeed.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_innerWallSpeed);
        });
        input_topBottomSpeed.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_topBottomSpeed);
        });
        input_travelSpeed.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_travelSpeed);
        });

        input_initialLayerPrintSpeed.onEndEdit.AddListener(delegate
         {
             onEndEdit_left(input_initialLayerPrintSpeed);
         });
        input_initialLayerTravelSpeed.onEndEdit.AddListener(delegate
        {
            onEndEdit_left(input_initialLayerTravelSpeed);
        });

        //right widget
        input_bedTemp.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_bedTemp);
        });
        input_bedTempInitialLayer.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_bedTempInitialLayer);
        });
        input_printTemp.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_printTemp);
        });
        input_printTempInitialLayer.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_printTempInitialLayer);
        });
        input_finalPrintTemp.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_finalPrintTemp);
        });
        input_diameter.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_diameter);
        });
        input_flow.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_flow);
        });

        input_retractDis.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_retractDis);
        });
        input_retractSpeed.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_retractSpeed);
        });

        toggle_enableHeatedBed.onValueChanged.AddListener(delegate
        {
            toggleChanged(toggle_enableHeatedBed);
        });
        toggle_enableRetract.onValueChanged.AddListener(delegate
        {
            toggleChanged(toggle_enableRetract);
        });
        toggle_retractLayerChange.onValueChanged.AddListener(delegate
        {
            toggleChanged(toggle_retractLayerChange);
        });
        toggle_spiralize.onValueChanged.AddListener(delegate
        {
            toggleChanged(toggle_spiralize);
        });
        toggle_flowMeshSurface.onValueChanged.AddListener(delegate
        {
            toggleChanged(toggle_flowMeshSurface);
        });

        //travel
        input_z_hop_height.onEndEdit.AddListener(delegate
        {
            onEndEdit_right(input_z_hop_height);
        });

        toggle_z_hop_when_retract.onValueChanged.AddListener(delegate
        {
            toggleChanged(toggle_z_hop_when_retract);
        });

        //other widget
        btn_arrow.GetComponent<Button>().onClick.AddListener(onClick_arrow);

        //others
        ss_arrowDown = new SpriteState();
        ss_arrowUp = new SpriteState();
        ss_arrowDown.pressedSprite = Resources.Load("Images/arrow-down-hover", typeof(Sprite)) as Sprite;
        ss_arrowUp.pressedSprite = Resources.Load("Images/arrow-up-hover", typeof(Sprite)) as Sprite;

        sp_arrowUp = Resources.Load("Images/arrow-up-normal", typeof(Sprite)) as Sprite;
        sp_arrowDown = Resources.Load("Images/arrow-down-normal", typeof(Sprite)) as Sprite;

        //add EventTrigger and listen MouseEnter&MouseExit
        foreach (Selectable widget in gameObject.GetComponentsInChildren<Toggle>())
        {

            widget.gameObject.AddComponent<EventTrigger>();

            {
                EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback = new EventTrigger.TriggerEvent();
                entryEnter.callback.AddListener(delegate
                {
                    OnMouseEnter(widget);
                });

                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback = new EventTrigger.TriggerEvent();
                entryExit.callback.AddListener(delegate
                {
                    OnMouseExit(widget);
                });

                EventTrigger trigger = widget.gameObject.GetComponent<EventTrigger>();
                trigger.triggers.Add(entryEnter);
                trigger.triggers.Add(entryExit);
            }
        }

        foreach (Selectable widget in gameObject.GetComponentsInChildren<InputField>())
        {

            widget.gameObject.AddComponent<EventTrigger>();

            {
                EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback = new EventTrigger.TriggerEvent();
                entryEnter.callback.AddListener(delegate
                {
                    OnMouseEnter(widget);
                });

                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback = new EventTrigger.TriggerEvent();
                entryExit.callback.AddListener(delegate
                {
                    OnMouseExit(widget);
                });

                EventTrigger trigger = widget.gameObject.GetComponent<EventTrigger>();
                trigger.triggers.Add(entryEnter);
                trigger.triggers.Add(entryExit);
            }
        }
        Invoke("HideConfigDisplayPanelAnima", 1f);

        //edit
        input_name.text = "123456";
        input_name.onEndEdit.AddListener(delegate
        {
            onEndEdit_edit(input_name);
        });
        btn_edit.GetComponent<Button>().onClick.AddListener(onClick_edit);
        btn_duplicate.GetComponent<Button>().onClick.AddListener(onClick_duplicate);
        btn_remove.GetComponent<Button>().onClick.AddListener(onClick_remove);
        btn_import.GetComponent<Button>().onClick.AddListener(onClick_import);
        btn_export.GetComponent<Button>().onClick.AddListener(onClick_export);


        btn_infill_hollow.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnClick_infill(btn_infill_hollow);
        });

        btn_infill_light.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnClick_infill(btn_infill_light);
        });

        btn_infill_dense.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnClick_infill(btn_infill_dense);
        });

        btn_infill_solid.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnClick_infill(btn_infill_solid);
        });
    }

    private void OnMouseEnter(Selectable sender)
    {
        showParamDescriptionPanel(sender);
    }

    private void OnMouseExit(Selectable sender)
    {
        hideParamDescriptionPanel();
    }

    void OnClick_infill(Button sender)
    {
        MyPrintConfigBean myBean = ConfigManager.GetInstance().GetSelectedMyBean();
        PrintConfigBean bean = myBean.bean;
        if (bean == null)
        {
            Debug.LogError("bean == null");
            return;
        }

        if (sender == btn_infill_hollow)
        {
            bean.overrides.infill_sparse_density.default_value = infill_value_hollow;
        }
        else if (sender == btn_infill_light)
        {
            bean.overrides.infill_sparse_density.default_value = infill_value_light;
        }
        else if (sender == btn_infill_dense)
        {
            bean.overrides.infill_sparse_density.default_value = infill_value_dense;
        }
        else if (sender == btn_infill_solid)
        {
            bean.overrides.infill_sparse_density.default_value = infill_value_solid;
        }
    }

    void Update()
    {
        MyPrintConfigBean myBean = ConfigManager.GetInstance().GetSelectedMyBean();
        PrintConfigBean bean = myBean.bean;
        if (bean == null)
        {
            Debug.LogError("bean == null");
            return;
        }
        bool writable = myBean.writable;

        //1.set values
        /********** left ************/
        //quality
        input_layerHeight.interactable = writable;
        input_layerHeight.GetComponent<Image>().color = checkValueRange(input_layerHeight) ? _color_ok : _color_error;
        if (!input_layerHeight.isFocused)
        {
            input_layerHeight.text = bean.overrides.layer_height.default_value.ToString();
        }

        input_layerHeight_0.interactable = writable;
        input_layerHeight_0.GetComponent<Image>().color = checkValueRange(input_layerHeight_0) ? _color_ok : _color_error;
        if (!input_layerHeight_0.isFocused)
        {
            input_layerHeight_0.text = bean.overrides.layer_height_0.default_value.ToString();
        }

        input_initial_layer_line_width_factor.interactable = writable;
        input_initial_layer_line_width_factor.GetComponent<Image>().color = checkValueRange(input_initial_layer_line_width_factor) ? _color_ok : _color_error;
        if (!input_initial_layer_line_width_factor.isFocused)
        {
            input_initial_layer_line_width_factor.text = bean.overrides.initial_layer_line_width_factor.default_value.ToString();
        }

        //shell
        input_wallThickness.interactable = writable;
        input_wallThickness.GetComponent<Image>().color = checkValueRange(input_wallThickness) ? _color_ok : _color_error;
        if (!input_wallThickness.isFocused)
        {
            input_wallThickness.text = bean.overrides.wall_thickness.default_value.ToString();
        }

        input_topThickness.interactable = writable;
        input_topThickness.GetComponent<Image>().color = checkValueRange(input_topThickness) ? _color_ok : _color_error;
        if (!input_topThickness.isFocused)
        {
            input_topThickness.text = bean.overrides.top_thickness.default_value.ToString();
        }

        input_bottomThickness.interactable = writable;
        input_bottomThickness.GetComponent<Image>().color = checkValueRange(input_bottomThickness) ? _color_ok : _color_error;
        if (!input_bottomThickness.isFocused)
        {
            input_bottomThickness.text = bean.overrides.bottom_thickness.default_value.ToString();
        }

        //infill
        input_infillDesnsity.interactable = writable;
        input_infillDesnsity.GetComponent<Image>().color = checkValueRange(input_infillDesnsity) ? _color_ok : _color_error;
        if (!input_infillDesnsity.isFocused)
        {
            input_infillDesnsity.text = bean.overrides.infill_sparse_density.default_value.ToString();
        }

        //speed
        input_printSpeed.interactable = writable;
        input_printSpeed.GetComponent<Image>().color = checkValueRange(input_printSpeed) ? _color_ok : _color_error;
        if (!input_printSpeed.isFocused)
        {
            input_printSpeed.text = bean.overrides.speed_print.default_value.ToString();
        }

        input_infillSpeed.interactable = writable;
        input_infillSpeed.GetComponent<Image>().color = checkValueRange(input_infillSpeed) ? _color_ok : _color_error;
        if (!input_infillSpeed.isFocused)
        {
            input_infillSpeed.text = bean.overrides.speed_infill.default_value.ToString();
        }

        input_outerWallSpeed.interactable = writable;
        input_outerWallSpeed.GetComponent<Image>().color = checkValueRange(input_outerWallSpeed) ? _color_ok : _color_error;
        if (!input_outerWallSpeed.isFocused)
        {
            input_outerWallSpeed.text = bean.overrides.speed_wall_0.default_value.ToString();
        }

        input_innerWallSpeed.interactable = writable;
        input_innerWallSpeed.GetComponent<Image>().color = checkValueRange(input_innerWallSpeed) ? _color_ok : _color_error;
        if (!input_innerWallSpeed.isFocused)
        {
            input_innerWallSpeed.text = bean.overrides.speed_wall_x.default_value.ToString();
        }

        input_topBottomSpeed.interactable = writable;
        input_topBottomSpeed.GetComponent<Image>().color = checkValueRange(input_topBottomSpeed) ? _color_ok : _color_error;
        if (!input_topBottomSpeed.isFocused)
        {
            input_topBottomSpeed.text = bean.overrides.speed_topbottom.default_value.ToString();
        }

        input_travelSpeed.interactable = writable;
        input_travelSpeed.GetComponent<Image>().color = checkValueRange(input_travelSpeed) ? _color_ok : _color_error;
        if (!input_travelSpeed.isFocused)
        {
            input_travelSpeed.text = bean.overrides.speed_travel.default_value.ToString();
        }

        input_initialLayerPrintSpeed.interactable = writable;
        input_initialLayerPrintSpeed.GetComponent<Image>().color = checkValueRange(input_initialLayerPrintSpeed) ? _color_ok : _color_error;
        if (!input_initialLayerPrintSpeed.isFocused)
        {
            input_initialLayerPrintSpeed.text = bean.overrides.speed_print_layer_0.default_value.ToString();
        }

        input_initialLayerTravelSpeed.interactable = writable;
        input_initialLayerTravelSpeed.GetComponent<Image>().color = checkValueRange(input_initialLayerTravelSpeed) ? _color_ok : _color_error;
        if (!input_initialLayerTravelSpeed.isFocused)
        {
            input_initialLayerTravelSpeed.text = bean.overrides.speed_travel_layer_0.default_value.ToString();
        }

        /********** right ************/
        //material
        toggle_enableHeatedBed.interactable = writable;
        toggle_enableHeatedBed.isOn = bean.overrides.machine_heated_bed.default_value;

        input_bedTemp.interactable = writable && bean.overrides.machine_heated_bed.default_value;
        input_bedTempInitialLayer.interactable = writable && bean.overrides.machine_heated_bed.default_value;

        if (bean.overrides.machine_heated_bed.default_value)
        {
            //check
            input_bedTemp.GetComponent<Image>().color = checkValueRange(input_bedTemp) ? _color_ok : _color_error;
            if (!input_bedTemp.isFocused)
            {
                input_bedTemp.text = bean.overrides.material_bed_temperature.default_value.ToString();
            }

            input_bedTempInitialLayer.GetComponent<Image>().color = checkValueRange(input_bedTempInitialLayer) ? _color_ok : _color_error;
            if (!input_bedTempInitialLayer.isFocused)
            {
                input_bedTempInitialLayer.text = bean.overrides.material_bed_temperature_layer_0.default_value.ToString();
            }
        }
        else
        {
            input_bedTemp.text = "";
            input_bedTemp.GetComponent<Image>().color = _color_ok;

            input_bedTempInitialLayer.text = "";
            input_bedTempInitialLayer.GetComponent<Image>().color = _color_ok;
        }

        input_printTemp.interactable = writable;
        input_printTemp.GetComponent<Image>().color = checkValueRange(input_printTemp) ? _color_ok : _color_error;
        if (!input_printTemp.isFocused)
        {
            input_printTemp.text = bean.overrides.material_print_temperature.default_value.ToString();
        }

        input_printTempInitialLayer.interactable = writable;
        input_printTempInitialLayer.GetComponent<Image>().color = checkValueRange(input_printTempInitialLayer) ? _color_ok : _color_error;
        if (!input_printTempInitialLayer.isFocused)
        {
            input_printTempInitialLayer.text = bean.overrides.material_print_temperature_layer_0.default_value.ToString();
        }

        input_finalPrintTemp.interactable = writable;
        input_finalPrintTemp.GetComponent<Image>().color = checkValueRange(input_finalPrintTemp) ? _color_ok : _color_error;
        if (!input_finalPrintTemp.isFocused)
        {
            input_finalPrintTemp.text = bean.overrides.material_final_print_temperature.default_value.ToString();
        }

        input_diameter.interactable = writable;
        input_diameter.GetComponent<Image>().color = checkValueRange(input_diameter) ? _color_ok : _color_error;
        if (!input_diameter.isFocused)
        {
            input_diameter.text = bean.overrides.material_diameter.default_value.ToString();
        }

        input_flow.interactable = writable;
        input_flow.GetComponent<Image>().color = checkValueRange(input_flow) ? _color_ok : _color_error;
        if (!input_flow.isFocused)
        {
            input_flow.text = bean.overrides.material_flow.default_value.ToString();
        }

        toggle_enableRetract.interactable = writable;
        toggle_enableRetract.isOn = bean.overrides.retraction_enable.default_value;

        toggle_retractLayerChange.interactable = writable;
        toggle_retractLayerChange.isOn = bean.overrides.retract_at_layer_change.default_value;


        input_retractDis.interactable = writable && bean.overrides.retraction_enable.default_value;
        input_retractSpeed.interactable = writable && bean.overrides.retraction_enable.default_value;
        if (bean.overrides.retraction_enable.default_value)
        {
            if (!input_retractDis.isFocused)
            {
                input_retractDis.text = bean.overrides.retraction_amount.default_value.ToString();
            }
            if (!input_retractSpeed.isFocused)
            {
                input_retractSpeed.text = bean.overrides.retraction_speed.default_value.ToString();
            }

            input_retractDis.GetComponent<Image>().color = checkValueRange(input_retractDis) ? _color_ok : _color_error;
            input_retractSpeed.GetComponent<Image>().color = checkValueRange(input_retractSpeed) ? _color_ok : _color_error;
        }
        else
        {
            input_retractDis.text = "";
            input_retractSpeed.text = "";

            input_retractDis.GetComponent<Image>().color = _color_ok;
            input_retractSpeed.GetComponent<Image>().color = _color_ok;
        }

        toggle_z_hop_when_retract.interactable = writable && bean.overrides.retraction_enable.default_value;
        input_z_hop_height.interactable = writable && bean.overrides.retraction_enable.default_value && bean.overrides.retraction_hop_enabled.default_value;

        toggle_z_hop_when_retract.isOn = bean.overrides.retraction_hop_enabled.default_value && bean.overrides.retraction_enable.default_value;

        if (bean.overrides.retraction_hop_enabled.default_value && bean.overrides.retraction_enable.default_value)
        {
            if (!input_z_hop_height.isFocused)
            {
                input_z_hop_height.text = bean.overrides.retraction_hop.default_value.ToString();
            }
            input_z_hop_height.GetComponent<Image>().color = checkValueRange(input_z_hop_height) ? _color_ok : _color_error;
        }
        else
        {
            input_z_hop_height.text = "";
            input_z_hop_height.GetComponent<Image>().color = _color_ok;
        }

        //surface mode
        toggle_spiralize.interactable = writable;
        toggle_spiralize.isOn = bean.overrides.magic_spiralize.default_value;

        toggle_flowMeshSurface.interactable = writable;
        toggle_flowMeshSurface.isOn = bean.overrides.magic_mesh_surface_mode.default_value == "surface";

        //set inputfield text color
        foreach (InputField input in gameObject.GetComponentsInChildren<InputField>())
        {
            foreach (Text text in input.GetComponentsInChildren<Text>())
            {
                if (input.interactable)
                {
                    text.color = _color_text_able;
                }
                else
                {
                    text.color = _color_text_disable;
                }
            }
        }

        //name
        if (!writable)
        {
            input_name.interactable = false;
        }
        if (!input_name.isFocused)
        {
            input_name.text = myBean.name;
        }

        btn_edit.gameObject.SetActive(writable);
        btn_remove.interactable = writable;

        //infill buttons
        btn_infill_hollow.interactable = writable;
        btn_infill_light.interactable = writable;
        btn_infill_dense.interactable = writable;
        btn_infill_solid.interactable = writable;

        switch ((int)bean.overrides.infill_sparse_density.default_value)
        {
            case infill_value_hollow:
                btn_infill_hollow.gameObject.GetComponent<Image>().sprite = sp_hollow_hover;
                btn_infill_light.gameObject.GetComponent<Image>().sprite = sp_light;
                btn_infill_dense.gameObject.GetComponent<Image>().sprite = sp_dense;
                btn_infill_solid.gameObject.GetComponent<Image>().sprite = sp_solid;
                break;
            case infill_value_light:
                btn_infill_hollow.gameObject.GetComponent<Image>().sprite = sp_hollow;
                btn_infill_light.gameObject.GetComponent<Image>().sprite = sp_light_hover;
                btn_infill_dense.gameObject.GetComponent<Image>().sprite = sp_dense;
                btn_infill_solid.gameObject.GetComponent<Image>().sprite = sp_solid;

                break;
            case infill_value_dense:
                btn_infill_hollow.gameObject.GetComponent<Image>().sprite = sp_hollow;
                btn_infill_light.gameObject.GetComponent<Image>().sprite = sp_light;
                btn_infill_dense.gameObject.GetComponent<Image>().sprite = sp_dense_hover;
                btn_infill_solid.gameObject.GetComponent<Image>().sprite = sp_solid;
                break;
            case infill_value_solid:
                btn_infill_hollow.gameObject.GetComponent<Image>().sprite = sp_hollow;
                btn_infill_light.gameObject.GetComponent<Image>().sprite = sp_light;
                btn_infill_dense.gameObject.GetComponent<Image>().sprite = sp_dense;
                btn_infill_solid.gameObject.GetComponent<Image>().sprite = sp_solid_hover;
                break;
            default:
                btn_infill_hollow.gameObject.GetComponent<Image>().sprite = sp_hollow;
                btn_infill_light.gameObject.GetComponent<Image>().sprite = sp_light;
                btn_infill_dense.gameObject.GetComponent<Image>().sprite = sp_dense;
                btn_infill_solid.gameObject.GetComponent<Image>().sprite = sp_solid;
                break;
        }

    }

    /********** edit ************/
    private void onEndEdit_edit(InputField input)
    {
        string newName = input.text.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            AlertMananger.GetInstance().ShowToast("Profile name cannot be empty.");
            return;
        }

        MyPrintConfigBean myBean = ConfigManager.GetInstance().GetSelectedMyBean();
        if (myBean.name == newName)
        {
            AlertMananger.GetInstance().ShowToast("The new profile name is the same as the old one.");
            return;
        }

        if (ConfigManager.GetInstance().IsProfileNameExist(newName))
        {
            AlertMananger.GetInstance().ShowToast("This name is in use. Please try another one.");
            return;
        }

        ConfigManager.GetInstance().RenameCustomProfile(myBean, newName);
        AlertMananger.GetInstance().ShowToast("Renamed successfully.");
    }

    private void onClick_edit()
    {
        input_name.interactable = true;
        input_name.ActivateInputField();
    }

    private bool checkCouldAddCustomProfile()
    {
        if (ConfigManager.GetInstance().GetCustomMyBeanList().Count >= 5)
        {
            AlertMananger.GetInstance().ShowToast("The number of custom profiles reaches its max limit. Please remove one first.");
            return false;
        }
        return true;
    }
    private void onClick_duplicate()
    {
        if (!checkCouldAddCustomProfile())
        {
            return;
        }

        if (checkCouldAddCustomProfile())
        {
            MyPrintConfigBean myBean = ConfigManager.GetInstance().GetSelectedMyBean();
            ConfigManager.GetInstance().DuplicateProfile(myBean);

            AlertMananger.GetInstance().ShowToast("Duplicated successfully.");
        }
    }

    private void onClick_remove()
    {
        AlertMananger.GetInstance().ShowConfirmDialog("Remove profile", "Are you sure you want to remove the profile?", this);
    }

    private void onClick_import()
    {
        if (!checkCouldAddCustomProfile())
        {
            return;
        }

        string path = FileDialogManager.GetInstance().ShowFileSelectDialog_snapmaker3dProfile("");
        if (string.IsNullOrEmpty(path) || path.Length == 0)
        {
            Debug.LogWarning("import path is empty");
        }
        else
        {
            MyPrintConfigBean myBean = ConfigManager.GetInstance().ImportProfile(path);
            if (myBean != null)
            {
                AlertMananger.GetInstance().ShowToast("Imported successfully.");
            }
            else
            {
                AlertMananger.GetInstance().ShowAlertMsg("Unable to parse the profile.");
            }
        }
    }

    private void onClick_export()
    {
        MyPrintConfigBean myBean = ConfigManager.GetInstance().GetSelectedMyBean();
        string exportPath = FileDialogManager.GetInstance().ShowFileSaveDialog_snapmaker3dProfile(myBean.name);
        if (string.IsNullOrEmpty(exportPath) || exportPath.Trim().Length == 0)
        {
            Debug.LogWarning("Export path is empty");
        }
        else
        {
            string originPath = myBean.filePath;
            File.Copy(originPath, exportPath, true);
            AlertMananger.GetInstance().ShowToast("Exported successfully.");
        }
    }

    /*************** AlertMananger.Listener call back ***************/
    public void OnCancel()
    {
    }

    public void OnConfirm()
    {
        MyPrintConfigBean myBean = ConfigManager.GetInstance().GetSelectedMyBean();
        ConfigManager.GetInstance().RemoveCustomProfile(myBean);
    }
}
