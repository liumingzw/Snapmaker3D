using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.IO;

public class ModelManager : DataModel.Listener
{
	/*************** listener ***************/
	public interface Listener
	{
		void OnModelManager_ParseModelStarted () ;

		void OnModelManager_ParseModelSucceed () ;

		void OnModelManager_RenderModelSucceed () ;

		void OnModelManager_ParseModelError () ;

		void OnModelManager_ParseModelProgress (float progress);
	}

    private List<Listener> _listenerList = new List<Listener>();

    public void AddListener(Listener listener)
    {
        if (!_listenerList.Contains(listener))
        {
            _listenerList.Add(listener);
        }
    }

	/*************** OperateBean ***************/
	public class OperateBean
	{
		//operation
		public float scale;
		public Vector2 move;
		public Vector3 rotate;

		//cacahed info
		public Vector3 localPosition;
		public Vector3 size;

		public OperateBean (Vector2 move, float scale, Vector3 rotate, Vector3 localPosition, Vector3 size)
		{
			this.move = move;
			this.scale = scale;
			this.rotate = rotate;
			this.localPosition = localPosition;
			this.size = size;
		}

		public OperateBean DeepCopy ()
		{
			return new OperateBean (move, scale, rotate, localPosition, size);
		}

		public int GetMyHashCode (string path)
		{
			int hash = 17;
			hash = hash * 23 + path.GetHashCode ();
			hash = hash * 23 + move.GetHashCode ();
			hash = hash * 23 + scale.GetHashCode ();
			hash = hash * 23 + rotate.GetHashCode ();
			return hash;
		}

		public override string ToString ()
		{
			string str = string.Format ("scale:" + "[{0}] ", scale);
			str = str + string.Format ("move:" + "[{0}] ", move);
			str = str + string.Format ("rotate:" + "[{0}] ", rotate);
			str = str + string.Format ("localPosition:" + "[{0}] ", localPosition);
			str = str + string.Format ("size:" + "[{0}] ", size);
			return str;
		}
	}


	/*************** info ***************/
	private InfoStruct _infoStruct;

	/*************** model operate ***************/
	private Model3d _model3d;
	private OperateBean _curOperateBean;
	private Vector2 _scaleRange;
	private int _triggerEnterObjectCount;

	/*************** undo & redo & reset ***************/
	//_undoStack : init,op0,op1,op2.....curOp
	private Stack<OperateBean> _undoStack = new Stack<OperateBean> ();
	private Stack<OperateBean> _redoStack = new Stack<OperateBean> ();

	/*************** model material ***************/
	private string _mt_current;
	private const string _mt_printable = "Materials/material_model_printable";
	private const string _mt_unprintable = "Materials/material_model_unprintable";
	private const string _mt_translucent = "Materials/material_model_translucent";

	/*************** others ***************/
	private static Vector3 _deviceSize;

	/*************** 1.single ***************/
	private static ModelManager _INSTANCE;

	private ModelManager ()
	{
	}

	public static ModelManager GetInstance ()
	{
		if (_INSTANCE == null) {
			_INSTANCE = new ModelManager ();
			_deviceSize = Global.GetInstance ().GetPrinterParamsStruct ().size;
		}
		return _INSTANCE;
	}

	/*************** 2.info struct ***************/
	public struct InfoStruct
	{
		public bool isParsing;
		public float parseProgress;
		public string modelPath;
		public bool printable;
	}

	public InfoStruct GetInfo ()
	{
		return _infoStruct;
	}

	/*************** 3.parse model thread ***************/
	public void StartSubThread_parseModel (string path)
	{
		_infoStruct.isParsing = true;
		_infoStruct.parseProgress = 0;
		_infoStruct.modelPath = path;
		_infoStruct.printable = false;

		if (_model3d != null) {
			UnityEngine.Object.Destroy (_model3d.renderedModel.shellCube);
		}
		_model3d = null;

		_curOperateBean = null;
		_scaleRange = Vector2.zero;
		_triggerEnterObjectCount = 0;

		_redoStack.Clear ();
		_undoStack.Clear ();

		_mt_current = null;

		Thread _readThread = new Thread (doParse);
		_readThread.Start ();
	}

