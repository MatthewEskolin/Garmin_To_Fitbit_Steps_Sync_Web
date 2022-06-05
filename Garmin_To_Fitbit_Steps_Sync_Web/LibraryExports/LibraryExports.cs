using System;
using System.Collections;
using System.Diagnostics;

namespace Garmin_To_Fitbit_Steps_Sync_Web.LibraryExports
{
    public class LibraryExports
    {
        public static void LogAllEnvVariables()
        {
            Debug.WriteLine("GetEnvironmentVariables: ");
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
                Debug.WriteLine("  {0} = {1}", de.Key, de.Value);


        }
    }
}
