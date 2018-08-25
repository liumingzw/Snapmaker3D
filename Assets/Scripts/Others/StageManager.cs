using System.Collections.Generic;
using UnityEngine;

public class StageManager
{
	
	private StageManager ()
	{
	}

	public enum Stage_Enum
	{
		Idle = 0,
		Load_Model = 1,
		Gcode_Create = 2,
		Gcode_Render = 3,
		Gcode_Send = 4,
	}

	private static Stage_Enum _curStage = Stage_Enum.Idle;

	private static List<Stage_Enum> _stageList = new List<Stage_Enum> ();

	public static void SetStage_Idle ()
	{
//		Debug.LogWarning ("Stage --> Idle" + "\n");
		_curStage = Stage_Enum.Idle;

		_stageList.Clear ();
		_stageList.Add (Stage_Enum.Idle);
	}

	public static void SetStage_Load_Model ()
	{
//		Debug.LogWarning ("Stage --> Load_Model" + "\n");
		_curStage = Stage_Enum.Load_Model;

		if (!_stageList.Contains (Stage_Enum.Gcode_Render)) {
			_stageList.Clear ();
			_stageList.Add (Stage_Enum.Idle);
			_stageList.Add (Stage_Enum.Load_Model);
		} 
	}

	public static void SetStage_Gcode_Create ()
	{
//		Debug.LogWarning ("Stage --> Gcode_Create" + "\n");
		_curStage = Stage_Enum.Gcode_Create;

		_stageList.Clear ();
		_stageList.Add (Stage_Enum.Idle);
		_stageList.Add (Stage_Enum.Load_Model);
		_stageList.Add (Stage_Enum.Gcode_Create);
	}

	public static void SetStage_Gcode_Render ()
	{
		
//		Debug.LogWarning ("Stage --> Gcode_Render" + "\n");
		_curStage = Stage_Enum.Gcode_Render;

		_stageList.Clear ();
		_stageList.Add (Stage_Enum.Idle);
		_stageList.Add (Stage_Enum.Load_Model);
		_stageList.Add (Stage_Enum.Gcode_Create);
		_stageList.Add (Stage_Enum.Gcode_Render);
	}

	public static void SetStage_Gcode_Send ()
	{
//		Debug.LogWarning ("Stage --> Gcode_Send" + "\n");
		_curStage = Stage_Enum.Gcode_Send;

		_stageList.Clear ();
		_stageList.Add (Stage_Enum.Idle);
		_stageList.Add (Stage_Enum.Load_Model);
		_stageList.Add (Stage_Enum.Gcode_Create);
		_stageList.Add (Stage_Enum.Gcode_Render);
		_stageList.Add (Stage_Enum.Gcode_Send);
	}

	public static List<Stage_Enum> GetStageList ()
	{
		return _stageList;
	}

	public static Stage_Enum GetCurStage ()
	{	
		return _curStage;
	}

	public static void PrintInfo ()
	{
		string info = "";
		switch (_curStage) {
		case StageManager.Stage_Enum.Idle:
			info = "Idle";
			break;
		case StageManager.Stage_Enum.Load_Model:
			info = "Load_Model";
			break;
		case StageManager.Stage_Enum.Gcode_Create:
			info = "Gcode_Create";
			break;
		case StageManager.Stage_Enum.Gcode_Render:
			info = "Gcode_Render";
			break;
		case StageManager.Stage_Enum.Gcode_Send:
			info = "Gcode_Send";
			break;
		}

		string info2 = "";
		if (_stageList.Contains (StageManager.Stage_Enum.Idle)) {
			info2 += "Idle";
		}
		if (_stageList.Contains (StageManager.Stage_Enum.Load_Model)) {
			info2 += " + Load_Model";
		}
		if (_stageList.Contains (StageManager.Stage_Enum.Gcode_Create)) {
			info2 += " + Gcode_Create";
		}
		if (_stageList.Contains (StageManager.Stage_Enum.Gcode_Render)) {
			info2 += " + Gcode_Render";
		}
		if (_stageList.Contains (StageManager.Stage_Enum.Gcode_Send)) {
			info2 += " + Gcode_Send";
		}
		Debug.LogWarning (info + " [" + info2 + "]" + "\n");
	}

}