	private void doParse ()
	{
        foreach(Listener listener in _listenerList){
            listener.OnModelManager_ParseModelStarted();
        }

		DataModel dataModel = null;

		if (_infoStruct.modelPath.Trim ().ToLower ().EndsWith (".stl")) {
			dataModel = new DataModelStl ();
		} else if (_infoStruct.modelPath.Trim ().ToLower ().EndsWith (".obj")) {
			dataModel = new DataModelObj ();
		} 

		dataModel.SetListener (this);
		dataModel.ParseFacetListFromFile (_infoStruct.modelPath);
	}

	public Model3d GetModel ()
	{
		return _model3d;
	}

	/*************** 4.DataModel Listener callback ***************/
	public void OnDataModel_ParseProgress (float progress)
	{
		_infoStruct.parseProgress = progress;

        foreach (Listener listener in _listenerList)
        {
            listener.OnModelManager_ParseModelProgress(progress);
        }
	}

	public void OnDataModel_ParseFailed ()
	{
		UnityEngine.Debug.LogError ("OnDataModel_ParseFailed " + "\n");
		_infoStruct.isParsing = false;

        foreach (Listener listener in _listenerList)
        {
            listener.OnModelManager_ParseModelError();
        }
	}

	public void OnDataModel_ParseComplete (DataModel dataModel)
	{
		UnityEngine.Debug.Log ("OnParseResult_Complete " + "\n");

		//1.CalculateSize and setToSymmetry
		dataModel.PreTreatedForRender ();

        foreach (Listener listener in _listenerList)
        {
            listener.OnModelManager_ParseModelSucceed();
        }

		Loom.RunAsync (
			() => {
				Thread t = new Thread (new ParameterizedThreadStart (renderModel));  
				t.Start (dataModel); 
			}
		);
	}

	private void renderModel (object obj)
	{  
		DataModel dataModel = (DataModel)obj;

		Loom.QueueOnMainThread ((param) => {

			_model3d = new Model3d (dataModel);

            foreach (Listener listener in _listenerList)
            {
                listener.OnModelManager_RenderModelSucceed();
            }

			_infoStruct.isParsing = false;

		}, null);
	}

	/*************** 5.operate model ***************/
	public void MoveRenderXTo (float value)
	{
		if (_model3d == null) {
			return;
		}
		value = value > _deviceSize.x / 2 ? _deviceSize.x / 2 : value;
		value = value < -_deviceSize.x / 2 ? -_deviceSize.x / 2 : value;

		Vector3 newPosition = _model3d.GetRenderLocalPosition ();
		newPosition.x = value / _deviceSize.x;
		_model3d.SetRenderLocalPosition (newPosition);

		_curOperateBean.move.x = value;
		_curOperateBean.localPosition = newPosition;
	}

	public void MoveRenderYTo (float value)
	{
		if (_model3d == null) {
			return;
		}
		value = value > _deviceSize.y / 2 ? _deviceSize.y / 2 : value;
		value = value < -_deviceSize.y / 2 ? -_deviceSize.y / 2 : value;

		Vector3 newPosition = _model3d.GetRenderLocalPosition ();
		newPosition.z = -value / _deviceSize.y;
		_model3d.SetRenderLocalPosition (newPosition);

		_curOperateBean.move.y = value;
		_curOperateBean.localPosition = newPosition;
	}

	public void ScaleRenderTo (float value)
	{
		if (_model3d == null) {
			return;
		}

		float min = _scaleRange.x;
		float max = _scaleRange.y;

		value = value > max ? max : value;
		value = value < min ? min : value;
		_model3d.ScaleRenderTo (value);

		_curOperateBean.scale = value;
	}

	public void RotateRenderXTo (float value)
	{
		if (_model3d == null) {
			return;
		}
		value = value > 180 ? 180 : value;
		value = value < -180 ? -180 : value;

		_curOperateBean.rotate.x = value;
		Vector3 eulerAngles = new Vector3 (-_curOperateBean.rotate.x, -_curOperateBean.rotate.z, -_curOperateBean.rotate.y);
		_model3d.SetRenderLocalEulerAngles (eulerAngles);
	}

