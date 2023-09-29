export const navigation = [
  {
    text: 'Inicio',
    path: '/home',
    icon: 'home'
  },
  {
    text: 'Facturación',
    icon: ['fas', 'receipt'],
    items: [
      {
        text: 'Fac. Electronica',
        icon: ['fas', 'download'],
        items: [
          {
            text: 'Facturas',
            path: '/facturacion/facturacion-electronica/facturas',
            icon: ['fas', 'file-invoice-dollar'],
          },
          {
            text: 'Notas de Crédito',
            path: '/facturacion/facturacion-electronica/notascredito',
            icon: ['fas', 'file-invoice-dollar']
          },
          {
            text: 'Retenciones',
            path: '/facturacion/facturacion-electronica/retenciones',
            icon: ['fas', 'file-invoice-dollar']
          }
        ]
      }
    ]
  },
  {
    text: 'Antivirus',
    icon: ['fas', 'shield-alt'],
    items: [
      {
        text: 'Configuración',
        path: '/antivirus/configuracion',
        icon: ['fas', 'sliders-h'],
      },
      {
        text: 'Renovaciones',
        path: '/antivirus/renovaciones',
        icon: ['fas', 'clipboard-check'],
      },
    ]
  },
  {
    text: 'Guias Transporte',
    icon: ['fas', 'truck'],
    items: [
      {
        text: 'Configuración',
        path: '/guias/configuracion',
        icon: ['fas', 'sliders-h'],
      },
      {
        text: 'Guías',
        path: '/guias/guias',
        icon: ['fas', 'qrcode'],
      },
    ]
  }
  , {
    text: 'Inventario',
    icon: ['fas', 'pallet'],
    items: [
      {
        text: 'Configuración',
        path: '/inventario/configuracion',
        icon: ['fas', 'sliders-h'],
      },
      {
        text: 'Codificación',
        path: '/inventario/codificacion',
        icon: ['fas', 'barcode'],
      },
      {
        text: 'Productos sin imagen',
        path: '/inventario/herramientas/productos-sin-imagen',
        icon: ['fas', 'image']
      },
      {
        text: 'Problemas',
        path: '/inventario/herramientas/productos-con-problemas',
        icon: ['fas', 'exclamation']
      }
    ]
  },
  {
    text: 'Prestashop',
    icon: ['fas', 'shopping-cart'],
    items: [
      {
        text: 'Pedidos',
        path: '/tiendas/prestashop/pedidos',
        icon: ['fas', 'file-invoice-dollar']
      },
      {
        text: 'Errores',
        path: '/tiendas/prestasop/errores-sincronizacion',
        icon: ['fas', 'exclamation-circle']
      }
    ]
  },
  {
    text: 'Tiendas en línea',
    icon: ['fas', 'shopping-cart'],
    items: [
      {
        text: 'Estado',
        path: '/tiendas/estado',
        icon: ['fas', 'info-circle']
      },
      {
        text: 'Mercado Libre',
        path: '/tiendas/mercadolibre',
        icon: ['fas', 'shopping-cart'],
      },
      {
        text: 'Configuración',
        path: '/tiendas/configuracion',
        icon: ['fas', 'sliders-h'],
      },
    ]
  }
];
