using Contracts.Models;

namespace Publisher.Services
{
    public interface ISqLiteRepo
    {
        void ClearAllJoystics();
        List<Joystic> GetAllJoystics();
        Joystic GetJoysticById(int id);
        void InsertAllJoystics(IList<Joystic> joystics);
        void InsertJoystic(Joystic joystic);
    }
}
