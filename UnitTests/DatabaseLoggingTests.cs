﻿using System;
using NUnit.Framework;
using PRISM.Logging;

namespace PRISMTest
{
    [TestFixture]
    class DatabaseLoggingTests
    {

        [TestCase(@"Gigasax", "DMS5", @"C:\Temp", "TestLogFileForDBLogging")]
        [TestCase(@"Gigasax", "DMS5", "", "")]
        [Category("DatabaseIntegrated")]
        public void TestDBLoggerIntegrated(string server, string database, string logFolder, string logFileNameBase)
        {
            TestDBLogger(server, database, "Integrated", "", logFolder, logFileNameBase);
        }

        [TestCase(@"Gigasax", "DMS5", @"C:\Temp", "TestLogFileForDBLogging")]
        [TestCase(@"Gigasax", "DMS5", "", "")]
        [Category("DatabaseNamedUser")]
        public void TestDBLoggerNamedUser(string server, string database, string logFolder, string logFileNameBase)
        {
            TestDBLogger(server, database, TestDBTools.DMS_READER, TestDBTools.DMS_READER_PASSWORD, logFolder, logFileNameBase);
        }

        private void TestDBLogger(string server, string database, string user, string password, string logFolder, string logFileNameBase)
        {
            var connectionString = TestDBTools.GetConnectionString(server, database, user, password);

            var moduleName = DatabaseLogger.MachineName + ":" + "DatabaseLoggingTests";
            var logger = new SQLServerDatabaseLogger(moduleName, connectionString)
            {
                EchoMessagesToFileLogger = true,
                LogLevel = BaseLogger.LogLevels.DEBUG
            };


            Console.WriteLine("Calling logger.PostEntry using " + database + " as user " + user);

            // Call stored procedure PostLogEntry
            logger.WriteLog(BaseLogger.LogLevels.DEBUG, "Test log entry on " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
        }
    }
}