public class Overrides
{
	public Overrides ()
	{
	}

	/********** 1.machine_settings ************/
	public TypeString machine_start_gcode;
	public TypeString machine_end_gcode;
	public TypeBool machine_heated_bed;

	/********** 2.resolution ************/
	public TypeDouble layer_height;
	public TypeDouble layer_height_0;
	public TypeDouble initial_layer_line_width_factor;

	/********** 3.shell ************/
	public TypeDouble wall_thickness;
	public TypeDouble wall_line_count;

	public TypeDouble top_thickness;
	public TypeDouble bottom_thickness;

	/********** 4.infill ************/
	public TypeDouble infill_sparse_density;
	public TypeDouble infill_line_distance; 

	/********** 5.material ************/
	public TypeDouble material_print_temperature;
	public TypeDouble material_print_temperature_layer_0;
	public TypeDouble material_final_print_temperature;
	public TypeDouble material_bed_temperature;
	public TypeDouble material_bed_temperature_layer_0;

	public TypeDouble material_diameter;
	public TypeDouble material_flow;

	public TypeBool retraction_enable;
	public TypeBool retract_at_layer_change;
	public TypeDouble retraction_amount;
	public TypeDouble retraction_speed;

	/********** 6.speed ************/
	public TypeDouble speed_print;
	public TypeDouble speed_infill;
	public TypeDouble speed_wall;
	public TypeDouble speed_wall_0;
	public TypeDouble speed_wall_x;
	public TypeDouble speed_topbottom;
	public TypeDouble speed_travel;

    public TypeDouble speed_print_layer_0;
    public TypeDouble speed_travel_layer_0;

	/********** 7.travel ************/
	public TypeBool retraction_hop_enabled;
	public TypeDouble retraction_hop;

	/********** 8.cooling ************/

	/********** 9.support ************/
	public TypeBool support_enable;
	//buildplate,everywhere
	public TypeString support_type;

	/********** 10.platform_adhesion ************/
	//skirt,brim,raft,none
	public TypeString adhesion_type;

	/********** 11.dual ************/
	/********** 12.meshfix ************/

	/********** 13.blackmagic ************/
	//normal,surface,both
	public TypeString magic_mesh_surface_mode;

	public TypeBool magic_spiralize;

	/********** 14.experimental ************/
	/********** 15.command_line_settings ************/

	/**********************************/
	public class TypeString
	{
		public string default_value;
	}

	public class TypeBool
	{
		public bool default_value;
	}

	public class TypeDouble
	{
		public double default_value;
	}

}