using AutomatizerInterop.Facturacion;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace Desarrollo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Task.Run(async () => await CrearFactura());
            Console.ReadKey();
        }

        static async Task CrearFactura()
        {
            try
            {
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddJsonFile("appsettings.json");
                var configuration = configurationBuilder.Build();
                var facHelper = new FacturacionHelper(configuration);
                var factura = FacturaVenta.CrearNueva(facHelper, "01", "Fl", 3, "CHRIS");
                using var sqlConex = facHelper.GetDbConnection();
                await sqlConex.OpenAsync();

                await factura.AgregarItem(5421, 1, 22.32m, 0, "B1");
                await factura.AgregarItem(8630, 1, 57.14m, 0, "B1");
                await factura.AgregarItem(3853, 1, 46.83m, 0, "B1");
                await factura.AgregarServicio("SRV_006", 1, 1.39m, 0, "");
                await factura.AgregarServicio("SRV_020", 1, 24.11m, 0, "");

                //await factura.AgregarItem(7475, 2, 32.035800m, 0, "B1");
                //await factura.AgregarItem(7025, 1, 10.491100m, 0, "B1");
                //await factura.AgregarServicio("SRV_032", 1, 7, 0, "");
                //await factura.AgregarItem(3599, 1, 26.000000m, 0, "B1");
                //await factura.AgregarItem(4065, 1, 102.678600m, 0, "B1");
                //await factura.AgregarItem(4066, 1, 88.445000m, 0, "B1");
                //await factura.AgregarItem(4403, 1, 218.750000m, 0, "B1");
                //await factura.AgregarItem(6959, 1, 35.625000m, 0, "B1");
                //await factura.AgregarItem(7489, 1, 19.596700m, 0, "B1");
                //await factura.AgregarItem(7524, 1, 32.269200m, 0, "B1");
                //await factura.AgregarItem(7534, 1, 19.592500m, 0, "B1");
                //await factura.AgregarItem(8184, 1, 91.830400m, 0, "B1");
                //await factura.AgregarItem(8366, 1, 397.024900m, 0, "B1");



                factura.Observaciones = $"Ref. {"UONEVNSQK"}";
                factura.Wrk = Environment.MachineName;
                factura.ClienteCodigo = "2569";
                factura.ClienteSucursal = 1;
                factura.VendedorCodigo = 3;
                await factura.Calcular();




                await factura.AgregarFormaPago(new FormaPagoCredito { NumeroDias = 0, NumeroPagos = 1, Valor = factura.Total });

                await factura.Guardar();

                




            }
            catch (Exception)
            {

                throw;
            }
            

        }

    }
}
