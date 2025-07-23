namespace Returns.Helpers.Interfaces
{
    public interface IReturnAmendmentPolicy
    {
        bool CanAutoAmend(DateTime today);
    }
}
