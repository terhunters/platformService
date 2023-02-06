using System.Collections.Generic;
using WebApplication3.Models;

namespace WebApplication3.Data
{
    public interface IPlatformRepo
    {
        bool SaveChanges();

        IEnumerable<Platform> GetAllPlatforms();

        Platform GetPlatformById(int id);

        void CreateNewPlatform(Platform plat);
    }
}