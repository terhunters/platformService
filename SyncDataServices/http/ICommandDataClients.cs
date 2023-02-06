using System.Threading.Tasks;
using WebApplication3.DTOs;

namespace WebApplication3.SyncDataServices.http
{
    public interface ICommandDataClients
    {
        Task SendPlatformToCommand(PlatformReadDto platform);
    }
}