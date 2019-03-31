﻿using OzetteLibrary.Exceptions;
using OzetteLibrary.Files;
using OzetteLibrary.Folders;
using OzettePowerShell.Utility;
using System;
using System.Linq;
using System.Management.Automation;

namespace OzettePowerShell.Functions.Public
{
    [Cmdlet(VerbsCommon.Add, "OzetteLocalSource")]
    public class AddOzetteLocalSourceCommand : BaseOzetteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string FolderPath { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateSet("Low", "Medium", "High")]
        public string Priority { get; set; }

        [Parameter(Mandatory = true)]
        public int Revisions { get; set; }

        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string MatchFilter = OzetteLibrary.Constants.CommandLine.DefaultSourceMatchFilter;

        protected override void ProcessRecord()
        {
            FileBackupPriority priorityEnum;
            if (!Enum.TryParse(Priority, out priorityEnum))
            {
                throw new ArgumentException(nameof(Priority) + " value was not valid.");
            }

            if (Revisions <= 0)
            {
                throw new ArgumentException(nameof(Revisions) + " value must be at least 1.");
            }

            var db = GetDatabaseConnection();

            WriteVerbose("Querying for existing scan sources to check for duplicates.");

            var allSources = db.GetSourceLocationsAsync().GetAwaiter().GetResult();
            var allLocalSources = allSources.Where(x => x is LocalSourceLocation).ToList();

            if (allLocalSources.Any(x => string.Equals(x.Path, FolderPath, StringComparison.CurrentCultureIgnoreCase)
                                      && string.Equals(x.FileMatchFilter, MatchFilter, StringComparison.CurrentCultureIgnoreCase)))
            {
                // there already exists a source with this folder location and match filter.
                throw new SourceLocationException("Unable to add source: the specified folder and match filter combination is already listed as a source.");
            }
            else
            {
                WriteVerbose("No duplicate sources found.");
            }

            var newSource = new LocalSourceLocation();
            newSource.Path = FolderPath;
            newSource.FileMatchFilter = MatchFilter;
            newSource.RevisionCount = Revisions;
            newSource.Priority = priorityEnum;

            WriteVerbose("Validating the source parameters are acceptable.");
            newSource.ValidateParameters();
            WriteVerbose("The specified scan source has normal parameters.");

            WriteVerbose("Saving the source to the database.");

            db.SetSourceLocationAsync(newSource).GetAwaiter().GetResult();

            WriteVerbose("Successfully saved source to the database.");
        }
    }
}
