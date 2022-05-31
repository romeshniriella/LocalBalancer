namespace LocalBalancer
{
    public interface ICacheHelper
    {
        int GetCurrentNodeIndex();

        void SetCurrentNodeIndex(int index);
    }
}