namespace BillingPayment.Interfaces
{
    public interface IRandomProvider
    {
        int Next(int minValue, int maxValue);
    }
}
