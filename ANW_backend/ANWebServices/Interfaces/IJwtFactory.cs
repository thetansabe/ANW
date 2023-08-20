using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ANWebServices.Interfaces
{
    public interface IJwtFactory
    {
        Task<string> GetToken(string userName, ClaimsIdentity id, int addMinute=0, int addHour=0);
        ClaimsIdentity GenerateId(string userName, string id, int permission);
    }
}
