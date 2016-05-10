using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Hosting;
using BenchmarkLibrary;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace ClientRole
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            if (!RoleEnvironment.IsEmulated)
            {
                var modelPath = RoleEnvironment.GetLocalResource("ModelStorage").RootPath;
                BenchmarkSetup.SetupOnAzure(modelPath + "\\", RoleEnvironment.GetConfigurationSettingValue("Smb_Path"), RoleEnvironment.GetConfigurationSettingValue("Smb_User"),
                    RoleEnvironment.GetConfigurationSettingValue("Smb_Password"));
            }

            return base.OnStart();
        }
    }
}
