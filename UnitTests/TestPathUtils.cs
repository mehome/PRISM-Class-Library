﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PRISM;

namespace PRISMTest
{
    [TestFixture]
    class TestPathUtils
    {

        [Test]
        [TestCase(@"/proc/12343/stat", @"/proc/12343/stat")]
        [TestCase(@"/proc/subdir\filename", @"/proc/subdir/filename")]
        [TestCase(@"/proc\subdir\filename.txt", @"/proc/subdir/filename.txt")]
        public void TestAssureLinuxPath(string pathSpec, string expectedResult)
        {
            var result = clsPathUtils.AssureLinuxPath(pathSpec);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(@"C:\DMS_WorkDir/12343/stat", @"C:\DMS_WorkDir\12343\stat")]
        [TestCase(@"C:\DMS_WorkDir/subdir\filename", @"C:\DMS_WorkDir\subdir\filename")]
        [TestCase(@"C:\DMS_WorkDir/subdir\filename.txt", @"C:\DMS_WorkDir\subdir\filename.txt")]
        [TestCase(@"C:\DMS_WorkDir\subdir\filename.txt", @"C:\DMS_WorkDir\subdir\filename.txt")]
        [TestCase(@"C:\DMS_WorkDir/subdir/filename.txt", @"C:\DMS_WorkDir\subdir\filename.txt")]
        [TestCase(@"C:\DMS_WorkDir\subdir/filename.txt", @"C:\DMS_WorkDir\subdir\filename.txt")]
        public void TestAssureWindowsPath(string pathSpec, string expectedResult)
        {
            var result = clsPathUtils.AssureWindowsPath(pathSpec);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(@"/proc/12343", "stat", @"/proc/12343/stat")]
        [TestCase(@"/proc/12343/", "stat", @"/proc/12343/stat")]
        [TestCase(@"/proc/12343", "/stat", @"/stat")]
        [TestCase(@"/proc/12343/", "/stat/", @"/stat/")]
        [TestCase(@"/share/item", "dataset/results", @"/share/item/dataset/results")]
        [TestCase(@"/share/item/", "dataset/results", @"/share/item/dataset/results")]
        [TestCase(@"/share/item", "/dataset/results", @"/dataset/results")]
        [TestCase(@"/share/item/", "/dataset/results/", @"/dataset/results/")]
        public void TestCombineLinuxPaths(string path1, string path2, string expectedResult)
        {
            var result = clsPathUtils.CombineLinuxPaths(path1, path2);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(@"C:\DMS_WorkDir", "subdir", @"C:\DMS_WorkDir\subdir")]
        [TestCase(@"C:\DMS_WorkDir\", "subdir", @"C:\DMS_WorkDir\subdir")]
        [TestCase(@"C:\DMS_WorkDir", @"subdir\filename.txt", @"C:\DMS_WorkDir\subdir\filename.txt")]
        [TestCase(@"C:\DMS_WorkDir\", @"subdir\filename.txt", @"C:\DMS_WorkDir\subdir\filename.txt")]
        public void TestCombineWindowsPaths(string path1, string path2, string expectedResult)
        {
            var result = clsPathUtils.CombineWindowsPaths(path1, path2);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(@"C:\DMS_WorkDir", "subdir", '\\', @"C:\DMS_WorkDir\subdir")]
        [TestCase(@"C:\DMS_WorkDir\", "subdir", '\\', @"C:\DMS_WorkDir\subdir")]
        [TestCase(@"C:\DMS_WorkDir", @"subdir\filename.txt", '\\', @"C:\DMS_WorkDir\subdir\filename.txt")]
        [TestCase(@"C:\DMS_WorkDir\", @"subdir\filename.txt", '\\', @"C:\DMS_WorkDir\subdir\filename.txt")]
        [TestCase("/proc/12343", "stat", '/', "/proc/12343/stat")]
        [TestCase("/proc/12343/", "stat", '/', "/proc/12343/stat")]
        [TestCase("/proc/12343", "/stat", '/', "/stat")]
        [TestCase("/proc/12343/", "/stat/", '/', "/stat/")]
        [TestCase("/share/item", "dataset/results", '/', "/share/item/dataset/results")]
        [TestCase("/share/item/", "dataset/results", '/', "/share/item/dataset/results")]
        public void TestCombinePaths(string path1, string path2, char directorySepChar, string expectedResult)
        {
            var result = clsPathUtils.CombinePaths(path1, path2, directorySepChar);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(@"\\proto-2\UnitTest_Files\PRISM", "*.fasta", "HumanContam.fasta, MP_06_01.fasta, Tryp_Pig_Bov.fasta", false)]
        [Category("PNL_Domain")]
        public void TestFindFilesWildcardInternal(string folderPath, string fileMask, string expectedFileNames, bool recurse)
        {
            TestFindFilesWildcardWork(folderPath, fileMask, expectedFileNames, recurse);
        }

        [Test]
        [TestCase(@"c:\windows", "*.ini", "system.ini, win.ini", false)]
        [TestCase(@"c:\windows\", "*.ini", "system.ini, win.ini", false)]
        [TestCase(@"c:\windows", "*.ini", "system.ini, win.ini", true)]
        [TestCase(@"c:\windows\", "*.dll", "perfos.dll, perfnet.dll", true)]
        public void TestFindFilesWildcard(string folderPath, string fileMask, string expectedFileNames, bool recurse)
        {
            TestFindFilesWildcardWork(folderPath, fileMask, expectedFileNames, recurse);
        }

        [Test]
        [TestCase(@"LinuxTestFiles\Ubuntu\proc\cpuinfo", "*info", "cpuinfo, meminfo", 6)]
        public void TestFindFilesWildcardRelativeFolder(string filePath, string fileMask, string expectedFileNames, int expectedFileCount)
        {
            // Get the full path to the LinuxTestFiles folder, 3 levels up from the cpuinfo test file
            var cpuInfoFile = FileRefs.GetTestFile(filePath);

            var currentDirectory = cpuInfoFile.Directory;
            for (var parentCount = 1; parentCount < 3; parentCount++)
            {
                var parentCandidate = currentDirectory.Parent;
                if (parentCandidate == null)
                    Assert.Fail("Cannot determine the parent directory of " + currentDirectory.FullName);

                currentDirectory = parentCandidate;
            }

            TestFindFilesWildcardWork(currentDirectory.FullName, fileMask, expectedFileNames, true, expectedFileCount);
        }

        private void TestFindFilesWildcardWork(string folderPath, string fileMask, string expectedFileNames, bool recurse, int expectedFileCount = 0)
        {
            var folder = new DirectoryInfo(folderPath);

            // Combine the folder path and the file mask
            var pathSpec = Path.Combine(folder.FullName, fileMask);

            var files1 = clsPathUtils.FindFilesWildcard(pathSpec, recurse);

            // Separately, send the DirectoryInfo object plus the file mask
            var files2 = clsPathUtils.FindFilesWildcard(folder, fileMask, recurse);

            int allowedVariance;

            // The results should be the same, though the number of .ini files in the Windows directory can vary so we allow some variance
            if (folderPath.ToLower().Contains(@"\windows") || files1.Count > 1000)
                allowedVariance = (int)Math.Floor(files1.Count * 0.05);
            else
                allowedVariance = 0;

            Console.WriteLine("Files via pathSpec: {0}", files1.Count);
            Console.WriteLine("Files via fileMask: {0}", files2.Count);

            var fileCountDifference = Math.Abs(files1.Count - files2.Count);

            Assert.LessOrEqual(fileCountDifference, allowedVariance, "File count mismatch; {1} > {0}", fileCountDifference, allowedVariance);

            if (string.IsNullOrWhiteSpace(expectedFileNames))
                return;

            // Make sure we found files with the expected names
            var expectedFileList = expectedFileNames.Split(',');

            var foundFileNames = new SortedSet<string>();
            foreach (var foundFile in files1)
            {
                if (foundFileNames.Contains(foundFile.Name))
                    continue;

                foundFileNames.Add(foundFile.Name);
                if (foundFileNames.Count == 1)
                    Console.Write(foundFile.Name);
                else if (foundFileNames.Count <= 5)
                    Console.Write(", " + foundFile.Name);
                else if (foundFileNames.Count == 6)
                    Console.WriteLine(" ...");
            }

            Console.WriteLine();
            Console.WriteLine("Found {0} files (recurse={1})", files1.Count, recurse);

            foreach (var expectedFile in expectedFileList)
            {
                if (!foundFileNames.Contains(expectedFile.Trim()))
                    Assert.Fail("Did not an expected file in {0}: {1}", folderPath, expectedFile);
            }

            if (expectedFileCount > 0)
                Assert.GreaterOrEqual(files1.Count, expectedFileCount, "Found {0} files; expected to find {1}", files1.Count, expectedFileCount);

        }

        [Test]
        [TestCase("Results.txt", "*.txt", true)]
        [TestCase("Results.txt", "*.zip", false)]
        [TestCase("Results.txt", "*", true)]
        [TestCase("MSGFDB_PartTryp_MetOx_20ppmParTol.txt", "MSGF*", true)]
        [TestCase("MSGFDB_PartTryp_MetOx_20ppmParTol.txt", "XT*", false)]
        public void TestFitsMask(string fileName, string fileMask, bool expectedResult)
        {
            var result = clsPathUtils.FitsMask(fileName, fileMask);

            Assert.AreEqual(expectedResult, result);

            if (result)
                Console.WriteLine("{0} matches\n{1}", fileName, fileMask);
            else
                Console.WriteLine("{0} does not match\n{1}", fileName, fileMask);
        }

        [Test]
        [TestCase(@"C:\Users\Public\Pictures", @"C:\Users\Public", "Pictures")]
        [TestCase(@"C:\Users\Public\Pictures\", @"C:\Users\Public", "Pictures")]
        [TestCase(@"C:\Windows\System32", @"C:\Windows", "System32")]
        [TestCase(@"C:\Windows", @"C:\", "Windows")]
        [TestCase(@"C:\Windows\", @"C:\", "Windows")]
        [TestCase(@"C:\", @"", @"C:\")]
        [TestCase(@"C:\DMS_WorkDir", @"C:\", "DMS_WorkDir")]
        [TestCase(@"C:\DMS_WorkDir\", @"C:\", "DMS_WorkDir")]
        [TestCase(@"C:\DMS_WorkDir\SubDir", @"C:\DMS_WorkDir", "SubDir")]
        [TestCase(@"C:\DMS_WorkDir\SubDir\", @"C:\DMS_WorkDir", "SubDir")]
        [TestCase(@"Microsoft SQL Server\Client SDK\ODBC", @"Microsoft SQL Server\Client SDK", "ODBC")]
        [TestCase(@"TortoiseGit\bin", @"TortoiseGit", "bin")]
        [TestCase(@"TortoiseGit\bin\", @"TortoiseGit", "bin")]
        [TestCase(@"TortoiseGit", @"", "TortoiseGit")]
        [TestCase(@"TortoiseGit\", @"", "TortoiseGit")]
        [TestCase(@"\\server\Share\Folder", @"\\server\Share", "Folder")]
        [TestCase(@"\\server\Share\Folder\", @"\\server\Share", "Folder")]
        [TestCase(@"\\server\Share", @"", "Share")]
        [TestCase(@"\\server\Share\", @"", "Share")]
        [TestCase(@"/etc/fonts/conf.d", @"/etc/fonts", "conf.d")]
        [TestCase(@"/etc/fonts/conf.d/", @"/etc/fonts", "conf.d")]
        [TestCase(@"/etc/fonts", @"/etc", "fonts")]
        [TestCase(@"/etc/fonts/", @"/etc", "fonts")]
        [TestCase(@"/etc", @"/", "etc")]
        [TestCase(@"/etc/", @"/", "etc")]
        [TestCase(@"/", @"", "")]
        [TestCase(@"log/xymon", @"log", "xymon")]
        [TestCase(@"log/xymon/old", @"log/xymon", "old")]
        [TestCase(@"log", @"", "log")]
        public void TestGetParentDirectoryPath(string directoryPath, string expectedParentPath, string expectedDirectoryName)
        {
            var parentPath = clsPathUtils.GetParentDirectoryPath(directoryPath, out var directoryName);

            if (string.IsNullOrWhiteSpace(parentPath))
            {
                Console.WriteLine("{0} has no parent; name is {1}", directoryPath, directoryName);
            }
            else
            {
                Console.WriteLine("{0} has parent {1} and name {2}", directoryPath, parentPath, directoryName);
            }

            Assert.AreEqual(expectedParentPath, parentPath, "Parent path mismatch");
            Assert.AreEqual(expectedDirectoryName, directoryName, "Directory name mismatch");
        }

        [Test]
        [TestCase(@"C:\DMS_WorkDir\SubDir", false)]
        [TestCase(@"C:\DMS_WorkDir\Result Directory", true)]
        [TestCase(@"C:\DMS_WorkDir\Result Directory\", true)]
        [TestCase(@"C:\DMS_WorkDir\ResultDirectory\filename.txt", false)]
        [TestCase(@"C:\DMS_WorkDir\Result Directory\filename.txt", true)]
        [TestCase(@"/proc/12343", false)]
        [TestCase(@"/proc/Result Directory", true)]
        [TestCase(@"/proc/ResultDirectory/filename.txt", false)]
        [TestCase(@"/proc/Result Directory/filename.txt", true)]
        public void TestPossiblyQuotePath(string filePath, bool expectedQuoteRequired)
        {
            var quotedPath = clsPathUtils.PossiblyQuotePath(filePath);

            var pathWasQuoted = !string.Equals(filePath, quotedPath);

            Assert.AreEqual(expectedQuoteRequired, pathWasQuoted, "Mismatch for " + filePath);
        }

        [Test]
        [TestCase(@"C:\DMS_WorkDir\filename.txt", "UpdatedFile.txt", @"C:\DMS_WorkDir\UpdatedFile.txt")]
        [TestCase(@"C:\DMS_WorkDir\Results Directory\filename.txt", "UpdatedFile.txt", @"C:\DMS_WorkDir\Results Directory\UpdatedFile.txt")]
        public void TestReplaceFilenameInPath(string existingFilePath, string newFileName, string expectedResult)
        {
            var newPath = clsPathUtils.ReplaceFilenameInPath(existingFilePath, newFileName);
            Assert.AreEqual(expectedResult, newPath);
        }

    }
}
