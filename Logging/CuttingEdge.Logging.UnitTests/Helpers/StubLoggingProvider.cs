﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    internal class StubLoggingProvider : LoggingProviderBase
    {
        protected override object LogInternal(LoggingEventType type, string message, Exception exception, string source)
        {
            return null;
        }
    }
}
