namespace Returns.Helpers
{
    public class Constants
    {
        public class SaccoType
        {
            public const string DepositTaking = "0";
            public const string NWDT = "1"; // Non-Withdrawable Deposit Taking
        }
    }

    public static class InspectionModuleConstants
    {
        public const string BaseUrl = "https://sasra-backend.agilebiz.co.ke/gateway";
        public const string ModulesRequestsEndpoint = "/api/inspection/modules-requests";
        public const string FullModulesRequestsUrl = BaseUrl + ModulesRequestsEndpoint;
        public const string SourceType = "Returns";
    }
}
