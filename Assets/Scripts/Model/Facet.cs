public class Facet
{
	public Point3d p1, p2, p3;

	// normal vector can be represented by a point in space
	public Point3d normal;

	//only binary stl has it
	public byte[] property = new byte[2];

	public Facet (Point3d p1, Point3d p2, Point3d p3)
	{
		this.p1 = p1;
		this.p2 = p2;
		this.p3 = p3;
	}

	public override string ToString ()
	{
		return "{ "+"p1:" + p1 + "," + "p2:" + p2 + "," + "p3:" + p3 + " }";
	}

	public Facet DeepCopy ()
	{
		Facet facetTemp = new Facet (this.p1.DeepCopy(), this.p2.DeepCopy(), this.p3.DeepCopy());
		if(this.normal == null){
			this.normal = new Point3d (0, 0, 0);
		}
		facetTemp.normal = this.normal.DeepCopy ();
		facetTemp.property = this.property;
		return facetTemp;
	}
  
}