	public void RotateRenderYTo (float value)
	{
		if (_model3d == null) {
			return;
		}
		value = value > 180 ? 180 : value;
		value = value < -180 ? -180 : value;

		_curOperateBean.rotate.y = value;
		Vector3 eulerAngles = new Vector3 (-_curOperateBean.rotate.x, -_curOperateBean.rotate.z, -_curOperateBean.rotate.y);
		_model3d.SetRenderLocalEulerAngles (eulerAngles);
	}

	public void RotateRenderZTo (float value)
	{
		if (_model3d == null) {
			return;
		}
		value = value > 180 ? 180 : value;
		value = value < -180 ? -180 : value;

		_curOperateBean.rotate.z = value;
		Vector3 eulerAngles = new Vector3 (-_curOperateBean.rotate.x, -_curOperateBean.rotate.z, -_curOperateBean.rotate.y);
		_model3d.SetRenderLocalEulerAngles (eulerAngles);
	}

	/*************** 6.printable & translucent ***************/
	public void SetPrintable (bool value)
	{
		if (_model3d == null) {
			return;
		}

		_infoStruct.printable = value;

		if (_infoStruct.printable) {
			if (_mt_current == _mt_printable) {
				return;
			} else {
				_mt_current = _mt_printable;
			}
		} else {
			if (_mt_current == _mt_unprintable) {
				return;
			} else {
				_mt_current = _mt_unprintable;
			}
		}

		_model3d.SetRenderMaterial (Resources.Load<Material> (_mt_current));
	}

	public void SetTranslucent ()
	{
		if (_mt_current == _mt_translucent) {
			return;
		} else {
			_mt_current = _mt_translucent;
		}
		_model3d.SetRenderMaterial (Resources.Load<Material> (_mt_current));
	}

	/*************** 7.Trigger ***************/
	public void OnTriggerEnter (Collider e)
	{
		if (_model3d == null) {
			return;
		}
		string name = e.transform.gameObject.name;
		if (name.StartsWith ("child")) {
			++_triggerEnterObjectCount;

			//AlertMananger.GetInstance ().ShowAlertMsg ("Please move, scale or rotate the model so that it's within the build volume.");

			SetPrintable (false);
		}
	}

	public void OnTriggerExit (Collider e)
	{
		if (_model3d == null) {
			return;
		}
		string name = e.transform.gameObject.name;
		if (name.StartsWith ("child")) {
			if (--_triggerEnterObjectCount == 0) {
				SetPrintable (true);
			}
		}
	}

	/*************** 8.data model ***************/
	public string SaveModelAsStlFile ()
	{
		if (_model3d == null) {
			return null;
		}

		string path = PathManager.dirPath_Temp () + Utils.CreateMD5 (_curOperateBean.GetMyHashCode (_infoStruct.modelPath).ToString ()) + ".stl";
		if (File.Exists (path)) {
			UnityEngine.Debug.Log ("Stl file had existed:" + path);
			return path;
		}

		_model3d.UpdateCurDataModel (
			_curOperateBean.scale,
			_curOperateBean.move.x,
			_curOperateBean.move.y,
			_curOperateBean.rotate.x,
			_curOperateBean.rotate.y,
			_curOperateBean.rotate.z
		);

		_model3d.SaveAsStlFile (path);
		return path;
	}

	private Vector2 calculateScaleRange (DataModel dataModel, Vector3 deviceSize)
	{
		float min, max;
		Vector3 size = dataModel.GetSize (); 

		//case1 : model is too big 
		if (size.x > deviceSize.x || size.y > deviceSize.y || size.z > deviceSize.z) {
			max = 1;
			min = 0.0f;
			return new Vector2 (min, max);
		}

		//case2 : model is too tiny 
		if (Mathf.Max (size.x, size.y, size.z) < 5) {
			max = Mathf.Min (deviceSize.x / size.x, deviceSize.y / size.y, deviceSize.z / size.z);
			min = 1.0f;
			return new Vector2 (min, max);
		}

		//case3 : normal
		{
			max = Mathf.Min (deviceSize.x / size.x, deviceSize.y / size.y, deviceSize.z / size.z);
			min = 0.0f;
			return new Vector2 (min, max);
		}

		return Vector2.zero;
	}

