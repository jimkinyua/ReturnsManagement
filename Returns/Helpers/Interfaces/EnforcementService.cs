namespace Returns.Helpers.Interfaces
{
    public interface EnforcementService
    {
        void PushToEnforcementModule(object enforcementData);
        string GetEnforcementReport();
    }
}
