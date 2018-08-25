using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using System.IO;
using System.Threading;

public class GcodeRenderManager
{
    /*************** listener ***************/
    public interface Listener
    {
        void OnGcodeParseSucceed();
        void OnGcodeParseFailed();
    }

    private List<Listener> _listenerList = new List<Listener>();

    public void AddListener(Listener listener)
    {
        if (!_listenerList.Contains(listener))
        {
            _listenerList.Add(listener);
        }
    }

    public GameObject gcodeRenderGameObject;

    /*************** single ***************/
    private static GcodeRenderManager _INSTANCE;

    private GcodeRenderManager()
    {
    }

    public static GcodeRenderManager GetInstance()
    {
        if (_INSTANCE == null)
        {
            _INSTANCE = new GcodeRenderManager();
            _INSTANCE.initColorScheme();
        }
        return _INSTANCE;
    }

    /*************** render struct***************/
    public struct InfoStruct
    {
        public bool isParsing;
        public float parseProgress;
        public bool isRendered;
    }

    private InfoStruct _struct;

    public InfoStruct GetInfo()
    {
        _struct.parseProgress = GcodeParser.GetInstance().GetParseProgress();
        return _struct;
    }

    /*************** public ***************/
    public bool StartParseGcodeFile(string path)
    {
        if (string.IsNullOrEmpty(path) || path.Trim().Length == 0)
        {
            Debug.LogError("Error occur: StartParseGcodeFile [path is null]" + "\n");
            return false;
        }

        if (!File.Exists(path))
        {
            Debug.LogError("Error occur: StartParseGcodeFile [file not exists:" + path + "]" + "\n");
            return false;
        }

        UnityEngine.Debug.Log("Start parse gcode file..." + "\n");

        _struct.isParsing = true;
        _struct.isRendered = false;
        _struct.parseProgress = 0;

        StageManager.SetStage_Gcode_Render();

        //cost long time if file is big
        GcodeParser.GetInstance().StartParseGcodeFile(path);

        if(GcodeParser.GetInstance().GetLayerCount() > 0){
            foreach (Listener listener in _listenerList)
            {
                listener.OnGcodeParseSucceed();
            }
        }else {
            foreach (Listener listener in _listenerList)
            {
                listener.OnGcodeParseFailed();
            }
        }

        _struct.isParsing = false;
        _struct.parseProgress = 1;

        return true;
    }

    //must call this method in LateUpdate every frame
    public void DrawLayers()
    {
        GcodeDrawLineManager.GetInstance().Draw();
    }

    public void AutoSetLineWidth(float distance)
    {
        /**
		 * 应该采集一些数据，做函数拟合，来计算线宽。参数如下：
		 * 1.模型的transform.z
		 * 2.Gcode parse result : layerHeight
		 **/

        //		float width = GcodeParser.GetInstance ().GetLayerHeight () * 1000 / distance ;
        //float width = 12 / Mathf.Sqrt(distance);
        float width = 700 / distance ;
        if (Mathf.Abs(width - GcodeDrawLineManager.GetInstance().GetLineWidth()) > 0.05f)
        {
            GcodeDrawLineManager.GetInstance().SetLineWidth(width);
        }
    }

    public GameObject RenderGcode()
    {
        Destroy();

        float lineWidth = GcodeParser.GetInstance().GetLayerHeight() * 5;

        GcodeDrawLineManager.GetInstance().SetGcodeRenderBeans(GcodeParser.GetInstance().GetGcodeRenderPointList());
        GcodeDrawLineManager.GetInstance().SetLineWidth(lineWidth);

        gcodeRenderGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gcodeRenderGameObject.name = "gcodeRenderGameObject";
        gcodeRenderGameObject.GetComponent<Renderer>().enabled = false;

        foreach (VectorLine vectorLine in GcodeDrawLineManager.GetInstance().GetVectorLineList_types())
        {
            vectorLine.drawTransform = gcodeRenderGameObject.transform;
            vectorLine.active = true;
        }

        foreach (VectorLine vectorLine in GcodeDrawLineManager.GetInstance().GetVectorLine_topLayer())
        {
            vectorLine.drawTransform = gcodeRenderGameObject.transform;
            vectorLine.active = true;
        }

        _struct.isRendered = true;

        initColorScheme();
        UpdatGcodePreviewColors();

        SetActive_Top_Layer(true);
        GcodeDrawLineManager.GetInstance().SetColor_topLayer(GcodeTypeColor.Top_Layer);

        return gcodeRenderGameObject;
    }

