﻿using OzetteLibrary.Folders;
using OzettePowerShell.Utility;
using System.Linq;
using System.Management.Automation;

namespace OzettePowerShell.Functions.Public
{
    [Cmdlet(VerbsCommon.Get, "OzetteNetworkSources")]
    public class GetOzetteNetworkSourcesCommand : BaseOzetteCmdlet
    {
        protected override void ProcessRecord()
        {
            var db = GetDatabaseConnection();

            WriteVerbose("Querying for existing backup sources.");

            var allSources = db.GetSourceLocationsAsync().GetAwaiter().GetResult();

            var filtered = allSources.Where(x => x is NetworkSourceLocation).ToArray();

            WriteVerbose(string.Format("Writing output results to pipeline (Objects: {0})", filtered.Length));

            foreach (var source in filtered)
            {
                WriteObject(source);
            }
        }
    }
}
