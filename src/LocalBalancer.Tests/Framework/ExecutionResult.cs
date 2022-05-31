using System;

namespace LocalBalancer.Tests.Framework
{
    public class ExecutionResult
    {
        public ExecutionResult(int counter, Uri nodeAddress)
        {
            this.Counter = counter;
            this.NetworkNodeAddress = nodeAddress;
        }

        public ExecutionResult(int counter, string nodeAddress)
        {
            this.Counter = counter;
            this.NetworkNodeAddress = new Uri(nodeAddress);
        }

        public int Counter { get; set; }

        public Uri NetworkNodeAddress { get; set; }

        public override string ToString()
        {
            return $"Execution:= {this.Counter}, Node:{this.NetworkNodeAddress}";
        }
    }
}