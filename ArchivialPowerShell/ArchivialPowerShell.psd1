#
# Module manifest for module 'ArchivialPowerShell'
#
# Generated by: Keith Babinec
#
# Generated on: 3/31/2019
#

@{

# Script module or binary module file associated with this manifest.
RootModule = 'ArchivialPowerShell.dll'

# Version number of this module.
ModuleVersion = '0.2.6'

# Supported PSEditions
# CompatiblePSEditions = @()

# ID used to uniquely identify this module
GUID = '1fe6787e-955e-4073-8f41-fcfcf2ab0516'

# Author of this module
Author = 'Keith Babinec'

# Company or vendor of this module
CompanyName = 'Keith Babinec'

# Copyright statement for this module
Copyright = '(c) 2019 Keith Babinec. All rights reserved.'

# Description of the functionality provided by this module
Description = 'Data backup agent software for Windows that automatically archives your local data to a cloud storage provider.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '5.1'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
# FormatsToProcess = @()

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @()

# Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
FunctionsToExport = @()

# Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
CmdletsToExport = @(
	"Add-ArchivialLocalSource",
	"Add-ArchivialNetworkSource",
	"Get-ArchivialCloudBackupOptions",
	"Get-ArchivialCloudBackupStatus",
	"Get-ArchivialLocalSources",
	"Get-ArchivialNetworkCredentials",
	"Get-ArchivialNetworkSources",
	"Get-ArchivialProviders",
	"Install-ArchivialCloudBackup",
	"Remove-ArchivialLocalSource",
	"Remove-ArchivialNetworkCredential",
	"Remove-ArchivialNetworkSource",
	"Remove-ArchivialProvider",
	"Set-ArchivialAzureProviderOptions",
	"Set-ArchivialCloudBackupOptions",
	"Set-ArchivialNetworkCredential",
	"Set-ArchivialTwilioProviderOptions",
	"Start-ArchivialLocalSourceRescan",
	"Start-ArchivialNetworkSourceRescan",
	"Uninstall-ArchivialCloudBackup"
)

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
AliasesToExport = @()

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
FileList = @(
	"Microsoft.Azure.KeyVault.Core.dll",
    "Microsoft.Data.Tools.Schema.Sql.dll",
    "Microsoft.Data.Tools.Utilities.dll",
    "Microsoft.IdentityModel.Logging.dll",
    "Microsoft.IdentityModel.Tokens.dll",
    "Microsoft.SqlServer.Dac.dll",
    "Microsoft.SqlServer.TransactSql.ScriptDom.dll",
    "Microsoft.SqlServer.Types.dll",
    "Microsoft.WindowsAzure.Storage.dll",
    "NCrontab.dll",
    "Newtonsoft.Json.dll",
    "ArchivialClientAgent.exe",
    "ArchivialClientAgent.exe.config",
    "ArchivialDB.dacpac",
    "ArchivialDB.dll",
    "ArchivialLibrary.dll",
	"ArchivialPowerShell.dll",
	"ArchivialPowerShell.dll-Help.xml",
    "System.IdentityModel.Tokens.Jwt.dll",
    "System.Management.Automation.dll",
    "Twilio.dll"
)

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @('Archivial', 'Archivial-Backup', 'Archivial-Cloud-Backup', 'Backup', 'Archive', 'Data-Archive', 'Data-Backup', 'Cloud-Backup', 'Online-Backup', 'Azure', 'Azure-Backup')

        # A URL to the license for this module.
        LicenseUri = 'https://github.com/keithbabinec/Archivial/blob/master/LICENSE'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/keithbabinec/Archivial'

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        ReleaseNotes = 'Archivial Cloud Backup is still under alpha release. Breaking changes may occur without notice. See project homepage for more information on feature availability.'

    } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}

