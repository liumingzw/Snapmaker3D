using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderModelTrigger : MonoBehaviour
{

	void OnTriggerEnter (Collider e)
	{
		GameObject shellCube = this.gameObject.transform.parent.gameObject;
		Transform[] children = shellCube.GetComponentsInChildren<Transform> ();
		for (int i = 0; i < children.Length; i++) {
			GameObject child = children [i].gameObject;
			if (child.name.StartsWith ("child")) {
				child.GetComponent<Renderer> ().material = Resources.Load<Material> ("StlUnavaiableMaterial");
			}
		}
	}

	void OnTriggerExit (Collider e)
	{
		GameObject shellCube = this.gameObject.transform.parent.gameObject;
		Transform[] children = shellCube.GetComponentsInChildren<Transform> ();
		for (int i = 0; i < children.Length; i++) {
			GameObject child = children [i].gameObject;
			if (child.name.StartsWith ("child")) {
				child.GetComponent<Renderer> ().material = Resources.Load<Material> ("StlMaterial");
			}
		}
	}
}
