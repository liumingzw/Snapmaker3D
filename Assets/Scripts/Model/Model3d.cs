using UnityEngine;
using System.Diagnostics;

public class Model3d
{
	/*************** 1.RenderModel ***************/
	public RenderedModel renderedModel;

	/*************** 2.DataModel ***************/
	public DataModel dataModel_origin;
	public DataModel dataModel_cur;

	/*************** constructor ***************/
	private  Model3d ()
	{
	}

	public Model3d (DataModel dataModel)
	{
		dataModel_origin = dataModel;

		dataModel_cur = dataModel_origin.DeepCopy ();

		renderedModel = new RenderedModel (dataModel_origin);
	}

	/*************** 1.RenderModel ***************/
	public void SetRenderLocalPosition (Vector3 vector3)
	{
		renderedModel.shellCube.transform.localPosition = vector3;
	}

	public Vector3 GetRenderLocalPosition ()
	{
		return renderedModel.shellCube.transform.localPosition;
	}

	public void SetRenderLocalEulerAngles (Vector3 vector3)
	{
		renderedModel.shellCube.transform.localEulerAngles = vector3;
	}

	public Vector3 GetRenderLocalEulerAngles ()
	{
		return renderedModel.shellCube.transform.localEulerAngles;
	}

	public void ScaleRenderTo (float value)
	{
		renderedModel.ScaleTo (value);
	}

	public void SetRenderMaterial (Material material)
	{
		Transform[] children = this.renderedModel.shellCube.GetComponentsInChildren<Transform> ();
		for (int i = 0; i < children.Length; i++) {
			GameObject child = children [i].gameObject;
			if (child.name.StartsWith ("child")) {
				child.GetComponent<Renderer> ().material = material;
			}
		}
	}

	/*************** 2.DataModel ***************/
	//todo:UpdateDataModel for save and for operate render model
	public void UpdateCurDataModel (float scale, float moveX, float moveY, float rotateX, float rotateY, float rotateZ)
	{
		dataModel_cur = dataModel_origin.DeepCopy ();
		dataModel_cur.OperateModel (scale, moveX, -moveY, rotateX, rotateY, rotateZ);
		dataModel_cur.CalculateSize ();
	}

	public void SaveAsStlFile (string path)
	{
		Stopwatch sw = new Stopwatch ();
		sw.Start ();

		dataModel_cur.SaveAsBinary (path);

		UnityEngine.Debug.Log ("Save stl to:" + path);

		sw.Stop ();
		UnityEngine.Debug.Log (string.Format ("Save stl cost: {0} ms", sw.ElapsedMilliseconds));
	}

	public Vector3 GetCurDataSize ()
	{
		return dataModel_cur.GetSize ();
	}

	public Vector3 GetOriginDataSize ()
	{
		return dataModel_origin.GetSize ();
	}
}
