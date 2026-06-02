namespace SGE.Services
{
    public static class FinanzasFormat
    {
        public static string Money(decimal amount)
        {
            return string.Format("S/ {0:N2}", amount);
        }

        public static string Money(decimal? amount)
        {
            return amount.HasValue ? string.Format("S/ {0:N2}", amount.Value) : "S/ 0.00";
        }
    }
}
