import {
  ProfilePage,
  FacturasPage,
  NotascreditoPage,
  RetencionesPage,
  HomePage
} from './pages';
import { ConfiguracionTiendas } from './pages/tiendas/configuracion/configuracion'
import { ErroresSincronizacion } from './pages/tiendas/errores-sincronizacion/errores-sincronizacion'
import { ProductosSinImagen } from './pages/inventario/herramientas/productos-sin-imagen/productos-sin-imagen';
import { AntivirusLicenciasRenovacion } from './pages/antivirus/licencias/antivirus-licencias-renovacion';
import InventarioCodificacion  from './pages/inventario/codificacion/inventario-codificacion'
import { InventarioConfiguracion } from './pages/inventario/configuracion/inventario-configuracion';
import { AntivirusConfiguracion } from './pages/antivirus/configuracion/configuracion';
import PedidosPrestashop from './pages/tiendas/prestashop/pedidos/pedidos';
import { GuiasTransporte } from './pages/guias-transporte/guias-transporte';
import GuiasTransporteConfiguration from './pages/guias-transporte/guias-guiatransporte-configuracion';
import ProductosConProblemas from './pages/inventario/herramientas/productos-con-problemas/productosConProblemas';

export default [
  {
    path: '/profile',
    component: ProfilePage
  }
  ,
  {
    path: '/facturacion/facturacion-electronica/facturas',
    component: FacturasPage
  },
  {
    path: '/facturacion/facturacion-electronica/notascredito',
    component: NotascreditoPage
  },
  {
    path: '/facturacion/facturacion-electronica/retenciones',
    component: RetencionesPage
  },
  {
    path: '/home',
    component: HomePage
  },
  {
    path: '/antivirus/configuracion',
    component: AntivirusConfiguracion
  },
  {
    path: '/antivirus/renovaciones',
    component: AntivirusLicenciasRenovacion
  },
  {
    path: '/tiendas/configuracion',
    component: ConfiguracionTiendas
  },
  {
    path: '/tiendas/prestashop/pedidos',
    component: PedidosPrestashop
  },
  {
    path: '/tiendas/prestasop/errores-sincronizacion',
    component: ErroresSincronizacion
  },
  {
    path: '/inventario/herramientas/productos-sin-imagen',
    component: ProductosSinImagen
  },
  {
    path: '/inventario/codificacion',
    component: InventarioCodificacion
  },
  {
    path: '/inventario/configuracion',
    component: InventarioConfiguracion
  },
  {
    path: '/inventario/herramientas/productos-con-problemas',
    component: ProductosConProblemas
  },
  {
    path: '/guias/guias',
    component: GuiasTransporte
  },
  {
    path: '/guias/configuracion',
    component: GuiasTransporteConfiguration
  }
];
