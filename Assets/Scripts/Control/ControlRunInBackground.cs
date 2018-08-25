using UnityEngine;

public class ControlRunInBackground : MonoBehaviour
{
    void Update()
    {
        switch (StageManager.GetCurStage())
        {
            case StageManager.Stage_Enum.Idle:
                Application.runInBackground = false;
                break;
            case StageManager.Stage_Enum.Load_Model:
                Application.runInBackground = ModelManager.GetInstance().GetInfo().isParsing;
                break;
            case StageManager.Stage_Enum.Gcode_Create:
                Application.runInBackground = GcodeCreateManager.GetInstance().GetInfo().isSlicing;
                break;
            case StageManager.Stage_Enum.Gcode_Render:
                Application.runInBackground = GcodeRenderManager.GetInstance().GetInfo().isParsing;
                break;
            case StageManager.Stage_Enum.Gcode_Send:
                Application.runInBackground = GcodeSenderManager.GetInstance().GetInfo().sending_GcodeFile;
                break;
        }
    }
}
