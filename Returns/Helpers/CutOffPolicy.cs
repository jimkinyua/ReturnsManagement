using Returns.Helpers.Interfaces;

namespace Returns.Helpers
{
    public class CutOffPolicy : IReturnAmendmentPolicy
    {
        public bool CanAutoAmend(DateTime today) => today.Day < 15;
    }
}
