using Contracts.Models;

namespace Publisher.Services
{
    public interface ISqLiteRepo
    {
        void ClearAllJoysticks();
        List<Joystick> GetAllJoysticks();
        Joystick GetJoystickById(int id);
        void InsertAllJoysticks(IList<Joystick> Joysticks);
        void InsertJoystick(Joystick Joystick);
    }
}
