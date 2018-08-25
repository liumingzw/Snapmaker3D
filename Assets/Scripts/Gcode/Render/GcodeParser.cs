using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Diagnostics;

public class GcodeParser
{
    /*************** TYPE ***************/
    private const string keyWord_TYPE = ";TYPE:";

    private GcodeType _curType = GcodeType.UNKNOWN;

    /*************** LAYER ***************/
    private const string keyWord_Layer_height = ";Layer height:";

    private const string keyWord_LAYER = ";LAYER:";

    private float _Layer_height = 0.2f;

    private int _layerCount = 0;

    private int _curLayerIndex = int.MinValue;

    /*************** properity ***************/
    private static char[] _buffer_parseG0G1 = new char[15];
    private static int _endIndex_parseG0G1 = 0;
    private static char _type_parseG0G1 = ' ';

    private Unit _curUnit;
    private Coordinate _curCoordinate;
    private Vector3 _curPosition;

    private float _parseProgress;

    private List<GcodeRenderBean> _gcodeRenderPointList = new List<GcodeRenderBean>();

    /*************** bounds ***************/
    private Vector2 _bounds_X, _bounds_Y, _bounds_Z;

    /*************** single ***************/
    private static GcodeParser _INSTANCE;

    private GcodeParser()
    {
    }

    public static GcodeParser GetInstance()
    {
        if (_INSTANCE == null)
        {
            _INSTANCE = new GcodeParser();
        }
        return _INSTANCE;
    }

    /******************************/
    public enum Coordinate
    {
        Absolute,
        Relative
    }

    public enum Unit
    {
        Inch,
        Millimeter
    }

    /*************** public method ***************/
    //parse gcode file and simple layer list
    //if start succeed, return true; else return false
    public void StartParseGcodeFile(string path)
    {
        //1.reset
        _Layer_height = 0.2f;
        _layerCount = 0;
        _curType = GcodeType.UNKNOWN;
        _curLayerIndex = int.MinValue;

        _curUnit = Unit.Millimeter;
        _curCoordinate = Coordinate.Absolute;
        _curPosition = Vector3.zero;

        _parseProgress = 0;

        _gcodeRenderPointList.Clear();

        _bounds_X = Vector2.zero;
        _bounds_Y = Vector2.zero;
        _bounds_Z = Vector2.zero;

        //2.parse
        string[] lineArray = File.ReadAllLines(path);

        Stopwatch sw = new Stopwatch();
        sw.Start();

        for (int i = 0; i < lineArray.Length; i++)
        {
            _parseProgress = ((float)i) / lineArray.Length;

            //";End GCode begin" means end of parsing gcode
            if (lineArray[i].Trim().StartsWith(";End GCode begin"))
            {
                break;
            }

            //parseLine: parse every line to layer, then add layers to layerList_origin
            parseLine(lineArray[i]);
        }

        _parseProgress = 1.0f;

        UnityEngine.Debug.Log("****************** Gcode File Parse Result ******************" + "\n");

        sw.Stop();
        UnityEngine.Debug.Log(string.Format("Parse Gcode File Cost : {0} ms" + "\n", sw.ElapsedMilliseconds));

        UnityEngine.Debug.Log("Gcode Render Point Count : " + _gcodeRenderPointList.Count + "\n");

        UnityEngine.Debug.Log("Layer Height : " + _Layer_height + "\n");

        UnityEngine.Debug.Log("Layer Count : " + _layerCount + "\n");

        int count = _gcodeRenderPointList.Count;

        UnityEngine.Debug.Log("Gcode Render Point count : " + count + "\n");

        if (count > 0)
        {
            UnityEngine.Debug.Log("Layer index min : " + _gcodeRenderPointList[0].layerIndex + "\n");

            UnityEngine.Debug.Log("Layer index max : " + _gcodeRenderPointList[_gcodeRenderPointList.Count - 1].layerIndex + "\n");
        }

        //		for (int i = 0; i < gcodePointList.Count; i++) {
        //			GcodePoint point = gcodePointList [i];
        //			UnityEngine.Debug.LogError (i+" " +point + "\n");
        //		}

        UnityEngine.Debug.Log("***************************************************" + "\n");
    }

    public float GetParseProgress()
    {
        return _parseProgress;
    }