    public void SetActive_gcodeRender(bool value)
    {
        GcodeDrawLineManager.GetInstance().SetActive_types(value);
        GcodeDrawLineManager.GetInstance().SetActive_topLayer(value);
    }

    public void Destroy()
    {
        _struct.isParsing = false;
        _struct.parseProgress = 0;
        _struct.isRendered = false;

        GcodeDrawLineManager.GetInstance().Destroy();

        if (gcodeRenderGameObject != null)
        {
            UnityEngine.Object.Destroy(gcodeRenderGameObject);
        }
    }

    public int GetLayerCount()
    {
        return GcodeParser.GetInstance().GetLayerCount();
    }

    public enum ColorScheme
    {
        Material,
        Line_Type
    }

    private ColorScheme _curColorScheme;

    private Dictionary<GcodeType, bool> _showDic_types = new Dictionary<GcodeType, bool>();

    Dictionary<GcodeType, Color32> _colorsDic_types = new Dictionary<GcodeType, Color32>();

    private Dictionary<GcodeType, bool> _showDic_topLayer = new Dictionary<GcodeType, bool>();

    Dictionary<GcodeType, Color32> _colorsDic_topLayer = new Dictionary<GcodeType, Color32>();

    public ColorScheme GetCurColorScheme()
    {
        return _curColorScheme;
    }

    public void SetColorScheme(ColorScheme colorScheme)
    {
        if (_curColorScheme == colorScheme)
        {
            return;
        }

        Debug.Log("SetColorScheme:" + _curColorScheme.ToString() + "--->" + colorScheme.ToString() + "\n");

        _curColorScheme = colorScheme;

        UpdatGcodePreviewColors();
    }

    //1
    public void SetActive_WALL_INNER(bool active)
    {
        _showDic_types[GcodeType.WALL_INNER] = active;
        UpdatGcodePreviewColors();
    }

    //2
    public void SetActive_WALL_OUTER(bool active)
    {
        _showDic_types[GcodeType.WALL_OUTER] = active;
        UpdatGcodePreviewColors();
    }

    //3
    public void SetActive_SKIN(bool active)
    {
        _showDic_types[GcodeType.SKIN] = active;
        UpdatGcodePreviewColors();
    }

    //4
    public void SetActive_SKIRT(bool active)
    {
        _showDic_types[GcodeType.SKIRT] = active;
        UpdatGcodePreviewColors();
    }

    //5
    public void SetActive_SUPPORT(bool active)
    {
        _showDic_types[GcodeType.SUPPORT] = active;
        UpdatGcodePreviewColors();
    }

    //6
    public void SetActive_FILL(bool active)
    {
        _showDic_types[GcodeType.FILL] = active;
        UpdatGcodePreviewColors();
    }

    //7
    public void SetActive_UNKNOWN(bool active)
    {
        _showDic_types[GcodeType.UNKNOWN] = active;
        UpdatGcodePreviewColors();
    }

    //8 not include in gcode : travel
    //7
    public void SetActive_Travel(bool active)
    {
        _showDic_types[GcodeType.Travel] = active;
        UpdatGcodePreviewColors();
    }

