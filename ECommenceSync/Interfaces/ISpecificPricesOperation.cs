using System.Collections.Generic;

namespace ECommenceSync.Interfaces
{

    /// <summary>
    /// Representa la intefaz para una operacón que soncroniza precios especificos
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface ISpecificPricesOperation<TKey>
        where TKey:struct
    {
        Dictionary<string, TKey> GetPricesToSync();
    }
}
