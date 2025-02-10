namespace RabbitmqSubscriber.Services
{
    public interface IRosContractor
    {
        Task CallTheFunctionAsync(string parameters);
        Task GazeboContractor(string dataString);
    }
}