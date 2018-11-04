﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using OzetteLibrary.Exceptions;
using OzetteLibrary.Folders;
using System;
using System.Linq;

namespace OzetteLibraryTests.Folders
{
    [TestClass]
    public class SourceLocationTests
    {
        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidFolderPathException))]
        public void SourceLocationValidateThrowsExceptionWhenInvalidFolderPathIsProvided()
        {
            var loc = new LocalSourceLocation();
            loc.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidFolderPathException))]
        public void SourceLocationValidateThrowsExceptionWhenInvalidFolderPathIsProvided2()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory + "\\somefolderthatdoesntexist";
            loc.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidFileMatchFilterException))]
        public void SourceLocationValidateThrowsExceptionWhenInvalidFileMatchPatternIsProvided()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.FileMatchFilter = "aaaa";
            loc.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidFileMatchFilterException))]
        public void SourceLocationValidateThrowsExceptionWhenInvalidFileMatchPatternIsProvided2()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.FileMatchFilter = "test.mp3";
            loc.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidRevisionCountException))]
        public void SourceLocationValidateThrowsExceptionWhenInvalidRevisionCountProvided()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 0;
            loc.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidRevisionCountException))]
        public void SourceLocationValidateThrowsExceptionWhenInvalidRevisionCountProvided2()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = -15;
            loc.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidIDException))]
        public void SourceLocationValidateThrowsExceptionWhenInvalidIDProvided()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 0;
            loc.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidIDException))]
        public void SourceLocationValidateThrowsExceptionWhenInvalidIDProvided2()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = -10;
            loc.Validate();
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample1()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample2()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 10;
            loc.ID = 1;
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample3()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 12345678;
            loc.ID = 1;
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample4()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample5()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "*";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample6()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "*.*";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample7()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "test*";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample8()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "test*.doc";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample9()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "*.doc";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample10()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "test.*";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample11()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "t?st";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample12()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "t?st.doc";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample13()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "t?st.*";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample14()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.FileMatchFilter = "t?st.do?";
            loc.ID = 1;
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationValidatePassesValidExample15()
        {
            var loc = new LocalSourceLocation();
            loc.FolderPath = Environment.CurrentDirectory;
            loc.RevisionCount = 1;
            loc.ID = 1;
            loc.FileMatchFilter = "*.d?";
            loc.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationShouldScanExample1()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = null;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.LowPriorityScanFrequencyInHours = 72;
            options.MedPriorityScanFrequencyInHours = 24;
            options.HighPriorityScanFrequencyInHours = 1;

            Assert.IsTrue(loc.ShouldScan(options));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SourceLocationShouldScanExample2()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = null;

            loc.ShouldScan(null);
        }

        [TestMethod]
        public void SourceLocationShouldScanExample3()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddMinutes(-15);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.High;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.HighPriorityScanFrequencyInHours = 1;

            Assert.IsFalse(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample4()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddMinutes(-59);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.High;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.HighPriorityScanFrequencyInHours = 1;

            Assert.IsFalse(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample5()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddMinutes(-61);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.High;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.HighPriorityScanFrequencyInHours = 1;

            Assert.IsTrue(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample6()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddMinutes(-125);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.High;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.HighPriorityScanFrequencyInHours = 1;

            Assert.IsTrue(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample7()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddHours(-15);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.Medium;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.MedPriorityScanFrequencyInHours = 24;

            Assert.IsFalse(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample8()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddHours(-23);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.Medium;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.MedPriorityScanFrequencyInHours = 24;

            Assert.IsFalse(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample9()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddHours(-25);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.Medium;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.MedPriorityScanFrequencyInHours = 24;

            Assert.IsTrue(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample10()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddHours(-125);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.Medium;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.MedPriorityScanFrequencyInHours = 24;

            Assert.IsTrue(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample11()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddHours(-15);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.Low;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.LowPriorityScanFrequencyInHours = 72;

            Assert.IsFalse(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample12()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddHours(-71);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.Low;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.LowPriorityScanFrequencyInHours = 72;

            Assert.IsFalse(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample13()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddHours(-73);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.Low;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.LowPriorityScanFrequencyInHours = 72;

            Assert.IsTrue(loc.ShouldScan(options));
        }

        [TestMethod]
        public void SourceLocationShouldScanExample14()
        {
            var loc = new LocalSourceLocation();
            loc.LastCompletedScan = DateTime.Now.AddHours(-1250);
            loc.Priority = OzetteLibrary.Files.FileBackupPriority.Low;

            var options = new OzetteLibrary.Folders.ScanFrequencies();
            options.LowPriorityScanFrequencyInHours = 72;

            Assert.IsTrue(loc.ShouldScan(options));
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationsDuplicateIDException))]
        public void SourceLocationsValidateThrowsExceptionOnDuplicateIDs()
        {
            var loc1 = new LocalSourceLocation();
            loc1.FolderPath = Environment.CurrentDirectory;
            loc1.RevisionCount = 1;
            loc1.ID = 1;
            loc1.FileMatchFilter = "*";

            var loc2 = new LocalSourceLocation();
            loc2.FolderPath = Environment.CurrentDirectory;
            loc2.RevisionCount = 1;
            loc2.ID = 2;
            loc2.FileMatchFilter = "*";

            var loc3 = new LocalSourceLocation();
            loc3.FolderPath = Environment.CurrentDirectory;
            loc3.RevisionCount = 1;
            loc3.ID = 2;
            loc3.FileMatchFilter = "*";

            var locations = new OzetteLibrary.Folders.SourceLocations();
            locations.Add(loc1);
            locations.Add(loc2);
            locations.Add(loc3);

            // should throw
            locations.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidRevisionCountException))]
        public void SourceLocationsValidateCallsValidateOnSourcesInsideCollection()
        {
            var loc1 = new LocalSourceLocation();
            loc1.FolderPath = Environment.CurrentDirectory;
            loc1.RevisionCount = 1;
            loc1.ID = 1;
            loc1.FileMatchFilter = "*";

            var loc2 = new LocalSourceLocation();
            loc2.FolderPath = Environment.CurrentDirectory;
            loc2.RevisionCount = 1;
            loc2.ID = 2;
            loc2.FileMatchFilter = "*";

            var loc3 = new LocalSourceLocation();
            loc3.FolderPath = Environment.CurrentDirectory;
            loc3.RevisionCount = 0; // this should cause a validation error
            loc3.ID = 3;
            loc3.FileMatchFilter = "*";

            var locations = new OzetteLibrary.Folders.SourceLocations();
            locations.Add(loc1);
            locations.Add(loc2);
            locations.Add(loc3);

            // should throw
            locations.Validate();
        }

        [TestMethod]
        public void SourceLocationsValidateDoesNotThrowOnAllValidSources()
        {
            var loc1 = new LocalSourceLocation();
            loc1.FolderPath = Environment.CurrentDirectory;
            loc1.RevisionCount = 1;
            loc1.ID = 1;
            loc1.FileMatchFilter = "*";

            var loc2 = new LocalSourceLocation();
            loc2.FolderPath = Environment.CurrentDirectory;
            loc2.RevisionCount = 1;
            loc2.ID = 2;
            loc2.FileMatchFilter = "*";

            var loc3 = new LocalSourceLocation();
            loc3.FolderPath = Environment.CurrentDirectory;
            loc3.RevisionCount = 1;
            loc3.ID = 3;
            loc3.FileMatchFilter = "*";

            var locations = new OzetteLibrary.Folders.SourceLocations();
            locations.Add(loc1);
            locations.Add(loc2);
            locations.Add(loc3);

            // should throw
            locations.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SourceLocationsValidateDoesNotThrowOnSingleValidSource()
        {
            var loc1 = new LocalSourceLocation();
            loc1.FolderPath = Environment.CurrentDirectory;
            loc1.RevisionCount = 1;
            loc1.ID = 1;
            loc1.FileMatchFilter = "*";

            var locations = new OzetteLibrary.Folders.SourceLocations();
            locations.Add(loc1);

            // should throw
            locations.Validate();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(SourceLocationInvalidIDException))]
        public void SourceLocationsValidateDoesThrowOnSingleInvalidSource()
        {
            var loc1 = new LocalSourceLocation();
            loc1.FolderPath = Environment.CurrentDirectory;
            loc1.RevisionCount = 1;
            loc1.ID = 0;
            loc1.FileMatchFilter = "*";

            var locations = new OzetteLibrary.Folders.SourceLocations();
            locations.Add(loc1);

            // should throw
            locations.Validate();

            Assert.IsTrue(true);
        }
    }
}
