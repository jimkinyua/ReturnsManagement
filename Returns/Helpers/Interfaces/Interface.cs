namespace Returns.Helpers.Interfaces
{
    public interface IReturnAmendmentPolicy
    {
        public bool CanAutoAmend(DateTime today)
        {
            // Allow automatic amendments only on or before the 15th of the month
            return today.Day <= 15;
        }
    }
}
