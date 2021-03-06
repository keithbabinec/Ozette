---
external help file: ArchivialPowerShell.dll-Help.xml
Module Name: ArchivialPowerShell
online version:
schema: 2.0.0
---

# Set-ArchivialCloudBackupOptions

## SYNOPSIS
Sets one or more application options for Archivial Cloud Backup.

## SYNTAX

```
Set-ArchivialCloudBackupOptions [-LogFilesRetentionInDays <Int32>] [-DatabaseBackupsRetentionInDays <Int32>]
 [-BackupEngineInstancesCount <Int32>] [-BackupEngineStartupDelayInSeconds <Int32>]
 [-LowPriorityScanFrequencyInHours <Int32>] [-MedPriorityScanFrequencyInHours <Int32>]
 [-HighPriorityScanFrequencyInHours <Int32>] [-ProtectionIV <String>] [-StatusUpdateSchedule <String>]
 [-MasterExclusionMatches <String[]>] [<CommonParameters>]
```

## DESCRIPTION
Specify at least one option to change.
To see existing options, run Get-ArchivialCloudBackupOptions

## EXAMPLES

### EXAMPLE 1
```
C:\> Set-ArchivialCloudBackupOptions -LogFilesRetentionInDays 60
```

Sets the local log file retention to 60 days.

### EXAMPLE 2
```
C:\> Set-ArchivialCloudBackupOptions -LowPriorityScanFrequencyInHours 4 -MedPriorityScanFrequencyInHours 2
```

Sets low priority and medium priority scanning frequencies.

### EXAMPLE 3
```
C:\> Set-ArchivialCloudBackupOptions -StatusUpdateSchedule "0 8 * * *"
```

Sets the status update notification schedule to once per day at 8am.

### EXAMPLE 4
```
C:\> Set-ArchivialCloudBackupOptions -MasterExclusionMatches "^._",".DS_Store"
```

Sets the master exclusions to exlude Mac OS related file system metadata files.

## PARAMETERS

### -LogFilesRetentionInDays
Optionally specify the length of time (in days) that locally stored log files should be retained.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -DatabaseBackupsRetentionInDays
Optionally specify the length of time (in days) that locally stored database backups should be retrained.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -BackupEngineInstancesCount
Optionally specify the number of concurrent backup engine instances to run.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -BackupEngineStartupDelayInSeconds
Optionally specify the time delay in seconds between the startup of each backup engine instance.
This helps avoid sudden, excessive filesystem load by staggering the engine startups.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -LowPriorityScanFrequencyInHours
Optionally specify how often (in hours) that low-priority folder scans should be performed.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -MedPriorityScanFrequencyInHours
Optionally specify how often (in hours) that medium-priority folder scans should be performed.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -HighPriorityScanFrequencyInHours
Optionally specify how often (in hours) that high-priority folder scans should be performed.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProtectionIV
Optionally specify the local encryption initialization vector key.
Key should be a crypto-random 16-byte array, expressed as a base64 encoded string.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StatusUpdateSchedule
Optionally specify the schedule for when status notification messages should be sent.
Expressed as a cron schedule.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MasterExclusionMatches
Optionally specify a set of regex match exclusions that should apply to all scanned folders.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS
