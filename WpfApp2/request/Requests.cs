using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.service;
using WpfApp2.isecurity;

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
                applicationService.BannedApplications = response.policy.bannedApps;
                applicationService.BannedSites = response.policy.bannedSites;
                applicationService.banApplications();
                applicationService.banSites();
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



    }
}
