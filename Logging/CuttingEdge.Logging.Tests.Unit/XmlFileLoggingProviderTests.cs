﻿using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    [TestClass]
    public class XmlFileLoggingProviderTests
    {
        [TestMethod]
        public void Constructor_DefaultConstructor_Succeeds()
        {
            new XmlFileLoggingProvider();
        }

        [TestMethod]
        public void Constructor_WithRelativePath_Succeeds()
        {
            string relativePath = "logs\\..\\log.xml";

            CreateXmlFileLogger(relativePath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullPath_ThrowException()
        {
            // Arrange
            string invalidPath = null;

            // Act
            CreateXmlFileLogger(invalidPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithEmptyPath_ThrowsException()
        {
            // Arrange
            string invalidPath = string.Empty;

            // Act
            CreateXmlFileLogger(invalidPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithInvalidFileNamePath_ThrowsException()
        {
            // Arrange
            string invalidFileName = "log.*";

            // Act
            CreateXmlFileLogger(invalidFileName);
        }

        [TestMethod]
        public void Constructor_WithRootedPath_Succeeds()
        {
            // Arrange
            string validPath = "c:\\log.xml";          

            // Act
            CreateXmlFileLogger(validPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SerializeLogEntry_WithNullArgument_ThrowsException()
        {
            // Arrange
            var provider = new FakeXmlFileLoggingProvider();

            LogEntry invalidEntry = null;

            // Act
            provider.SerializeLogEntry(invalidEntry);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteEntryInnerElements_WithNullWriter_ThrowsException()
        {
            // Arrange
            var provider = new FakeXmlFileLoggingProvider();

            XmlWriter invalidWriter = null;
            LogEntry validEntry = new LogEntry(LoggingEventType.Debug, "Message", null, null);

            // Act
            provider.WriteEntryInnerElements(invalidWriter, validEntry);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteEntryInnerElements_WithNullEntry_ThrowsException()
        {
            // Arrange
            var provider = new FakeXmlFileLoggingProvider();

            XmlWriter validWriter = XmlWriter.Create(new MemoryStream());
            LogEntry invalidEntry = null;

            // Act
            provider.WriteEntryInnerElements(validWriter, invalidEntry);
        }

        [TestMethod]
        public void Path_ConstructorSuppliedWithRelativePath_ReturnsRootedPath()
        {
            // Arrange
            string relativePath = "log.txt";

            // Act
            XmlFileLoggingProvider provider = CreateXmlFileLogger(relativePath);
            
            // Assert
            Assert.IsTrue(Path.IsPathRooted(provider.Path),
                "XmlFileLoggingProvider.Path did not return a rooted path. Path: " + provider.Path);
        }

        [TestMethod]
        public void Path_ConstructorSuppliedWithUncanonicalName_ReturnsCanonicalName()
        {
            // Arrange
            string uncanonicalPath = "c:\\windows\\..\\log.xml";
            string expectedCanonicalPath = "c:\\log.xml";

            // Act
            XmlFileLoggingProvider provider = CreateXmlFileLogger(uncanonicalPath);

            // Assert
            Assert.AreEqual(expectedCanonicalPath, provider.Path,
                "XmlFileLoggingProvider.Path did not return a canonical path. Path: " + provider.Path);
        }

        [TestMethod]
        public void FallbackProvider_ConstructorSuppliedWithFallbackProvider_ReturnsThatInstance()
        {
            // Arrange
            var expectedFallbackProvider = new MemoryLoggingProvider();

            // Act
            var provider = CreateValidXmlFileLogger(expectedFallbackProvider);

            // Assert
            Assert.AreEqual(expectedFallbackProvider, provider.FallbackProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Initialize_CalledWhenCreatedWithNonDefaultConstructor_ThrowsException()
        {
            // Arrange
            var provider = CreateValidXmlFileLogger();
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize("validName", validConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_ConfigurationWithEmptyPath_ThrowsException()
        {
            // Arrange
            var provider = new FakeXmlFileLoggingProvider();

            var invalidConfiguration = CreateValidConfiguration();
            invalidConfiguration["path"] = string.Empty;

            // Act
            provider.Initialize("validName", invalidConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_ConfigurationWithUnknownAttribute_ThrowsException()
        {
            // Arrange
            var provider = new FakeXmlFileLoggingProvider();

            var invalidConfiguration = CreateValidConfiguration();
            invalidConfiguration["unknown"] = "some value";

            // Act
            provider.Initialize("validName", invalidConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new FakeXmlFileLoggingProvider();

            // Act
            provider.Initialize("validName", null);
        }

        [TestMethod]
        public void Path_InitializedProvider_ReturnsExpectedValue()
        {
            // Arrange
            string expectedPath = "c:\\errors.log";
            var provider = new FakeXmlFileLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["path"] = expectedPath;

            // Act
            provider.Initialize("valid name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedPath, provider.Path);
        }

        [TestMethod]
        public void Log_UninitializedProvider_ThrowsDescriptiveException()
        {
            // Arrange
            var provider = new XmlFileLoggingProvider();

            try
            {
                // Act
                provider.Log("Some message");

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("The provider has not been initialized"),
                     "A provider that hasn't been initialized correctly, should throw a descriptive " +
                     "exception. Actual: " + ex.Message + Environment.NewLine + ex.StackTrace);

                Assert.IsTrue(ex.Message.Contains("XmlFileLoggingProvider"),
                    "The message should contain the type name of the unitialized provider. Actual: " +
                    ex.Message);
            }
        }

#if DEBUG
        [TestMethod]
        public void Log_AfterCallingInitialize_LogsSuccesfully()
        {
            // Arrange
            var expectedMessage = "Some message";
            var provider = new FakeXmlFileLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

            provider.Initialize("Valid name", validConfiguration);

            // Act
            provider.Log(expectedMessage);

            // Assert
            Assert.IsTrue(provider.LoggedText.Contains(expectedMessage), provider.LoggedText);
        }

        [TestMethod]
        public void Log_ValidEvent_HasExpectedRootElement()
        {
            // Arrange
            var provider = CreateValidXmlFileLogger();

            // Act
            provider.Log("Some message");

            var xml = XDocument.Parse(provider.LoggedText);

            // Assert
            Assert.AreEqual("LogEntry", xml.Root.Name);
        }

        [TestMethod]
        public void Log_ValidEvent_LogsExpectedTime()
        {
            // Arrange
            DateTime currentTime = new DateTime(2010, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            var provider = CreateValidXmlFileLogger();
            provider.SetCurrentTime(currentTime);

            // Act
            provider.Log("Some message");

            var xml = XDocument.Parse(provider.LoggedText);

            // Assert
            Assert.AreEqual("2010-01-02T03:04:05Z", xml.Root.Element("EventTime").Value);
        }

        [TestMethod]
        public void Log_ValidEvent_LogsExpectedMessage()
        {
            // Arrange
            string expectedMessage = "This message is expected.";

            var provider = CreateValidXmlFileLogger();

            // Act
            provider.Log(expectedMessage);

            var xml = XDocument.Parse(provider.LoggedText);

            // Assert
            Assert.AreEqual(expectedMessage, xml.Root.Element("Message").Value);
        }

        [TestMethod]
        public void Log_ValidEventWithoutSource_DoesIncludeEmptySourceElementInOutput()
        {
            // Arrange
            var provider = CreateValidXmlFileLogger();

            // Act
            provider.Log("some message");

            var xml = XDocument.Parse(provider.LoggedText);

            // Assert
            Assert.IsNotNull(xml.Root.Element("Source"));
            Assert.AreEqual(string.Empty, xml.Root.Element("Source").Value);
        }

        [TestMethod]
        public void Log_ValidEvent_LogsExpectedSource()
        {
            // Arrange
            string expectedSource = "This source is expected.";

            var provider = CreateValidXmlFileLogger();

            // Act
            provider.Log(LoggingEventType.Debug, "Some message", expectedSource);

            var xml = XDocument.Parse(provider.LoggedText);

            // Assert
            Assert.AreEqual(expectedSource, xml.Root.Element("Source").Value);
        }

        [TestMethod]
        public void Log_ValidEvent_LogsExpectedExceptionType()
        {
            // Arrange
            string expectedType = "System.InvalidOperationException";

            var provider = CreateValidXmlFileLogger();

            Exception exceptionToLog;

            try
            {
                throw new InvalidOperationException("Bad bad!");
            }
            catch (Exception ex)
            {
                exceptionToLog = ex;
            }

            // Act
            provider.Log(exceptionToLog);

            var xml = XDocument.Parse(provider.LoggedText);

            // Assert
            Assert.AreEqual(expectedType, xml.Root.Element("Exception").Element("ExceptionType").Value);
        }

        [TestMethod]
        public void Log_ValidEventWithoutException_DoesNotIncludeExceptionElementInOutput()
        {
            // Arrange
            var provider = CreateValidXmlFileLogger();

            // Act
            provider.Log("some message");

            var xml = XDocument.Parse(provider.LoggedText);

            // Assert
            Assert.IsNull(xml.Root.Element("Exception"));
        }

        [TestMethod]
        public void Log_Exception_LogsExpectedXml()
        {
            // Arrange
            Exception exceptionToLog;

            try
            {
                throw new CompositeException("Composite", new Exception[]
                {
                    new InvalidCastException(),
                    new InvalidOperationException()
                });
            }
            catch (Exception ex)
            {
                exceptionToLog = ex;
            }

            var provider = CreateValidXmlFileLogger();

            // Act
            provider.Log(exceptionToLog);

            var xml = XDocument.Parse(provider.LoggedText);
            
            // Assert
            Assert.IsNotNull(xml.Root.Element("Exception"));
        }

        [TestMethod]
        public void Constructor_UnableToCreateLogFile_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            string file = "log.xml";
            string message = "IO message.";
            Exception expectedInnerException = new IOException(message);

            FakeXmlFileLoggingProvider.ExceptionToThrowFromAppendAllText = expectedInnerException;

            try
            {
                // Act
                new FakeXmlFileLoggingProvider(LoggingEventType.Debug, file, null);

                // Assert
                Assert.Fail("Exception was expected");
            }
            catch (ProviderException ex)
            {
                Assert.IsTrue(ex.Message.Contains("error creating or writing to the file"));
                Assert.IsTrue(ex.Message.Contains(file));
                Assert.IsTrue(ex.Message.Contains(message));
                Assert.AreEqual(expectedInnerException, ex.InnerException);
            }
            finally
            {
                FakeXmlFileLoggingProvider.ExceptionToThrowFromAppendAllText = null;
            }
        }
#endif

        [TestMethod]
        public void Initialize_WithMissingPath_ThrowsExpectedException()
        {
            // Arrange
            var provider = new XmlFileLoggingProvider();
            var invalidConfiguration = new NameValueCollection();

            try
            {
                // Act
                provider.Initialize("Valid name", invalidConfiguration);

                // Assert
                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("missing 'path' attribute"),
                    "Exception not expressive enough. Actual message: " + ex.Message);
            }
        }

        private static NameValueCollection CreateValidConfiguration()
        {
            var configuration = new NameValueCollection();

            configuration["path"] = "log.xml";

            return configuration;
        }

        private static FakeXmlFileLoggingProvider CreateValidXmlFileLogger()
        {
            return CreateXmlFileLogger("log.xml", null);
        }

        private static FakeXmlFileLoggingProvider CreateValidXmlFileLogger(LoggingProviderBase fallbackProvider)
        {
            return CreateXmlFileLogger("log.xml", fallbackProvider);
        }

        private static FakeXmlFileLoggingProvider CreateXmlFileLogger(string path)
        {
            return new FakeXmlFileLoggingProvider(LoggingEventType.Debug, path, null);
        }

        private static FakeXmlFileLoggingProvider CreateXmlFileLogger(string path, 
            LoggingProviderBase fallbackProvider)
        {
            return new FakeXmlFileLoggingProvider(LoggingEventType.Debug, path, fallbackProvider);
        }

        private sealed class FakeXmlFileLoggingProvider : XmlFileLoggingProvider
        {
            [ThreadStatic]
            private static Exception exceptionToThrowFromAppendAllText;

            private DateTime currentTime;

            public FakeXmlFileLoggingProvider()
            {
            }

            public FakeXmlFileLoggingProvider(LoggingEventType severity, string path, 
                LoggingProviderBase fallbackProvider)
                : base(severity, path, fallbackProvider)
            {
            }

            public static Exception ExceptionToThrowFromAppendAllText
            {
                get { return exceptionToThrowFromAppendAllText; }
                set { exceptionToThrowFromAppendAllText = value; }
            }

#if DEBUG
            public string LoggedText { get; set; }

            internal override DateTime CurrentTime
            {
                get { return this.currentTime; }
            }
#endif
            public void SetCurrentTime(DateTime currentTime)
            {
                this.currentTime = currentTime;
            }

            public new void SerializeLogEntry(LogEntry entry)
            {
                base.SerializeLogEntry(entry);
            }

            public new void WriteEntryInnerElements(XmlWriter writer, LogEntry entry)
            {
                base.WriteEntryInnerElements(writer, entry);
            }

#if DEBUG
            internal override void AppendAllText(string contents)
            {
                if (exceptionToThrowFromAppendAllText != null)
                {
                    throw exceptionToThrowFromAppendAllText;
                }

                this.LoggedText = contents;
            }
#endif
        }
    }
}