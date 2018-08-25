using System.IO;
using System.Collections.Generic;
using System;

public class DataModelObj : DataModel
{
    //all points
    private List<Point3d> _pointList = new List<Point3d>();

    //index list of a section, and .OBJ files may have 1 or more sections
    private List<int> _indexSectionList = new List<int>();

    private System.Text.RegularExpressions.Regex _regexBlank = new System.Text.RegularExpressions.Regex("[\\s]+");
    private System.Text.RegularExpressions.Regex _regexSlash = new System.Text.RegularExpressions.Regex("[\\/]+");

    public DataModelObj()
    {
    }

    /**
     * file format : https://en.wikipedia.org/wiki/Wavefront_.obj_file
     * 
     * general case : vvvvv.....fffff
     * exceptional case1 : vvvvv.....fffff.....vvvvv.....fffff......
     * exceptional case2 : v vn vt v vn vt.....fffff.....v vn vt v vn vt.....fffff......
     * 
     * But 'v' must be ahead of 'f'
     * 
     * face format : [f index1 index2 index3 ....]
     * PS : Each face can contain three or more vertices.
     * And index can be positive or negative. 
     * 
     * code1 : Cura do https://github.com/Ultimaker/Uranium/blob/master/plugins/FileHandlers/OBJReader/OBJReader.py#L52
     * 
     * todo : process concave polygon
     * todo : "\" appeare at the end of a line http://blog.csdn.net/hahajinbu/article/details/49833851#reply
     */
    public override void ParseFacetListFromFile(string path)
    {
        float progress = 0;

        base.facetList.Clear();

        _pointList.Clear();

        _indexSectionList.Clear();

        string[] allLines = File.ReadAllLines(path);

        int lineCount = allLines.Length;

        try
        {
            for (int i = 0; i < lineCount; i++)
            {
                //tell listener
                if (_listener != null)
                {
                    if ((float)i / lineCount - progress >= 0.01f)
                    {
                        progress = (float)i / lineCount;
                        _listener.OnDataModel_ParseProgress(progress);
                    }
                }

                string line = allLines[i].Trim().ToLower();
                //at least: v 1 2 3, so Length >= 7
                //just focuse "v v1 v2 v3" and "f index1 index2 index3 ...." 
                //and not process 'vn' or 'vt'
                if (!string.IsNullOrEmpty(line) && line.Length >= 7 && line[1] == ' ' && (line[0] == 'v' || line[0] == 'f'))
                {
                    switch (line[0])
                    {
                        case 'v':
                            {
                                if (_indexSectionList.Count > 0)
                                {
                                    processDataSection(_pointList, _indexSectionList);
                                }

                                /** 
                                 * List of geometric vertices, with (x,y,z[,w]) coordinates, w is optional and defaults to 1.0.
                                 * example :
                                 * v 0.123 0.234 0.345 1.0
                                 */
                                //replace blanks by one blank, and split by one blank to array
                                string[] dataArray_V = _regexBlank.Replace(line, " ").Split(' ');

                                if (dataArray_V.Length >= 4)
                                {
                                    Point3d point = new Point3d(float.Parse(dataArray_V[1]), float.Parse(dataArray_V[2]), float.Parse(dataArray_V[3]));
                                    _pointList.Add(point);
                                }
                            }
                            break;
                        case 'f':
                            {
                                /** 
                                 * example of 3 vertices :
                                 * f 1 2 3
                                 * f 3/1 4/2 5/3
                                 * f 6/4/1 3/5/3 7/6/5
                                 * f 7//1 8//2 9//3
                                 * f ...
                                 * 
                                 * example of more than vertices :
                                 * f 1 2 3 4
                                 * f 3/1 4/2 5/3 4/3
                                 * f 6/4/1 3/5/3 7/6/5 5/6/5
                                 * f 7//1 8//2 9//3 8//3
                                 * f ...
                                 */
                                //replace blanks by one blank, and split by one blank to array
                                string[] dataArray_F = _regexBlank.Replace(line, " ").Split(' ');

                                //@todo: process concave polygon

                                /**
                                 * process as convex polygon
                                 * Transform convex polygon into several triangles
                                 * example : 
                                 * polygon -----> triangles
                                 * f 1 2 3 4 5 -----> [1,2,3] ; [1,3,4] ; [1,4,5]
                                 */
                                //a triangle : index1,indexA,indexB
                                if (dataArray_F.Length >= 4)
                                {
                                    string item1 = dataArray_F[1];
                                    //replace many '/' by one blank, and split by one blank to array
                                    string[] itemArray1 = _regexSlash.Replace(item1, " ").Split(' ');
                                    //the first integer
                                    int index1 = int.Parse(itemArray1[0]);

                                    for (int k = 2; k < dataArray_F.Length - 1;)
                                    {
                                        _indexSectionList.Add(index1);

                                        {
                                            string item_A = dataArray_F[k];
                                            string[] itemArray_A = _regexSlash.Replace(item_A, " ").Split(' ');
                                            int indexA = int.Parse(itemArray_A[0]);
                                            _indexSectionList.Add(indexA);
                                        }

                                        {
                                            ++k;
                                            string item_B = dataArray_F[k];
                                            string[] itemArray_B = _regexSlash.Replace(item_B, " ").Split(' ');
                                            int indexB = int.Parse(itemArray_B[0]);
                                            _indexSectionList.Add(indexB);
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            processDataSection(_pointList, _indexSectionList);
        }
        catch (Exception e)
        {
            base.facetList.Clear();
            UnityEngine.Debug.LogError("Error occur: parse obj file : " + e.ToString() + "\n");
        }
        finally
        {
            if (_listener != null)
            {
                if (base.facetList.Count == 0)
                {
                    progress = 1.0f;
                    _listener.OnDataModel_ParseProgress(progress);
                    _listener.OnDataModel_ParseFailed();
                }
                else
                {
                    progress = 1.0f;
                    _listener.OnDataModel_ParseProgress(progress);
                    _listener.OnDataModel_ParseComplete(this);
                }
            }
        }
    }

    private void processDataSection(List<Point3d> pointList, List<int> indexSectionList)
    {
        int pointCount = pointList.Count;

        for (int i = 0; i < indexSectionList.Count; i++)
        {
            /*
             * If an index is positive then it refers to the offset in that vertex list, starting at 1.
             * If an index is negative then it relatively refers to the end of the vertex list, 
             * -1 referring to the last element.
             */
            int index1 = indexSectionList[i];
            int index2 = indexSectionList[++i];
            int index3 = indexSectionList[++i];

            index1 = index1 > 0 ? (--index1) : (pointCount + index1);
            index2 = index2 > 0 ? (--index2) : (pointCount + index2);
            index3 = index3 > 0 ? (--index3) : (pointCount + index3);

            //UnityEngine.Debug.Log("index:" + index1 + " / " + index2 + " / " + index3 + "\n");

            Point3d p1 = pointList[index1];
            Point3d p2 = pointList[index2];
            Point3d p3 = pointList[index3];

            Facet facetTemp = new Facet(
                                  new Point3d(p1.x, p1.y, p1.z),
                                  new Point3d(p2.x, p2.y, p2.z),
                                  new Point3d(p3.x, p3.y, p3.z)
                              );
            base.facetList.Add(facetTemp);
        }

        //Attention 1: do not execute "pointList.Clear()", reason: section_2 may use the points in section_1
        //Attention 2: must execute "indexSectionList.Clear()", reason:indexSectionList has been processed
        indexSectionList.Clear();
    }
}
