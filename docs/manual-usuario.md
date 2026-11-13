# Manual de usuario — tienda EccomerceProject

Este manual describe las pantallas disponibles para compradores y administradores en la versión actual del proyecto. La tienda muestra precios en pesos colombianos (COP). El encabezado permite cambiar entre español e inglés mediante el botón **ES/EN**.

## 1. Acceso a la tienda

1. Abra la dirección de la aplicación y seleccione **Catálogo** o **Inicio**. El catálogo puede consultarse sin iniciar sesión.
2. Para comprar, pulse **Iniciar sesión**. Escriba correo y contraseña y seleccione **Iniciar sesión**.
3. Si aún no tiene cuenta, pulse **Crear una cuenta**, complete nombre, apellidos, correo y contraseña y envíe el formulario. Después entre con sus datos.
4. Para salir, abra el menú de perfil (icono de persona) y elija **Cerrar sesión**.

El detalle de un producto, el carrito, las direcciones, el pago y los pedidos requieren una sesión iniciada. La aplicación dirige al inicio de sesión cuando intenta abrir una ruta protegida sin una sesión válida. El registro crea cuentas de comprador; no ofrece una opción para elegir el rol de administrador.

## 2. Buscar productos

1. En **Catálogo**, escriba el nombre o término de búsqueda en el buscador. Puede filtrar por las categorías que aparecen debajo.
2. Use el botón de vista para alternar entre cuadrícula y lista. El precio y las unidades disponibles aparecen junto a cada producto.
3. Seleccione un producto para ver su imagen, descripción, precio, categoría y disponibilidad.
4. Si tiene existencias, ajuste la cantidad con **+** y **−** y pulse **Agregar al carrito**. La cantidad no puede superar el stock mostrado.

Si la búsqueda no devuelve resultados, borre el texto o pulse **Limpiar filtros**. Si falla la carga del catálogo, use **Reintentar**.

## 3. Carrito

Abra el icono de bolsa en la barra superior. En el carrito puede cambiar cantidades, eliminar un producto o usar **Vaciar carrito** para eliminar todos los artículos tras confirmar la acción. El resumen muestra el precio de los artículos marcados y el botón **Proceder al pago** abre la pantalla de compra.

**Límite de la versión actual:** las casillas de selección modifican el total visible en el carrito, pero la pantalla de pago toma todos los artículos que permanecen en él. Si no desea comprar un artículo, elimínelo del carrito antes de continuar.

## 4. Direcciones de entrega

1. En el menú de perfil, elija **Mis direcciones** y pulse **Nueva dirección**.
2. Complete dirección, ciudad, departamento, código postal y teléfono de diez dígitos. Las notas adicionales son opcionales.
3. Si lo desea, marque **Dirección predeterminada** y guarde.
4. Desde la lista puede marcar otra como predeterminada, editarla o eliminarla.

Durante el pago debe elegir una dirección. Si todavía no tiene ninguna, el enlace de la pantalla de pago abre el formulario de nueva dirección y permite regresar al proceso.

## 5. Crear un pedido y pagar

1. En el carrito, revise los artículos que permanecen en él y pulse **Proceder al pago**.
2. Seleccione la dirección de entrega; la predeterminada se selecciona automáticamente si existe. Revise productos y total.
3. Pulse el botón de continuar/pagar. La aplicación crea el pedido y prepara una referencia de pago.
4. En la siguiente pantalla pulse **Pagar con Wompi Sandbox** y complete el flujo del proveedor de pagos de prueba.
5. Al volver a la aplicación, consulte la página de confirmación. Si el pago aún no aparece confirmado, use **Actualizar estado** o revise **Mis pedidos** más tarde.

El entorno mostrado por el botón es **Sandbox**: sirve para pruebas y no debe interpretarse como un cobro de producción. El pedido puede aparecer pendiente mientras se procesa la confirmación. No repita el pago solo porque la confirmación tarde unos momentos; revise primero el estado del pedido.

## 6. Consultar y cancelar pedidos

En el menú de perfil, abra **Mis pedidos** para ver los pedidos más recientes primero, su estado, dirección, productos y total. El estado se actualiza al volver a la pestaña y cada 15 segundos mientras permanezca visible. **Pendiente de despacho** indica que Wompi confirmó el pago; después puede cambiar a **Despachado** y **Entregado**. Solo puede pulsar **Cancelar pedido** mientras el pago siga pendiente. Confirme la cancelación después de verificar el número del pedido.

