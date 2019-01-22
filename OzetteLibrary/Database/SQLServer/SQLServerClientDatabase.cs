﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac;
using OzetteLibrary.Files;
using OzetteLibrary.Folders;
using OzetteLibrary.Logging;
using OzetteLibrary.Providers;
using OzetteLibrary.Secrets;
using OzetteLibrary.ServiceCore;
using OzetteLibrary.StorageProviders;

namespace OzetteLibrary.Database.SQLServer
{
    /// <summary>
    /// A SQL Server implementation of the client database. 
    /// </summary>
    public class SQLServerClientDatabase : IClientDatabase
    {
        /// <summary>
        /// Instantiates a client DB from database connection string.
        /// </summary>
        /// <remarks>Instance of the logger.</remarks>
        /// <param name="connectionString"></param>
        public SQLServerClientDatabase(string connectionString, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }
            if (logger == null)
            {
                throw new ArgumentException(nameof(logger));
            }
            
            DatabaseConnectionString = connectionString;
            Logger = logger;
        }

        /// <summary>
        /// The database connection string.
        /// </summary>
        /// <remarks>
        /// A memory stream or database file is used, but not both.
        /// </remarks>
        private string DatabaseConnectionString;

        /// <summary>
        /// An instance of the logger.
        /// </summary>
        private ILogger Logger;

