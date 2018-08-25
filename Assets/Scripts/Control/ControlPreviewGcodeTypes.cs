using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class ControlPreviewGcodeTypes : MonoBehaviour,GcodeRenderManager.Listener,GcodeSenderManager.Listener
{
	private enum AnimationState_Enum
	{
		Changing,
		Shown,
		Hidden,
		None,
	}

	private AnimationState_Enum _animationState;

	public GameObject panel_colorScheme;
	public Dropdown dropdown;
	public Button btn_arrow;

	public Toggle toggle_Show_Shell, toggle_Skirt_Brim,
		toggle_Support_Raft, toggle_Infill, toggle_Travels, toggle_Top_Layer;

	public Image image_WALL_INNER, image_WALL_OUTER, image_SKIN, image_SKIRT,
		image_SUPPORT, image_FILL, image_Travel, image_TopLayer;

	private SpriteState ss_arrowDown, ss_arrowUp;
	private Sprite sp_arrowDown, sp_arrowUp;

	void Awake ()
	{
		GcodeRenderManager.GetInstance ().AddListener (this);
		GcodeSenderManager.GetInstance ().AddListener (this);
	}

	void Start ()
	{
		btn_arrow.GetComponent<Button> ().onClick.AddListener (onClick_arrow);
		dropdown.onValueChanged.AddListener (valueChanged_dropDown);

		toggle_Show_Shell.onValueChanged.AddListener (delegate {
			toggleChanged (toggle_Show_Shell);
		});

		toggle_Skirt_Brim.onValueChanged.AddListener (delegate {
			toggleChanged (toggle_Skirt_Brim);
		});

		toggle_Support_Raft.onValueChanged.AddListener (delegate {
			toggleChanged (toggle_Support_Raft);
		});

		toggle_Infill.onValueChanged.AddListener (delegate {
			toggleChanged (toggle_Infill);
		});

		toggle_Travels.onValueChanged.AddListener (delegate {
			toggleChanged (toggle_Travels);
		});

		toggle_Top_Layer.onValueChanged.AddListener (delegate {
			toggleChanged (toggle_Top_Layer);
		});

		//others
		ss_arrowDown = new SpriteState ();
		ss_arrowUp = new SpriteState ();
		ss_arrowDown.pressedSprite = Resources.Load ("Images/panel-in-hover", typeof(Sprite))as Sprite;
		ss_arrowUp.pressedSprite = Resources.Load ("Images/panel-out-hover", typeof(Sprite))as Sprite;

		sp_arrowDown = Resources.Load ("Images/panel-in", typeof(Sprite))as Sprite;
		sp_arrowUp = Resources.Load ("Images/panel-out-normal", typeof(Sprite))as Sprite;

		image_WALL_INNER.GetComponent<Image> ().color = GcodeTypeColor.WALL_INNER;
		image_WALL_OUTER.GetComponent<Image> ().color = GcodeTypeColor.WALL_OUTER;
		image_SKIN.GetComponent<Image> ().color = GcodeTypeColor.SKIN;

		image_SKIRT.GetComponent<Image> ().color = GcodeTypeColor.SKIRT;
		image_SUPPORT.GetComponent<Image> ().color = GcodeTypeColor.SUPPORT;
		image_FILL.GetComponent<Image> ().color = GcodeTypeColor.FILL;
		image_Travel.GetComponent<Image> ().color = GcodeTypeColor.Travel;
		image_TopLayer.GetComponent<Image> ().color = GcodeTypeColor.Top_Layer;
	
		_animationState = AnimationState_Enum.None;
		ShowConfigDisplayPanelAnima ();
	}

	void Update ()
	{
		//dropdown
		GcodeRenderManager.ColorScheme colorScheme = GcodeRenderManager.GetInstance ().GetCurColorScheme ();
		if (colorScheme == GcodeRenderManager.ColorScheme.Material) {
			dropdown.value = 0;
		} else if (colorScheme == GcodeRenderManager.ColorScheme.Line_Type) {
			dropdown.value = 1;
		}

		//image
		if (colorScheme == GcodeRenderManager.ColorScheme.Material) {
			image_WALL_INNER.enabled = false;
			image_WALL_OUTER.enabled = false;
			image_SKIN.enabled = false;
			image_SKIRT.enabled = false;
			image_SUPPORT.enabled = false;
			image_FILL.enabled = false;
			image_Travel.enabled = false;
			image_TopLayer.enabled = false;
		} else if (colorScheme == GcodeRenderManager.ColorScheme.Line_Type) {
			image_WALL_INNER.enabled = true;
			image_WALL_OUTER.enabled = true;
			image_SKIN.enabled = true;
			image_SKIRT.enabled = true;
			image_SUPPORT.enabled = true;
			image_FILL.enabled = true;
			image_Travel.enabled = true;
			image_TopLayer.enabled = true;
		}

		//toggle
		toggle_Show_Shell.isOn = 
		GcodeRenderManager.GetInstance ().IsGcodeTypeShow (GcodeType.WALL_OUTER) &&
		GcodeRenderManager.GetInstance ().IsGcodeTypeShow (GcodeType.WALL_INNER) &&
		GcodeRenderManager.GetInstance ().IsGcodeTypeShow (GcodeType.SKIN);

		toggle_Skirt_Brim.isOn = GcodeRenderManager.GetInstance ().IsGcodeTypeShow (GcodeType.SKIRT);
		toggle_Support_Raft.isOn = GcodeRenderManager.GetInstance ().IsGcodeTypeShow (GcodeType.SUPPORT);
		toggle_Infill.isOn = GcodeRenderManager.GetInstance ().IsGcodeTypeShow (GcodeType.FILL);
		toggle_Travels.isOn = GcodeRenderManager.GetInstance ().IsGcodeTypeShow (GcodeType.Travel);
		toggle_Top_Layer.isOn = GcodeRenderManager.GetInstance ().GetActive_Top_Layer ();
	
	
		//btn_arrow
		switch (StageManager.GetCurStage ()) {
		case StageManager.Stage_Enum.Idle:
		case StageManager.Stage_Enum.Load_Model:
		case StageManager.Stage_Enum.Gcode_Create:
		case StageManager.Stage_Enum.Gcode_Send:
			btn_arrow.GetComponent<Button> ().gameObject.SetActive (false);
			break;

		case StageManager.Stage_Enum.Gcode_Render:

			if (GcodeRenderManager.GetInstance ().GetInfo ().isRendered) {
				btn_arrow.GetComponent<Button> ().gameObject.SetActive (true);
			} else {
				btn_arrow.GetComponent<Button> ().gameObject.SetActive (false);
			}
			break;
		}

		//panel_colorScheme
		panel_colorScheme.GetComponent<Transform> ().gameObject.SetActive (btn_arrow.GetComponent<Button> ().gameObject.activeInHierarchy);
	}

	void onClick_arrow ()
	{
		switch (_animationState) {
		case AnimationState_Enum.Changing:
		case AnimationState_Enum.None:
			break;
		case AnimationState_Enum.Shown:
			HideConfigDisplayPanelAnima ();
			break;
		case AnimationState_Enum.Hidden:
			ShowConfigDisplayPanelAnima ();
			break;
		}
	}

	void valueChanged_dropDown (int index)
	{
		if (index == 0) {
			GcodeRenderManager.GetInstance ().SetColorScheme (GcodeRenderManager.ColorScheme.Material);
		} else if (index == 1) {
			GcodeRenderManager.GetInstance ().SetColorScheme (GcodeRenderManager.ColorScheme.Line_Type);
		}
	}

	/********** show&hide panel ************/
	public void ShowConfigDisplayPanelAnima ()
	{
		switch (_animationState) {
		case AnimationState_Enum.Changing:
		case AnimationState_Enum.Shown:
			break;
		case AnimationState_Enum.Hidden:
		case AnimationState_Enum.None:
			_animationState = AnimationState_Enum.Changing;
			float x = 0;
			Debug.Log ("Show:" + x);
			iTween.MoveTo (panel_colorScheme, iTween.Hash ("x", x, "islocal", true, "easeType", "easeInOutExpo", "loopType", "none", "delay", 0, "oncomplete", "oncomplete_show", "oncompletetarget", this.gameObject));
			break;
		}		
	}

	public void HideConfigDisplayPanelAnima ()
	{
		switch (_animationState) {
		case AnimationState_Enum.Changing:
		case AnimationState_Enum.Hidden:
			break;
		case AnimationState_Enum.Shown:
		case AnimationState_Enum.None:
			_animationState = AnimationState_Enum.Changing;
			float x = btn_arrow.transform.localPosition.x;
			Debug.Log ("hide:" + x);
			iTween.MoveTo (panel_colorScheme, iTween.Hash ("x", x, "islocal", true, "easeType", "easeInOutExpo", "loopType", "none", "delay", 0, "oncomplete", "oncomplete_hide", "oncompletetarget", this.gameObject));
			break;
		}
	}

	public void oncomplete_show ()
	{
		_animationState = AnimationState_Enum.Shown;
		btn_arrow.spriteState = ss_arrowDown;
		btn_arrow.gameObject.GetComponent<Image> ().sprite = sp_arrowDown;
	}

	public  void oncomplete_hide ()
	{
		_animationState = AnimationState_Enum.Hidden;
		btn_arrow.spriteState = ss_arrowUp;
		btn_arrow.gameObject.GetComponent<Image> ().sprite = sp_arrowUp;
	}

	void toggleChanged (Toggle sender)
	{
		if (sender == toggle_Show_Shell) {
			Debug.Log ("toggle_Show_Shell");
			GcodeRenderManager.GetInstance ().SetActive_WALL_INNER (sender.isOn);
			GcodeRenderManager.GetInstance ().SetActive_WALL_OUTER (sender.isOn);
			GcodeRenderManager.GetInstance ().SetActive_SKIN (sender.isOn);

		} else if (sender == toggle_Skirt_Brim) {
			Debug.Log ("toggle_Skirt_Brim");
			GcodeRenderManager.GetInstance ().SetActive_SKIRT (sender.isOn);

		} else if (sender == toggle_Support_Raft) {
			Debug.Log ("toggle_Support_Raft");
			GcodeRenderManager.GetInstance ().SetActive_SUPPORT (sender.isOn);
			GcodeRenderManager.GetInstance ().SetActive_UNKNOWN (sender.isOn);

		} else if (sender == toggle_Infill) {
			Debug.Log ("toggle_Infill");
			GcodeRenderManager.GetInstance ().SetActive_FILL (sender.isOn);

		} else if (sender == toggle_Travels) {
			Debug.Log ("toggle_Travels");
			GcodeRenderManager.GetInstance ().SetActive_Travel (sender.isOn);

		} else if (sender == toggle_Top_Layer) {
			Debug.Log ("toggle_Top_Layer");
			GcodeRenderManager.GetInstance ().SetActive_Top_Layer (sender.isOn);
		}
	}


	/*************** GcodeRenderManager.Listener call back ***************/
	public void OnGcodeParseSucceed ()
	{
		Loom.RunAsync (
			() => {
				Thread t = new Thread (setUI_mainThread);  
				t.Start ();  
			}
		);
	}

    public void OnGcodeParseFailed()
    {

    }

	private void setUI_mainThread ()
	{  
		Loom.QueueOnMainThread ((param) => {
			_animationState = AnimationState_Enum.None;
			ShowConfigDisplayPanelAnima ();
		}, null);
	}

	/*************** GcodeSenderManager.Listener call back ***************/
	public void OnSendStarted ()
	{
		Debug.Log ("OnSendStarted");
		dropdown.value = 1;
		GcodeRenderManager.GetInstance ().SetActive_WALL_INNER (true);
		GcodeRenderManager.GetInstance ().SetActive_WALL_OUTER (true);
		GcodeRenderManager.GetInstance ().SetActive_SKIN (true);

		GcodeRenderManager.GetInstance ().SetActive_SKIRT (true);

		GcodeRenderManager.GetInstance ().SetActive_SUPPORT (true);
		GcodeRenderManager.GetInstance ().SetActive_UNKNOWN (true);

		GcodeRenderManager.GetInstance ().SetActive_FILL (true);

		GcodeRenderManager.GetInstance ().SetActive_Travel (false);

		GcodeRenderManager.GetInstance ().SetActive_Top_Layer (true);

	}

	public void OnSendCompleted ()
	{
	}

	public void OnSendStoped ()
	{
	}
}
