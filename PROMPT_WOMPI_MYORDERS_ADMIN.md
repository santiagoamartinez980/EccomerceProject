# Prompt: Integración Wompi, My Orders y Admin Order Management

## 1. INTEGRACIÓN WOMPI (Payment Gateway)

### 1.1 Configuración Backend (C#)

**WompiService.cs** - Crear nuevo servicio:
- Constructor inyecta `IConfiguration` para obtener credenciales Wompi (API Key, Public Key)
- Método `CreatePaymentTransaction()`: 
  - Recibe `orderId`, `amount`, `reference`
  - Llama API Wompi para crear transacción
  - Retorna `WompiTransactionDto` con `transactionId`, `publicData` (para mostrar en frontend)
- Método `ValidatePayment()`:
  - Recibe webhook data de Wompi
  - Verifica firma (validar con API key)
  - Retorna true/false si es válido
- Método `GetTransactionStatus()`:
  - Consulta estado de transacción en Wompi
  - Retorna estado actual (APPROVED, DECLINED, PENDING, etc.)

**PaymentController.cs** - Nuevo endpoint:
- `POST /api/Payment/create-transaction` → crea transacción en Wompi
- `POST /api/Payment/webhook` → recibe confirmación de pago de Wompi
  - Al confirmarse pago: llama `OrderService.ConfirmPayment(orderId)` (ya existe)
  - Cambia estado Order de Pending → Paid
  - Descuenta stock

**WompiWebhookDto.cs** - DTO para webhook:
- `transactionId`, `status`, `amount`, `reference`, `signature`

### 1.2 Configuración Frontend (Angular)

**payment.service.ts** - Nuevo servicio:
- Inyecta `HttpClient`
- Método `createTransaction(orderId, amount)`: POST al backend
- Método `getTransactionStatus(transactionId)`: GET status

**checkout.component.ts** - Modificar:
- Después de `createOrder()`, llamar `paymentService.createTransaction()`
- Mostrar iframe/redirect de Wompi para pago
- Después de confirmación, navegar a `/my-orders`

---

## 2. MY ORDERS PAGE

### 2.1 Backend Endpoints (Existen, pero verificar)

- `GET /api/Order` → listar órdenes del usuario (ya existe)
- `GET /api/Order/{id}` → detalle de orden (ya existe)

### 2.2 Frontend: my-orders Component

**Estructura**:
```
/features/my-orders/
├─ pages/
│  └─ my-orders-list/
│     ├─ my-orders-list.ts
│     ├─ my-orders-list.html
│     ├─ my-orders-list.css
│  └─ my-orders-detail/
│     ├─ my-orders-detail.ts
│     ├─ my-orders-detail.html
│     ├─ my-orders-detail.css
├─ services/
│  └─ order.service.ts
└─ my-orders.routes.ts
```

**my-orders-list.ts**:
- Signal: `orders: signal<OrderDto[]>([])`
- Signal: `loading: signal(boolean)`
- `ngOnInit()`: llamar `orderService.getOrders()`
- Mostrar tabla/cards con:
  - Orden ID, fecha, estado (color badge), total
  - Botón "Ver detalle"
  - Botón "Cancelar" (si estado = Pending)

**my-orders-detail.ts**:
- Inyectar `ActivatedRoute` para obtener `orderId`
- Signal: `order: signal<OrderDto | null>(null)`
- Mostrar:
  - Datos orden (ID, fecha, estado, total)
  - Dirección de entrega
  - Items (productos, cantidades, precios)
  - Timeline visual: Pending → Paid → Shipped → Delivered

---

## 3. ADMIN ORDER MANAGEMENT

### 3.1 Backend Endpoints (Existen, pero pueden mejorar)

- `GET /api/Order` → list all orders (admin solo)
- `PATCH /api/Order/{id}/status` → cambiar estado (admin solo)
- Agregar: `GET /api/Order/admin/stats` → órdenes por estado, ingresos totales

**Restricciones**:
- Agregar `[Authorize(Roles = "Admin")]` a endpoints

### 3.2 Frontend: Admin Orders Component

