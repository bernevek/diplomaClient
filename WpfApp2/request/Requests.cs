using System;
using System.Collections.Generic;
using Microsoft.Win32;
using WpfApp2.service;
using WpfApp2.isecurity;
using System.Net;

namespace WpfApp2.request
{
    class Requests
    {
        private ApplicationService applicationService = ApplicationService.getApplicationService();
        private static Requests requests = null;
        private SecurityClient client = new SecurityClient("ISecuritySoap11");

        public static Requests getRequests()
        {
            if (requests == null)
            {
                requests = new Requests();
            }
            return requests;
        }

        public void getAndApplyPolicy(String session)
        {
            GetPolicyRequest request = new GetPolicyRequest();
            request.session = MainWindow.Session;
            try
            {
                GetPolicyResponse response = client.GetPolicy(request);
                closeApps();
                getServices();
                applicationService.BannedApplications = response.policy.bannedApps;
                applicationService.BannedSites = response.policy.bannedSites;
                applicationService.BannedServices = response.policy.bannedServices;
                applicationService.banApplications();
                applicationService.banSites();
                applicationService.banServices();
            }
            catch (Exception ex)
            {
                return;
            }

        }

        private void closeApps()
        {
            GetAppsRequest request = new GetAppsRequest();
            request.session = MainWindow.Session;
            try
            {
                applicationService.AllApplications = client.GetApps(request);
                applicationService.closeApplications();
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public long sendComputerDetails()
        {
            SendComputerDetailsRequest request = new SendComputerDetailsRequest();
            ComputerDetails computerDetails = new ComputerDetails();
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\isecurity");
            if (key.ValueCount != 0)
            {
                computerDetails.serverId = long.Parse(key.GetValue("serverId").ToString());
                computerDetails.localId = long.Parse(key.GetValue("localId").ToString());
                key.Close();
            }
            else
            {
                computerDetails.localId = DateTime.Now.Ticks/1000;
            }
            computerDetails.localUserName = Environment.UserName;
            computerDetails.computerName = Environment.MachineName;
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            computerDetails.ip = myIP;
            request.ComputerDetails = computerDetails;
            SendComputerDetailsResponse response;
            try
            {
                response = client.SendComputerDetails(request);
                if (computerDetails.serverId == 0)
                {
                    key.SetValue("serverId", response.ComputerDetails.serverId);
                    key.SetValue("localId", response.ComputerDetails.localId);
                    key.Close();
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return response.ComputerDetails.serverId;
        }

        public Boolean getServices() {
            GetServicesRequest request = new GetServicesRequest();
            request.session = MainWindow.Session;
            try
            {
                applicationService.AllServices = client.GetServices(request);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string logout(string session)
        {
            LogoutRequest request = new LogoutRequest();
            request.session = session;
            LogoutResponse response = client.Logout(request);
            return response.status;
        }
    }
}
