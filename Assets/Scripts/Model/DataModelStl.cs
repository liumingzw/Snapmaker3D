using System;
using System.IO;
using System.Collections.Generic;

public class DataModelStl : DataModel
{
	private enum Format_Enum
	{
		Ascii,
		Binary,
		Unknown
	}

	private string _header_ascii;
	private byte[] _header_binary;

	public DataModelStl ()
	{
	}

	public override void ParseFacetListFromFile (string path)
	{
		base.facetList.Clear ();

		Format_Enum format = parseFormat (path);

		switch (format) {
		case Format_Enum.Ascii:
			parseAsAscii (path);
			break;
		case Format_Enum.Binary:
			parseAsBinary (path);
			break;
		case Format_Enum.Unknown:
			if (_listener != null) {
				_listener.OnDataModel_ParseFailed ();
			}
			break;
		}

		UnityEngine.Debug.Log ("****************** Parse STL File Result ******************" + "\n");

		System.IO.FileInfo fileInfo = new System.IO.FileInfo (path);

		float file_size = fileInfo.Length / 1024.0f;

		UnityEngine.Debug.Log ("File Path : " + path + "\n");

		UnityEngine.Debug.Log ("File Size : " + file_size + "KB" + "\n");

		UnityEngine.Debug.Log ("STL Format : " + format.ToString () + "\n");

		UnityEngine.Debug.Log ("Facet Count : " + base.facetList.Count + "\n");

		UnityEngine.Debug.Log ("***************************************************" + "\n");
	}

	private void parseAsBinary (string path)
	{
		FileStream fs = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		BinaryReader br = new BinaryReader (fs);
		_header_binary = br.ReadBytes (80);

		int num_faces = br.ReadInt32 ();

		if (num_faces < 1 || num_faces > 1000000000) {
			if (_listener != null) {
				_listener.OnDataModel_ParseFailed ();
			}
			return;
		}

		if (new System.IO.FileInfo (path).Length < num_faces * 50 + 84) {
			return;
		}

		float progress = 0;

		try {
			for (int i = 0; i < num_faces; i++) {
				Point3d normal = new Point3d (br.ReadSingle (), br.ReadSingle (), br.ReadSingle ());
				Point3d p1 = new Point3d (br.ReadSingle (), br.ReadSingle (), br.ReadSingle ());
				Point3d p2 = new Point3d (br.ReadSingle (), br.ReadSingle (), br.ReadSingle ());
				Point3d p3 = new Point3d (br.ReadSingle (), br.ReadSingle (), br.ReadSingle ());
				br.ReadBytes (2);

				Facet facet = new Facet (p1, p2, p3);
				facet.normal = normal;

				facetList.Add (facet);

				if (_listener != null) {
					if ((float)i / num_faces - progress >= 0.01f) {
						progress = (float)i / num_faces;
						_listener.OnDataModel_ParseProgress (progress);
					}
				}
			}
		} catch (Exception e) {

			base.facetList.Clear ();
			UnityEngine.Debug.LogError ("Error occur: parseAsBinary : " + e.ToString () + "\n");

		} finally {
			
			br.Close ();
			fs.Close ();

			if (_listener != null) {
				if (base.facetList.Count == 0) {
					progress = 1.0f;
					_listener.OnDataModel_ParseProgress (progress);
					_listener.OnDataModel_ParseFailed ();
				} else {
					progress = 1.0f;
					_listener.OnDataModel_ParseProgress (progress);
					_listener.OnDataModel_ParseComplete (this);
				}
			}
		}
	}

