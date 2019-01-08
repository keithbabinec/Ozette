﻿using OzetteLibrary.Database;
using OzetteLibrary.Events;
using OzetteLibrary.Logging;
using OzetteLibrary.MessagingProviders;
using OzetteLibrary.StorageProviders;
using System;
using System.Collections.Generic;
using System.Threading;

namespace OzetteLibrary.Engine
{
    /// <summary>
    /// An abstract base class for deriving custom Engines.
    /// </summary>
    /// <remarks>
    /// The base engine contains implemented functionality common to all engines, such as logging, synchronization, and events.
    /// </remarks>
    public abstract class BaseEngine
    {
        /// <summary>
        /// Constructor that accepts a database and logger.
        /// </summary>
        /// <param name="database">The client database connection.</param>
        /// <param name="logger">A logging instance.</param>
        /// <param name="storageProviders">A collection of cloud backup storage provider connections.</param>
        /// <param name="messagingProviders">A collection of messaging provider connections.</param>
        protected BaseEngine(IClientDatabase database, ILogger logger, StorageProviderConnectionsCollection storageProviders, MessagingProviderConnectionsCollection messagingProviders)
        {
            // note: its ok to have no messaging providers (zero count).
            // it is not ok to have zero backup providers.

            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (storageProviders == null)
            {
                throw new ArgumentNullException(nameof(storageProviders));
            }
            if (storageProviders.Count == 0)
            {
                throw new ArgumentException(nameof(storageProviders) + " must be provided.");
            }
            if (messagingProviders == null)
            {
                throw new ArgumentNullException(nameof(messagingProviders));
            }

            Database = database;
            Logger = logger;
            StorageProviders = storageProviders;
            MessagingProviders = messagingProviders;
        }

        /// <summary>
        /// This event is triggered when the engine has been stopped.
        /// </summary>
        public event EventHandler<EngineStoppedEventArgs> Stopped;

        /// <summary>
        /// Begins to start the engine, returns immediately to the caller.
        /// </summary>
        abstract public void BeginStart();

        /// <summary>
        /// Begins to stop the engine, returns immediately to the caller.
        /// </summary>
        abstract public void BeginStop();

        /// <summary>
        /// Internal function to invoke the Stopped event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStopped(EngineStoppedEventArgs e)
        {
            Stopped?.Invoke(this, e);
        }

        /// <summary>
        /// A flag to indicate if the engine is running.
        /// </summary>
        protected volatile bool Running = false;

        /// <summary>
        /// A reference to the logger.
        /// </summary>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// A reference to the database.
        /// </summary>
        protected IClientDatabase Database { get; set; }

        /// <summary>
        /// A collection of cloud storage providers.
        /// </summary>
        protected StorageProviderConnectionsCollection StorageProviders { get; set; }

        /// <summary>
        /// A collection of messaging providers.
        /// </summary>
        protected MessagingProviderConnectionsCollection MessagingProviders { get; set; }

        /// <summary>
        /// Sleeps the engine for the specified time, while checking periodically for stop request.
        /// </summary>
        /// <param name="SleepTime"></param>
        protected void ThreadSleepWithStopRequestCheck(TimeSpan SleepTime)
        {
            DateTime sleepUntil = DateTime.Now.Add(SleepTime);

            while (Running)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));

                if (DateTime.Now > sleepUntil)
                {
                    break;
                }
            }
        }
    }
}
