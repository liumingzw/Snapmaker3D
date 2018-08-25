using UnityEngine;
using System.IO;

public class ControlInit : MonoBehaviour, AlertMananger.Listener
{
    void Awake()
    {
        delteTempDir();

        Global.GetInstance().Init();

        Loom.Initialize();

        StageManager.SetStage_Idle();
    }

    void Start()
    {
        string version = "2.0";
        PlayerPrefsManager.GetInstance().SetCurVersion(version);

        //first launch the version
        if (PlayerPrefsManager.GetInstance().GetLaunchTimes_CurVersion() == 1)
        {
            Debug.Log("First launch: Vserion=" + version);

            string originPath_customProfile = PathManager.profilePath_custom();

            string fileName_customProfile = Path.GetFileName(originPath_customProfile);

            string dirPath = PathManager.CustomPrintProfilesArchiveDir();

            string destinationPath_customProfile = dirPath + "/" + fileName_customProfile;

            if (!File.Exists(originPath_customProfile))
            {
                Debug.LogWarning("File not exist:" + originPath_customProfile);
            }

            if (File.Exists(destinationPath_customProfile))
            {
                Debug.LogWarning("File exist:" + destinationPath_customProfile);
            }

            if (File.Exists(originPath_customProfile) && !File.Exists(destinationPath_customProfile)){
                //copy file : originPath_customProfile ----> destinationPath_customProfile
                Debug.LogError("Copy file:" + originPath_customProfile + " ---> " + destinationPath_customProfile);
                File.Copy(originPath_customProfile, destinationPath_customProfile);
            }
        }

        /********** config ************/
        ConfigManager.GetInstance().UnarchiveProfiles();

        ConfigManager.GetInstance().SelectProfile_fastPrint();
    }

    void OnDestroy()
    {
        delteTempDir();

        ConfigManager.GetInstance().ArchiveCustomProfiles();

        ConfigManager.GetInstance().ArchiveParamsOfCustomMaterial();

        PortConnectManager.GetInstance().ReleaseAll();
    }

    private void delteTempDir()
    {
        if (Directory.Exists(PathManager.dirPath_Temp()))
        {
            Utils.DelectDir(PathManager.dirPath_Temp());
        }
    }

    public void OnCancel()
    {
        Application.CancelQuit();
    }

    public void OnConfirm()
    {
        Application.Quit();
    }

}
