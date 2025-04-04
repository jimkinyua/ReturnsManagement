namespace Returns.Helpers.Interfaces
{
    public interface IAdditionalInformationRequestService
    {
        Task RequestAdditionalInformationAsync(string returnId, string requestingUserId, string additionalInfoDetails);
    }
}
