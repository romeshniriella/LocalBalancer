using System.Collections.Generic;
using System.Linq;
using LocalBalancer.Abstractions;

namespace LocalBalancer
{
    public class Balancer : IBalancer
    {
        private readonly List<NetworkNodeSettings> _activeNodes;
        private readonly ICacheHelper _cache;
        private readonly BalancerConfiguration _nodeConfiguration;

        public Balancer(IBalancerConfigSource nodeConfigSource, ICacheHelper cacheHelper)
        {
            _nodeConfiguration = nodeConfigSource.GetBalancerConfiguration();
            _cache = cacheHelper;

            _activeNodes = this.GetActiveNodes(_nodeConfiguration);
        }

        public int ActiveNodeCount
            => _activeNodes.Count(x => x.TrackingState == TrackingState.Online);

        public double CircuitBreakDurationMinutes
            => _nodeConfiguration.CircuitBreakDurationMinutes;

        public int CircuitBreakThreadhold
            => _nodeConfiguration.CircuitBreakThreadhold;

        public int RetryCount
            => _nodeConfiguration.RetryCount;

        public NetworkNodeSettings GetCurrentNodeSettings()
        {
            if (_activeNodes.Count == 0)
            {
                throw new NetworkNodeException("Nodes are not configured.");
            }

            if (this.ActiveNodeCount == 1 && !_nodeConfiguration.IsTrackingEnabled)
            {
                return _activeNodes[0];
            }

            return _activeNodes.ElementAt(_cache.GetCurrentNodeIndex());
        }

        public void MoveToNextNode()
        {
            if (this.ActiveNodeCount == 0)
            {
                throw new NetworkNodeException("None of the nodes are active at the moment. Please try again later.");
            }

            if (this.ActiveNodeCount == 1 && !_nodeConfiguration.IsTrackingEnabled)
            {
                return;
            }

            int currentNodeIndex = _cache.GetCurrentNodeIndex();

            int nextNodeIndex = (currentNodeIndex + 1) % _activeNodes.Count;

            _cache.SetCurrentNodeIndex(nextNodeIndex);
        }

        public void Reset()
        {
            if (_nodeConfiguration.IsTrackingEnabled)
            {
                foreach (NetworkNodeSettings node in _activeNodes)
                {
                    node.TrackingState = TrackingState.Online;
                }
            }
        }

        public void SetTrackingState(NetworkNodeSettings node, TrackingState state)
        {
            if (_nodeConfiguration.IsTrackingEnabled)
            {
                node.TrackingState = state;
            }
        }

        public bool ShouldBreak()
        {
            if (!_nodeConfiguration.IsTrackingEnabled)
            {
                return true;
            }

            return this.ActiveNodeCount > 0;
        }

        private List<NetworkNodeSettings> GetActiveNodes(BalancerConfiguration config)
        {
            foreach (KeyValuePair<string, NetworkNodeSettings> node in config.Nodes)
            {
                node.Value.Name = node.Key;
            }

            return config
                .Nodes
                .Where(x => x.Value.IsActive)
                .OrderBy(x => x.Value.Priority)
                .Select(x => x.Value)
                .ToList();
        }
    }
}