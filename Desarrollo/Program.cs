using ECommenceSync.Prestashop.Helpers;
using Microsoft.Extensions.Configuration;
using System;

namespace Desarrollo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            var helper = new PrestashopDatabaseHelper(configuration);
            var ophelper = new PrestashopOperationsHelper(configuration);
            
            // var taskAddresses = new AddressesOperation(helper, ophelper);
            // //taskAddresses.Start();
            //
            // var paymentsOp = new OrdersOperation(helper, ophelper);
            // paymentsOp.Start();
            //
            // var customerThreadsOp = new CustomerServiceThreadsOperation(helper, ophelper);
            //customerThreadsOp.Start();

            Console.ReadKey();
        }
    }
}
