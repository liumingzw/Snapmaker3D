using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using System.Text;
using System;

public class Utils
{

	//not effective
	public static T DeepCopy<T> (T obj)
	{
		object retval;
		using (MemoryStream ms = new MemoryStream ()) {
			XmlSerializer xml = new XmlSerializer (typeof(T));
			xml.Serialize (ms, obj);
			ms.Seek (0, SeekOrigin.Begin);
			retval = xml.Deserialize (ms);
			ms.Close ();
		}
		return (T)retval;
	}

	//delete files and directorys in srcPath. Without delete directory(srcPath)
	public static void DelectDir (string srcPath)
	{
		try {
			DirectoryInfo dir = new DirectoryInfo (srcPath);
			FileSystemInfo[] fileinfo = dir.GetFileSystemInfos ();  
			foreach (FileSystemInfo i in fileinfo) {
				if (i is DirectoryInfo) {            
					DirectoryInfo subdir = new DirectoryInfo (i.FullName);
					subdir.Delete (true);          
				} else {
					File.Delete (i.FullName);      
				}
			}                
		} catch (System.Exception e) {
			throw e;
		}
	}

	public static float CalculateTextLength (UnityEngine.UI.Text text)
	{
		//if use FontStyle.Bold, width is 0 ? strange !
		text.font.RequestCharactersInTexture (text.text, text.fontSize, FontStyle.Normal);
		CharacterInfo characterInfo = new CharacterInfo ();
		int totalLength = 0;
		foreach (char c in text.text.ToCharArray ()) {
			text.font.GetCharacterInfo (c, out characterInfo, text.fontSize);
			totalLength += characterInfo.advance;
		}
		return totalLength;
	}

	/************* tool *******************/
	public static string CreateMD5 (string input)
	{
		// Use input string to calculate MD5 hash
		using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create ()) {
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes (input);
			byte[] hashBytes = md5.ComputeHash (inputBytes);

			// Convert the byte array to hexadecimal string
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < hashBytes.Length; i++) {
				sb.Append (hashBytes [i].ToString ("X2"));
			}
			return sb.ToString ();
		}
	}

	public static string GetMD5HashFromFile (string fileName)
	{
		try {
			FileStream file = new FileStream (fileName, FileMode.Open);
			System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider ();
			byte[] retVal = md5.ComputeHash (file);
			file.Close ();

			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < retVal.Length; i++) {
				sb.Append (retVal [i].ToString ("x2"));
			}
			return sb.ToString ();
		} catch (Exception ex) {
			throw new Exception ("GetMD5HashFromFile() fail,error:" + ex.Message);
		}
	}
}



