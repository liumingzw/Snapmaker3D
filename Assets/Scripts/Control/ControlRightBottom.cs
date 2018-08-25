using UnityEngine;
using UnityEngine.UI;

public class ControlRightBottom : MonoBehaviour
{
	public GameObject panel_content;
	public Text text_modeFileName, text_modelSize, text_printTime, text_materialCost;
	public Image image_material, image_time;

	void Update ()
	{
		//1.set active
		bool active = StageManager.GetCurStage () != StageManager.Stage_Enum.Idle;
		panel_content.SetActive (active);
		if (!active) {
			return;
		}

		//2.set model file name
		string modelFileName = System.IO.Path.GetFileName (ModelManager.GetInstance ().GetInfo ().modelPath);

		//file name is too long, so cut
        if (!string.IsNullOrEmpty(modelFileName) && modelFileName.Length > 32) {
			modelFileName = modelFileName.Substring (0, 14) + "..." + modelFileName.Substring (modelFileName.Length - 14, 14);
        }else {
            modelFileName = "";
        }
		text_modeFileName.text = modelFileName;

		//3.set text_printTime and text_materialCost
        if (!GcodeCreateManager.GetInstance ().GetInfo().isCurGcodeCreateBeanAvailable) {
			text_printTime.text = "~ h ~ min";
			text_materialCost.text = "~ m / ~ g";
		} else {
			if (GcodeCreateManager.GetInstance ().curGcodeBean.printTime <= 0) {
				text_printTime.text = "~ h ~ min";
				text_materialCost.text = "~ m / ~ g";
			} else {
				int hours = GcodeCreateManager.GetInstance ().curGcodeBean.printTime / 3600;
				int minutes = (GcodeCreateManager.GetInstance ().curGcodeBean.printTime - hours * 3600) / 60;
				//less than 1 minute
				if (hours == 0 && minutes < 1) {
					minutes = 1;
				}
				string printTimeStr = hours > 0 ? (hours + " h " + minutes + " min") : (minutes + " min");
				text_printTime.text = printTimeStr;

				float materialLength = GcodeCreateManager.GetInstance ().curGcodeBean.filamentLength;
				if (materialLength < 0.1f) {
					materialLength = 0.1f;
				}

				int materialWeight = Mathf.CeilToInt (GcodeCreateManager.GetInstance ().curGcodeBean.filamentWeight);

				string materialCostStr = materialLength.ToString ("0.0") + "m / " + materialWeight + "g";
				text_materialCost.text = materialCostStr;
			}
		}

		//4.set size
		Vector3 size = Vector3.zero;
		if (ModelManager.GetInstance ().GetModel () != null) {
			size = ModelManager.GetInstance ().GetRenderOperateBean ().size;
		} 
		text_modelSize.text = size.x.ToString ("0.0") + " x " + size.y.ToString ("0.0") + " x " + size.z.ToString ("0.0") + " mm";

		//5.set image icon position
		{
			//move material icon next to text_materialCost
			Vector3 newPos_imgMaterial = image_material.transform.localPosition;
			newPos_imgMaterial.x = -Utils.CalculateTextLength (text_materialCost) + 100;
			image_material.transform.localPosition = newPos_imgMaterial;
		}

		{
			//move time icon next to text_printTime
			Vector3 newPos_imgTime = image_time.transform.localPosition;
			newPos_imgTime.x = -Utils.CalculateTextLength (text_printTime) + 100;
			image_time.transform.localPosition = newPos_imgTime;
		}
	}
}
