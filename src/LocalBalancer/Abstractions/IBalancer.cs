namespace LocalBalancer.Abstractions
{
    /// <summary>
    /// The contract of the local balancer.
    /// </summary>
    public interface IBalancer
    {
        /// <summary>
        /// Currently active network node count
        /// </summary>
        int ActiveNodeCount { get; }

        /// <summary>
        /// How long do we wanna keep the circuit open?
        /// </summary>
        double CircuitBreakDurationMinutes { get; }

        /// <summary>
        /// How many errors are we willing to let go?
        /// </summary>
        int CircuitBreakThreadhold { get; }

        /// <summary>
        /// How many times should the balancer retry on a node
        /// </summary>
        int RetryCount { get; }

        /// <summary>
        /// Get the settings of the the currently active node
        /// </summary>
        /// <returns></returns>
        NetworkNodeSettings GetCurrentNodeSettings();

        /// <summary>
        /// Force a move to next configured node
        /// </summary>
        void MoveToNextNode();

        /// <summary>
        /// Resets the balancer.
        /// Will start from scratch.
        /// </summary>
        void Reset();

        /// <summary>
        /// Sets the state of a network node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="state"></param>
        void SetTrackingState(NetworkNodeSettings node, TrackingState state);
         
        bool ShouldBreak();
    }
}