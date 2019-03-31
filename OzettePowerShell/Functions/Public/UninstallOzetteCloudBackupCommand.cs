﻿using OzettePowerShell.Exceptions;
using OzettePowerShell.Utility;
using System.Management.Automation;

namespace OzettePowerShell.Functions.Public
{
    [Cmdlet(VerbsLifecycle.Uninstall, "OzetteCloudBackup", ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
    public class UninstallOzetteCloudBackupCommand : BaseOzetteCmdlet
    {
        [Parameter(Mandatory = false)]
        public SwitchParameter Force = false;

        public UninstallOzetteCloudBackupCommand()
        {
            ActivityName = "Uninstallation";
            ActivityID = 2;
        }

        protected override void ProcessRecord()
        {
            if (!Elevation.IsRunningElevated())
            {
                throw new CmdletNotElevatedException("This cmdlet requires elevated (run-as administrator) privileges. Please re-launch the cmdlet in an elevated window.");
            }
            if (!Force && !ShouldProcess("Uninstalling Ozette Cloud Backup Software", "Are you sure that you would like to uninstall this software? It will permenantly delete the installation folder, state database, and log files. This action is not reversable.", "Uninstall Ozette Cloud Backup"))
            {
                throw new CmdletExecutionNotApprovedException("This action must be approved (or provide the -force switch) to run.");
            }

            WriteVerbose("Removing Ozette Cloud Backup installation.");

            var db = GetDatabaseConnection();

            WriteVerboseAndProgress(10, "Stopping OzetteCloudBackup Windows Service.");
            Uninstallation.StopClientService();

            WriteVerboseAndProgress(40, "Removing Ozette client database.");
            db.DeleteClientDatabaseAsync().GetAwaiter().GetResult();

            WriteVerboseAndProgress(55, "Removing OzetteCloudBackup Windows Service.");
            Uninstallation.DeleteClientService();

            WriteVerboseAndProgress(70, "Removing installation files and folders.");
            Uninstallation.DeleteInstallationDirectories();

            WriteVerboseAndProgress(80, "Removing custom event log source.");
            Uninstallation.DeleteEventLogContents();

            WriteVerboseAndProgress(90, "Removing core settings.");
            Uninstallation.DeleteCoreSettings();

            WriteVerboseAndProgress(100, "Uninstallation completed.");
        }
    }
}
