using System.Threading.Tasks;

namespace Convey.Persistence.OpenStack.OCS.Auth
{
    internal interface IAuthManager
    {
        Task<AuthData> Authenticate();
    }
}