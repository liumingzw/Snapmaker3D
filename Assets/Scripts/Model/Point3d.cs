public class Point3d
{
    public float x, y, z;

    public Point3d(float x, float y, float z)
    {
        this.x = x;
		this.y = y;
		this.z = z;
    }

    public override string ToString()
    {
        return "[" + x + "/" + y + "/" + z + "/" + "]";
    }

	public Point3d DeepCopy(){
		return new Point3d (this.x, this.y, this.z);
	}
}
