using Contracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace Publisher.Services
{
    public interface ISender
    {
        public Task Send(IList<Joystick> message);
    }
}