    /*************** private method ***************/
    private void parseLine(string line)
    {
        if (line.Trim().StartsWith(keyWord_TYPE))
        {

            string typeStr = line.Substring(keyWord_TYPE.Length);
            if (typeStr == "WALL-INNER")
            {
                _curType = GcodeType.WALL_INNER;
            }
            else if (typeStr == "WALL-OUTER")
            {
                _curType = GcodeType.WALL_OUTER;
            }
            else if (typeStr == "SKIN")
            {
                _curType = GcodeType.SKIN;
            }
            else if (typeStr == "SKIRT")
            {
                _curType = GcodeType.SKIRT;
            }
            else if (typeStr == "SUPPORT")
            {
                _curType = GcodeType.SUPPORT;
            }
            else if (typeStr == "FILL")
            {
                _curType = GcodeType.FILL;
            }
            else
            {
                _curType = GcodeType.UNKNOWN;
                UnityEngine.Debug.LogWarning("Unknown Gcode type" + "\n");
            }
            return;
        }
        else if (line.Trim().StartsWith(keyWord_LAYER))
        {
            ++_layerCount;
            _curLayerIndex = int.Parse(line.Substring(keyWord_LAYER.Length));
            return;
        }
        else if (line.Trim().StartsWith(keyWord_Layer_height))
        {
            _Layer_height = float.Parse(line.Substring(keyWord_Layer_height.Length));
            return;
        }

        //Remove comments (if any)
        char[] charArray = line.Split(';')[0].Trim().ToCharArray();
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
            else if (charArray[1] == '2' && charArray[2] == '0')
            {
                //G20
                //unit is inch 
                _curUnit = Unit.Inch;
            }
            else if (charArray[1] == '2' && charArray[2] == '1')
            {
                //G21
                //unit is millimeter 
                _curUnit = Unit.Millimeter;
            }
            else if (charArray[1] == '2' && charArray[2] == '8')
            {
                //G28
                //Auto home
                _curPosition = Vector3.zero;
            }
            else if (charArray[1] == '9' && charArray[2] == '0')
            {
                //G90
                _curCoordinate = Coordinate.Absolute;
            }
            else if (charArray[1] == '9' && charArray[2] == '1')
            {
                //G91
                _curCoordinate = Coordinate.Relative;
            }
            else if (charArray[1] == '9' && charArray[2] == '2')
            {
                //G92
                //todo G92: set coordinate zero point 
            }
        }
    }

    private void parseG0G1(char[] charArray)
    {
        //data is useless until _curLayerIndex != int.MinValue
        if (_curLayerIndex == int.MinValue)
        {
            return;
        }

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

        if (tempPos != _curPosition)
        {
            GcodeRenderBean bean = new GcodeRenderBean(_curPosition, valueE, _curType, _curLayerIndex);
            _gcodeRenderPointList.Add(bean);
        }
    }

    public float GetLayerHeight()
    {
        return _Layer_height;
    }

    public List<GcodeRenderBean> GetGcodeRenderPointList()
    {
        return _gcodeRenderPointList;
    }

    public int GetLayerCount()
    {
        return _layerCount;
    }

    public List<Vector2> GetBounds()
    {
        if (_gcodeRenderPointList.Count == 0)
        {
            return null;
        }

        if (_bounds_X == Vector2.zero && _bounds_Y == Vector2.zero && _bounds_Z == Vector2.zero)
        {
            float min = float.MinValue;
            float max = float.MaxValue;

            float maxX = min;
            float minX = max;

            float maxY = min;
            float minY = max;

            float maxZ = min;
            float minZ = max;

            foreach (GcodeRenderBean bean in _gcodeRenderPointList)
            {
                maxX = Mathf.Max(bean.vector3.x, maxX);
                minX = Mathf.Min(bean.vector3.x, minX);

                maxY = Mathf.Max(bean.vector3.y, maxY);
                minY = Mathf.Min(bean.vector3.y, minY);

                maxZ = Mathf.Max(bean.vector3.z, maxZ);
                minZ = Mathf.Min(bean.vector3.z, minZ);
            }
            _bounds_X = new Vector2(minX, maxX);
            _bounds_Y = new Vector2(minY, maxY);
            _bounds_Z = new Vector2(minZ, maxZ);

            //Vector3 size = new Vector3((maxX - minX), (maxY - minY), (maxZ - minZ));
            //UnityEngine.Debug.LogError("size:" + size.x + "/" + size.y + "/" + size.z + "\n");
        }

        List<Vector2> result = new List<Vector2>(3);
        result.Add(_bounds_X);
        result.Add(_bounds_Y);
        result.Add(_bounds_Z);

        return result;
    }
}
