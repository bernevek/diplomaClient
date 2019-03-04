using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace WpfApp2.service
{
    class ApplicationService
    {

        private String[] bannedApplications;
        private String[] allApplications;
        private String[] browsers;
        private static ApplicationService applicationService = null;

        public string[] BannedApplications { get => bannedApplications; set => bannedApplications = value; }
        public string[] AllApplications { get => allApplications; set => allApplications = value; }
        public string[] Browsers { get => browsers; set => browsers = value; }

        public static ApplicationService getApplicationService()
        {
            if (applicationService == null)
            {
                applicationService = new ApplicationService();
            }
            return applicationService;
        }

        public Boolean closeApplications()
        {
            try
            {
                foreach (String application in allApplications)
                {
                    Process[] proc = Process.GetProcessesByName(application.Replace(".exe", "").Replace(".msc", ""));
                    foreach (Process element in proc)
                    {
                        Console.WriteLine(proc.Length);
                        element.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public Boolean banApplications()
        {
            RegistryKey currentUserKey = Registry.CurrentUser;
            RegistryKey explorer = currentUserKey.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", true);
            Object disRun = explorer.GetValue("DisallowRun");
            if (disRun == null)
            {
                explorer.SetValue("DisallowRun", 1, RegistryValueKind.DWord);
            }
            RegistryKey disallowRun = explorer.OpenSubKey("DisallowRun", true);
            if (disallowRun != null)
            {
                explorer.DeleteSubKey("DisallowRun");
            }
            disallowRun = explorer.CreateSubKey("DisallowRun");
            for (int i = 1; i <= bannedApplications.Length; i++)
            {
                disallowRun.SetValue(i.ToString(), bannedApplications[i-1]);
            }
            currentUserKey.Close();
            return true;
        }

        public Boolean cleanBrowsersHistory()
        {
            String path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            //path += "\\Local\\Goolle";
            //path += "\\Roaming\\Mozilla";

            foreach (String browser in browsers)
            {
                Directory.Delete(@path + browser, true);
            }
            return true;
        }

        public Boolean banSites()
        {

            return true;
        }

    }
}
