using BillingPayment.Interfaces;

namespace BillingPayment.Providers
{
    public class DefaultRandomProvider : IRandomProvider
    {
        private readonly Random _random = new();
        public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
    }
}
