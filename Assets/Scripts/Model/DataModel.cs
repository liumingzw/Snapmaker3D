using System;
using System.IO;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;

public abstract class DataModel
{
	protected Vector3 size;
	protected List<Facet> facetList;
	private float _minZ;

	/*************** listener ***************/
	public interface Listener
	{
		void OnDataModel_ParseFailed () ;

		void OnDataModel_ParseProgress (float progress) ;

		void OnDataModel_ParseComplete (DataModel dataModel) ;
	}

	protected Listener _listener;

	public void SetListener (Listener listener)
	{
		_listener = listener;
	}

	/*************** Constructor ***************/
	public DataModel ()
	{
		this.size = new Vector3 (0, 0, 0);
		this.facetList = new List<Facet> ();
	}

	public List<Facet> GetFacetList ()
	{
		return this.facetList;
	}

	//parse file to get facetList
	public abstract void ParseFacetListFromFile (string path);

	public Vector3 GetSize ()
	{
		return size;
	}

	public void SetSize (Vector3 size)
	{
		this.size = size;
	}

	public void CalculateSize ()
	{
		float min = float.MinValue;
		float max = float.MaxValue;
		float maxX = min;
		float minX = max;
		float maxY = min;
		float minY = max;
		float maxZ = min;
		float minZ = max;

		foreach (Facet facet in facetList) {

			maxX = Mathf.Max (facet.p1.x, facet.p2.x, facet.p3.x, maxX);
			minX = Mathf.Min (facet.p1.x, facet.p2.x, facet.p3.x, minX);

			maxY = Mathf.Max (facet.p1.y, facet.p2.y, facet.p3.y, maxY);
			minY = Mathf.Min (facet.p1.y, facet.p2.y, facet.p3.y, minY);

			maxZ = Mathf.Max (facet.p1.z, facet.p2.z, facet.p3.z, maxZ);
			minZ = Mathf.Min (facet.p1.z, facet.p2.z, facet.p3.z, minZ);
		}

		_minZ = minZ;

		this.size = new Vector3 ((maxX - minX), (maxY - minY), (maxZ - minZ));

		UnityEngine.Debug.Log ("****************** CalculateSize ******************" + "\n");
		string info = "X:" + "[" + minX + ", " + maxX + "] " +
		              "Y:" + "[" + minY + ", " + maxY + "] " +
		              "Z:" + "[" + minZ + ", " + maxZ + "]";
		UnityEngine.Debug.Log (info + "\n");
		UnityEngine.Debug.Log ("Size:" + this.size + "\n");
		UnityEngine.Debug.Log ("***************************************************" + "\n");
	}

	private void setToSymmetry ()
	{
		float min = float.MinValue;
		float max = float.MaxValue;
		float maxX = min;
		float minX = max;
		float maxY = min;
		float minY = max;
		float maxZ = min;
		float minZ = max;

		foreach (Facet facet in facetList) {

			maxX = Mathf.Max (facet.p1.x, facet.p2.x, facet.p3.x, maxX);
			minX = Mathf.Min (facet.p1.x, facet.p2.x, facet.p3.x, minX);

			maxY = Mathf.Max (facet.p1.y, facet.p2.y, facet.p3.y, maxY);
			minY = Mathf.Min (facet.p1.y, facet.p2.y, facet.p3.y, minY);

			maxZ = Mathf.Max (facet.p1.z, facet.p2.z, facet.p3.z, maxZ);
			minZ = Mathf.Min (facet.p1.z, facet.p2.z, facet.p3.z, minZ);
		}

		//Do it for before render
		float moveX = -(maxX + minX) / 2.0f;
		float moveY = -(maxY + minY) / 2.0f;
		float moveZ = -(maxZ + minZ) / 2.0f;

		foreach (Facet facet in facetList) {
			facet.p1.x += moveX;
			facet.p1.y += moveY;
			facet.p1.z += moveZ;

			facet.p2.x += moveX;
			facet.p2.y += moveY;
			facet.p2.z += moveZ;

			facet.p3.x += moveX;
			facet.p3.y += moveY;
			facet.p3.z += moveZ;
		}

		_minZ = -this.size.z / 2;
	}



	public DataModel DeepCopy ()
	{
		DataModel deepCopy = new DataModelStl ();

		List<Facet> facetsDeepCopy = new List<Facet> ();
		foreach (Facet facet in facetList) {
			facetsDeepCopy.Add (facet.DeepCopy ());
		}
		deepCopy.facetList = facetsDeepCopy;

		deepCopy.size = this.size;
		deepCopy._minZ = this._minZ;
		return deepCopy;
	}

	/*************** matrix ***************/
	private Matrix<double> getMatrix_rotate (float x, float y, float z)
	{
		x = -Mathf.Deg2Rad * x;
		y = -Mathf.Deg2Rad * y;
		z = -Mathf.Deg2Rad * z;
		Matrix<double> rotateY = DenseMatrix.OfArray (new double[,] {
			{ Math.Cos (y), 0, Math.Sin (y) },
			{ 0, 1, 0 },
			{ -Math.Sin (y), 0, Math.Cos (y) }
		});

		Matrix<double> rotateX = DenseMatrix.OfArray (new double[,] {
			{ 1, 0, 0 },
			{ 0, Math.Cos (x), -Math.Sin (x) },
			{ 0, Math.Sin (x), Math.Cos (x) }
		});

		Matrix<double> rotateZ = DenseMatrix.OfArray (new double[,] {
			{ Math.Cos (z), -Math.Sin (z), 0 },
			{ Math.Sin (z), Math.Cos (z), 0 },
			{ 0, 0, 1 }
		});

		return rotateY * rotateX * rotateZ;
	}

