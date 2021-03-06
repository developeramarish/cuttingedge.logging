﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    /// <summary>
    /// Tests the <see cref="CompositeLoggingProvider"/> class.
    /// </summary>
    [TestClass]
    public class CompositeLoggingProviderTests
    {
        [TestMethod]
        public void Log_WithProviderInitializedWithCustomConstructor_LogsToAllSuppliedProviders()
        {
            // Arrange
            var provider1 = new MemoryLoggingProvider();
            var provider2 = new MemoryLoggingProvider();

            var provider =
                new CompositeLoggingProvider(LoggingEventType.Debug, null, provider1, provider2);

            // Act
            provider.Log("Test");

            // Assert
            Assert.AreEqual(1, provider1.GetLoggedEntries().Count(), "CompositeLoggingProvider did not log.");
            Assert.AreEqual(1, provider2.GetLoggedEntries().Count(), "CompositeLoggingProvider did not log.");
        }

        [TestMethod]
        public void Log_UnitializedProvider_ShouldFail()
        {
            // Arrange
            var provider = new CompositeLoggingProvider();

            try
            {
                // Act
                provider.Log("Some message");

                // Assert
                Assert.Fail("Exception expected");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("The provider has not been initialized"),
                    "A provider that hasn't been initialized correctly, should throw a descriptive " +
                    "exception. Actual: " + ex.Message + Environment.NewLine + ex.StackTrace);

                Assert.IsTrue(ex.Message.Contains("CompositeLoggingProvider"),
                    "The message should contain the type name of the unitialized provider. Actual: " + 
                    ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogInternal_WithNullArgument_ThrowsException()
        {
            // Arrange
            var provider = new FakeCompositeLoggingProvider();

            // Act
            provider.LogInternal(null);
        }

        [TestMethod]
        public void Constructor_WithValidArguments_Succeeds()
        {
            // Arrange
            var validThreshold = LoggingEventType.Critical;
            LoggingProviderBase validFallbackProvider = null;
            LoggingProviderBase validProvider = new FakeLoggingProvider();

            // Act
            new CompositeLoggingProvider(validThreshold, validFallbackProvider, validProvider);
        }

        [TestMethod]
        public void Constructor_WithMultipleProviders_Succeeds()
        {
            // Arrange
            var validThreshold = LoggingEventType.Critical;
            LoggingProviderBase validFallbackProvider = null;
            LoggingProviderBase provider1 = new FakeLoggingProvider();
            LoggingProviderBase provider2 = new FakeLoggingProvider();

            // Act
            new CompositeLoggingProvider(validThreshold, validFallbackProvider, provider1, provider2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void Constructor_WithInvalidThreshold_ThrowsException()
        {
            var invalidThreshold = (LoggingEventType)(-1);
            LoggingProviderBase validFallbackProvider = null;
            LoggingProviderBase validProvider = new FakeLoggingProvider();

            // Act
            new CompositeLoggingProvider(invalidThreshold, validFallbackProvider, validProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithoutProviders_ThrowsException()
        {
            // Arrange
            var validThreshold = LoggingEventType.Critical;
            LoggingProviderBase validFallbackProvider = null;

            // Act
            new CompositeLoggingProvider(validThreshold, validFallbackProvider, null);
        }

        [TestMethod]
        public void Constructor_WithDuplicateProvider_ThrowsException()
        {
            // Arrange
            var validThreshold = LoggingEventType.Critical;
            LoggingProviderBase validFallbackProvider = null;

            LoggingProviderBase provider = new FakeLoggingProvider();

            LoggingProviderBase[] invalidProviderList = new[] { provider, provider };

            try
            {
                // Act
                new CompositeLoggingProvider(validThreshold, validFallbackProvider, invalidProviderList);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));

                var msg = ex.Message;

                Assert.IsTrue(msg.Contains("providers"), "Message should contain argument name.");
                Assert.IsTrue(msg.Contains("collection contains") && msg.Contains("duplicate references"), 
                    "Message should be descriptive enough. Actual message: " + msg);
            }
        }

        [TestMethod]
        public void Constructor_WithEmptyProviderCollection_ThrowsException()
        {
            // Arrange
            var validThreshold = LoggingEventType.Critical;
            LoggingProviderBase validFallbackProvider = null;

            var invalidProviderList = new LoggingProviderBase[0];

            try
            {
                // Act
                new CompositeLoggingProvider(validThreshold, validFallbackProvider, invalidProviderList);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));

                var message = ex.Message;

                Assert.IsTrue(message.Contains("providers"), "Message should contain argument name.");
                Assert.IsTrue(message.Contains("Collection should contain") && message.Contains("at least one"),
                    "Message should be descriptive enough. Actual message: " + message);
            }
        }

        [TestMethod]
        public void Constructor_WithNullProvider_ThrowsException()
        {
            // Arrange
            var validThreshold = LoggingEventType.Critical;
            LoggingProviderBase validFallbackProvider = null;

            IEnumerable<LoggingProviderBase> invalidProviderList = 
                new LoggingProviderBase[] { new FakeLoggingProvider(), null };

            try
            {
                // Act
                new CompositeLoggingProvider(validThreshold, validFallbackProvider, invalidProviderList);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));

                var message = ex.Message;

                Assert.IsTrue(message.Contains("providers"), "Message should contain argument name.");
                Assert.IsTrue(message.Contains("collection contains") && message.Contains("null references"),
                    "Message should be descriptive enough. Actual message: " + message);
            }
        }

        [TestMethod]
        public void Initialize_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var provider = new CompositeLoggingProvider();
            var validConfiguration = CreateValidConfiguration("OtherProvider");

            // Act
            provider.Initialize("Valid provider name", validConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new CompositeLoggingProvider();
            NameValueCollection invalidConfiguration = null;

            // Act
            provider.Initialize("Valid provider name", invalidConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialization_NoProvidersConfigured_ThrowsException()
        {
            // Arrange
            var providerUnderTest = new CompositeLoggingProvider();
            var invalidConfiguration = new NameValueCollection();

            // Act
            providerUnderTest.Initialize("Valid provider name", invalidConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialization_WithInvalidAttribute_ThrowsException()
        {
            // Arrange
            var providerUnderTest = new CompositeLoggingProvider();
            var invalidConfiguration = new NameValueCollection();
            invalidConfiguration["_provider1"] = "MemoryProvider";

            // Act
            providerUnderTest.Initialize("Valid provider name", invalidConfiguration);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "Composite logging provider";
            var provider = new CompositeLoggingProvider();
            var validConfiguration = CreateValidConfiguration("Referenced Provider");

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithCustomDescription_SetsSpecifiedDescription()
        {
            // Arrange
            var expectedDescription = "My forwarder";
            var provider = new CompositeLoggingProvider();
            var validConfiguration = CreateValidConfiguration("Referenced Provider");
            validConfiguration["description"] = expectedDescription;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Log_OnPartiallyInitializedProvider_Fails()
        {
            // Arrange
            var provider = new CompositeLoggingProvider();
            var validConfiguration = CreateValidConfiguration("Referenced Provider");

            provider.Initialize("Valid provider name", validConfiguration);

            // Act
            // The Composite logging provider is the only provider that can not be initialized by hand by
            // calling the Initialize() method. It needs to be either initialized by configuring it in the
            // application configuration file, or by instantiating it with an overloaded constructor.
            provider.Log("Some message");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Providers_UninitializedInstance_ThrowsException()
        {
            // Arrange
            var providerUnderTest = new CompositeLoggingProvider();

            // Act
            var providers = providerUnderTest.Providers;
        }

#if DEBUG
        [TestMethod]
        public void CompleteInitialization_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var expectedReferencedProvider = CreateInitializedMemoryLogger("Other Provider");
            var defaultProvider = CreateInitializedMemoryLogger("Default Provider");
            var providerUnderTest = new CompositeLoggingProvider();
            var validConfiguration = CreateValidConfiguration(expectedReferencedProvider.Name);
            providerUnderTest.Initialize("Valid provider name", validConfiguration);         
            
            var configuredProviders = new LoggingProviderCollection()
            {
                providerUnderTest,
                expectedReferencedProvider,
                defaultProvider
            };          

            // Act
            providerUnderTest.CompleteInitialization(configuredProviders, defaultProvider);
            var actualReferencedProvider = providerUnderTest.Providers[0];

            // Assert
            Assert.AreEqual(1, providerUnderTest.Providers.Count, 
                "The provider is expected to reference a single provider.");

            Assert.IsNotNull(actualReferencedProvider, 
                "The referenced provider should not be a null reference.");

            Assert.AreEqual(expectedReferencedProvider, actualReferencedProvider,
                "The referenced provider is not the expected provider. Actual referenced provider: " + 
                actualReferencedProvider.Name);
        }

        [TestMethod]
        public void CompleteInitialization_WithMultipleProviders_Succeeds()
        {
            // Arrange
            // List of referenced providers which names would sort them in opposite order.
            var firstExpectedReferencedProvider = CreateInitializedMemoryLogger("Z first provider");
            var secondExpectedReferencedProvider = CreateInitializedMemoryLogger("Y second provider");
            var thirdExpectedReferencedProvider = CreateInitializedMemoryLogger("X third provider");
            var defaultProvider = CreateInitializedMemoryLogger("Default Provider");
            var providerUnderTest = new CompositeLoggingProvider();
            var validConfiguration = CreateValidConfiguration(
                firstExpectedReferencedProvider.Name,
                secondExpectedReferencedProvider.Name,
                thirdExpectedReferencedProvider.Name);
            providerUnderTest.Initialize("Valid provider name", validConfiguration);

            // List of configured providers in order 
            var configuredProviders = new LoggingProviderCollection()
            {
                thirdExpectedReferencedProvider,
                defaultProvider,
                firstExpectedReferencedProvider,
                secondExpectedReferencedProvider,
                providerUnderTest
            };

            // Act
            providerUnderTest.CompleteInitialization(configuredProviders, defaultProvider);
            var actualFirstReferencedProvider = providerUnderTest.Providers[0];
            var actualSecondReferencedProvider = providerUnderTest.Providers[1];
            var actualThirdReferencedFirstProvider = providerUnderTest.Providers[2];

            // Assert
            Assert.IsTrue(firstExpectedReferencedProvider == actualFirstReferencedProvider,
                "The first provider in the list is not the expected provider. Expected: {0}, Actual: {1}",
                firstExpectedReferencedProvider.Name, actualFirstReferencedProvider.Name);

            Assert.AreEqual(secondExpectedReferencedProvider, actualSecondReferencedProvider,
                "The second provider in the list is not the expected provider. Expected: {0}, Actual: {1}",
                firstExpectedReferencedProvider.Name, actualFirstReferencedProvider.Name);

            Assert.AreEqual(thirdExpectedReferencedProvider, actualThirdReferencedFirstProvider,
                "The third provider in the list is not the expected provider. Expected: {0}, Actual: {1}",
                firstExpectedReferencedProvider.Name, actualFirstReferencedProvider.Name);
        }

        [TestMethod]
        public void CompleteInitialization_WithArbitraryNumberedProviders_Succeeds()
        {
            // Arrange
            // List of referenced providers which names would sort them in opposite order.
            var firstExpectedReferencedProvider = CreateInitializedMemoryLogger("First provider");
            var secondExpectedReferencedProvider = CreateInitializedMemoryLogger("Second provider");
            var thirdExpectedReferencedProvider = CreateInitializedMemoryLogger("Third provider");
            var defaultProvider = CreateInitializedMemoryLogger("Default Provider");
            var providerUnderTest = new CompositeLoggingProvider();
            var validConfiguration = new NameValueCollection();

            // Configuration with provider attributes other than 1, 2 and 3.
            validConfiguration["provider3"] = firstExpectedReferencedProvider.Name;
            validConfiguration["provider143"] = thirdExpectedReferencedProvider.Name;
            validConfiguration["provider66"] = secondExpectedReferencedProvider.Name;
            providerUnderTest.Initialize("Valid provider name", validConfiguration);

            // List of configured providers in order 
            var configuredProviders = new LoggingProviderCollection()
            {
                thirdExpectedReferencedProvider,
                defaultProvider,
                firstExpectedReferencedProvider,
                secondExpectedReferencedProvider,
                providerUnderTest
            };

            // Act
            providerUnderTest.CompleteInitialization(configuredProviders, defaultProvider);
            var actualFirstReferencedProvider = providerUnderTest.Providers[0];
            var actualSecondReferencedProvider = providerUnderTest.Providers[1];
            var actualThirdReferencedFirstProvider = providerUnderTest.Providers[2];

            // Assert
            Assert.IsTrue(firstExpectedReferencedProvider == actualFirstReferencedProvider,
                "The first provider in the list is not the expected provider. Expected: {0}, Actual: {1}",
                firstExpectedReferencedProvider.Name, actualFirstReferencedProvider.Name);

            Assert.AreEqual(secondExpectedReferencedProvider, actualSecondReferencedProvider,
                "The second provider in the list is not the expected provider. Expected: {0}, Actual: {1}",
                firstExpectedReferencedProvider.Name, actualFirstReferencedProvider.Name);

            Assert.AreEqual(thirdExpectedReferencedProvider, actualThirdReferencedFirstProvider,
                "The third provider in the list is not the expected provider. Expected: {0}, Actual: {1}",
                firstExpectedReferencedProvider.Name, actualFirstReferencedProvider.Name);
        }

        [TestMethod]
        public void CompleteInitialization_NonExistingProviderName_ThrowsException()
        {
            // Arrange
            string providerName = "Valid provider name";
            string nonExistingProviderName = "Non existing provider name";
            var providerUnderTest = new CompositeLoggingProvider();
            var validConfiguration = new NameValueCollection();
            validConfiguration["provider1"] = nonExistingProviderName;
            providerUnderTest.Initialize(providerName, validConfiguration);

            // List of configured providers in order 
            var configuredProviders = new LoggingProviderCollection()
            {
                providerUnderTest
            };

            try
            {
                // Act
                providerUnderTest.CompleteInitialization(configuredProviders, providerUnderTest);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ProviderException ex)
            {
                string msg = ex.Message ?? string.Empty;

                Assert.IsTrue(msg.Contains("references a provider") && msg.Contains("that does not exist"),
                    "Exception message should describe the problem. Actual: " + msg);

                Assert.IsTrue(msg.Contains("CompositeLoggingProvider"),
                    "Exception message should describe the provider type. Actual: " + msg);

                Assert.IsTrue(msg.Contains(providerName),
                    "Exception message should describe the provider name. Actual: " + msg);

                Assert.IsTrue(msg.Contains(nonExistingProviderName),
                    "Exception message should describe the name of the referenced provider. Actual: " + msg);

                Assert.IsTrue(msg.Contains("Make sure the name is spelled correctly."),
                    "Exception message should describe how to solve the problem. Actual: " + msg);
            }
        }

        [TestMethod]
        public void CompleteInitialization_NonExistingProviderNameOnCustomProvider_ThrowsException()
        {
            // Arrange
            string providerName = "Valid provider name";
            string nonExistingProviderName = "Non existing provider name";
            var providerUnderTest = new CustomProvider();
            var validConfiguration = new NameValueCollection();
            validConfiguration["provider1"] = nonExistingProviderName;
            providerUnderTest.Initialize(providerName, validConfiguration);

            // List of configured providers in order 
            var configuredProviders = new LoggingProviderCollection()
            {
                providerUnderTest
            };

            try
            {
                // Act
                providerUnderTest.CompleteInitialization(configuredProviders, providerUnderTest);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ProviderException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(CustomProvider).FullName),
                    "Exception message should state the provider's full name, because the type is not a " +
                    "type defined by the library itself. Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void CompleteInitialization_SameProviderNameSpelledTwice_ThrowsExceptoin()
        {
            // Arrange
            const string ProviderName = "Valid provider name";
            const string ReferencedProviderName = "MemoryProvider";
            var defaultProvider = CreateInitializedMemoryLogger(ReferencedProviderName);
            var providerUnderTest = new CompositeLoggingProvider();
            var validConfiguration = new NameValueCollection();
            validConfiguration["provider1"] = ReferencedProviderName;
            validConfiguration["provider2"] = ReferencedProviderName;

            try
            {
                // Act
                providerUnderTest.Initialize(ProviderName, validConfiguration);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ProviderException ex)
            {
                string msg = ex.Message ?? string.Empty;

                Assert.IsTrue(msg.Contains("references provider") && msg.Contains("multiple times"),
                    "Exception message should describe the problem. Actual: " + msg);

                Assert.IsTrue(msg.Contains("CompositeLoggingProvider"),
                    "Exception message should describe the provider type. Actual: " + msg);

                Assert.IsTrue(msg.Contains(ProviderName),
                    "Exception message should describe the provider name. Actual: " + msg);

                Assert.IsTrue(msg.Contains(ReferencedProviderName),
                    "Exception message should describe the name of the referenced provider. Actual: " + msg);

                Assert.IsTrue(msg.Contains("A provider should only be referenced once"),
                    "Exception message should describe how to solve the problem. Actual: " + msg);
            }
        }

        [TestMethod]
        public void Log_WithSingleReferencedProvider_LogsToReferencedProvider()
        {
            // Arrange
            var memoryLogger = CreateInitializedMemoryLogger("MemoryLogger");
            var configuredProviders = new LoggingProviderCollection() { memoryLogger };
            var providerUnderTest = CreateInitializedCompositeLoggingProvider(configuredProviders);
            var expectedMessage = "Some message";

            // Act
            providerUnderTest.Log("Some message");

            // Assert
            Assert.AreEqual(1, memoryLogger.GetLoggedEntries().Length);
            Assert.AreEqual(expectedMessage, memoryLogger.GetLoggedEntries().First().Message);
        }

        [TestMethod]
        public void Log_WithMultipleReferencedProviders_LogsToAllReferencedProviders()
        {
            // Arrange
            var logger1 = CreateInitializedMemoryLogger("MemoryLogger1");
            var logger2 = CreateInitializedMemoryLogger("MemoryLogger2");
            var configuredProviders = new LoggingProviderCollection() { logger1, logger2 };
            var providerUnderTest = CreateInitializedCompositeLoggingProvider(configuredProviders);
            var expectedMessage = "Some message";

            // Act
            providerUnderTest.Log("Some message");

            // Assert
            Assert.AreEqual(1, logger1.GetLoggedEntries().Length);
            Assert.AreEqual(1, logger2.GetLoggedEntries().Length);
            Assert.AreEqual(expectedMessage, logger1.GetLoggedEntries().First().Message);
            Assert.AreEqual(expectedMessage, logger2.GetLoggedEntries().First().Message);
        }

        [TestMethod]
        public void Log_WithFailingProvider_LogsToRemainingLoggers()
        {
            // Arrange
            var logger1 = new FailingLoggingProvider("Failure") { ExceptionToThrow = new Exception() };
            var logger2 = CreateInitializedMemoryLogger("MemoryLogger");
            var configuredProviders = new LoggingProviderCollection() { logger1, logger2 };
            var providerUnderTest = CreateInitializedCompositeLoggingProvider(configuredProviders);
            var expectedMessage = "Some message";

            // Act
            try
            {
                providerUnderTest.Log(expectedMessage);

                // Assert
                Assert.Fail("An exception was expected to be thrown.");
            }
            catch
            {
                // We're not interested in the exception
            }

            Assert.AreEqual(1, logger2.GetLoggedEntries().Length);
            Assert.AreEqual(expectedMessage, logger2.GetLoggedEntries().First().Message);
        }

        [TestMethod]
        public void Log_WithFailingProviders_ThrowsExceptionWithExpectedTypeAndMessage()
        {
            // Arrange
            var logger1 = new FailingLoggingProvider("Faile1") { ExceptionToThrow = new Exception("foo") };
            var logger2 = new FailingLoggingProvider("Faile2") { ExceptionToThrow = new Exception("bar") };
            var logger3 = CreateInitializedMemoryLogger("MemoryLogger");
            var configuredProviders = new LoggingProviderCollection() { logger1, logger2, logger3 };
            var providerUnderTest = CreateInitializedCompositeLoggingProvider(configuredProviders);
            var expectedMessage = "Some message";

            // Act
            try
            {
                providerUnderTest.Log(expectedMessage);

                // Assert
                Assert.Fail("An exception was expected to be thrown.");
            }
            catch (Exception ex)
            {
                // When logging to multiple providers, the provider should wrap the thrown exceptions in a
                // CompositeException, even if there is only one Exception (re throwing the same exception
                // would make us loose the stack trace).
                Assert.IsInstanceOfType(ex, typeof(CompositeException));
                Assert.IsTrue(ex.Message.Contains("foo"), 
                    "Exception message should contain all inner exception messages. (foo missing)");
                Assert.IsTrue(ex.Message.Contains("bar"),
                    "Exception message should contain all inner exception messages. (bar missing)");
            }
        }
#endif // DEBUG

        [TestMethod]
        public void Configuration_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Forwarder",
                    Providers =
                    {
                        // <provider name="Forwarder" type="CompositeLoggingProvider" provider1="MemLogger" />
                        new ProviderConfigLine()
                        {
                            Name = "Forwarder",
                            Type = typeof(CompositeLoggingProvider),
                            CustomAttributes = "provider1=\"MemLogger\" "
                        },

                        // <provider name="MemLogger" type="MemoryLoggingProvider" />
                        new ProviderConfigLine()
                        {
                            Name = "MemLogger",
                            Type = typeof(MemoryLoggingProvider),
                        },
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Configuration_CircularReferencingSelf_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "C1",
                    Providers =
                    {
                        // <provider name="C1" type="CompositeLoggingProvider" provider1="C2" />
                        new ProviderConfigLine()
                        {
                            Name = "C1",
                            Type = typeof(CompositeLoggingProvider),
                            CustomAttributes = "provider1=\"C2\" "
                        },

                        // <provider name="C2" type="CompositeLoggingProvider" provider1="C1" />
                        new ProviderConfigLine()
                        {
                            Name = "C2",
                            Type = typeof(CompositeLoggingProvider),
                            CustomAttributes = "provider1=\"C1\" "
                        },
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("An exception was expected.");
                }
                catch (Exception ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(ConfigurationErrorsException));
                    Assert.IsTrue(ex.Message.Contains("circular"), 
                        "Exception message should describe the problem: a circular reference.");
                }
            }
        }

        [TestMethod]
        public void Configuration_CircularReferencingSelfThroughFallbackProvider_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "C1",
                    Providers =
                    {
                        // <provider name="C1" type="Composite" provider1="MemLogger" fallbackProvider="C2" />
                        new ProviderConfigLine()
                        {
                            Name = "C1",
                            Type = typeof(CompositeLoggingProvider),
                            CustomAttributes = "provider1=\"MemLogger\" fallbackProvider=\"C2\" "
                        },

                        // <provider name="C2" type="CompositeLoggingProvider" provider1="C1" />
                        new ProviderConfigLine()
                        {
                            Name = "C2",
                            Type = typeof(CompositeLoggingProvider),
                            CustomAttributes = "provider1=\"C1\" "
                        },

                        // <provider name="MemLogger" type="MemoryLoggingProvider" />
                        new ProviderConfigLine()
                        {
                            Name = "MemLogger",
                            Type = typeof(MemoryLoggingProvider),
                        }
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("An exception was expected.");
                }
                catch (Exception ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(ConfigurationErrorsException));
                    Assert.IsTrue(ex.Message.Contains("circular"),
                        "Exception message should describe the problem: a circular reference.");
                }
            }
        }

        [TestMethod]
        public void CompleteInitialization_ReferencingSelfDirectly_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "C1",
                    Providers =
                    {
                        // <provider name="C1" type="CompositeLoggingProvider" provider1="C1" />
                        new ProviderConfigLine()
                        {
                            Name = "C1",
                            Type = typeof(CompositeLoggingProvider),
                            CustomAttributes = "provider1=\"C1\" "
                        }
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("An exception was expected.");
                }
                catch (Exception ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(ConfigurationErrorsException));
                    Assert.IsTrue(ex.Message.Contains("circular"),
                        "Exception message should describe the problem: a circular reference.");
                }
            }
        }

        private static MemoryLoggingProvider CreateInitializedMemoryLogger(string name)
        {
            var provider = new MemoryLoggingProvider();

            provider.Initialize(name, new NameValueCollection());

            return provider;
        }

