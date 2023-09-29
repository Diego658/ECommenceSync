using AutomatizerInterop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatizerInterop.WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
    }
}