	private Matrix<double> getMatrix_scale (float percent)
	{
		Matrix<double> matrix = DenseMatrix.OfArray (new double[,] {
			{ percent, 0, 0 },
			{ 0, percent, 0 },
			{ 0, 0, percent }
		});
		return matrix;
	}

	private Matrix<double> getMatrix_move (float x, float y)
	{
		Matrix<double> matrix = DenseMatrix.OfArray (new double[,] {
			{ x, y, 0 },
			{ x, y, 0 },
			{ x, y, 0 }
		});
		return matrix;
	}

	/*************** operate model ***************/
	public void OperateModel (float scale, float moveX, float moveY, float rotateX, float rotateY, float rotateZ)
	{
		string info = "operate:"
		              + " scale=" + scale
		              + " moveX=" + moveX
		              + " moveY=" + moveY
		              + " rotateX=" + rotateX
		              + " rotateY=" + rotateY
		              + " rotateZ=" + rotateZ;
		UnityEngine.Debug.Log (info + "\n");

		Matrix<double> matrixMove = getMatrix_move (moveX, moveY);
		Matrix<double> matrixScale = getMatrix_scale (scale);
		Matrix<double> matrixRotate = getMatrix_rotate (rotateX, rotateY, rotateZ);

		foreach (Facet facet in facetList) {

			Matrix<double> f = DenseMatrix.OfArray (new double[,] {
				{ facet.p1.x, facet.p1.y, facet.p1.z },
				{ facet.p2.x, facet.p2.y, facet.p2.z },
				{ facet.p3.x, facet.p3.y, facet.p3.z }
			});

			Matrix<double> result = f * matrixScale * matrixRotate + matrixMove;

			facet.p1.x = (float)result.At (0, 0);
			facet.p1.y = (float)result.At (0, 1);
			facet.p1.z = (float)result.At (0, 2);

			facet.p2.x = (float)result.At (1, 0);
			facet.p2.y = (float)result.At (1, 1);
			facet.p2.z = (float)result.At (1, 2);

			facet.p3.x = (float)result.At (2, 0);
			facet.p3.y = (float)result.At (2, 1);
			facet.p3.z = (float)result.At (2, 2);
		}
	}

	/*************** save ***************/
	public void SaveAsBinary (string path)
	{
		//operations would change max&min, so need to recalculate max&min, then reset z 
		CalculateSize ();
		setZToZero ();

		UnityEngine.Debug.Log ("SaveAsBinary:" + path + "\n");

		FileStream fs = new FileStream (path, FileMode.Create, FileAccess.ReadWrite);
		BinaryWriter bw = new BinaryWriter (fs);
		byte[] _header_binary = new byte[80];

		bw.Write (_header_binary);

		bw.Write ((uint)facetList.Count);
		foreach (Facet facet in facetList) {
			bw.Write (facet.normal.x);
			bw.Write (facet.normal.y);
			bw.Write (facet.normal.z);

			bw.Write (facet.p1.x);
			bw.Write (facet.p1.y);
			bw.Write (facet.p1.z);

			bw.Write (facet.p2.x);
			bw.Write (facet.p2.y);
			bw.Write (facet.p2.z);

			bw.Write (facet.p3.x);
			bw.Write (facet.p3.y);
			bw.Write (facet.p3.z);

			bw.Write (facet.property);
		}
		bw.Flush ();
		bw.Close ();

        fs.Close();
	}

	private void setZToZero ()
	{
		UnityEngine.Debug.Log ("setZToZero" + "\n");
		foreach (Facet facet in facetList) {
			facet.p1.z -= _minZ;
			facet.p2.z -= _minZ;
			facet.p3.z -= _minZ;
		}
	}

	public float GetMinZ ()
	{
		return _minZ;
	}

	public void SetMinZ (float minZ)
	{
		this._minZ = minZ;
	}

	/*************** Pre Treated For Render ***************/
	public void PreTreatedForRender ()
	{
		CalculateSize ();
//		if (autoScale ()) {
//			CalculateSize ();
//		}
		setToSymmetry ();
	}

	private bool autoScale ()
	{
		bool needSacle = false;
		float max = Global.GetInstance ().GetPrinterParamsStruct ().size.x;
		float min = 10.0f;
		if (Mathf.Max (this.size.x, this.size.y, this.size.z) > max) {
			needSacle = true;
			OperateModel (max / Mathf.Max (this.size.x, this.size.y, this.size.z), 0, 0, 0, 0, 0);
		} else if (Mathf.Max (this.size.x, this.size.y, this.size.z) < min) {
			needSacle = true;
			OperateModel (min / Mathf.Max (this.size.x, this.size.y, this.size.z), 0, 0, 0, 0, 0);
		}
		if (needSacle) {
			UnityEngine.Debug.Log ("Need auto scale" + "\n");
		} else {
			UnityEngine.Debug.Log ("No need auto scale" + "\n");
		}
		return needSacle;
	}

	public bool NeedScale(){
		bool needSacle = false;
		float max = Global.GetInstance ().GetPrinterParamsStruct ().size.x;
		float min = 10.0f;
		if (Mathf.Max (this.size.x, this.size.y, this.size.z) > max) {
			needSacle = true;
		} else if (Mathf.Max (this.size.x, this.size.y, this.size.z) < min) {
			needSacle = true;
		}
		if (needSacle) {
			UnityEngine.Debug.Log ("Need scale" + "\n");
		} else {
			UnityEngine.Debug.Log ("No need scale" + "\n");
		}
		return needSacle;
	}

}