    private void UpdatGcodePreviewColors()
    {
        //1.if active==false ---> color_Transparent
        if (!_showDic_types[GcodeType.WALL_INNER])
        {
            _colorsDic_types[GcodeType.WALL_INNER] = GcodeTypeColor.Transparent;
        }
        if (!_showDic_types[GcodeType.WALL_OUTER])
        {
            _colorsDic_types[GcodeType.WALL_OUTER] = GcodeTypeColor.Transparent;
        }
        if (!_showDic_types[GcodeType.SKIN])
        {
            _colorsDic_types[GcodeType.SKIN] = GcodeTypeColor.Transparent;
        }
        if (!_showDic_types[GcodeType.SKIRT])
        {
            _colorsDic_types[GcodeType.SKIRT] = GcodeTypeColor.Transparent;
        }
        if (!_showDic_types[GcodeType.SUPPORT])
        {
            _colorsDic_types[GcodeType.SUPPORT] = GcodeTypeColor.Transparent;
        }
        if (!_showDic_types[GcodeType.FILL])
        {
            _colorsDic_types[GcodeType.FILL] = GcodeTypeColor.Transparent;
        }
        if (!_showDic_types[GcodeType.UNKNOWN])
        {
            _colorsDic_types[GcodeType.UNKNOWN] = GcodeTypeColor.Transparent;
        }
        if (!_showDic_types[GcodeType.Travel])
        {
            _colorsDic_types[GcodeType.Travel] = GcodeTypeColor.Transparent;
        }

        //2.
        if (_showDic_types[GcodeType.WALL_INNER])
        {
            if (_curColorScheme == ColorScheme.Line_Type)
            {
                _colorsDic_types[GcodeType.WALL_INNER] = GcodeTypeColor.WALL_INNER;
            }
            else if (_curColorScheme == ColorScheme.Material)
            {
                _colorsDic_types[GcodeType.WALL_INNER] = GcodeTypeColor.Material_Color;
            }
        }
        if (_showDic_types[GcodeType.WALL_OUTER])
        {
            if (_curColorScheme == ColorScheme.Line_Type)
            {
                _colorsDic_types[GcodeType.WALL_OUTER] = GcodeTypeColor.WALL_OUTER;
            }
            else if (_curColorScheme == ColorScheme.Material)
            {
                _colorsDic_types[GcodeType.WALL_OUTER] = GcodeTypeColor.Material_Color;
            }
        }
        if (_showDic_types[GcodeType.SKIN])
        {
            if (_curColorScheme == ColorScheme.Line_Type)
            {
                _colorsDic_types[GcodeType.SKIN] = GcodeTypeColor.SKIN;
            }
            else if (_curColorScheme == ColorScheme.Material)
            {
                _colorsDic_types[GcodeType.SKIN] = GcodeTypeColor.Material_Color;
            }
        }
        if (_showDic_types[GcodeType.SKIRT])
        {
            if (_curColorScheme == ColorScheme.Line_Type)
            {
                _colorsDic_types[GcodeType.SKIRT] = GcodeTypeColor.SKIRT;
            }
            else if (_curColorScheme == ColorScheme.Material)
            {
                _colorsDic_types[GcodeType.SKIRT] = GcodeTypeColor.Material_Color;
            }
        }
        if (_showDic_types[GcodeType.SUPPORT])
        {
            if (_curColorScheme == ColorScheme.Line_Type)
            {
                _colorsDic_types[GcodeType.SUPPORT] = GcodeTypeColor.SUPPORT;
            }
            else if (_curColorScheme == ColorScheme.Material)
            {
                _colorsDic_types[GcodeType.SUPPORT] = GcodeTypeColor.Material_Color;
            }
        }
        if (_showDic_types[GcodeType.FILL])
        {
            if (_curColorScheme == ColorScheme.Line_Type)
            {
                _colorsDic_types[GcodeType.FILL] = GcodeTypeColor.FILL;
            }
            else if (_curColorScheme == ColorScheme.Material)
            {
                _colorsDic_types[GcodeType.FILL] = GcodeTypeColor.Material_Color;
            }
        }
        if (_showDic_types[GcodeType.UNKNOWN])
        {
            if (_curColorScheme == ColorScheme.Line_Type)
            {
                _colorsDic_types[GcodeType.UNKNOWN] = GcodeTypeColor.UNKNOWN;
            }
            else if (_curColorScheme == ColorScheme.Material)
            {
                _colorsDic_types[GcodeType.UNKNOWN] = GcodeTypeColor.Material_Color;
            }
        }
        if (_showDic_types[GcodeType.Travel])
        {
            if (_curColorScheme == ColorScheme.Line_Type)
            {
                _colorsDic_types[GcodeType.Travel] = GcodeTypeColor.Travel;
            }
            else if (_curColorScheme == ColorScheme.Material)
            {
                _colorsDic_types[GcodeType.Travel] = GcodeTypeColor.Material_Color;
            }
        }

        GcodeDrawLineManager.GetInstance().SetColor_types(_colorsDic_types);
    }