	public OperateBean GetRenderOperateBean ()
	{
		return _curOperateBean;
	}

	public Vector2 GetScaleRange ()
	{
		return _scaleRange;
	}

	public void SetModelInitialLocalPosition ()
	{
		Vector3 originDataSize = _model3d.GetOriginDataSize ();
		//data <---> render : swap y <---> z
		float y = -(_deviceSize.y - originDataSize.z) / 2.0f;
		Vector3 newPosition = _model3d.GetRenderLocalPosition ();
		newPosition.y = y / _deviceSize.y;
		newPosition.x = 0;
		newPosition.z = 0;
		_model3d.SetRenderLocalPosition (newPosition);

		_scaleRange = calculateScaleRange (_model3d.dataModel_origin, _deviceSize);

		//init _operateBean
		Vector2 move = Vector2.zero;
		float scale = 1.0f;
		Vector3 rotate = Vector3.zero;
		Vector3 renderModel_LocalPosition = newPosition;
		Vector3 dataModel_Size = _model3d.GetOriginDataSize ();

		_curOperateBean = new OperateBean (move, scale, rotate, renderModel_LocalPosition, dataModel_Size);

		_undoStack.Clear ();
		_redoStack.Clear ();

		_undoStack.Push (_curOperateBean.DeepCopy ());
	}

	public Vector3 GetCurDataSize ()
	{
		return _model3d.GetCurDataSize ();
	}

	public void OnOperateStart ()
	{
		GcodeRenderManager.GetInstance ().SetActive_gcodeRender (false);
	}

	public void OnOperateEnd ()
	{
		//1.check
		if (_model3d == null) {
			return;
		}

		OperateBean lastBean = _undoStack.Peek ();
		if (lastBean.scale == _curOperateBean.scale &&
		    lastBean.move == _curOperateBean.move &&
		    lastBean.rotate == _curOperateBean.rotate) {

			Debug.Log ("Model is not moved/scaled/rotated, return" + "\n");

			GcodeRenderManager.GetInstance ().SetActive_gcodeRender (true);

			return;
		}

		UnityEngine.Debug.Log ("************************ OnOperateEnd ***************************" + "\n");

		//2.release about gcode
        GcodeCreateManager.GetInstance ().UnavailableCurGcodeCreateBean();
		GcodeRenderManager.GetInstance ().Destroy ();

		//3.operate
		bool moveChanged = lastBean.move != _curOperateBean.move;
		bool scaleChanged = lastBean.scale != _curOperateBean.scale;
		bool rotateChanged = lastBean.rotate != _curOperateBean.rotate;

		if (moveChanged) {
			Debug.Log ("Model is moved" + "\n");
		}
		if (scaleChanged) {
			Debug.Log ("Model is scaled" + "\n");
		}
		if (rotateChanged) {
			Debug.Log ("Model is rotated" + "\n");
		}

		//case1: only move param changed
		if (moveChanged && !scaleChanged && !rotateChanged) {
			//No need to update dataModel, vertical value not changed
			//update dataModel may costs lot time
			Debug.Log ("Model is only moved" + "\n");
		} else {
			//case2: scale/rotate changed
			Debug.Log ("Model is scaled/rotated" + "\n");

			//UpdateCurDataModel
			//dataModel will change
			_model3d.UpdateCurDataModel (
				_curOperateBean.scale,
				_curOperateBean.move.x,
				_curOperateBean.move.y,
				_curOperateBean.rotate.x,
				_curOperateBean.rotate.y,
				_curOperateBean.rotate.z
			);

			//set render position
			float minZ_cur = _model3d.dataModel_cur.GetMinZ ();
			float minZ_origin = _model3d.dataModel_origin.GetMinZ ();
			float disZ = minZ_origin - minZ_cur;

			Vector3 newPosition = _model3d.renderedModel.shellCube.transform.localPosition;

			float y = -(_deviceSize.y - _model3d.GetOriginDataSize ().z) / 2.0f;
			float localY = y / _deviceSize.y;

			newPosition.y = localY + disZ / _deviceSize.y;
			_model3d.renderedModel.shellCube.transform.localPosition = newPosition;

			//record
			_curOperateBean.localPosition = newPosition;
			//after UpdateCurDataModel and operate render model,
			//the dataModel_cur.GetSize is actural renderModel size
			_curOperateBean.size = _model3d.dataModel_cur.GetSize ();
		}

		//4.undo/redo
		_undoStack.Push (_curOperateBean.DeepCopy ());
		_redoStack.Clear ();

		UnityEngine.Debug.Log ("***************************************************" + "\n");
	}

