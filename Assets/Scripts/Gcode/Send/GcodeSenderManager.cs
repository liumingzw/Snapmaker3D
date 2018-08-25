using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System;

public class GcodeSenderManager: PortConnectManager.Listener
{

	/*************** listener ***************/
	public interface Listener
	{
		void OnSendStarted () ;

		void OnSendCompleted () ;

		void OnSendStoped () ;
	}

	private List<Listener> _listenerList = new List<Listener> ();

	public void AddListener (Listener listener)
	{
		if (!_listenerList.Contains (listener)) {
			_listenerList.Add (listener);
		}
	}

	//todo : unity serial port WndProc
	//https://www.baidu.com/s?ie=utf-8&f=8&rsv_bp=1&srcqid=1049826088228721423&tn=50000033_hao_pg&wd=unity%E4%B8%B2%E5%8F%A3%20WndProc&oq=c%2523%2520%25E4%25B8%25B2%25E5%258F%25A3%2520WndProc&rsv_pq=fd580c6c0022f052&rsv_t=0c70F7I3R4iMq9VJRjFQ%2FD%2B7BkFQ1LU8ZF2hLLDXPm3KKEq7D2sheQ%2FNFDXzDTI1AZojT28W&rqlang=cn&rsv_enter=1&inputT=1109&rsv_sug3=50&rsv_sug2=0&rsv_sug4=1109&rsv_sug=1

	/*************** send struct***************/
	public struct InfoStruct
	{
		public float temp_target_nozzle;
		public float temp_cur_nozzle;
		public float temp_target_bed;
		public float temp_cur_bed;

		public bool sending_GcodeFile;

		public bool stop_Manaual;

		public float progress;

        public float completedZ;//the Z value of completed part
	}

	private InfoStruct _struct;

	public InfoStruct GetInfo ()
	{
		return _struct;
	}

	private Queue<string> _gcodeQueue = new Queue<string> ();
	private int _gcodeLineCount;

	private Queue<string> _endGcodeQueue_stopManaual = new Queue<string> ();

	/*************** single ***************/
	private static GcodeSenderManager _INSTANCE;

	private GcodeSenderManager ()
	{
	}

	public static GcodeSenderManager GetInstance ()
	{
		if (_INSTANCE == null) {
			_INSTANCE = new GcodeSenderManager ();
			PortConnectManager.GetInstance ().AddListener (_INSTANCE);
		}
		return _INSTANCE;
	}

	/*************** main  functions ***************/
	public bool StartSend (string gcodeFilePath)
	{
		if (string.IsNullOrEmpty (gcodeFilePath) || !File.Exists (gcodeFilePath)) {
			Debug.Log ("StartSend failed : Gcode File Path Exception" + "\n");
			return false;
		}

		//check port is ready ?
		if (!PortConnectManager.GetInstance ().IsConnected ()) {
			Debug.LogWarning ("Port not connect " + "\n");
			return false;
		}

		foreach (Listener listener in _listenerList) {
			listener.OnSendStarted ();
		}

		StageManager.SetStage_Gcode_Send ();

		_struct.sending_GcodeFile = true;
		_struct.stop_Manaual = false;
		_struct.progress = 0;
        _struct.completedZ = 0;

		_gcodeQueue.Clear (); 

		//M1001 L  lock screen
//		_gcodeQueue.Enqueue ("M1001 L");

		string[] gcodeList = File.ReadAllLines (gcodeFilePath);

		for (int i = 0; i < gcodeList.Length; i++) {
			string cmd = gcodeList [i];
			//_gcodeQueue not include notes
			if (!cmd.StartsWith (";") && !string.IsNullOrEmpty (cmd) && cmd.Trim ().Length > 0) {
				_gcodeQueue.Enqueue (cmd.Trim ());
			}
			//insert M105 per 10 lines
			if (i % 10 == 0) {
				_gcodeQueue.Enqueue ("M105");
			}
		}

		_gcodeLineCount = _gcodeQueue.Count;

		//send a cmd and receive "ok", so start send
		PortConnectManager.GetInstance ().WriteCmd ("M105");

		Debug.Log ("StartSend succeed" + "\n");
		return true;
	}

	//return Sending
	public bool PauseOrContinue ()
	{
		if (_struct.sending_GcodeFile) {
			Debug.Log ("Pause printing" + "\n");
			_struct.sending_GcodeFile = false;
		} else {
			Debug.Log ("Continue printing" + "\n");
			_struct.sending_GcodeFile = true;
		}
		return _struct.sending_GcodeFile;
	}

