namespace RESTClient.Services
{
    public interface IRosContractor
    {
        Task GazeboContractor(string dataString);
    }
}