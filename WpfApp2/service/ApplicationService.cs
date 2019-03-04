using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text;

namespace WpfApp2.service
{
    class ApplicationService
    {
        private String separator = "#---isecurity---";
        private String[] bannedApplications;
        private String[] allApplications;
        private String[] browsers;
        private String[] bannedSites;
        private static ApplicationService applicationService = null;

        public string[] BannedApplications { get => bannedApplications; set => bannedApplications = value; }
        public string[] AllApplications { get => allApplications; set => allApplications = value; }
        public string[] Browsers { get => browsers; set => browsers = value; }
        public string[] BannedSites { get => bannedSites; set => bannedSites = value; }

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
            if (bannedApplications != null)
            {
                for (int i = 1; i <= bannedApplications.Length; i++)
                {
                    disallowRun.SetValue(i.ToString(), bannedApplications[i - 1]);
                }
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
            List<String> fileHostsLines = readHostsFile();
            foreach (String site in bannedSites)
            {
                fileHostsLines.Add("127.0.0.1   " + site);
            }
            writeHostsFile(fileHostsLines);
            return true;
        }

        private List<String> readHostsFile()
        {
            List<String> lines = new List<String>();
            foreach (String line in File.ReadAllLines(@"C:\Windows\System32\drivers\etc\hosts", Encoding.UTF8))
            {
                if (line.Equals(separator))
                {
                    break;
                }
                lines.Add(line);
                // process the line
            }
            lines.Add(separator);
            return lines;
        }

        private Boolean writeHostsFile(List<String> fileHostsLines)
        {
            File.WriteAllLines(@"C:\Windows\System32\drivers\etc\hosts", fileHostsLines.ToArray());
            return true;
        }

    }
}
