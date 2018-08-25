public class GcodeCreateBean
{
    public string stlFilePath, configJsonFilePath;
    public string stlFileMD5, configJsonFileMD5;

    public float filamentLength;
    public float filamentWeight;
    public int printTime;

    public string gcodePath;

    public GcodeCreateBean(string stlFilePath, string configJsonFilePath, string gcodeFilePath)
    {
        this.stlFilePath = stlFilePath;
        this.configJsonFilePath = configJsonFilePath;
        this.gcodePath = gcodeFilePath;

        this.stlFileMD5 = Utils.GetMD5HashFromFile(stlFilePath);
        this.configJsonFileMD5 = Utils.GetMD5HashFromFile(configJsonFilePath);

        filamentLength = 0;
        filamentWeight = 0;
        printTime = 0;
    }

    public GcodeCreateBean()
    {
    }
}
