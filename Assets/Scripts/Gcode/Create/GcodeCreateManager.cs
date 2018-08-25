using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GcodeCreateManager
{
	/*************** listener ***************/
	public interface Listener
	{
		void OnGcodeCreated (GcodeCreateBean gcodeBean) ;

		void OnGcodeExist (GcodeCreateBean gcodeBean) ;
	}

	private List<Listener> _listenerList = new List<Listener> ();

	public void AddListener (Listener listener)
	{
		if (!_listenerList.Contains (listener)) {
			_listenerList.Add (listener);
		}
	}

	/*************** single ***************/
	private static GcodeCreateManager _INSTANCE;

	private GcodeCreateManager ()
	{
	}

	public static GcodeCreateManager GetInstance ()
	{
		if (_INSTANCE == null) {
			_INSTANCE = new GcodeCreateManager ();
            _INSTANCE.curGcodeBean = new GcodeCreateBean();
            _INSTANCE._infoStruct.isSlicing = false;
            _INSTANCE._infoStruct.progress = 0.0f;
            _INSTANCE._infoStruct.isCurGcodeCreateBeanAvailable = false;
		}
		return _INSTANCE;
	}

	/*************** Create Gcode ***************/
	public GcodeCreateBean curGcodeBean;

    public void StartSubThread_CreateGcode (string stlFilePath, string configJsonFilePath)
	{
        //1.check need slice
        if (!NeedSlice(stlFilePath, configJsonFilePath))
        {
            Debug.LogWarning("StartSubThread_CreateGcode result : GcodeExist" + "\n");
            foreach (Listener listener in _listenerList)
            {
                listener.OnGcodeExist(curGcodeBean);
            }
            return;
        }

        //2.slice to create gcode file
		string gcodePath = PathManager.dirPath_Temp () + "output.gcode";

		Debug.Log ("StartSubThread_CreateGcode..." + "\n");
        Debug.Log ("configPath:" + configJsonFilePath + "\n");
		Debug.Log ("stlPath:" + stlFilePath + "\n");
		Debug.Log ("gcodePath:" + gcodePath + "\n");

		StageManager.SetStage_Gcode_Create ();
        sliceToCreateGcode (stlFilePath, configJsonFilePath, gcodePath);
	}

    private void sliceToCreateGcode (string stlFilePath, string configJsonFilePath, string gcodeFilePath)
	{
		Debug.Log ("sliceToCreateGcode..." + "\n");

        _infoStruct.isSlicing = true;
        _infoStruct.progress = 0.0f;
        _infoStruct.isCurGcodeCreateBeanAvailable = false;

        curGcodeBean = new GcodeCreateBean( stlFilePath,  configJsonFilePath,  gcodeFilePath);

		//2.new process to call shell: use CuraEngine to slice and create g-code
		System.Diagnostics.Process process = new System.Diagnostics.Process ();

		string shellPath = null;
		string CuraEnginePath = null;

		//Attention: mac and win, Arguments and FileName are different
		//In shell/bat : CuraEnginePath="$1"  configPath="$2" gcodePath="$3" stlPath="$4"
		switch (Global.GetInstance ().GetOSPlatform ()) {
		case Global.OS_Platform_Enum.Mac:
			
			shellPath = PathManager.shellPath_createGcode_mac ();
			CuraEnginePath = PathManager.enginePath_mac ();

			process.StartInfo.FileName = "/bin/sh";
			process.StartInfo.Arguments = 
			"\"" +
			shellPath +
			"\"" +
			" " +
			"\"" +
			CuraEnginePath +
			"\"" +
			" " +
			"\"" +
			configJsonFilePath +
			"\"" +
			" " +
			"\"" +
			gcodeFilePath +
			"\"" +
			" " +
			"\"" +
			stlFilePath +
			"\"";

			break;
		case Global.OS_Platform_Enum.Win_64:
			shellPath = PathManager.shellPath_createGcode_win ();
			CuraEnginePath = PathManager.enginePath_win64 ();

			process.StartInfo.FileName = shellPath;
			process.StartInfo.Arguments = 
				"\"" +
				CuraEnginePath +
				"\"" +
				" " +
				"\"" +
				configJsonFilePath +
				"\"" +
				" " +
				"\"" +
				gcodeFilePath +
				"\"" +
				" " +
				"\"" +
				stlFilePath +
				"\"";

			break;
		case Global.OS_Platform_Enum.Win_32:
			shellPath = PathManager.shellPath_createGcode_win ();
			CuraEnginePath = PathManager.enginePath_win32 ();

			process.StartInfo.FileName = shellPath;
			process.StartInfo.Arguments = 
			"\"" +
			CuraEnginePath +
			"\"" +
			" " +
			"\"" +
			configJsonFilePath +
			"\"" +
			" " +
			"\"" +
			gcodeFilePath +
			"\"" +
			" " +
			"\"" +
			stlFilePath +
			"\"";

			break;
		}

		Debug.Log ("process FileName:" + process.StartInfo.FileName + "\n");
		Debug.Log ("process Arguments :" + process.StartInfo.Arguments.Replace (" ", "\n") + "\n");

		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;

		process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler ((sender, e) => {
			if (!System.String.IsNullOrEmpty (e.Data)) {
				string outputStr = e.Data.Trim ();
//				Debug.Log (outputStr + "\n");
				if (outputStr.StartsWith ("Progress:inset+skin:") || outputStr.StartsWith ("Progress:export:")) {
					if (outputStr.EndsWith ("%")) {
                        _infoStruct.progress = float.Parse (outputStr.Substring (outputStr.Length - 9, 8)); //not include %
					}
				} else if (outputStr.StartsWith (";Filament used:")) {
					//Filament is either in mm3 or mm (depending on your g-code flavour. Reprap uses mm, ultiCode uses mm3)
					curGcodeBean.filamentLength = float.Parse (outputStr.Split (':') [1].Replace ("m", "").Trim ());
					curGcodeBean.filamentWeight = Mathf.PI * (1.75f / 2) * (1.75f / 2) * curGcodeBean.filamentLength * 1.24f;
				} else if (outputStr.StartsWith ("Print time:")) {
                    //add empirical parameter : 1.07
                    curGcodeBean.printTime = (int)(int.Parse (outputStr.Split (':') [1].Trim ()) * 1.07f);
				}
			}
		});

		process.EnableRaisingEvents = true;

		//3.slice finish
		process.Exited += (sender, e) => {
			Debug.Log ("Slice process exited" + "\n");

            _infoStruct.progress = 1.0F;
            _infoStruct.isSlicing = false;
            _infoStruct.isCurGcodeCreateBeanAvailable = true;

			foreach (Listener listener in _listenerList) {
				listener.OnGcodeCreated (curGcodeBean);
			}
		};

		process.Start ();
		process.BeginOutputReadLine ();
	}

    public bool NeedSlice (string stlFilePath, string configJsonFilePath)
	{
        if(!File.Exists(stlFilePath) || !File.Exists(configJsonFilePath)){
            return true;
        }

        if(stlFilePath == curGcodeBean.stlFilePath &&
           configJsonFilePath== curGcodeBean.configJsonFilePath &&
           Utils.GetMD5HashFromFile(stlFilePath) == curGcodeBean.stlFileMD5 &&
           Utils.GetMD5HashFromFile(configJsonFilePath) == curGcodeBean.configJsonFileMD5){
           return false;
        }

		return true;
	}

    /*************** info struct ***************/
    public struct InfoStruct
    {
        public bool isSlicing;
        public float progress;
        public bool isCurGcodeCreateBeanAvailable;
    }

    public InfoStruct GetInfo()
    {
        return _infoStruct;
    }

    private InfoStruct _infoStruct;

    public void UnavailableCurGcodeCreateBean(){
        _infoStruct.isCurGcodeCreateBeanAvailable = false;
    }
}