	public void StopSend ()
	{
		_struct.sending_GcodeFile = false;
		_struct.stop_Manaual = true;

		_gcodeQueue.Clear ();
		_gcodeLineCount = 0;

		_struct.temp_target_nozzle = 0;
		_struct.temp_cur_nozzle = 0;
		_struct.temp_target_bed = 0;
		_struct.temp_cur_bed = 0;

		StageManager.GetStageList ().Remove (StageManager.Stage_Enum.Gcode_Send);
		StageManager.SetStage_Gcode_Render ();

		foreach (Listener listener in _listenerList) {
			listener.OnSendStoped ();
		}
	}

	public void SetStopEndGcode (List<string> stopEndGcodeList)
	{
		_endGcodeQueue_stopManaual.Clear ();

		for (int i = 0; i < stopEndGcodeList.Count; i++) {
			string cmd = stopEndGcodeList [i];
			if (!cmd.StartsWith (";") && !string.IsNullOrEmpty (cmd) && cmd.Trim ().Length > 0) {
				_endGcodeQueue_stopManaual.Enqueue (cmd.Trim ());
			}
		}
	}

	/*************** temp and  progress ***************/
	public void updateProgress ()
	{
		if (_gcodeLineCount == 0 || _gcodeQueue.Count == 0) {
			_struct.progress = 0;
		}
		_struct.progress = (_gcodeLineCount - _gcodeQueue.Count) / (_gcodeLineCount + 0.0f);
	}

	private void updateTemp (string dataReceivedTemp)
	{
		if (!string.IsNullOrEmpty (dataReceivedTemp)) {
			//ok T:26.4 /0.0 B:47.9 /0.0 B@:0 @:0
			//T:68.7 /0.0 B:53.8 /55.0 B@:127 @:0
			//T:200.2 /0.0 B:49.4 /50.0 B@:0 @:0 W:?
			//200.2 0.0 B:49.4 50.0 B@:0 @:0 W:?
			string str = dataReceivedTemp.Replace ("T", "").Replace ("ok", "").Replace (":", "").Replace ("/", "").Replace ("B", "").Trim ();
			string[] tempArray = str.Split (' ');

			if (tempArray.Length >= 5) {
				_struct.temp_cur_nozzle = float.Parse (tempArray [0]);
				_struct.temp_target_nozzle = float.Parse (tempArray [1]);
				_struct.temp_cur_bed = float.Parse (tempArray [2]);
				_struct.temp_target_bed = float.Parse (tempArray [3]);
			}
		}
	}

	/*************** PortConnectManager.Listener call back ***************/
	public void OnPortConnecting (string portName)
	{
	}

	public void OnPortConnectStarted (string portName)
	{
	}

	public void OnPortConnected (string portName)
	{
	}

	public void OnPortDisconnect (string portName)
	{
		_struct.sending_GcodeFile = false;
		_struct.stop_Manaual = false;

		_gcodeQueue.Clear ();
		_gcodeLineCount = 0;

		_struct.temp_target_nozzle = 0;
		_struct.temp_cur_nozzle = 0;
		_struct.temp_target_bed = 0;
		_struct.temp_cur_bed = 0;

        if(StageManager.GetStageList().Contains(StageManager.Stage_Enum.Gcode_Send)){
            StageManager.GetStageList().Remove(StageManager.Stage_Enum.Gcode_Send);
            StageManager.SetStage_Gcode_Render();
        }

        if(GcodeSenderManager.GetInstance()._struct.sending_GcodeFile){
            foreach (Listener listener in _listenerList)
            {
                listener.OnSendStoped();
            }
        }
	}

	public void OnPortConnectTimeOut (string portName)
	{
	}

	public void OnPortInsert (string portName)
	{
	}

	public void OnPortPullOut (string portName)
	{
	}

    public void OnPortError (string portName, string errorMsg)
	{
	}

