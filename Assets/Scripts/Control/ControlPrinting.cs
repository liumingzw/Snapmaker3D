using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using System.Text.RegularExpressions;

public class ControlPrinting : MonoBehaviour, GcodeSenderManager.Listener, AlertMananger.Listener
{
	private Color _textBlue = new Color (48 / 255.0f, 149 / 255.0f, 218 / 255.0f);
	private Color _textBlack = new Color (108 / 255.0f, 108 / 255.0f, 108 / 255.0f);
	private Color _textWhite = Color.white;

	public GameObject panel_content;

	public Text text_nozzle, text_nozzle_temp, text_bed, text_bed_temp, text_print_progress;
	public Button btn_pause, btn_stop;
	public Image img_pause_icon;
	public Image image_print_progress;

	void Awake ()
	{
		GcodeSenderManager.GetInstance ().AddListener (this);
	}

	void Start ()
	{
		text_nozzle.color = this._textBlack;
		text_nozzle_temp.color = this._textBlue;

		text_bed.color = this._textBlack;
		text_bed_temp.color = this._textBlue;

		text_print_progress.color = this._textWhite;

		btn_pause.GetComponent<Button> ().onClick.AddListener (onClick_pause);
		btn_pause.GetComponentInChildren<Text> ().color = this._textWhite;

		btn_stop.GetComponent<Button> ().onClick.AddListener (onClick_stop);
		btn_stop.GetComponentInChildren<Text> ().color = this._textWhite;

		image_print_progress.fillAmount = 0;
		text_print_progress.text = "0%";
	}


	void Update ()
	{
		btn_pause.GetComponent<Transform> ().gameObject.SetActive (false);

		switch (StageManager.GetCurStage ()) {
		case StageManager.Stage_Enum.Idle:
		case StageManager.Stage_Enum.Load_Model:
			panel_content.SetActive (false);
			break;

		case StageManager.Stage_Enum.Gcode_Create:
			
			panel_content.SetActive (true);

			//widget : invisible
			btn_stop.GetComponent<Transform> ().gameObject.SetActive (false);
			text_nozzle.GetComponent<Transform> ().gameObject.SetActive (false);
			text_bed.GetComponent<Transform> ().gameObject.SetActive (false);
			text_nozzle_temp.GetComponent<Transform> ().gameObject.SetActive (false);
			text_bed_temp.GetComponent<Transform> ().gameObject.SetActive (false);

//			btn_pause.interactable = false;
//			btn_stop.interactable = false;
//			btn_pause.GetComponentInChildren<Text> ().text = "Pause";
//			img_pause_icon.sprite = Resources.Load ("Images/Pause-Icon", typeof(Sprite))as Sprite;

			//progress 
            float sliceProgress = GcodeCreateManager.GetInstance ().GetInfo().progress;
			text_print_progress.text = "Slicing : " + (((int)(sliceProgress * 1000)) / 10.0f).ToString ("0.0") + "%";
			image_print_progress.fillAmount = sliceProgress;

			break;

		case StageManager.Stage_Enum.Gcode_Render:

			//widget : invisible
			btn_stop.GetComponent<Transform> ().gameObject.SetActive (false);
			text_nozzle.GetComponent<Transform> ().gameObject.SetActive (false);
			text_bed.GetComponent<Transform> ().gameObject.SetActive (false);
			text_nozzle_temp.GetComponent<Transform> ().gameObject.SetActive (false);
			text_bed_temp.GetComponent<Transform> ().gameObject.SetActive (false);

			if (GcodeRenderManager.GetInstance ().GetInfo ().isParsing) {

				panel_content.SetActive (true);


				//show render progress
				float progress = GcodeRenderManager.GetInstance ().GetInfo ().parseProgress;
				text_print_progress.text = "Rendering : " + (((int)(progress * 1000)) / 10.0f).ToString ("0.0") + "%";
				image_print_progress.fillAmount = progress;
			} else {
				panel_content.SetActive (false);
			}
			break;

		case StageManager.Stage_Enum.Gcode_Send:
			{
				panel_content.SetActive (true);

				//widget : invisible
				btn_stop.GetComponent<Transform> ().gameObject.SetActive (true);
				text_nozzle.GetComponent<Transform> ().gameObject.SetActive (true);
				text_bed.GetComponent<Transform> ().gameObject.SetActive (true);
				text_nozzle_temp.GetComponent<Transform> ().gameObject.SetActive (true);
				text_bed_temp.GetComponent<Transform> ().gameObject.SetActive (true);

				//temp
				float temp_cur_bed = GcodeSenderManager.GetInstance ().GetInfo ().temp_cur_bed;
				float temp_tar_bed = GcodeSenderManager.GetInstance ().GetInfo ().temp_target_bed;
				float temp_cur_nozzle = GcodeSenderManager.GetInstance ().GetInfo ().temp_cur_nozzle;
				float temp_tar_nozzle = GcodeSenderManager.GetInstance ().GetInfo ().temp_target_nozzle;

				text_bed_temp.text = Math.Round (temp_cur_bed) + "/" + temp_tar_bed + "°C";
				text_nozzle_temp.text = Math.Round (temp_cur_nozzle) + "/" + temp_tar_nozzle + "°C";

				//progress 
				float progress = GcodeSenderManager.GetInstance ().GetInfo ().progress;
				text_print_progress.text = "Waiting : " + (((int)(progress * 1000)) / 10.0f).ToString ("0.0") + "%";
				image_print_progress.fillAmount = progress;

				if (GcodeSenderManager.GetInstance ().GetInfo ().sending_GcodeFile) {
					btn_pause.interactable = true;
					btn_stop.interactable = true;
					btn_pause.GetComponentInChildren<Text> ().text = "PAUSE";
					img_pause_icon.sprite = Resources.Load ("Images/Pause-Icon", typeof(Sprite))as Sprite;
				} else {
					btn_pause.interactable = true;
					btn_stop.interactable = true;
					btn_pause.GetComponentInChildren<Text> ().text = "START";
					img_pause_icon.sprite = Resources.Load ("Images/START-Icon", typeof(Sprite))as Sprite;
				}
			}
			break;
		}
	}

