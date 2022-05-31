using System;

namespace LocalBalancer
{
    public class NetworkNodeException : Exception
    {
        public NetworkNodeException(string message)
            : base(message)
        {

        }
    }
}