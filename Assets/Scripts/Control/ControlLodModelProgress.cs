using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlLodModelProgress : MonoBehaviour
{
	public Image image_print_progress, image_print_bg;
	public Text text_print_progress;

	void Update ()
	{
		if (ModelManager.GetInstance ().GetInfo ().isParsing) {
			image_print_progress.GetComponent<Transform> ().gameObject.SetActive (true);
			image_print_bg.GetComponent<Transform> ().gameObject.SetActive (true);

			image_print_progress.fillAmount = ModelManager.GetInstance ().GetInfo ().parseProgress;
			text_print_progress.text = "Loading : " + (int)(ModelManager.GetInstance ().GetInfo ().parseProgress * 100) + "%";
		} else {

			image_print_progress.GetComponent<Transform> ().gameObject.SetActive (false);
			image_print_bg.GetComponent<Transform> ().gameObject.SetActive (false);
			text_print_progress.text = "";
		}
	}
}
