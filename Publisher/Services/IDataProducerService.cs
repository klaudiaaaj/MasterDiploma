using Contracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace Publisher.Services
{
    public interface IDataProducerService
    {
        IList<Joystic> GetJoysticData();
    }
}
