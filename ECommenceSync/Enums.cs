namespace ECommenceSync
{
    public enum Operations
    {
        Taxes,
        CustomersGroups,
        Customers,
        Products,
        ProductsCategories,
        ProductFeatures,
        ProductImages,
        ProductStocks,
        Orders,
        Invoices,
        Shipping,
        Brands,
        ProductPrices,
        Attributes,
        AttributesTerms,
        Tags,
        ProductTags,
        Addresses,
        ProductsVariations,
        Carriers = 100,
        OrdersStates = 101,
        CustomerServiceThreads=102,
        CronRequest=200
    }

    public enum OperationModes
    {
        Automatic,
        Manual,
        Proccesor
    }
    /// <summary>
    /// 
    /// </summary>
    public enum OperationDirections
    {
        /// <summary>
        /// Operaciones que migran información desde un ERP hacia la tienda, solo es necesario implementar la lógica de enlaces, los valores modificados en la tienda siempre serán reemplazados por los recuperados del ERP. 
        /// </summary>
        ErpToStore,
        /// <summary>
        /// Operaciones de procesamiento de datos del ERP, no tiene ninguna conexión con la tienda, solo procesa información que ya existe en la base del ERP (O base intermedia de sincronización del ERP), un ejemplo sería una operación que procesa la creación de clientes a partir de los datos que ya descargo otra operación.
        /// </summary>
        ErpToErp,
        /// <summary>
        /// Operaciones que migran información desde la tienda hacia un ERP, solo es necesario implementar la lógica de enlaces, los valores modificados en el ERP siempre serán reemplazados por los recuperados de la tienda (La tienda no puede saber que datos necesita el ERP, simplemente sincroniza todos los datos que dispone). Ejemplo: datos de clientes registrados en el sitio web, lo lógico seria coger los datos que nos dan los clientes en la página, no modificarlos.
        /// </summary>
        StoreToErp,
        /// <summary>
        /// Operaciones de procesamiento de datos de la tienda, no tiene ninguna conexión con el ERP, solo procesa información que ya existe en la tienda o en la base intermedia de la tienda, un ejemplo sería una operación que deshabilita productos sin existencia.
        /// </summary>
        StoreToStore,
        /// <summary>
        /// Operaciones que comunican directamente la tienda con el ERP y que admiten actualización bidireccional, un ejemplo sería una operación que sincroniza las órdenes y recupera información acerca de estas hacia el ERP, pero también información del ERP hacia la tienda como la información de facturación o envíos para las órdenes.
        /// </summary>
        TwoWay
    }


    public enum SyncWorkerStatus
    {
        NotConfigured,
        Configuring,
        Configured,
        Starting,
        Started,
        StartingSynchronization,
        Synchronizing,
        SynchronizationCompleted,
        WaitingForNewSynchronization,
        Stopping,
        Stopped,

    }

    public enum OperationStatus
    {
        Created,
        Working,
        Stopped
    }

    public enum SyncResult
    {
        Created,
        Updated,
        NotSynchronizedPostponed,
        NotSynchronized,
        Error
    }

    public enum ImageTypes
    {
        Producto,
        Categoria,
        ProductoVariable
    }

    public enum ImageOperations
    {
        Add,
        Delete,
        Update,
        SetDefault
    }



}
