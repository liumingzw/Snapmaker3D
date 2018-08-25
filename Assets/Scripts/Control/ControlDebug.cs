using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ControlDebug : MonoBehaviour , AlertMananger.Listener
{

	public Button btn1, btn2, btn3;

	public Toggle 
		WALL_INNER,
		WALL_OUTER,
		SKIN,
		SKIRT,
		SUPPORT,
		FILL,
		TRAVEL,
		UNKNOWN;

	public Text text;

	void Start ()
	{
		btn1.GetComponent<Button> ().onClick.AddListener (onClick_btn1);
		btn2.GetComponent<Button> ().onClick.AddListener (onClick_btn2);
		btn3.GetComponent<Button> ().onClick.AddListener (onClick_btn3);


		WALL_INNER.onValueChanged.AddListener (delegate {
			toggleChanged (WALL_INNER);
		});
		WALL_OUTER.onValueChanged.AddListener (delegate {
			toggleChanged (WALL_OUTER);
		});
		SKIN.onValueChanged.AddListener (delegate {
			toggleChanged (SKIN);
		});
		SKIRT.onValueChanged.AddListener (delegate {
			toggleChanged (SKIRT);
		});
		SUPPORT.onValueChanged.AddListener (delegate {
			toggleChanged (SUPPORT);
		});
		FILL.onValueChanged.AddListener (delegate {
			toggleChanged (FILL);
		});
		TRAVEL.onValueChanged.AddListener (delegate {
			toggleChanged (TRAVEL);
		});
		UNKNOWN.onValueChanged.AddListener (delegate {
			toggleChanged (UNKNOWN);
		});

	}

	void onClick_btn1 ()
	{

		GcodeDrawLineManager.GetInstance ().SetColor_types (null);
	}

	void onClick_btn2 ()
	{
//		ModelManager.GetInstance ().Reset ();
	}

	void onClick_btn3 ()
	{
//		GameObject.Find ("VectorCanvas").SetActive (true);
	}


	public void OnCancel ()
	{
		Debug.Log ("OnCancel");
	}

	public void OnConfirm ()
	{
		Debug.Log ("OnConfirm");
	}

	void toggleChanged (Toggle sender)
	{
		if (sender == WALL_INNER) {
			Debug.Log ("WALL_INNER");
			GcodeRenderManager.GetInstance ().SetActive_WALL_INNER (sender.isOn);
		} else if (sender == WALL_OUTER) {
			Debug.Log ("WALL_OUTER");
			GcodeRenderManager.GetInstance ().SetActive_WALL_OUTER (sender.isOn);
		} else if (sender == SKIN) {
			Debug.Log ("SKIN");
			GcodeRenderManager.GetInstance ().SetActive_SKIN (sender.isOn);
		} else if (sender == SKIRT) {
			Debug.Log ("SKIRT");
			GcodeRenderManager.GetInstance ().SetActive_SKIRT (sender.isOn);
		}else if (sender == SUPPORT) {
			Debug.Log ("SUPPORT");
			GcodeRenderManager.GetInstance ().SetActive_SUPPORT (sender.isOn);
		}else if (sender == FILL) {
			Debug.Log ("FILL");
			GcodeRenderManager.GetInstance ().SetActive_FILL (sender.isOn);
		}else if (sender == TRAVEL) {
			Debug.Log ("TRAVEL");
//			GcodeRenderManager.GetInstance ().SetActive_TRAVEL (sender.isOn);
		}else if (sender == UNKNOWN) {
			Debug.Log ("UNKNOWN");
			GcodeRenderManager.GetInstance ().SetActive_UNKNOWN (sender.isOn);
		}
	}

}