    private void initColorScheme()
    {
        //1.color scheme
        _curColorScheme = ColorScheme.Line_Type;

        //2.types
        //show
        _showDic_types.Clear();
        _showDic_types.Add(GcodeType.WALL_INNER, true);
        _showDic_types.Add(GcodeType.WALL_OUTER, true);
        _showDic_types.Add(GcodeType.SKIN, true);
        _showDic_types.Add(GcodeType.SKIRT, true);
        _showDic_types.Add(GcodeType.SUPPORT, true);
        _showDic_types.Add(GcodeType.FILL, true);
        _showDic_types.Add(GcodeType.UNKNOWN, true);
        _showDic_types.Add(GcodeType.Travel, false);
        //color
        _colorsDic_types.Clear();
        _colorsDic_types.Add(GcodeType.WALL_INNER, GcodeTypeColor.WALL_INNER);
        _colorsDic_types.Add(GcodeType.WALL_OUTER, GcodeTypeColor.WALL_OUTER);
        _colorsDic_types.Add(GcodeType.SKIN, GcodeTypeColor.SKIN);
        _colorsDic_types.Add(GcodeType.SKIRT, GcodeTypeColor.SKIRT);
        _colorsDic_types.Add(GcodeType.SUPPORT, GcodeTypeColor.SUPPORT);
        _colorsDic_types.Add(GcodeType.FILL, GcodeTypeColor.FILL);
        _colorsDic_types.Add(GcodeType.UNKNOWN, GcodeTypeColor.UNKNOWN);
        _colorsDic_types.Add(GcodeType.Travel, GcodeTypeColor.Travel);

        //3.top color
        _showDic_topLayer.Clear();
        _showDic_topLayer.Add(GcodeType.Top_Layer, true);

        _colorsDic_topLayer.Clear();
        _colorsDic_topLayer.Add(GcodeType.Top_Layer, GcodeTypeColor.Top_Layer);
    }

    public bool IsGcodeTypeShow(GcodeType type)
    {
        if (_showDic_types.ContainsKey(type))
        {
            return _showDic_types[type];
        }
        else
        {
            return false;
        }
    }

    public void ShowFirstLayers(int count)
    {
        GcodeDrawLineManager.GetInstance().SetVisiableLayerCount(count);
    }

    public void SetActive_Top_Layer(bool active)
    {
        _showDic_topLayer[GcodeType.Top_Layer] = active;
        if (active)
        {
            _colorsDic_topLayer[GcodeType.Top_Layer] = GcodeTypeColor.Top_Layer;
        }
        else
        {
            _colorsDic_topLayer[GcodeType.Top_Layer] = GcodeTypeColor.Transparent;
        }

        GcodeDrawLineManager.GetInstance().SetActive_topLayer(active);
        GcodeDrawLineManager.GetInstance().SetColor_topLayer(_colorsDic_topLayer[GcodeType.Top_Layer]);
    }

    public bool GetActive_Top_Layer()
    {
        if (_showDic_topLayer.ContainsKey(GcodeType.Top_Layer))
        {
            return _showDic_topLayer[GcodeType.Top_Layer];
        }
        else
        {
            return false;
        }
    }

    public bool IsInBounds()
    {
        List<Vector2> list = GcodeParser.GetInstance().GetBounds();
        if(list == null){
            return false;
        }
        Vector3 deviceSize = Global.GetInstance().GetPrinterParamsStruct().size;
        return
            (list[0].x >= 0 && list[0].y <= deviceSize.x )
            &&
            (list[1].x >= 0 && list[1].y <= deviceSize.y )
            &&
            (list[2].y <= deviceSize.z );
    }

    public void ShowLayerLessThanZ(float z)
    {
        GcodeDrawLineManager.GetInstance().SetVisiableLayerLessThanZ(z);
    }
}
