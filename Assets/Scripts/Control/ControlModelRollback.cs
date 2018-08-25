using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlModelRollback : MonoBehaviour
{
	public Button btn_undo, btn_redo, btn_reset;
	public Button btn_manual;

	void Start ()
	{
		btn_undo.GetComponent<Button> ().onClick.AddListener (onClick_undo);
		btn_redo.GetComponent<Button> ().onClick.AddListener (onClick_redo);
		btn_reset.GetComponent<Button> ().onClick.AddListener (onClick_reset);
		btn_manual.GetComponent<Button> ().onClick.AddListener (onClick_manual);
	}
	
	void Update ()
	{
		btn_undo.interactable = ModelManager.GetInstance().CanExecuteUndo();
		btn_redo.interactable = ModelManager.GetInstance().CanExecuteRedo();
		btn_reset.interactable = ModelManager.GetInstance().CanExecuteReset();
	}

	void onClick_undo ()
	{
		ModelManager.GetInstance ().Undo ();
	}

	void onClick_redo ()
	{
		ModelManager.GetInstance ().Redo ();
	}

	void onClick_reset ()
	{
		ModelManager.GetInstance ().Reset ();
		GameObject.Find ("panel_viewModel").GetComponent<ControlViewModel> ().ResetPrintCubeLocalPosistion ();
		GameObject.Find ("panel_viewModel").GetComponent<ControlViewModel> ().RotatePrintCubeTo (0,0,0);
	}

	void onClick_manual(){
		Application.OpenURL("https://manual.snapmaker.com/3d_printing/");
	}
}
