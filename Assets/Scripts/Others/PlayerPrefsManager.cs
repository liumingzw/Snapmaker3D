using UnityEngine;

public class PlayerPrefsManager
{

	/*************** single ***************/
	private static PlayerPrefsManager _INSTANCE;

	private PlayerPrefsManager ()
	{
	}

	public static PlayerPrefsManager GetInstance ()
	{
		if (_INSTANCE == null) {
			_INSTANCE = new PlayerPrefsManager ();
		}
		return _INSTANCE;
	}


    private string _curVersion;

	public int GetLaunchTimes_CurVersion ()
	{
		return PlayerPrefs.GetInt (_curVersion, 0);  
	}

	//call it in Start and called only once
	public void SetCurVersion (string version)
	{
		_curVersion = version;
        int luanchTimes = PlayerPrefs.GetInt (version, 0);  
		PlayerPrefs.SetInt (version, ++luanchTimes);
        Debug.Log ("SetCurVersion : " + "[version " + version + "] " + " [luanch times " + luanchTimes + "]" + "\n");
	}
}
