# Load Tests - Pruebas de Carga y Estrés

Pruebas de carga y estrés para endpoints de alta prioridad del ecommerce.

## 📋 Estructura

```
LoadTests/
├── ProductEndpointLoadTests.cs    → Pruebas GET /api/Producto (catálogo)
├── OrderEndpointLoadTests.cs      → Pruebas POST/GET /api/Order
├── SearchEndpointLoadTests.cs     → Pruebas /api/Producto/buscar y /categoria
└── README.md
```

## 🚀 Cómo Ejecutar

### Requisitos

1. **Servidor Backend ejecutándose**
   ```bash
   cd backend/APIEccomerce
   dotnet run
   # Debería estar en http://localhost:5000
   ```

2. **Base de datos con datos de prueba** (al menos algunos productos)

3. **Token JWT** (para pruebas de Order)
   - Obtener haciendo login: `POST /api/Access/login`
   - Reemplazar `AuthToken` en `OrderEndpointLoadTests.cs`

### Ejecutar Todas las Pruebas

```bash
dotnet test --filter "Category=LoadTests" --verbosity normal
```

### Ejecutar una Categoría Específica

```bash
# Solo Products
dotnet test --filter "FullyQualifiedName~ProductEndpointLoadTests" --verbosity normal

# Solo Orders
dotnet test --filter "FullyQualifiedName~OrderEndpointLoadTests" --verbosity normal

# Solo Search
dotnet test --filter "FullyQualifiedName~SearchEndpointLoadTests" --verbosity normal
```

### Ejecutar una Prueba Individual

```bash
dotnet test --filter "Name=GetProductos_DeberiaSoportar_500_RequestsPerSecond" --verbosity normal
```

## 📊 Métricas Verificadas

| Métrica | Descripción | Objetivo |
|---------|-------------|----------|
| **RPS** | Requests por segundo | >500 para GET, >100 para POST |
| **P95 Latency** | 95% de requests responden en X ms | <200ms (GET), <500ms (POST) |
| **P99 Latency** | 99% de requests responden en X ms | <500ms (GET), <1000ms (POST) |
| **Error Rate** | Porcentaje de fallos | <1% para GET, <2% para POST |
| **Success Rate** | Porcentaje de éxito bajo estrés | >95% |

## 🔧 Configuración

### Ajustar Carga

En cada test, modifica `Simulation.KeepConstant(copies: X, during: TimeSpan.FromSeconds(Y))`:

- `copies`: Número de usuarios simultáneos
- `during`: Duración de la prueba

**Ejemplo: 200 usuarios por 60 segundos**

```csharp
.WithLoadSimulations(
    Simulation.KeepConstant(copies: 200, during: TimeSpan.FromSeconds(60))
)
```

### Rampa de Carga (Stress Test)

Para simular aumento gradual de usuarios:

```csharp
.WithLoadSimulations(
    Simulation.RampUp(copies: 1000, during: TimeSpan.FromSeconds(60))
)
```

## 🎯 Escenarios por Endpoint

### 1. GET /api/Producto (Catálogo - Público)

**Pruebas:**
- ✅ 500+ req/seg
- ✅ P95 latency <200ms
- ✅ P99 latency <500ms
- ✅ Error rate <1%
- ✅ Soporta 1000 usuarios simultáneos

**Por qué:** Endpoint más visitado, acceso público, puede recibir picos de tráfico.

### 2. POST /api/Order (Crear Orden - Crítico)

**Pruebas:**
- ✅ 100+ órdenes/seg
- ✅ P95 latency <500ms
- ✅ Error rate <2%
- ✅ Soporta 200 usuarios simultáneos

**Por qué:** Misión crítica, impacta ingresos, requiere validaciones/BD.

### 3. GET /api/Producto/buscar (Búsqueda - Pesada)

**Pruebas:**
- ✅ 300+ búsquedas/seg
- ✅ P95 latency <300ms
- ✅ Error rate <1%
- ✅ Soporta 500 usuarios simultáneos

**Por qué:** Puede ser cara (LIKE SQL), índices críticos.

## ⚠️ Notas Importantes

1. **Skip Attribute**
   - Los tests están marcados con `[Skip]` para no ejecutarse automáticamente
   - Requieren servidor ejecutándose manualmente
   - Descomenta el `[Skip]` cuando quieras ejecutar

2. **Autenticación**
   - `OrderEndpointLoadTests` requiere token JWT válido
   - Obtén uno haciendo login y reemplaza en la línea:
     ```csharp
     private const string AuthToken = "tu_token_aqui";
     ```

3. **Timeout**
   - Las pruebas de stress aumentan el timeout a 10-15 segundos
   - Si tienes timeouts, verifica la salud del servidor

4. **Base de Datos**
   - Las pruebas de creación de órdenes impactarán la BD
   - Considera usar una BD de prueba
   - Prepara datos válidos (addressId, productId, etc.)

## 📈 Cómo Interpretar Resultados

```
Scenario: get_productos
─────────────────────────────────────────
RPS: 523.4 requests/sec ✓ (>500)
Ok: 15,702
Fail: 42
Latency P50: 45.2 ms
Latency P95: 198.3 ms ✓ (<200ms)
Latency P99: 489.1 ms ✓ (<500ms)
Error Rate: 0.27% ✓ (<1%)
```

## 🐛 Troubleshooting

### "Connection refused" (127.0.0.1:5000)
→ Asegúrate de que el backend está ejecutándose en el puerto 5000

### "401 Unauthorized" (Orders)
→ El token JWT es inválido o expiró. Obtén uno nuevo.

### "429 Too Many Requests"
→ El servidor está bloqueando por rate limiting (normal en stress tests)

### Latencia muy alta
→ Aumenta el tiempo de la prueba (during) o reduce usuarios simultáneos

## 📝 Próximos Pasos

1. Ejecutar tests de forma manual inicialmente
2. Monitorear Application Insights / logs durante las pruebas
3. Identificar cuellos de botella (BD, red, CPU)
4. Optimizar (indexes, caché, scaler out)
5. Integrar en CI/CD (post-deployment testing)

## 📚 Referencias

- [NBomber Docs](https://nbomber.com/)
- [Performance Testing Best Practices](https://nbomber.com/docs/getting-started/tutorial)
- [Load Testing Strategy](https://en.wikipedia.org/wiki/Load_testing)