	void onClick_pause ()
	{
		GcodeSenderManager.GetInstance ().PauseOrContinue ();
	}

	void onClick_stop ()
	{
        AlertMananger.GetInstance ().ShowConfirmDialog ("Printing", "Are you sure you want to stop? The printing cannot resume if you stop now.", this);
	}

	/*************** GcodeSenderManager.Listener call back ***************/
	public void OnSendStarted ()
	{
		Debug.Log ("OnSendStarted" + "\n");
        AlertMananger.GetInstance ().ShowToast ("Before printing starts, it will take a few minutes to heat the heated bed and extruder.");

		//hack
		if (ConfigManager.GetInstance ().GetSelectedMyBean ().bean.name == "customize") {
			ConfigManager.GetInstance ().GetSelectedMyBean ().writable = false;
		}
	}

	public void OnSendCompleted ()
	{
		Debug.Log ("OnSendCompleted" + "\n");
		AlertMananger.GetInstance ().ShowAlertMsg ("Printing completed. Please do NOT touch the heated bed and nozzle.");

		//hack
		if (ConfigManager.GetInstance ().GetSelectedMyBean ().bean.name == "customize") {
			ConfigManager.GetInstance ().GetSelectedMyBean ().writable = true;
		}
	}

	public void OnSendStoped ()
	{
		Debug.Log ("OnSendStoped" + "\n");
		AlertMananger.GetInstance ().ShowAlertMsg ("Printing stoped. Please do NOT touch the heated bed and nozzle.");

		//hack
		if (ConfigManager.GetInstance ().GetSelectedMyBean ().bean.name == "customize") {
			ConfigManager.GetInstance ().GetSelectedMyBean ().writable = true;
		}
	}

	/*************** AlertMananger.Listener call back ***************/
	public void OnCancel ()
	{
		Debug.Log ("OnCancel");
	}

	public void OnConfirm ()
	{
		float modelMaxZ = ModelManager.GetInstance ().GetModel ().GetCurDataSize ().z;
		List<string> endGcode_Stop = StartAndEndGcodeUtils.GetEndGcode_StopPrint (modelMaxZ);
		GcodeSenderManager.GetInstance ().SetStopEndGcode (endGcode_Stop);
		GcodeSenderManager.GetInstance ().StopSend ();
	}
}
