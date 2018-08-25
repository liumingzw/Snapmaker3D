using UnityEngine;

public class GcodeTypeColor
{
	/*************** cura color ***************/
	//dark green
//	public static Color32 WALL_INNER = new Color32 (157, 255, 0, 255);
//	
//	//red
//	public static Color32 WALL_OUTER = new Color32 (221, 0, 0, 255);
//	
//	public static Color32 SKIN = new Color32 (221, 0, 0, 255);
//	
//	//green
//	public static Color32 SKIRT = new Color32 (0, 255, 0, 255);
//	
//	//green & yellow
//	public static Color32 SUPPORT = new Color32 (140, 255, 255, 255);
//	
//	//yellow
//	public static Color32 FILL = new Color32 (234, 190, 0, 255);
//
//	//black
//	public static Color32 UNKNOWN = new Color32 (0, 0, 0, 255);
//
//	//transparent
//	public static Color32 Transparent = new Color32 (0, 0, 0, 0);
//
//	//material
//	public static Color32 Material_Color = new Color32 (184, 156, 20, 255);
//
//	public static Color32 Top_Layer = new Color32 (0, 0, 0, 255);
//
//	public static Color32 Travel = new Color32 (31, 0, 255, 255);


	/*************** sunlight color ***************/
	private static Color32 Sun_red = new Color32 (255, 33, 33, 255);
	private static Color32 Sun_orange = new Color32 (250, 140, 53, 255);
	private static Color32 Sun_yellow = new Color32 (255, 255, 0, 255);
	private static Color32 Sun_green = new Color32 (0, 255, 0, 255);
	private static Color32 Sun_blue = new Color32 (68, 206, 246, 255);
	private static Color32 Sun_indigo = new Color32 (75, 0, 130, 255);
	private static Color32 Sun_purple = new Color32 (141, 75, 187, 255);

	public static Color32 WALL_OUTER = Sun_red;
	public static Color32 WALL_INNER = Sun_green;
	public static Color32 SKIN = Sun_yellow;
	public static Color32 SKIRT = Sun_orange;
	public static Color32 SUPPORT = Sun_indigo;
	public static Color32 FILL = Sun_purple;

	public static Color32 Travel = Sun_blue;

	public static Color32 Material_Color = new Color32 (184, 156, 20, 255);

	//hack: UNKNOWN only appeare in initial layer on condition of "Raft"
	public static Color32 UNKNOWN = SUPPORT;

	//transparent
	public static Color32 Transparent = new Color32 (0, 0, 0, 0);

	public static Color32 Top_Layer = new Color32 (0, 0, 0, 255);

//	public static Color32 Travels = new Color32 (31, 0, 255, 255);
//	//public static Color32 Helpers = new Color32 (159, 255, 255, 255);
//	public static Color32 Helpers = new Color32 (108, 174, 174, 255);
//	public static Color32 Shell = new Color32 (209, 0, 0, 255);
//	public static Color32 Infill = new Color32 (234, 190, 0, 255);
//	public static Color32 TopBottom = new Color32 (254, 255, 0, 255);
//	public static Color32 InnerWall = new Color32 (157, 255, 0, 255);

	//	public static Color32 WALL_INNER = InnerWall;
	//	public static Color32 WALL_OUTER = Shell;
	//	public static Color32 SKIN = Shell;
	//	public static Color32 SKIRT = Helpers;
	//	public static Color32 SUPPORT = Helpers;
	//	public static Color32 FILL = Infill;
}
