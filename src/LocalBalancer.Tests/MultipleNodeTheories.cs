using Xunit;

namespace LocalBalancer.Tests
{
    public partial class MultipleNodeTheories : TheoryData<MultipleNodeTheoryData, string>
    {
        public MultipleNodeTheories()
        {
            this.SingleNodeRetryTests();
            this.SingleNodeCircuitsEnabled();

            this.MultiNodeRetryTest();
        }
    }
}