	public void OnPortReceive (string line)
	{
		line = line.Trim ();
		if (line.StartsWith ("T:") || line.StartsWith ("ok T:")) {
			updateTemp (line);
		} 

		if (line.ToLower ().Contains ("ok")) {

			if (_struct.stop_Manaual) {
				if (_endGcodeQueue_stopManaual.Count > 0) {
					string cmd = _endGcodeQueue_stopManaual.Dequeue ();
					PortConnectManager.GetInstance ().WriteCmd (cmd);
					return;
				}
			}

			if (_struct.sending_GcodeFile) {
				if (_gcodeQueue.Count > 0) {
					string cmd = _gcodeQueue.Dequeue ();
					PortConnectManager.GetInstance ().WriteCmd (cmd);
					updateProgress ();
                    updateCompletedZ(cmd);
				} else {
					//print completed
					Debug.Log ("Printing completed" + "\n");
					_struct.sending_GcodeFile = false;
					_struct.stop_Manaual = false;

					_gcodeQueue.Clear ();
					_gcodeLineCount = 0;

					_struct.temp_target_nozzle = 0;
					_struct.temp_cur_nozzle = 0;
					_struct.temp_target_bed = 0;
					_struct.temp_cur_bed = 0;

					StageManager.GetStageList ().Remove (StageManager.Stage_Enum.Gcode_Send);
					StageManager.SetStage_Gcode_Render ();

					foreach (Listener listener in _listenerList) {
						listener.OnSendCompleted ();
					}
				}
			} else {
				//sub thread
				Thread.Sleep (1000);
				PortConnectManager.GetInstance ().WriteCmd ("M105");
			}
		}
	}

	public void OnPortOpenSucceed (string portName)
	{

	}

	public void OnPortOpenFailed (string portName, string reason)
	{
	}

    private void updateCompletedZ(string cmd){
        //Remove comments (if any)
        char[] charArray = cmd.Split(';')[0].Trim().ToCharArray();

        if (charArray.Length < 3)
        {
            return;
        }

        if (charArray[0] == 'G')
        {
            if (charArray[1] == '0')
            {
                //G0
                parseG0G1(charArray);
            }
            else if (charArray[1] == '1' && charArray[2] == ' ')
            {
                //G1
                parseG0G1(charArray);
            }
        }
    }

    /*************** the following code is from GcodeParser ***************/
    private static char[] _buffer_parseG0G1 = new char[15];
    private static int _endIndex_parseG0G1 = 0;
    private static char _type_parseG0G1 = ' ';
    private Vector3 _curPosition;
    private void parseG0G1(char[] charArray)
    {
        //data is useless until _curLayerIndex != int.MinValue
        //if (_curLayerIndex == int.MinValue)
        //{
        //    return;
        //}

        Vector3 tempPos = _curPosition;

        _type_parseG0G1 = ' ';
        float valueE = 0;

        //gcode for example: G1 X64.633 Y60.314 E635.56884
        //PS : float.Parse("1") == float.Parse("1 ")
        //switch case is little slower than if-else
        for (int i = 3; i < charArray.Length; i++)
        {
            if (charArray[i] == 'X' ||
                charArray[i] == 'Y' ||
                charArray[i] == 'Z' ||
                charArray[i] == 'E' ||
                charArray[i] == 'F')
            {
                _type_parseG0G1 = charArray[i];
            }
            else if (charArray[i] == ' ')
            {
                _buffer_parseG0G1[_endIndex_parseG0G1] = '\0';
                _endIndex_parseG0G1 = 0;
                if (_type_parseG0G1 == 'X')
                {
                    _curPosition.x = float.Parse(new string(_buffer_parseG0G1));
                }
                else if (_type_parseG0G1 == 'Y')
                {
                    _curPosition.y = float.Parse(new string(_buffer_parseG0G1));
                }
                else if (_type_parseG0G1 == 'Z')
                {
                    _curPosition.z = float.Parse(new string(_buffer_parseG0G1));
                }
                else if (_type_parseG0G1 == 'E')
                {
                    valueE = float.Parse(new string(_buffer_parseG0G1));
                }
            }
            else
            {
                _buffer_parseG0G1[_endIndex_parseG0G1++] = charArray[i];
            }
        }

        if (_endIndex_parseG0G1 > 0)
        {
            _buffer_parseG0G1[_endIndex_parseG0G1] = '\0';
            _endIndex_parseG0G1 = 0;
            if (_type_parseG0G1 == 'X')
            {
                _curPosition.x = float.Parse(new string(_buffer_parseG0G1));
            }
            else if (_type_parseG0G1 == 'Y')
            {
                _curPosition.y = float.Parse(new string(_buffer_parseG0G1));
            }
            else if (_type_parseG0G1 == 'Z')
            {
                _curPosition.z = float.Parse(new string(_buffer_parseG0G1));
            }
            else if (_type_parseG0G1 == 'E')
            {
                valueE = float.Parse(new string(_buffer_parseG0G1));
            }
        }

        //if (tempPos != _curPosition)
        //{
        //    GcodeRenderBean bean = new GcodeRenderBean(_curPosition, valueE, _curType, _curLayerIndex);
        //    _gcodeRenderPointList.Add(bean);
        //}

        _struct.completedZ = _curPosition.z;
    }
}


