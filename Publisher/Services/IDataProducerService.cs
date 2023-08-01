using Contracts.Models;

namespace Publisher.Services
{
    public interface IDataProducerService
    {
        IList<Joystic> GetJoysticData();
    }
}
