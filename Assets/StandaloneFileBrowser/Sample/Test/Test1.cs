using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFB;

public class Test1 : MonoBehaviour
{
	public Button btn_openStlObj, btn_saveGcode;
	public Text text_result;

	void Start ()
	{
		btn_openStlObj.onClick.AddListener (OnClick_openStlObj);
		btn_saveGcode.onClick.AddListener (OnClick_saveGcode);
	}

	void OnClick_openStlObj ()
	{
		Debug.Log ("OnClick_openStlObj"+ "\n");

		var extensions = new [] {
			new ExtensionFilter ("", "stl", "obj")
		};

		string defaultDir = Application.dataPath;

		Debug.Log ("defaultDir:" + defaultDir + "\n");

		string[] pathArray = StandaloneFileBrowser.OpenFilePanel ("Open File", defaultDir, extensions, false);

		//  file:///Users/liuming/3dModel/pc.stl
		foreach (string item in pathArray) {
			Debug.Log ("item:" + item + "\n");
		}

	}

	void OnClick_saveGcode ()
	{
		Debug.Log ("OnClick_saveGcode"+ "\n");

		string defaultDir = Application.dataPath;
		defaultDir = "";

		string defaultName = "defaultName";
		string extention = "txt";
		//  /Users/liuming/Desktop/defaultName.txt
		string path = StandaloneFileBrowser.SaveFilePanel ("Save File", defaultDir, defaultName, extention);
		Debug.Log ("path:" + path + "\n");
	}

}
