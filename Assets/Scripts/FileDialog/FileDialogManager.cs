using SFB;

public class FileDialogManager
{
	private static FileDialogManager _INSTANCE;

	private FileDialogManager ()
	{
	}

	public static FileDialogManager GetInstance ()
	{
		if (_INSTANCE == null) {
			_INSTANCE = new FileDialogManager ();
		}
		return _INSTANCE;
	}

	public string ShowFileSelectDialog_STL_OBJ (string defaultDir)
	{
		if (defaultDir == null || defaultDir.Trim ().Length == 0) {
			defaultDir = "";
		}

		var extensions = new [] {
			new ExtensionFilter ("", "stl", "obj")
		};

		string[] pathArray = StandaloneFileBrowser.OpenFilePanel ("Open File", defaultDir, extensions, false);

        //path: file:///Users/liuming/3dModel/pc.stl
        //localPath: /Users/liuming/3dModel/pc.stl
        //It is ok if path has Chinese
        if (pathArray.Length == 0)
        {
            return null;
        }
		string path = pathArray [0];
		if (string.IsNullOrEmpty (path)) {
			return path;
		}
		string localPath = new System.Uri (path).LocalPath;
		return localPath;
	}

	public string ShowFileSaveDialog_Gcode (string defaultName)
	{
		if (defaultName == null || defaultName.Trim ().Length == 0) {
			defaultName = "Untitled";
		}
		string extention = "gcode";
		//path example:/Users/liuming/Desktop/defaultName.txt
		string path = StandaloneFileBrowser.SaveFilePanel ("Save File", "", defaultName, extention);
		return path;
	}

    public string ShowFileSaveDialog_snapmaker3dProfile(string defaultName)
    {
        if (defaultName == null || defaultName.Trim().Length == 0)
        {
            defaultName = "Untitled";
        }
        string extention = Global.extension_Profile;
        string path = StandaloneFileBrowser.SaveFilePanel("Save File", "", defaultName, extention);
        return path;
    }
		
    public string ShowFileSelectDialog_snapmaker3dProfile(string defaultDir)
    {
        if (defaultDir == null || defaultDir.Trim().Length == 0)
        {
            defaultDir = "";
        }

        var extensions = new[] {
            new ExtensionFilter ("", Global.extension_Profile)
        };

        string[] pathArray = StandaloneFileBrowser.OpenFilePanel("Open File", defaultDir, extensions, false);
        if (pathArray.Length == 0)
        {
            return null;
        }
        string path = pathArray[0];
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }
        string localPath = new System.Uri(path).LocalPath;
        return localPath;
    }

}
