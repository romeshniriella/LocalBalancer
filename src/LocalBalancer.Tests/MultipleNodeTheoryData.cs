using System;
using System.Collections.Generic;
using LocalBalancer.Tests.Framework;

namespace LocalBalancer.Tests
{
    public class MultipleNodeTheoryData
    {
        public Dictionary<string, string> CustomSettings { get; internal set; }

        public int ExpectedCurrentNodeIndex { get; internal set; }

        public string ExpectedExceptionMessage { get; internal set; }

        public Type ExpectedExceptionType { get; internal set; }

        public List<ExecutionResult> ExpectedTestResults { get; internal set; }

        public List<string> ExpecteLogMessages { get; internal set; }
    }
}