## 7. Panel de administrador

Una cuenta con rol **Admin** entra al panel después de iniciar sesión. El menú lateral contiene **Dashboard**, **Pedidos**, **Productos**, **Categorías** y **Copias de seguridad**. El botón del pie permite ir al catálogo o cerrar sesión. Las cuentas normales no pueden abrir estas pantallas. Consulte el [manual detallado del administrador](manual-administrador.md), con capturas y pasos de cada tarea.

### Pedidos

Abra **Pedidos** para filtrar por estado y revisar cliente, dirección, productos y total. Un pedido pagado queda **Pendiente de despacho**; puede marcarlo **Despachado** y luego **Entregado**, o devolverlo a pendiente si todavía no salió. Los pedidos pendientes de pago se pueden cancelar. La aplicación no realiza reembolsos automáticos de pedidos pagados.

### Productos

Abra **Productos** para consultar nombre, categoría, precio, stock y estado activo/inactivo. El buscador filtra por nombre o categoría.

- **Nuevo producto:** pulse el botón, complete nombre, descripción, precio en COP, stock y categoría. Puede subir una imagen y elegir si el producto queda activo. Guarde el formulario.
- **Editar:** pulse el icono de lápiz de la fila, cambie los datos y guarde.
- **Eliminar:** pulse el icono de papelera y confirme. La interfaz advierte que esta acción no se puede deshacer.

Un producto inactivo o sin stock no estará disponible para una compra normal.

### Categorías

Abra **Categorías**, escriba el nombre en **Nueva categoría** y pulse **Crear categoría**. La tabla muestra las categorías existentes y permite eliminarlas con el icono de papelera. Si una categoría está asociada a productos, el servidor puede impedir su eliminación; revise el mensaje de error y los productos vinculados.

### Copias de seguridad

1. Abra **Copias de seguridad** y pulse **Crear copia ahora**. Espere el mensaje de finalización.
2. En la lista de copias, pulse **Descargar** y guarde el archivo `.bak` en un almacenamiento diferente del servidor de la tienda.
3. Para volver a una copia que permanece en el servidor, pulse **Restaurar** en la fila correcta y confirme. Esta acción reemplaza la base actual por el contenido de esa copia y cierra las conexiones activas. Después debe volver a iniciar sesión.

Las copias del panel son manuales. La persona que opera la tienda debe conservar copias recientes fuera del servidor. Si se pierde la base de datos, también se pierde el registro de usuarios administradores y puede ser imposible entrar al panel: en ese caso un operador con acceso al servidor debe usar el procedimiento externo descrito en [Copias de seguridad](copias-de-seguridad.md). No publique ni comparta los archivos `.bak`, porque contienen datos de la tienda y de sus usuarios.

## 8. Problemas frecuentes

| Situación | Qué hacer |
| --- | --- |
| No puedo entrar a una pantalla protegida | Inicie sesión de nuevo. Si la sesión expiró, la aplicación le enviará al formulario de acceso. |
| El correo o la contraseña no permiten entrar | Revise los datos escritos e intente otra vez. Si persiste, contacte a quien administra la tienda; la interfaz no incluye recuperación de contraseña. |
| No puedo agregar un producto al carrito | Compruebe que inició sesión y que el producto tiene stock suficiente. |
| No puedo continuar al pago | Compruebe que el carrito contiene artículos y que seleccionó una dirección de entrega. |
| El pago no aparece confirmado inmediatamente | Actualice el estado en la confirmación o consulte **Mis pedidos**; la confirmación puede tardar. |
| No se abre Wompi Sandbox | Compruebe la conexión a Internet y vuelva a intentarlo desde la pantalla de pago. |
| No se puede cancelar un pedido | El servidor solo permite cancelar pedidos pendientes. Revise su estado actual en **Mis pedidos**. |
| El panel de copias muestra un error de configuración o permisos | Solicite al responsable técnico que revise la ruta de copias y los permisos de SQL Server y de la API. |
| La base de datos se perdió | Solicite al operador del servidor la restauración externa de un archivo `.bak` conservado fuera de la aplicación. |

## Alcance de este manual

Las pantallas visibles no incluyen recuperación de contraseña, números de guía de transporte ni gestión de usuarios desde el panel. La vista de confirmación contiene textos informativos sobre notificación y seguimiento, pero este manual no los trata como servicios implementados.