        /// <summary>
        /// Prepares the database.
        /// </summary>
        /// <returns></returns>
        public async Task PrepareDatabaseAsync()
        {
            Logger.WriteTraceMessage("Attempting to connect to the database engine.");
            Logger.WriteTraceMessage("Instance: " + Constants.Database.DefaultSqlExpressInstanceConnectionString);

            using (SqlConnection sqlcon = new SqlConnection(Constants.Database.DefaultSqlExpressInstanceConnectionString))
            {
                await sqlcon.OpenAsync();

                Logger.WriteTraceMessage("Successfully connected to the database engine.");
                Logger.WriteTraceMessage(string.Format("Checking if database ({0}) is present.", Constants.Database.DatabaseName));

                bool databasePresent = false;

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = sqlcon;
                    cmd.CommandText = string.Format("SELECT 1 FROM sys.databases WHERE [Name] = '{0}'", Constants.Database.DatabaseName);
                    cmd.CommandType = System.Data.CommandType.Text;

                    using (var rdr = await cmd.ExecuteReaderAsync())
                    {
                        if (rdr.HasRows)
                        {
                            databasePresent = true;
                            Logger.WriteTraceMessage("Database was found.");
                        }
                        else
                        {
                            Logger.WriteTraceMessage("Database was not found.");
                        }
                    }
                }

                // create the database, if missing.

                if (databasePresent == false)
                {
                    Logger.WriteTraceMessage("Attempting to create the database.");

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;

                        var fileName = string.Format("{0}\\{1}.mdf", CoreSettings.DatabaseDirectory, Constants.Database.DatabaseName);

                        Logger.WriteTraceMessage("Database File: " + fileName);

                        var createDbCommand = string.Format(
                            "CREATE DATABASE {0} ON ( NAME='{0}', FILENAME='{1}' )", Constants.Database.DatabaseName, fileName);

                        cmd.CommandText = createDbCommand;
                        cmd.CommandType = System.Data.CommandType.Text;

                        await cmd.ExecuteNonQueryAsync();

                        Logger.WriteTraceMessage("Database was created successfully.");
                    }
                }
            }

            // publish the database package (.dacpac)
            // but only if we need to-- according to the publish flag.

            if (CoreSettings.DatabasePublishIsRequired)
            {
                Logger.WriteTraceMessage("Database publish is required, starting publish now.");

                var packagePath = string.Format("{0}\\{1}.dacpac", CoreSettings.InstallationDirectory, Constants.Database.DatabaseName);

                Logger.WriteTraceMessage("Database package file: " + packagePath);

                using (DacPackage package = DacPackage.Load(packagePath, DacSchemaModelStorageType.Memory))
                {
                    DacServices services = new DacServices(Constants.Database.DefaultSqlExpressInstanceConnectionString);
                    services.Message += dacMessages_Received;
                    services.Deploy(package, Constants.Database.DatabaseName, true);

                    CoreSettings.DatabasePublishIsRequired = false;
                    Logger.WriteTraceMessage("Database publish completed.");
                }
            }
            else
            {
                Logger.WriteTraceMessage("Database publish is not required.");
            }
        }

        /// <summary>
        /// An event handler for receiving DAC services messages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dacMessages_Received(object sender, DacMessageEventArgs e)
        {
            Logger.WriteTraceMessage(string.Format("[DATABASE]: {0}", e.Message.ToString()));
        }

        /// <summary>
        /// Saves an application setting to the database.
        /// </summary>
        /// <param name="OptionName">Option name</param>
        /// <param name="OptionValue">Option value</param>
        public async Task SetApplicationOptionAsync(string OptionName, string OptionValue)
        {
            if (string.IsNullOrWhiteSpace(OptionName))
            {
                throw new ArgumentException(nameof(OptionName) + " must be provided.");
            }
            if (string.IsNullOrWhiteSpace(OptionValue))
            {
                throw new ArgumentException(nameof(OptionValue) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.SetApplicationOption";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Name", OptionName);
                        cmd.Parameters.AddWithValue("@Value", OptionValue);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves an application setting value from the database.
        /// </summary>
        /// <remarks>
        /// Returns null if the setting is not found.
        /// </remarks>
        /// <param name="OptionName">Option name</param>
        /// <returns>The setting value.</returns>
        public async Task<string> GetApplicationOptionAsync(string OptionName)
        {
            if (string.IsNullOrWhiteSpace(OptionName))
            {
                throw new ArgumentException(nameof(OptionName) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.GetApplicationOption";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Name", OptionName);

                        var value = await cmd.ExecuteScalarAsync();

                        if (value != null)
                        {
                            return value.ToString();
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Removes an application setting value from the database.
        /// </summary>
        /// <param name="OptionName">Option name</param>
        public async Task RemoveApplicationOptionAsync(string OptionName)
        {
            if (string.IsNullOrWhiteSpace(OptionName))
            {
                throw new ArgumentException(nameof(OptionName) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.RemoveApplicationOption";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Name", OptionName);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a list of all providers defined in the database for the specified type.
        /// </summary>
        /// <param name="Type">The type of providers to return.</param>
        /// <returns><c>ProviderCollection</c></returns>
        public async Task<ProviderCollection> GetProvidersAsync(ProviderTypes Type)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.GetProviders";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Type", Type);

                        var result = new ProviderCollection();

                        using (var rdr = await cmd.ExecuteReaderAsync())
                        {
                            if (rdr.HasRows)
                            {
                                while (await rdr.ReadAsync())
                                {
                                    result.Add(new Provider()
                                    {
                                        ID = rdr.GetInt32(0),
                                        Name = rdr.GetString(1),
                                        Type = (ProviderTypes)rdr.GetInt32(2)
                                    });
                                }
                            }
                        }

                        return result;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Removes the specified provider by name.
        /// </summary>
        /// <param name="ProviderName">Provider name.</param>
        public async Task RemoveProviderAsync(string ProviderName)
        {
            if (string.IsNullOrWhiteSpace(ProviderName))
            {
                throw new ArgumentException(nameof(ProviderName) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.RemoveProvider";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Name", ProviderName);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Adds the specified Provider object to the database.
        /// </summary>
        /// <param name="Provider"></param>
        public async Task AddProviderAsync(Provider Provider)
        {
            if (Provider == null)
            {
                throw new ArgumentNullException(nameof(Provider));
            }
            if (string.IsNullOrWhiteSpace(Provider.Name))
            {
                throw new ArgumentException(nameof(Provider.Name) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.AddProvider";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Name", Provider.Name);
                        cmd.Parameters.AddWithValue("@Type", Provider.Type);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Adds a net credential to the database.
        /// </summary>
        /// <param name="Credential"></param>
        /// <returns></returns>
        public async Task AddNetCredentialAsync(NetCredential Credential)
        {
            if (Credential == null)
            {
                throw new ArgumentNullException(nameof(Credential));
            }
            if (string.IsNullOrWhiteSpace(Credential.CredentialName))
            {
                throw new ArgumentException(nameof(Credential.CredentialName) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.AddNetCredential";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Name", Credential.CredentialName);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Removes the specified net credential by Name.
        /// </summary>
        /// <param name="CredentialName">Provider name.</param>
        public async Task RemoveNetCredentialAsync(string CredentialName)
        {
            if (string.IsNullOrWhiteSpace(CredentialName))
            {
                throw new ArgumentException(nameof(CredentialName) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.RemoveNetCredential";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Name", CredentialName);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Returns all of the net credentials defined in the database.
        /// </summary>
        /// <returns>A collection of net credentials.</returns>
        public async Task<NetCredentialsCollection> GetNetCredentialsAsync()
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.GetNetCredentials";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        var result = new NetCredentialsCollection();

                        using (var rdr = await cmd.ExecuteReaderAsync())
                        {
                            if (rdr.HasRows)
                            {
                                while (await rdr.ReadAsync())
                                {
                                    result.Add(new NetCredential()
                                    {
                                        ID = rdr.GetInt32(0),
                                        CredentialName = rdr.GetString(1),
                                    });
                                }
                            }
                        }

                        return result;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Checks the index for a file matching the provided name, path, filesize, and lastmodified date.
        /// </summary>
        /// <param name="FullFilePath">Full file path (file name and path)</param>
        /// <param name="FileSizeBytes">File size in bytes</param>
        /// <param name="FileLastModified">File last modified timestamp</param>
        /// <returns></returns>
        public async Task<BackupFileLookup> FindBackupFileAsync(string FullFilePath, long FileSizeBytes, DateTime FileLastModified)
        {
            if (string.IsNullOrWhiteSpace(FullFilePath))
            {
                throw new ArgumentException(nameof(FullFilePath) + " must be provided.");
            }
            if (FileSizeBytes <= 0)
            {
                throw new ArgumentException(nameof(FileSizeBytes) + " must be provided.");
            }
            if (FileLastModified == DateTime.MinValue)
            {
                throw new ArgumentException(nameof(FileLastModified) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.FindBackupFile";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@FullFilePath", FullFilePath);
                        cmd.Parameters.AddWithValue("@FileSizeBytes", FileSizeBytes);
                        cmd.Parameters.AddWithValue("@FileLastModified", FileLastModified);

                        using (var rdr = await cmd.ExecuteReaderAsync())
                        {
                            if (rdr.HasRows)
                            {
                                // first result set is the lookup result value (new, updated, or existing)

                                await rdr.ReadAsync();

                                var item = new BackupFileLookup()
                                {
                                    Result = (BackupFileLookupResult)rdr.GetInt32(0)
                                };

                                // second result set, if available, is the resulting found file.

                                if (await rdr.NextResultAsync())
                                {
                                    if (rdr.HasRows)
                                    {
                                        await rdr.ReadAsync();

                                        item.File = new BackupFile()
                                        {
                                            FileID = rdr.GetGuid(0),
                                            Filename = rdr.GetString(1),
                                            Directory = rdr.GetString(2),
                                            FullSourcePath = rdr.GetString(3),
                                            FileSizeBytes = rdr.GetInt64(4),
                                            LastModified = rdr.GetDateTime(5),
                                            TotalFileBlocks = rdr.GetInt32(6),
                                            FileHash = (byte[])rdr["FileHash"], // special handling for varbinary column
                                            FileHashString = rdr.GetString(8),
                                            Priority = (FileBackupPriority)rdr.GetInt32(9),
                                            FileRevisionNumber = rdr.GetInt32(10),
                                            HashAlgorithmType = rdr.GetString(11),
                                            LastChecked = rdr.GetDateTime(12),
                                            LastUpdated = rdr.GetDateTime(13),
                                            OverallState = (FileStatus)rdr.GetInt32(14)
                                        };
                                    }
                                }

                                return item;
                            }
                            else
                            {
                                throw new Exception("Failed to search for the backup file. No output was returned from the database.");
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returns the directory map item for the specified local directory.
        /// </summary>
        /// <remarks>
        /// A new directory map item will be created if none currently exists for the specified folder.
        /// </remarks>
        /// <param name="DirectoryPath">Local directory path. Ex: 'C:\bin\programs'</param>
        /// <returns><c>DirectoryMapItem</c></returns>
        public async Task<DirectoryMapItem> GetDirectoryMapItemAsync(string DirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(DirectoryPath))
            {
                throw new ArgumentException(nameof(DirectoryPath) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.GetDirectoryMapItem";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@DirectoryPath", DirectoryPath.ToLower());

                        using (var rdr = await cmd.ExecuteReaderAsync())
                        {
                            if (rdr.HasRows)
                            {
                                // should only be exactly one row.
                                await rdr.ReadAsync();

                                var item = new DirectoryMapItem()
                                {
                                    ID = rdr.GetGuid(0),
                                    LocalPath = rdr.GetString(1)
                                };

                                return item;
                            }
                            else
                            {
                                throw new Exception("Failed to generate a directory map item. No output was returned from the database.");
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returns all source locations defined in the database.
        /// </summary>
        /// <returns><c>SourceLocations</c></returns>
        public async Task<SourceLocations> GetSourceLocationsAsync()
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.GetSourceLocations";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        var result = new SourceLocations();

                        using (var rdr = await cmd.ExecuteReaderAsync())
                        {
                            // local sources

                            while (await rdr.ReadAsync())
                            {
                                result.Add(new LocalSourceLocation()
                                {
                                    ID = rdr.GetInt32(0),
                                    Path = rdr.GetString(1),
                                    FileMatchFilter = rdr.GetString(2),
                                    Priority = (FileBackupPriority)rdr.GetInt32(3),
                                    RevisionCount = rdr.GetInt32(4),
                                    LastCompletedScan = rdr.IsDBNull(5) ? (DateTime?)null : rdr.GetDateTime(5) 
                                });
                            }

                            // network sources

                            if (await rdr.NextResultAsync())
                            {
                                while (await rdr.ReadAsync())
                                {
                                    result.Add(new NetworkSourceLocation()
                                    {
                                        ID = rdr.GetInt32(0),
                                        Path = rdr.GetString(1),
                                        FileMatchFilter = rdr.GetString(2),
                                        Priority = (FileBackupPriority)rdr.GetInt32(3),
                                        RevisionCount = rdr.GetInt32(4),
                                        LastCompletedScan = rdr.IsDBNull(5) ? (DateTime?)null : rdr.GetDateTime(5),
                                        CredentialName = rdr.GetString(6),
                                        IsConnected = rdr.GetBoolean(7),
                                        IsFailed = rdr.GetBoolean(8),
                                        LastConnectionCheck = rdr.IsDBNull(9) ? (DateTime?)null : rdr.GetDateTime(9)
                                    });
                                }
                            }
                        }

                        return result;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Adds or updates a single source location.
        /// </summary>
        /// <param name="Location"><c>SourceLocation</c></param>
        public async Task SetSourceLocationAsync(SourceLocation Location)
        {
            if (Location == null)
            {
                throw new ArgumentNullException(nameof(Location));
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;

                        if (Location is LocalSourceLocation)
                        {
                            cmd.CommandText = "dbo.SetLocalSourceLocation";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            var lsl = Location as LocalSourceLocation;

                            cmd.Parameters.AddWithValue("@Path", lsl.Path);
                            cmd.Parameters.AddWithValue("@FileMatchFilter", lsl.FileMatchFilter);
                            cmd.Parameters.AddWithValue("@Priority", lsl.Priority);
                            cmd.Parameters.AddWithValue("@RevisionCount", lsl.RevisionCount);

                            if (lsl.LastCompletedScan == null)
                            {
                                cmd.Parameters.AddWithValue("@LastCompletedScan", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@LastCompletedScan", lsl.LastCompletedScan);
                            }
                        }
                        else if (Location is NetworkSourceLocation)
                        {
                            cmd.CommandText = "dbo.SetNetworkSourceLocation";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            var nsl = Location as NetworkSourceLocation;

                            cmd.Parameters.AddWithValue("@Path", nsl.Path);
                            cmd.Parameters.AddWithValue("@FileMatchFilter", nsl.FileMatchFilter);
                            cmd.Parameters.AddWithValue("@Priority", nsl.Priority);
                            cmd.Parameters.AddWithValue("@RevisionCount", nsl.RevisionCount);

                            if (nsl.LastCompletedScan == null)
                            {
                                cmd.Parameters.AddWithValue("@LastCompletedScan", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@LastCompletedScan", nsl.LastCompletedScan);
                            }

                            cmd.Parameters.AddWithValue("@CredentialName", nsl.CredentialName);
                            cmd.Parameters.AddWithValue("@IsConnected", nsl.IsConnected);
                            cmd.Parameters.AddWithValue("@IsFailed", nsl.IsFailed);

                            if (nsl.LastConnectionCheck == null)
                            {
                                cmd.Parameters.AddWithValue("@LastConnectionCheck", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@LastConnectionCheck", nsl.LastConnectionCheck);
                            }
                        }
                        else
                        {
                            throw new NotImplementedException("Unexpected source location type: " + Location.GetType().FullName);
                        }

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Removes a single source location.
        /// </summary>
        /// <param name="Location"></param>
        /// <returns></returns>
        public async Task RemoveSourceLocationAsync(SourceLocation Location)
        {
            if (Location == null)
            {
                throw new ArgumentNullException(nameof(Location));
            }
            if (Location.ID <= 0)
            {
                throw new ArgumentException(nameof(Location.ID) + " must be provided.");
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;

                        if (Location is LocalSourceLocation)
                        {
                            cmd.CommandText = "dbo.RemoveLocalSourceLocation";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ID", Location.ID);
                        }
                        else if (Location is NetworkSourceLocation)
                        {
                            cmd.CommandText = "dbo.RemoveNetworkSourceLocation";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ID", Location.ID);
                        }
                        else
                        {
                            throw new NotImplementedException("Unexpected source location type: " + Location.GetType().FullName);
                        }

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the next file that needs to be backed up.
        /// </summary>
        /// <remarks>
        /// If no files need to be backed up, return null.
        /// </remarks>
        /// <param name="EngineInstanceID">The engine instance.</param>
        /// <returns><c>BackupFile</c></returns>
        public async Task<BackupFile> FindNextFileToBackupAsync(int EngineInstanceID)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.FindNextFileToBackup";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@EngineInstanceID", EngineInstanceID);

                        using (var rdr = await cmd.ExecuteReaderAsync())
                        {
                            if (rdr.HasRows)
                            {
                                await rdr.ReadAsync();

                                var file = new BackupFile()
                                {
                                    FileID = rdr.GetGuid(0),
                                    Filename = rdr.GetString(1),
                                    Directory = rdr.GetString(2),
                                    FullSourcePath = rdr.GetString(3),
                                    FileSizeBytes = rdr.GetInt64(4),
                                    LastModified = rdr.GetDateTime(5),
                                    TotalFileBlocks = rdr.GetInt32(6),
                                    FileHash = (byte[])rdr["FileHash"], // special handling for varbinary column
                                    FileHashString = rdr.GetString(8),
                                    Priority = (FileBackupPriority)rdr.GetInt32(9),
                                    FileRevisionNumber = rdr.GetInt32(10),
                                    HashAlgorithmType = rdr.GetString(11),
                                    LastChecked = rdr.GetDateTime(12),
                                    LastUpdated = rdr.GetDateTime(13),
                                    OverallState = (FileStatus)rdr.GetInt32(14),
                                    CopyState = new Dictionary<StorageProviderTypes, StorageProviderFileStatus>()
                                };

                                if (await rdr.NextResultAsync())
                                {
                                    while (await rdr.ReadAsync())
                                    {
                                        file.CopyState.Add(
                                            (StorageProviderTypes)rdr.GetInt32(0), 
                                            new StorageProviderFileStatus()
                                            {
                                                Provider = (StorageProviderTypes)rdr.GetInt32(0),
                                                SyncStatus = (FileStatus)rdr.GetInt32(1),
                                                HydrationStatus = (StorageProviderHydrationStatus)rdr.GetInt32(2),
                                                LastCompletedFileBlockIndex = rdr.GetInt32(3)
                                            });
                                    }
                                }
                                else
                                {
                                    throw new Exception("Failed to lookup next backup file. No copystate output was returned from the database.");
                                }

                                return file;
                            }
                            else
                            {
                                // no backup file is a valid output.
                                // this means all files are backed up (or in a failed state), so there is nothing to transfer.
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Calculates and returns the overall backup progress.
        /// </summary>
        /// <returns></returns>
        public async Task<BackupProgress> GetBackupProgressAsync()
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.GetBackupProgress";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        using (var rdr = await cmd.ExecuteReaderAsync())
                        {
                            if (rdr.HasRows)
                            {
                                // should only be exactly one row.
                                await rdr.ReadAsync();

                                var item = new BackupProgress()
                                {
                                    TotalFileCount = rdr.GetInt64(0),
                                    TotalFileSizeBytes = rdr.GetInt64(1),
                                    BackedUpFileCount = rdr.GetInt64(2),
                                    BackedUpFileSizeBytes = rdr.GetInt64(3),
                                    RemainingFileCount = rdr.GetInt64(4),
                                    RemainingFileSizeBytes = rdr.GetInt64(5),
                                    FailedFileCount = rdr.GetInt64(6),
                                    FailedFileSizeBytes = rdr.GetInt64(7)
                                };

                                // set the overall completion rate
                                if (item.TotalFileSizeBytes != 0)
                                {
                                    item.OverallPercentage = ((double)item.BackedUpFileSizeBytes / item.TotalFileSizeBytes).ToString("P2", CultureInfo.CreateSpecificCulture("US"));
                                }
                                else
                                {
                                    item.OverallPercentage = "0.00 %";
                                }

                                return item;
                            }
                            else
                            {
                                throw new Exception("Failed to generate the backup progress. No output was returned from the database.");
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Adds a new file to the database.
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public async Task AddBackupFileAsync(BackupFile File)
        {
            if (File == null)
            {
                throw new ArgumentNullException(nameof(File));
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.AddBackupFile";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ID", File.FileID);
                        cmd.Parameters.AddWithValue("@FileName", File.Filename);
                        cmd.Parameters.AddWithValue("@Directory", File.Directory);
                        cmd.Parameters.AddWithValue("@FullSourcePath", File.FullSourcePath);
                        cmd.Parameters.AddWithValue("@FileSizeBytes", File.FileSizeBytes);
                        cmd.Parameters.AddWithValue("@LastModified", File.LastModified);
                        cmd.Parameters.AddWithValue("@TotalFileBlocks", File.TotalFileBlocks);
                        cmd.Parameters.AddWithValue("@Priority", File.Priority);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    foreach (var provider in await GetProvidersAsync(ProviderTypes.Storage))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = sqlcon;
                            cmd.CommandText = "dbo.SetCopyState";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@FileID", File.FileID);
                            cmd.Parameters.AddWithValue("@StorageProvider", provider.Type);
                            cmd.Parameters.AddWithValue("@SyncStatus", FileStatus.Unsynced);
                            cmd.Parameters.AddWithValue("@HydrationStatus", StorageProviderHydrationStatus.None);
                            cmd.Parameters.AddWithValue("@LastCompletedFileBlockIndex", -1);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Resets the copy state of a backup file back to unsynced.
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public async Task ResetBackupFileStateAsync(BackupFile File)
        {
            if (File == null)
            {
                throw new ArgumentNullException(nameof(File));
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.ResetBackupFile";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ID", File.FileID);
                        cmd.Parameters.AddWithValue("@FileSizeBytes", File.FileSizeBytes);
                        cmd.Parameters.AddWithValue("@LastModified", File.LastModified);
                        cmd.Parameters.AddWithValue("@TotalFileBlocks", File.TotalFileBlocks);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    foreach (var provider in await GetProvidersAsync(ProviderTypes.Storage))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = sqlcon;
                            cmd.CommandText = "dbo.SetCopyState";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@FileID", File.FileID);
                            cmd.Parameters.AddWithValue("@StorageProvider", provider.Type);
                            cmd.Parameters.AddWithValue("@SyncStatus", FileStatus.Unsynced);
                            cmd.Parameters.AddWithValue("@HydrationStatus", StorageProviderHydrationStatus.None);
                            cmd.Parameters.AddWithValue("@LastCompletedFileBlockIndex", -1);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Resets the last scanned date of a backup file.
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public async Task SetBackupFileLastScannedAsync(Guid FileID)
        {
            if (FileID == Guid.Empty)
            {
                throw new ArgumentException(nameof(FileID));
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.SetBackupFileLastScanned";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ID", FileID);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes the backup file from the backup files table/index.
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public async Task DeleteBackupFileAsync(BackupFile File)
        {
            if (File == null)
            {
                throw new ArgumentException(nameof(File));
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.DeleteBackupFile";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ID", File.FileID);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Flags a backup file as failed state.
        /// </summary>
        /// <param name="File"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public async Task SetBackupFileAsFailedAsync(BackupFile File, string Message)
        {
            if (File == null)
            {
                throw new ArgumentException(nameof(File));
            }
            if (string.IsNullOrEmpty(Message))
            {
                throw new ArgumentException(nameof(Message));
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.SetBackupFileAsFailed";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ID", File.FileID);
                        cmd.Parameters.AddWithValue("@Message", Message);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Updates the backup file hash.
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public async Task SetBackupFileHashAsync(BackupFile File)
        {
            if (File == null)
            {
                throw new ArgumentException(nameof(File));
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.SetBackupFileHash";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ID", File.FileID);
                        cmd.Parameters.AddWithValue("@FileHash", File.FileHash);
                        cmd.Parameters.AddWithValue("@FileHashString", File.FileHashString);
                        cmd.Parameters.AddWithValue("@HashAlgorithm", File.HashAlgorithmType);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Updates the backup files copy state.
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public async Task UpdateBackupFileCopyStateAsync(BackupFile File)
        {
            if (File == null)
            {
                throw new ArgumentException(nameof(File));
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DatabaseConnectionString))
                {
                    await sqlcon.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = sqlcon;
                        cmd.CommandText = "dbo.SetBackupFileOverallState";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ID", File.FileID);
                        cmd.Parameters.AddWithValue("@OverallState", File.OverallState);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    if (File.CopyState != null)
                    {
                        foreach (var provider in File.CopyState)
                        {
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = sqlcon;
                                cmd.CommandText = "dbo.SetCopyState";
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                                cmd.Parameters.AddWithValue("@FileID", File.FileID);
                                cmd.Parameters.AddWithValue("@StorageProvider", provider.Key);
                                cmd.Parameters.AddWithValue("@SyncStatus", provider.Value.SyncStatus);
                                cmd.Parameters.AddWithValue("@HydrationStatus", provider.Value.HydrationStatus);
                                cmd.Parameters.AddWithValue("@LastCompletedFileBlockIndex", provider.Value.LastCompletedFileBlockIndex);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
