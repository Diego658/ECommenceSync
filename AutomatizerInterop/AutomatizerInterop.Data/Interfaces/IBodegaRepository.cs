using AutomatizerInterop.Data.Entities;
using AutomatizerInterop.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutomatizerInterop.Data.Interfaces
{
    public interface IBodegaRepository 
    {
        Task<List<Bodega>> GetBodegas();
        Task<List<BodegaItem>> GetBodegasItem(int itemID);
        Task<List<BodegaItemDetallado>> GetBodegasItemDetallado(int itemID);
    }
}
