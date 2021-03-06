﻿using ArchivialPowerShell.Utility;
using System.Management.Automation;

namespace ArchivialPowerShell.Functions.Public
{
    /// <summary>
    ///   <para type="synopsis">Returns all of the configured Archivial Providers.</para>
    ///   <para type="description">Returns all of the configured Archivial Providers. An Archivial Provider is a connection to an external service for either cloud storage (ex: Azure, AWS) or message notifications (ex: Sendgrid email, Twilio SMS/text).</para>
    ///   <para type="description">Note: Only the name and ID of the provider will be returned. The encrypted secure setting values will not returned in the output.</para>
    ///   <para type="description">The output from this command can be piped to the Remove-ArchivialProvider cmdlet.</para>
    /// </summary>
    /// <example>
    ///   <code>C:\> Get-ArchivialProviders</code>
    ///   <para>Returns all of the configured Archivial Providers.</para>
    ///   <para></para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "ArchivialProviders")]
    public class GetArchivialProvidersCommand : BaseArchivialCmdlet
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public GetArchivialProvidersCommand() : base() { }

        /// <summary>
        /// Secondary constructor for dependency injection.
        /// </summary>
        /// <param name="dependencies"></param>
        public GetArchivialProvidersCommand(CmdletDependencies dependencies) : base(dependencies) { }

        protected override void ProcessRecord()
        {
            var db = GetDatabaseConnection();

            WriteVerbose("Querying for configured providers.");

            var allProviders = db.GetProvidersAsync(ArchivialLibrary.Providers.ProviderTypes.Any).GetAwaiter().GetResult();

            WriteVerbose(string.Format("Writing output results to pipeline (Objects: {0})", allProviders.Count));

            foreach (var provider in allProviders)
            {
                WriteObject(provider);
            }
        }
    }
}
