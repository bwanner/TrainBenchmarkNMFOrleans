using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkLibrary;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;

namespace SiloRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private AzureSilo silo;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("OrleansSiloWorker is running");

            Trace.WriteLine("OrleansAzureSilos-Run entry point called", "Information");

            Trace.WriteLine("OrleansAzureSilos-OnStart Starting Orleans silo", "Information");

            var config = new ClusterConfiguration();
            config.LoadFromFile("OrleansConfiguration.xml");

            // First example of how to configure an existing provider

            // It is IMPORTANT to start the silo not in OnStart but in Run.
            // Azure may not have the firewalls open yet (on the remote silos) at the OnStart phase.
            silo = new AzureSilo();
            bool ok = silo.Start(config);

            Trace.WriteLine("OrleansAzureSilos-OnStart Orleans silo started ok=" + ok, "Information");

            silo.Run(); // Call will block until silo is shutdown
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            if (!RoleEnvironment.IsEmulated)
            {
                var modelPath = RoleEnvironment.GetLocalResource("ModelStorage").RootPath;
                BenchmarkSetup.SetupOnAzure(modelPath + "\\", RoleEnvironment.GetConfigurationSettingValue("Smb_Path"), RoleEnvironment.GetConfigurationSettingValue("Smb_User"),
                    RoleEnvironment.GetConfigurationSettingValue("Smb_Password"));
            }

            Trace.TraceInformation("SiloRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("SiloRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("SiloRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