**Estructura**:
```
/features/admin/
└─ pages/
   └─ order-management/
      ├─ order-management.ts
      ├─ order-management.html
      ├─ order-management.css
├─ services/
   └─ admin-order.service.ts
└─ admin.routes.ts
```

**order-management.ts**:
- Signal: `orders: signal<OrderDto[]>([])`
- Signal: `stats: signal<OrderStats>()` → total órdenes, ingresos, por estado
- Signal: `filterStatus: signal<OrderStatus | 'ALL'>()`
- Signal: `updatingOrderId: signal<number | null>()`
- `ngOnInit()`: cargar todas las órdenes
- Método `updateOrderStatus(orderId, newStatus)`:
  - Llama backend PATCH
  - Actualiza signal localmente
  - Muestra snackbar de confirmación
- Método `exportOrders()`: descargar CSV

**order-management.html**:
- **Stats row** (superior):
  - Total órdenes, ingresos totales, órdenes por estado
- **Filtros**:
  - Dropdown: All / Pending / Paid / Shipped / Delivered / Cancelled
  - Date range picker (opcional)
- **Tabla** con columnas:
  - Order ID
  - Customer (nombre/email)
  - Address (ciudad, departamento)
  - Status (con select dropdown para cambiar)
  - Total
  - Created Date
  - Acciones: Ver detalle, Cambiar estado

---

## 4. FLUJO COMPLETO

### Usuario:
1. Checkout → selecciona dirección, ve resumen
2. Click "Crear pedido" → orden creada en estado Pending
3. Redirect a Wompi (iframe/redirect)
4. Pago confirmado → webhook backend
5. Backend: Order status Pending → Paid, descuenta stock
6. Redirect a `/my-orders`
7. Usuario ve orden con estado Paid

### Admin:
1. Login con rol Admin
2. Va a `/admin/orders`
3. Ve tabla con todas las órdenes
4. Click en dropdown de status → selecciona "Shipped"
5. Orden actualiza automáticamente a Shipped
6. Usuario recibe notificación (opcional: email/push)

---

## 5. MODELOS/DTOs NECESARIOS

**WompiTransactionDto**:
```
- transactionId: string
- status: string
- amount: decimal
- reference: string
- publicData: object (para frontend)
```

**OrderStats**:
```
- totalOrders: int
- totalRevenue: decimal
- byStatus: { Pending, Paid, Shipped, Delivered, Cancelled }
```

**OrderManagementDto** (extend OrderDto):
```
- customerEmail: string
- customerName: string
- addressCity: string
- addressDepartment: string
- itemCount: int
```

---

## 6. CHECKLIST IMPLEMENTACIÓN

- [ ] Crear `WompiService.cs` en backend
- [ ] Crear `PaymentController.cs` endpoints
- [ ] Agregar credenciales Wompi en `appsettings.json`
- [ ] Crear `payment.service.ts` en frontend
- [ ] Modificar `checkout.component.ts` para integrar Wompi
- [ ] Crear `/features/my-orders` componentes (list + detail)
- [ ] Crear `/features/admin/order-management` componentes
- [ ] Agregar rutas en `app.routes.ts`
- [ ] Agregar `[Authorize(Roles = "Admin")]` a endpoints admin
- [ ] Crear admin menu/navbar para acceder a order management
- [ ] Testing flujo completo: checkout → pago → órdenes visibles
- [ ] (Opcional) Agregar notificaciones (email/push) al cambiar status

---

## 7. CONSIDERACIONES IMPORTANTES

- **Webhook signature validation**: Wompi envía firma en webhook — SIEMPRE validarla
- **Idempotencia**: Wompi puede reenviar webhook múltiples veces → usar `transactionId` como key
- **Stock management**: Descuento stock SOLO cuando pago confirmado (ya en `ConfirmPayment`)
- **User isolation**: Endpoint `/api/Order` debe filtrar por usuario autenticado (usuario no puede ver órdenes ajenas)
- **Admin isolation**: Endpoint admin `/api/Order/admin/all` SÍ debe mostrar todas (con rol Admin)
- **Error handling**: Si Wompi falla → order queda Pending, usuario puede reintentar pago
