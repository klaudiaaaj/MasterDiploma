namespace RabbitmqSubscriber.Services
{
    public interface IRosContractor
    {
        Task GazeboContractor(string dataString);
    }
}