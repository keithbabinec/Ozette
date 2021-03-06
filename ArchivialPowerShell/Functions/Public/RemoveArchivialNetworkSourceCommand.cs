﻿using ArchivialLibrary.Database;
using ArchivialLibrary.Folders;
using ArchivialPowerShell.Utility;
using System.Linq;
using System.Management.Automation;

namespace ArchivialPowerShell.Functions.Public
{
    /// <summary>
    ///   <para type="synopsis">Removes the specified Network Source.</para>
    ///   <para type="description">A Network Source is a folder on your network (referenced by UNC Path) that Archivial backs up and automatically monitors for new and updated files.</para>
    ///	  <para type="description">Removing a Network Source means that new or updated files from that location will not be backed up anymore, but existing files already backed up to cloud storage will remain.</para>
    ///   <para type="description">To view existing Network Sources, run Get-ArchivialNetworkSource. This command supports piping from Get-ArchivialNetworkSource or manual invoke from the specified source ID.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\> Remove-ArchivialNetworkSource -ID 3</code>
    ///   <para>Removes the Network Source with the specified ID.</para>
    ///   <para></para>
    /// </example>
    /// <example>
    ///   <code>C:\> Get-ArchivialNetworkSource | Where Path -like "*\\drobo-nas\documents*" | Remove-ArchivialNetworkSource</code>
    ///   <para>Removes any configured Network Source that matches a path containing the specified filter (using the pipeline scenario).</para>
    ///   <para></para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "ArchivialNetworkSource")]
    public class RemoveArchivialNetworkSourceCommand : BaseArchivialCmdlet
    {
        /// <summary>
        ///   <para type="description">Specify the ID of the Network Source to remove.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByName")]
        [ValidateRange(0, int.MaxValue)]
        public int SourceID { get; set; }

        /// <summary>
        ///   <para type="description">Specify the object (from pipeline) to remove.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByObject", ValueFromPipeline = true)]
        [ValidateNotNull]
        public NetworkSourceLocation NetworkSource { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RemoveArchivialNetworkSourceCommand() : base() { }

        /// <summary>
        /// Secondary constructor for dependency injection.
        /// </summary>
        /// <param name="dependencies"></param>
        public RemoveArchivialNetworkSourceCommand(CmdletDependencies dependencies) : base(dependencies) { }

        private IClientDatabase database { get; set; }

        protected override void BeginProcessing()
        {
            database = GetDatabaseConnection();
        }

        protected override void ProcessRecord()
        {
            if (NetworkSource == null)
            {
                WriteVerbose("Querying for existing scan sources to see if the specified source exists.");

                var allSources = database.GetSourceLocationsAsync().GetAwaiter().GetResult();

                var sourceMatch = allSources.FirstOrDefault(x => x.ID == SourceID && x is NetworkSourceLocation);

                if (sourceMatch == null)
                {
                    throw new ItemNotFoundException("There was no network source location found with the specified ID: " + SourceID);
                }
                else
                {
                    NetworkSource = (NetworkSourceLocation)sourceMatch;
                }
            }

            WriteVerbose("Removing the network source location now.");

            database.RemoveSourceLocationAsync(NetworkSource).GetAwaiter().GetResult();

            WriteVerbose("Successfully removed the network source location from the database.");
        }
    }
}
