namespace LocalBalancer
{
    internal sealed class Constants
    {
        public const string CurrentNodeIndexCacheKey = "NodeSelector.CurrentNode.Index";

        public const double DefaultCircuitBreakDuration = 1;

        public const int DefaultCircuitBreakThreadhold = 2;

        public const int DefaultRetryCount = 3;

        public const string JsonContentType = "application/json";
    }
}