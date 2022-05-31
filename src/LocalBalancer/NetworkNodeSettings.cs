using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LocalBalancer
{
    public enum TrackingState
    {
        Online,
        Offline
    }

    public class NetworkNodeSettings
    {
        /// <summary>
        /// Authentication headers configuration
        /// </summary>
        [Required]
        [MinLength(1)]
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Is this node active or not?
        /// </summary>
        [Required]
        public bool IsActive { get; set; }

        /// <summary>
        /// Give it a name. good for logging and debugging
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// URI of the network node
        /// </summary>
        [Required]
        public Uri NetworkAddress { get; set; }

        /// <summary>
        /// Nodes will be sorted by their priority (ie: 1=highest, 10=lowest)
        /// </summary>
        [Required]
        public int Priority { get; set; }

        /// <summary>
        /// Controls the tracking state.
        /// Imagine a scenario where a sattelite blows up because of a solar flare!
        /// We'd wanna take it offline.
        /// </summary>
        public TrackingState TrackingState { get; internal set; }

        /// <summary>
        /// gives you a nice debug output.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.TrackingState}: {this.NetworkAddress}";
        }
    }
}