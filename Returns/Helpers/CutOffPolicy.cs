using Returns.Helpers.Interfaces;

namespace Returns.Helpers
{
    public class CutOffPolicy : IReturnAmendmentPolicy
    {
        public bool CanAutoAmend(DateTime today) => true; //today.Day < 15;
    }
}