	//How cura do : https://github.com/Ultimaker/Uranium/blob/master/plugins/FileHandlers/STLReader/STLReader.py
	private void parseAsAscii (string path)
	{
//		Stopwatch sw = new Stopwatch ();
//		sw.Start ();

		string[] allLines = File.ReadAllLines (path);

		int lineCount = allLines.Length;

		//check
		if (lineCount < 9) {
			UnityEngine.Debug.LogError ("Error occur: parseAsAscii [no data in file:" + path + "]" + "\n");
			if (_listener != null) {
				_listener.OnDataModel_ParseFailed ();
			}
			return;
		}

		float progress = 0;

		List<Point3d> point3dBuffer = new List<Point3d> ();

		System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex ("[\\s]+");

		try {
			for (int i = 0; i < lineCount; i++) {
			
				string line = allLines [i];
				if (line.Contains ("vertex")) {
				
					//replace blanks by one blank
					string[] dataArray = regex.Replace (line, " ").Trim ().Split (' '); 

					if (dataArray.Length == 4) {
						Point3d point = new Point3d (float.Parse (dataArray [1]), float.Parse (dataArray [2]), float.Parse (dataArray [3]));
						point3dBuffer.Add (point);
						if (point3dBuffer.Count == 3) {
							Facet facet = new Facet (point3dBuffer [0], point3dBuffer [1], point3dBuffer [2]);
							base.facetList.Add (facet);
							point3dBuffer.Clear ();
						}
					}

					if (_listener != null) {
						if ((float)i / lineCount - progress >= 0.01f) {
							progress = (float)i / lineCount;
							_listener.OnDataModel_ParseProgress (progress);
						}
					}
				}
			}
		} catch (Exception e) {
			
			base.facetList.Clear ();
			UnityEngine.Debug.LogError ("Error occur: parseAsAscii : " + e.ToString () + "\n");

		} finally {
			
			if (_listener != null) {
				if (base.facetList.Count == 0) {
					progress = 1.0f;
					_listener.OnDataModel_ParseProgress (progress);
					_listener.OnDataModel_ParseFailed ();
				} else {
					progress = 1.0f;
					_listener.OnDataModel_ParseProgress (progress);
					_listener.OnDataModel_ParseComplete (this);
				}
			}
		}

//		sw.Stop ();
//		UnityEngine.Debug.Log (string.Format ("Parse stl File Cost : {0} ms" + "\n", sw.ElapsedMilliseconds));
	}

	private Format_Enum parseFormat (string path)
	{
		//1.check file is empty
		if (new System.IO.FileInfo (path).Length == 0) {
			UnityEngine.Debug.LogError ("Error occur: parseFormat [file is empty:" + path + "]" + "\n");
			return Format_Enum.Unknown;
		}

		//2.check whether is ascii
		{
			FileStream fs_ascii = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			StreamReader sr = new StreamReader (fs_ascii);
			List<string> firstTwoLines = new List<string> ();

			//read first 2 lines
			while (sr.Peek () > -1) {
				if (firstTwoLines.Count == 2) {
					break;
				}
				string line = sr.ReadLine ();
				if (line != null && line.Trim ().Length > 0) {
					firstTwoLines.Add (line);
				}
			}

			sr.Close ();
			fs_ascii.Close ();

			if (firstTwoLines.Count == 2) {
//				UnityEngine.Debug.Log ("line1=" + firstTwoLines [0]);
//				UnityEngine.Debug.Log ("line2=" + firstTwoLines [1]);

				if (firstTwoLines [0].ToLower ().Trim ().StartsWith ("solid") &&
				    firstTwoLines [1].ToLower ().Trim ().StartsWith ("facet")) {
					return Format_Enum.Ascii;
				}
			}
		}

		//3.check whether is binary
		{
			FileStream fs_binary = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			BinaryReader br = new BinaryReader (fs_binary);
			_header_binary = br.ReadBytes (80);

			int num_faces = br.ReadInt32 ();

			br.Close ();
			fs_binary.Close ();

			if (num_faces < 1 || num_faces > 1000000000) {
				return Format_Enum.Unknown;
			}

			if (new System.IO.FileInfo (path).Length < num_faces * 50 + 84) {
				return Format_Enum.Unknown;
			}
		}

		return Format_Enum.Binary;
	}
}
