using System;
using UnityEngine;

public class Global
{
    public static string  extension_Profile = "snapmaker3dProfile" ; 
    //public static string  extension_Profile = "json" ; 
	/*************** OS platform ***************/
	public enum OS_Platform_Enum
	{
		Mac,
		Win_32,
		Win_64,
		Linux,
		Unsupport
	}

	private OS_Platform_Enum OS_platform;

	public OS_Platform_Enum GetOSPlatform ()
	{
		return OS_platform;
	}

	/*************** device params ***************/
	public struct DeviceParamsStruct
	{
		public Vector3 size;
		public string name;
	}

	private DeviceParamsStruct deviceParamsStruct;

	public DeviceParamsStruct GetPrinterParamsStruct ()
	{
		return deviceParamsStruct;
	}

	/*************** single ***************/
	private static Global _INSTANCE;

	private Global ()
	{
	}

	public static Global GetInstance ()
	{
		if (_INSTANCE == null) {
			_INSTANCE = new Global ();
		}
		return _INSTANCE;
	}

	/*************** function ***************/
	public void Init ()
	{
		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
			if (IntPtr.Size == 8) { 
				//64 bit
				OS_platform = Global.OS_Platform_Enum.Win_64;
			} else {
				//32 bit
				OS_platform = Global.OS_Platform_Enum.Win_32;
			} 
		} else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
			OS_platform = Global.OS_Platform_Enum.Mac;
		} else {
			OS_platform = Global.OS_Platform_Enum.Unsupport;
		}

		deviceParamsStruct.size = new Vector3 (125, 125, 125);
		deviceParamsStruct.name = "Snapmaker_1";

		printDeviceInfo ();

		printPcInfo ();
	}

	private void printDeviceInfo ()
	{
		UnityEngine.Debug.Log ("****************** Device Info ******************" + "\n");

		UnityEngine.Debug.Log ("OS platform : " + OS_platform.ToString () + "\n");

		UnityEngine.Debug.Log ("Device name : " + deviceParamsStruct.name + "\n");

		UnityEngine.Debug.Log ("Device size : " + deviceParamsStruct.size + "\n");

		UnityEngine.Debug.Log ("***************************************************" + "\n");
	}

	private void printPcInfo ()
	{
		UnityEngine.Debug.Log ("****************** PC Info ******************" + "\n");

		Debug.Log ("deviceModel : " + SystemInfo.deviceModel + "\n");

		Debug.Log ("deviceName : " + SystemInfo.deviceName + "\n");

		Debug.Log ("deviceType : " + SystemInfo.deviceType.ToString () + "\n");

		Debug.Log ("systemMemorySize(MB) : " + SystemInfo.systemMemorySize.ToString () + "\n");

		Debug.Log ("operatingSystem : " + SystemInfo.operatingSystem + "\n");

		Debug.Log ("deviceUniqueIdentifier : " + SystemInfo.deviceUniqueIdentifier + "\n");

		Debug.Log ("graphicsDeviceID : " + SystemInfo.graphicsDeviceID.ToString () + "\n");

		Debug.Log ("graphicsDeviceName : " + SystemInfo.graphicsDeviceName + "\n");

		Debug.Log ("graphicsDeviceType : " + SystemInfo.graphicsDeviceType.ToString () + "\n");

		Debug.Log ("graphicsDeviceVendor : " + SystemInfo.graphicsDeviceVendor + "\n");

		Debug.Log ("graphicsDeviceVendorID : " + SystemInfo.graphicsDeviceVendorID.ToString () + "\n");

		Debug.Log ("graphicsDeviceVersion : " + SystemInfo.graphicsDeviceVersion + "\n");

		Debug.Log ("graphicsMemorySize(MB) : " + SystemInfo.graphicsMemorySize.ToString () + "\n");

		Debug.Log ("graphicsMultiThreaded support : " + SystemInfo.graphicsMultiThreaded.ToString () + "\n");

		Debug.Log ("supportedRenderTargetCount : " + SystemInfo.supportedRenderTargetCount.ToString () + "\n");

		UnityEngine.Debug.Log ("***************************************************" + "\n");
	}
}
