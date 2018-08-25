using UnityEngine;
using UnityEngine.UI;

public class ControlConfirmDialog : MonoBehaviour
{
	//confirm
	public GameObject panel_confirmDialog;
	public Button btn_close, btn_cancel, btn_confirm;
	public Text text_title, text_confirmMsg;

	private AlertMananger.Listener _listener;

	void Start ()
	{
		btn_close.GetComponent<Button> ().onClick.AddListener (onClick_close);
		btn_cancel.GetComponent<Button> ().onClick.AddListener (onClick_cancel);
		btn_confirm.GetComponent<Button> ().onClick.AddListener (onClick_confirm);
	}

	void onClick_cancel ()
	{
		panel_confirmDialog.SetActive (false); 
		if (_listener != null) {
			_listener.OnCancel ();
		}
		_listener = null;
	}

	void onClick_close ()
	{
		panel_confirmDialog.SetActive (false); 
	}

	void onClick_confirm ()
	{
		panel_confirmDialog.SetActive (false); 
		if (_listener != null) {
			_listener.OnConfirm ();
		}
		_listener = null;
	}

	public void ShowConfirmDialog (string title, string msg, AlertMananger.Listener listener)
	{
		_listener = listener;

		text_title.text = title;
		text_confirmMsg.text = msg;

		panel_confirmDialog.SetActive (true); 
	}
}
