﻿using System;
using System.Collections.Concurrent;

namespace OzetteLibrary.Models
{
    /// <summary>
    /// Describes a single file to be backed up.
    /// </summary>
    /// <remarks>
    /// Contains extra properties that only apply for files sitting in the client side.
    /// </remarks>
    public class ClientFile : BackupFile
    {
        /// <summary>
        /// The last time this file was scanned in the backup source.
        /// </summary>
        public DateTime? LastChecked { get; set; }

        /// <summary>
        /// The state of this file across one or more targets.
        /// </summary>
        /// <remarks>
        /// The dictionary key is the target ID.
        /// The dictionary value is the copy state.
        /// </remarks>
        public ConcurrentDictionary<int, TargetCopyState> CopyState { get; set; }
    }
}