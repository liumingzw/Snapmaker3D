using System.Collections.Generic;
using UnityEngine;

public class RenderedModel
{
	// Limit of Unity
	private const int VECTORS_LIMIT = 65535 - 1;

	public GameObject shellCube;
	public Vector3 shellCubeOriginLocalScale;

	public RenderedModel (DataModel dataModel)
	{
		//1.create shellCube  
		Vector3 cubeSize = new Vector3 (dataModel.GetSize ().x, dataModel.GetSize ().z, dataModel.GetSize ().y);
		shellCube = createCube (cubeSize);
		shellCube.GetComponent<Renderer> ().enabled = true;
		shellCube.name = "shellCube";
		UnityEngine.Object.Destroy (shellCube.GetComponent<BoxCollider> ());

		//2.get mesh data
		List<Mesh> meshList = new List<Mesh> ();
		List<Vector3> vectorsBuffer = new List<Vector3> ();//3d point list
		List<int> triangleList = new List<int> ();
		int triangleIndex = 0;

		foreach(Facet facet in dataModel.GetFacetList ()){
			// y <-- switch ---> z
			//Attention!!! RenderModel point Y&Z is switched from DataModel
			Vector3 v1 = new Vector3 (facet.p1.x, facet.p1.z, facet.p1.y);
			Vector3 v2 = new Vector3 (facet.p2.x, facet.p2.z, facet.p2.y);
			Vector3 v3 = new Vector3 (facet.p3.x, facet.p3.z, facet.p3.y);

			vectorsBuffer.Add (v3);
			vectorsBuffer.Add (v2);
			vectorsBuffer.Add (v1);
			triangleList.Add (triangleIndex++);
			triangleList.Add (triangleIndex++);
			triangleList.Add (triangleIndex++);

			if (vectorsBuffer.Count > VECTORS_LIMIT) {

				Mesh mesh = new Mesh ();
				mesh.vertices = vectorsBuffer.ToArray ();
				mesh.triangles = triangleList.ToArray ();
				mesh.RecalculateNormals ();
				mesh.RecalculateBounds ();
				meshList.Add (mesh);

				vectorsBuffer.Clear ();
				triangleList.Clear ();
				triangleIndex = 0;
			}
		}

		if (vectorsBuffer.Count > 0) {

			Mesh mesh = new Mesh ();
			mesh.vertices = vectorsBuffer.ToArray ();
			mesh.triangles = triangleList.ToArray ();
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
			meshList.Add (mesh);

			triangleList.Clear ();
			triangleIndex = 0;
			vectorsBuffer.Clear ();
		}

		//3.create child and add to shellCube
		for (int i = 0; i < meshList.Count; i++) {
			
			GameObject child = GameObject.CreatePrimitive (PrimitiveType.Cube);
			child.name = "child" + i;

			//3.1 add meshFilter for every child
			MeshFilter filter = child.GetComponent<MeshFilter> ();
			filter.mesh = meshList [i];

			//3.2 provision for uv warning
			Vector3[] vertices = filter.mesh.vertices;
			Vector2[] uvs = new Vector2[vertices.Length];
			int j = 0;
			while (j < uvs.Length) {
				uvs [j] = new Vector2 (vertices [j].x, vertices [j].z);
				j++;
			}
			filter.mesh.uv = uvs;

			//3.3 add MeshCollider
			UnityEngine.Object.Destroy (child.GetComponent<BoxCollider> ());
			child.AddComponent<MeshCollider> ();

			//3.4 add to shellCube
			child.transform.parent = shellCube.transform;
			child.transform.localPosition = new Vector3 (0, 0, 0);

			//			Material material = new Material (Shader.Find ("Transparent/Diffuse"));
			//			Color color = new Color(247/255.0f, 194/255.0f, 21/255.0f, 1.0f);
			//			material.SetColor ("_Color", color);
			//			MeshRenderer renderer = child.GetComponent<MeshRenderer> ();
			//			renderer.sharedMaterial = material;
		}
	}

	public void ScaleTo (float param)
	{
		shellCube.transform.localScale = shellCubeOriginLocalScale * param;
	}

	private GameObject createCube (Vector3 meshBoundSize)
	{
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cube);
		Mesh mesh = new Mesh ();
		float x = meshBoundSize.x / 2;
		float y = meshBoundSize.y / 2;
		float z = meshBoundSize.z / 2;
		mesh.vertices = new Vector3[] {   
			new Vector3 (-x, -y, -z),  
			new Vector3 (x, -y, -z),  
			new Vector3 (x, y, -z),  
			new Vector3 (-x, y, -z),  

			new Vector3 (-x, y, z),  
			new Vector3 (x, y, z),  
			new Vector3 (x, -y, z),  
			new Vector3 (-x, -y, z),
		};  

		//anticlockwise 
		mesh.triangles = new int[] {  
			3, 1, 0,  
			3, 2, 1,  
			2, 3, 4,  
			2, 4, 5,  
			7, 5, 4,  
			7, 6, 5,  
			0, 6, 7,  
			0, 1, 6,  
			3, 7, 4,  
			3, 0, 7,  
			1, 5, 6,  
			1, 2, 5  
		};  
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();

		go.GetComponent<MeshFilter> ().mesh = mesh;

		UnityEngine.Object.Destroy(go.GetComponent<MeshRenderer> ());

		return go;
	}

}
