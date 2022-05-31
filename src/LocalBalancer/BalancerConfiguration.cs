using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LocalBalancer
{
    public class BalancerConfiguration
    {
        public BalancerConfiguration()
        {
            this.Nodes = new Dictionary<string, NetworkNodeSettings>();
        }

        public double CircuitBreakDurationMinutes { get; set; } = Constants.DefaultCircuitBreakDuration;

        public int CircuitBreakThreadhold { get; set; } = Constants.DefaultCircuitBreakThreadhold;

        public bool IsTrackingEnabled { get; set; } = false;

        [Required]
        [MinLength(1)]
        public Dictionary<string, NetworkNodeSettings> Nodes { get; set; }

        public int RetryCount { get; set; } = Constants.DefaultRetryCount;
    }
}