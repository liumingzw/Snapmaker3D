using UnityEngine;
using System.IO;

public class PathManager
{
    private PathManager()
    {
    }

    /***************** CuraEngine path *****************/
    public static string enginePath_mac()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Mac/CuraEngine";
    }

    public static string enginePath_win32()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Win/Win_32/CuraEngine.exe";
    }

    public static string enginePath_win64()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Win/Win_64/CuraEngine.exe";
    }

    /***************** CocoaDialog path *****************/
    public static string CocoaDialogPath()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CocoaDialog.app/Contents/MacOS/CocoaDialog";
    }

    /***************** shell path *****************/
    public static string shellPath_createGcode_mac()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Mac/createGcode_mac.sh";
    }

    public static string shellPath_createGcode_win()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Win/createGcode_win.bat";
    }

    /************* print config (.json) *******************/
    public static string configPath_fastPrint()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Config/" + "fast_print.def.json";
    }

    public static string configPath_normalQuality()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Config/" + "normal_quality.def.json";
    }

    public static string configPath_highQuality()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Config/" + "high_quality.def.json";
    }

    //inner custom profile
    public static string profilePath_custom()
    {
        return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Config/" + "Custom." + Global.extension_Profile;
    }
    //public static string configPath_customize()
    //{
    //    return Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Config/" + "customize.def.json";
    //}

    //	public static List<string> configPath_expertCustom ()
    //	{
    //		//todo
    //		return null;
    //	}

    /***************** directory path *****************/
    public static string dirPath_Temp()
    {
        string dirPath = Application.streamingAssetsPath + "/CrossPlatform/CuraEngine/Temp/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
            Debug.Log("Create Dir:" + dirPath + "\n");
        }
        return dirPath;
    }

    public static string CustomPrintProfilesArchiveDir()
    {
        string dirPath = Application.persistentDataPath + "/PrintProfiles";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
            Debug.Log("Create Dir:" + dirPath + "\n");
        }
        return dirPath;
    }


    public static string CachePath()
    {
        string dirPath = Application.temporaryCachePath;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
            Debug.Log("Create Dir:" + dirPath + "\n");
        }
        return dirPath;
    }

}
