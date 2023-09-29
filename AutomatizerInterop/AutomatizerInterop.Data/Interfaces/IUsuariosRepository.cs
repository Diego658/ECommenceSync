using AutomatizerInterop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface IUsuariosRepository
    {
        User GetUser(string userName, string password);
    }
}
