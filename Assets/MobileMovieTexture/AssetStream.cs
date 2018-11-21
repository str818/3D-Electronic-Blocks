using UnityEngine;
using System;

namespace MMT
{
    public class AssetStream
    {
    #if UNITY_ANDROID && !UNITY_EDITOR	
        // The path and name of the last accessed zip file.
	    private static string lastZipFilePath = null;
	    
        // Cache to the last accessed zip file. 
	    // Note: This cache is prefered because creating an instance of a ZipResourceFile is a expensive process.
	    private static AndroidJavaObject cachedZipFile = null;
    #endif

        public static bool GetZipFileOffsetLength(string zipFilePath, string fileName, out long offset, out long length)
        {
            offset = 0;
            length = 0;

    #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaObject assetFileDescriptor;

            if (zipFilePath.EndsWith("apk"))
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var assetManager = activity.Call<AndroidJavaObject>("getAssets")) //android.content.res.AssetManager
                        {
                            assetFileDescriptor = assetManager.Call<AndroidJavaObject>("openFd", fileName); //assets/ //android.content.res.AssetFileDescriptor
                        }
                    }
                }
            }
            else
            {
                if (lastZipFilePath != zipFilePath) 
                {
			        lastZipFilePath = zipFilePath;
    			
			        // Cleanup previous cached zip file resources
			        if (cachedZipFile != null) 
                    {
				        cachedZipFile.Dispose();
				        cachedZipFile = null;
			        }

			        cachedZipFile = new AndroidJavaObject("com.android.vending.expansion.zipfile.ZipResourceFile", zipFilePath);
		        }

                assetFileDescriptor = cachedZipFile.Call<AndroidJavaObject>("getAssetFileDescriptor", "assets/" + fileName);
            }

	        if (assetFileDescriptor != null && assetFileDescriptor.GetRawObject() != IntPtr.Zero) 
            {
		        offset = assetFileDescriptor.Call<long>("getStartOffset");
		        length = assetFileDescriptor.Call<long>("getLength");

                assetFileDescriptor.Dispose();
                assetFileDescriptor = null;
	        } 
            else 
            {
		        Debug.LogError("Couldn't find file: " +fileName + " in: "+ zipFilePath);
                return false;
	        }
    #endif
            return true;

        }

    }
}