	public void Undo ()
	{
		//1.check
		if (!CanExecuteUndo ()) {
			Debug.Log ("Can not execute undo" + "\n");
			return;
		}

		//2.handle gcode render
        GcodeCreateManager.GetInstance().UnavailableCurGcodeCreateBean();
		GcodeRenderManager.GetInstance ().Destroy ();

		//3._undoStack Pop and _redoStack push 
		_redoStack.Push (_undoStack.Pop ());

		OperateBean targetRenderBean = _undoStack.Peek ();

		Debug.Log ("undo:" + "\n" + _curOperateBean + "-->" + "\n" + targetRenderBean + "\n");

		operateRenderModel (targetRenderBean);

		_curOperateBean = targetRenderBean.DeepCopy ();
	}

	public void Redo ()
	{
		//1.check
		if (!CanExecuteRedo ()) {
			Debug.Log ("Can not execute redo" + "\n");
			return;
		}

		//handle gcode render
        GcodeCreateManager.GetInstance().UnavailableCurGcodeCreateBean();
		GcodeRenderManager.GetInstance ().Destroy ();

		//_undoStack Pop and _redoStack push 
		_undoStack.Push (_redoStack.Pop ());

		OperateBean targetRenderBean = _undoStack.Peek ();

		Debug.Log ("redo:" + "\n" + _curOperateBean + "-->" + "\n" + targetRenderBean + "\n");

		operateRenderModel (targetRenderBean);

		_curOperateBean = targetRenderBean.DeepCopy ();
	}

	public void Reset ()
	{
		//1.check
		if (!CanExecuteReset ()) {
			Debug.Log ("Can not execute reset" + "\n");
			return;
		}

		//2.handle gcode render
        GcodeCreateManager.GetInstance().UnavailableCurGcodeCreateBean();
		GcodeRenderManager.GetInstance ().Destroy ();

		//3.remove all bean except initBean
		while (_undoStack.Count > 1) {
			_undoStack.Pop ();
		}
		_redoStack.Clear ();

		OperateBean initRenderBean = _undoStack.Peek ();

		Debug.Log ("Reset:" + "\n" + _curOperateBean + "-->" + "\n" + initRenderBean + "\n");

		operateRenderModel (initRenderBean);

		_curOperateBean = initRenderBean.DeepCopy ();
	}

	private void operateRenderModel (OperateBean bean)
	{
		Vector3 eulerAngles = new Vector3 (-bean.rotate.x, -bean.rotate.z, -bean.rotate.y);
		_model3d.SetRenderLocalEulerAngles (eulerAngles);

		_model3d.SetRenderLocalPosition (bean.localPosition);
		_curOperateBean.scale = bean.scale;
	}

	public bool CanExecuteUndo ()
	{
		if (_model3d == null) {
			return false;
		}
		if (_undoStack.Count <= 1) {
			return false;
		}
		return true;
	}

	public bool CanExecuteRedo ()
	{
		if (_model3d == null) {
			return false;
		}
		if (_redoStack.Count == 0) {
			return false;
		}
		return true;
	}

	public bool CanExecuteReset ()
	{
		return CanExecuteUndo () || CanExecuteRedo ();
	}
}