#if DEBUG      
        private static CompositeLoggingProvider CreateInitializedCompositeLoggingProvider(
            LoggingProviderCollection providers)
        {
            var provider = new CompositeLoggingProvider();
            var configuration = new NameValueCollection();

            foreach (var p in providers.Select((p, i) => new { Provider = p, Index = i }))
            {
                configuration["provider" + p.Index] = p.Provider.Name;
            }

            provider.Initialize("Valid provider name", configuration);

            provider.CompleteInitialization(providers, provider);

            return provider;
        }
#endif // DEBUG

        private static NameValueCollection CreateValidConfiguration(params string[] providerNames)
        {
            Assert.IsNotNull(providerNames);

            var configuration = new NameValueCollection();

            for (int index = 0; index < providerNames.Length; index++)
            {
                configuration["provider" + (index + 1)] = providerNames[index];
            }

            return configuration;
        }

        private sealed class FailingLoggingProvider : LoggingProviderBase
        {
            public FailingLoggingProvider(string name)
            {
                this.Initialize(name, new NameValueCollection());
            }

            public Exception ExceptionToThrow { get; set; }

            protected override object LogInternal(LogEntry entry)
            {
                Assert.IsTrue(this.ExceptionToThrow != null);

                throw this.ExceptionToThrow;
            }
        }

        private class CustomProvider : CompositeLoggingProvider
        {
        }

        private sealed class FakeCompositeLoggingProvider : CompositeLoggingProvider
        {
            public new object LogInternal(LogEntry entry)
            {
                return base.LogInternal(entry);
            }
        }

        private sealed class FakeLoggingProvider : LoggingProviderBase
        {
            protected override object LogInternal(LogEntry entry)
            {
                return null;
            }
        }
    }
}