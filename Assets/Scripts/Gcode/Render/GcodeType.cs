public enum GcodeType
{
	WALL_INNER,
	WALL_OUTER,
	SKIN,
	SKIRT,
	SUPPORT,
	FILL,

	//not include in gcode
	Travel,
	UNKNOWN,
	Top_Layer
}