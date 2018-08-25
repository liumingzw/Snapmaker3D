using UnityEngine;

public class GcodeRenderBean
{
	public Vector3 vector3;
	public float e;
	public GcodeType type;
	public int layerIndex;

	public GcodeRenderBean (Vector3 vector3, float e, GcodeType type, int layerIndex)
	{
		this.vector3 = vector3;
		this.e = e;
		this.layerIndex = layerIndex;

		if (e == 0) {
			this.type = GcodeType.Travel;
		} else {
			this.type = type;
		}
	}

	public override string ToString ()
	{
		return  vector3 + " / " + layerIndex + " / " + e;
	}
}
