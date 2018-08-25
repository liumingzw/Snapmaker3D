using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class GcodeDrawLineManager
{
	private class DrawItem
	{
		// 1 vectorLine can draw less than 16383 points
		// N points need N-1 colors
		public VectorLine vectorLine;
		public List<GcodeRenderBean> gcodeRenderPointList;
		public List<Color32> colorList = new List<Color32> ();
	}

	//a VectorLine instance can draw points less than 16383
	private const int _pointCountLimit = 16383;

	//default value is 0.8f
	private float _lineWidth = 0.8f;

	//1.all GcodeRenderPoint
	private List<GcodeRenderBean> _gcodeRenderBeanList = new List<GcodeRenderBean> ();

	//2.types
	private List<DrawItem> _drawItemList_types = new List<DrawItem> ();
	private List<VectorLine> _vectorLineList_types = new List<VectorLine> ();
	private Dictionary<GcodeType, Color32> _colorsDic_types;

	//3.top layer
	private Color32 _colors_topLayer;
	private bool _active_topLayer;
	private List<DrawItem> _drawItemList_topLayer = new List<DrawItem> ();
	private List<VectorLine> _vectorLineList_topLayer = new List<VectorLine> ();

	//4.show layers
	//every item(List<Vector3>) is for : every layer
	//separate all Vector3 by layer index
    public List<List<Vector3>> _vectorsList_layer = new List<List<Vector3>> ();

	/*************** single ***************/
	private static GcodeDrawLineManager _INSTANCE;

	private GcodeDrawLineManager ()
	{
	}

	public static GcodeDrawLineManager GetInstance ()
	{
		if (_INSTANCE == null) {
			_INSTANCE = new GcodeDrawLineManager ();
		}
		return _INSTANCE;
	}


	public void SetLineWidth (float value)
	{
		if (value <= 0) {
			return;
		} 
		_lineWidth = value;
		foreach (VectorLine line in _vectorLineList_types) {
			line.SetWidth (_lineWidth);
		}
		foreach (VectorLine line in _vectorLineList_topLayer) {
			line.SetWidth (_lineWidth);
		}
	}

	public void SetGcodeRenderBeans (List<GcodeRenderBean> gcodeRenderBeanList)
	{
		if (gcodeRenderBeanList == null || gcodeRenderBeanList.Count == 0) {
			Debug.LogWarning ("gcodeRenderBeanList is empty" + "\n");
			return;
		}

		_gcodeRenderBeanList = gcodeRenderBeanList;

		//1.release
		VectorLine.Destroy (_vectorLineList_types.ToArray ());
		VectorLine.Destroy (_vectorLineList_topLayer.ToArray ());

		//types
		_drawItemList_types.Clear ();
		_vectorLineList_types.Clear ();
		_colorsDic_types = null;

		//top layer
		_colors_topLayer = GcodeTypeColor.Top_Layer;
		_active_topLayer = false;
		_drawItemList_topLayer.Clear ();
		_vectorLineList_topLayer.Clear ();

		_vectorsList_layer.Clear ();

		//2.separate gcodeRenderPoints by 16383
		//per vectorLine is assigned 16383 Vector3 
		for (int i = 0; i < Mathf.CeilToInt (_gcodeRenderBeanList.Count / (float)_pointCountLimit); i++) {
			
			List<GcodeRenderBean> gcodeRenderPointBuffer_line = null;

			List<Vector3> vectorsItem_line = new List<Vector3> ();

			if (_gcodeRenderBeanList.Count >= i * _pointCountLimit + _pointCountLimit) {
				gcodeRenderPointBuffer_line = _gcodeRenderBeanList.GetRange (i * _pointCountLimit, _pointCountLimit);
			} else {
				gcodeRenderPointBuffer_line = _gcodeRenderBeanList.GetRange (i * _pointCountLimit, _gcodeRenderBeanList.Count - i * _pointCountLimit);
			}

			foreach (GcodeRenderBean gcodePoint in gcodeRenderPointBuffer_line) {
				//Attention : switch y <====> z 
				Vector3 vec3 = new Vector3 (gcodePoint.vector3.x, gcodePoint.vector3.z, gcodePoint.vector3.y);
				vectorsItem_line.Add (vec3);
			}

			VectorLine vectorLine = new VectorLine ("line_" + i, vectorsItem_line, null, _lineWidth, LineType.Continuous, Joins.None);
			vectorLine.active = true;

			DrawItem drawItem = new DrawItem ();
			drawItem.vectorLine = vectorLine;
			drawItem.gcodeRenderPointList = gcodeRenderPointBuffer_line;
			_drawItemList_types.Add (drawItem);
		}

		foreach (DrawItem item in _drawItemList_types) {
			_vectorLineList_types.Add (item.vectorLine);
		}

		//3.separate all Vector3 by layer index
		//if the point count of 1 layer is less than 16383, draw them by 1 vectorLine instance
		//else draw them by more than 1 vectorLine instance
		List<GcodeRenderBean> gcodeRenderPointBuffer_layer = new List<GcodeRenderBean> ();
		gcodeRenderPointBuffer_layer.Add (_gcodeRenderBeanList [0]);
		List<Vector3> vectorsItem_layer = new List<Vector3> ();

		for (int i = 1; i < _gcodeRenderBeanList.Count; i++) {
			GcodeRenderBean pointNew = _gcodeRenderBeanList [i];
			GcodeRenderBean pointLast = gcodeRenderPointBuffer_layer [gcodeRenderPointBuffer_layer.Count - 1];

			if (pointNew.layerIndex == pointLast.layerIndex) {
				gcodeRenderPointBuffer_layer.Add (pointNew);
			} else {
				foreach (GcodeRenderBean temp in gcodeRenderPointBuffer_layer) {
					//Attention : switch y <====> z 
					Vector3 vec3 = new Vector3 (temp.vector3.x, temp.vector3.z, temp.vector3.y);
					vectorsItem_layer.Add (vec3);
				}

				//vectorsItem_layer will clear, so need deep copy
				_vectorsList_layer.Add (new List<Vector3> (vectorsItem_layer));
				vectorsItem_layer.Clear ();
				gcodeRenderPointBuffer_layer.Clear ();
				gcodeRenderPointBuffer_layer.Add (pointNew);
			}
		}

		//attention : there still are points in buff
		foreach (GcodeRenderBean gcodePoint in gcodeRenderPointBuffer_layer) {
			//Attention : switch y <====> z 
			Vector3 vec3 = new Vector3 (gcodePoint.vector3.x, gcodePoint.vector3.z, gcodePoint.vector3.y);
			vectorsItem_layer.Add (vec3);
		}

		//vectorsItem_layer will clear, so need deep copy
		_vectorsList_layer.Add (new List<Vector3> (vectorsItem_layer));
		vectorsItem_layer.Clear ();
		gcodeRenderPointBuffer_layer.Clear ();

		int pointCountMax_layer = 0;
		foreach (List<Vector3> list in _vectorsList_layer) {
			pointCountMax_layer = list.Count > pointCountMax_layer ? list.Count : pointCountMax_layer;
		}

		for (int i = 0; i < Mathf.CeilToInt (pointCountMax_layer / (float)_pointCountLimit); i++) {

			VectorLine vectorLine = new VectorLine ("line_top_" + i, new List<Vector3>(), null, _lineWidth, LineType.Continuous, Joins.None);
			vectorLine.active = true;
			_vectorLineList_topLayer.Add (vectorLine);
		}
	}
		 
	//Show several layers at the bottom
	//count=0 means invisiable
	public void SetVisiableLayerCount (int count)
	{
		if (_gcodeRenderBeanList.Count == 0) {
			return;
		}

		int _layerCount = _vectorsList_layer.Count;
		
		count = count > _layerCount ? _layerCount : count;
		count = count < 0 ? 0 : count;

		int visiablePointCount = 0;
		int startLayerIndex = _gcodeRenderBeanList [0].layerIndex;

		//todo : more effictive, use binary search
		for (int i = 0; i < _gcodeRenderBeanList.Count; i++) {
			if (_gcodeRenderBeanList [i].layerIndex - startLayerIndex < count) {
				++visiablePointCount;
			} else {
				break;
			}
		}

		//max value of vectorLine shown
		int vectorLineIndex_max = Mathf.CeilToInt (visiablePointCount / (float)_pointCountLimit);

		//less than k-1, show
		//greater than k-1, hide
		//equals k-1, show partly 
		for (int i = 0; i < _vectorLineList_types.Count; i++) {
			if (i < vectorLineIndex_max - 1) {
				_vectorLineList_types [i].drawEnd = _vectorLineList_types [i].points3.Count - 1;
			} else if (i > vectorLineIndex_max - 1) {
				_vectorLineList_types [i].drawEnd = 0;
			} else {
				_vectorLineList_types [i].drawEnd = visiablePointCount % _pointCountLimit;
			}
		}

		//_vectorLine_top.active = (count != 1);
		//index out of range exception
        if(count > 1){
            List<Vector3> vectorList = _vectorsList_layer[count - 1];

            for (int i = 0; i < Mathf.CeilToInt(vectorList.Count / (float)_pointCountLimit); i++)
            {
                List<Vector3> vectorsItem_line = new List<Vector3>();

                if (vectorList.Count >= i * _pointCountLimit + _pointCountLimit)
                {
                    vectorsItem_line = vectorList.GetRange(i * _pointCountLimit, _pointCountLimit);
                }
                else
                {
                    vectorsItem_line = vectorList.GetRange(i * _pointCountLimit, vectorList.Count - i * _pointCountLimit);
                }

                _vectorLineList_topLayer[i].points3 = vectorsItem_line;
            }

            for (int i = 0; i < Mathf.CeilToInt(vectorList.Count / (float)_pointCountLimit); i++)
            {
                if (i <= Mathf.CeilToInt(vectorList.Count / (float)_pointCountLimit))
                {
                    _vectorLineList_topLayer[i].active = true;
                }
                else
                {
                    _vectorLineList_topLayer[i].active = false;
                }
            }
        }else {
            foreach (VectorLine line in _vectorLineList_topLayer)
            {
                line.active = false;
            }
        }

		//must SetColor_type
		//set color should call at last
		SetColor_types (_colorsDic_types);

		SetColor_topLayer (_colors_topLayer);
	}

	public void SetActive_types (bool value)
	{
		foreach (VectorLine line in _vectorLineList_types) {
			line.active = value;
		}
	}

	public void SetActive_topLayer (bool value)
	{
		foreach (VectorLine line in _vectorLineList_topLayer) {
			line.active = value;
		}
	}

	public void SetColor_types (Dictionary<GcodeType, Color32> colorsDic)
	{
		if (colorsDic.Count == 0) {
			return;
		}
		_colorsDic_types = colorsDic;
		//change colors 
		Color32 color_WALL_INNER = colorsDic [GcodeType.WALL_INNER];
		Color32 color_WALL_OUTER = colorsDic [GcodeType.WALL_OUTER];
		Color32 color_SKIN = colorsDic [GcodeType.SKIN];
		Color32 color_SKIRT = colorsDic [GcodeType.SKIRT];
		Color32 color_SUPPORT = colorsDic [GcodeType.SUPPORT];
		Color32 color_FILL = colorsDic [GcodeType.FILL];
		Color32 color_UNKNOWN = colorsDic [GcodeType.UNKNOWN];
		Color32 color_Travel = colorsDic [GcodeType.Travel];

		for (int i = 0; i < _drawItemList_types.Count; i++) {
			DrawItem item = _drawItemList_types [i];
			item.colorList.Clear ();

			for (int k = 0; k < item.gcodeRenderPointList.Count; k++) {
				GcodeRenderBean point = item.gcodeRenderPointList [k];

				Color32 color32 = GcodeTypeColor.UNKNOWN;
				switch (point.type) {
				case GcodeType.WALL_INNER:
					color32 = color_WALL_INNER;
					break;
				case GcodeType.WALL_OUTER:
					color32 = color_WALL_OUTER;
					break;
				case GcodeType.SKIN:
					color32 = color_SKIN;
					break;
				case GcodeType.SKIRT:
					color32 = color_SKIRT;
					break;
				case GcodeType.SUPPORT:
					color32 = color_SUPPORT;
					break;
				case GcodeType.FILL:
					color32 = color_FILL;
					break;
				case GcodeType.UNKNOWN:
					color32 = color_UNKNOWN;
					break;
				case GcodeType.Travel:
					color32 = color_Travel;
					break;
				}

				item.colorList.Add (color32);
			}

			item.colorList.RemoveAt (0);
			item.vectorLine.SetColors (item.colorList);
		}
	}

	public void SetColor_topLayer (Color32 value)
	{
		_colors_topLayer = value;
		foreach (VectorLine line in _vectorLineList_topLayer) {
			line.color = value;
		}
	}

	public void Draw ()
	{
		foreach (VectorLine line in _vectorLineList_types) {
			line.Draw ();

//			SetColor_topLayer (_colors_topLayer);
		}
		foreach (VectorLine line in _vectorLineList_topLayer) {
			line.Draw ();
		}
	}

	//todo
	public void Destroy ()
	{
		VectorLine.Destroy (_vectorLineList_types.ToArray ());
		VectorLine.Destroy (_vectorLineList_topLayer.ToArray ());
		_vectorLineList_types.Clear ();
		_vectorsList_layer.Clear ();

		//_gcodeRenderPointList.Clear () will invoke out of range exception
		//_gcodeRenderPointList.Clear ();
	}

	public List<VectorLine> GetVectorLineList_types ()
	{
		return _vectorLineList_types;
	}

	public List<VectorLine> GetVectorLine_topLayer ()
	{
		return _vectorLineList_topLayer;
	}

	public float GetLineWidth ()
	{
		return _lineWidth;
	}

	public bool GetIsActive_topLayer ()
	{
		return _active_topLayer;
	}

    public void SetVisiableLayerLessThanZ(float z){
        //calculate layer count less than z
        int count = calculateLayerCoutLessThanZ(z);
        //Debug.LogError("count="+count+"\n");
        SetVisiableLayerCount(count);
    }

    private int calculateLayerCoutLessThanZ(float des){
        for (int i = 1; i < _vectorsList_layer.Count-1;i++){
            float z_Low = _vectorsList_layer[i-1][0].y;
            float z = _vectorsList_layer[i][0].y;
            float z_high = _vectorsList_layer[i+1][0].y;
            if (des>=z_Low && des<= z_high)
            {
                return i;
            }
        }
        return 0;
    }
}
