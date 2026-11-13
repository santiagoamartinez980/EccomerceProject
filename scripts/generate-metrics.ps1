$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$template = Join-Path $root 'Metricas AgroRuta.xlsx'
$output = Join-Path $root 'Metricas Ecommerce.xlsx'

if (-not (Test-Path -LiteralPath $template)) { throw 'No se encontro la plantilla.' }
Copy-Item -LiteralPath $template -Destination $output -Force

# A/B is a verified checklist or an observed test result, never a target score.
$rows = @(
    @{ R=5;  M='Cobertura de modulos principales'; A=6; B=7; K='Presentes: acceso, catalogo, carrito, direcciones, pedidos, pagos y administracion de catalogo. Falta administracion de pedidos.'; L='Implementar listado y acciones de pedidos para Admin.' },
    @{ R=6;  M='Flujos principales verificados'; A=4; B=5; K='Verificados: registro/login, catalogo, carrito y checkout Wompi Sandbox. Falta flujo administrativo de pedidos.'; L='Agregar prueba de gestion administrativa de pedidos.' },
    @{ R=7;  M='Suficiencia funcional'; A=4; B=5; K='Catalogo, carrito, direcciones y pago cubren el caso principal; la gestion administrativa de pedidos sigue incompleta.'; L='Completar pantalla y API de pedidos administrativos.' },
    @{ R=8;  M='Integridad de reglas del pedido'; A=4; B=5; K='Creacion, persistencia, webhook y descuento de stock funcionaron en Sandbox. Cancelar pedido devuelve stock no reservado.'; L='Eliminar la devolucion de stock en cancelacion pendiente.' },
    @{ R=9;  M='Exactitud de calculos de compra'; A=4; B=5; K='Precio decimal, total, centavos Wompi y descuento de una unidad se comprobaron. Cantidades negativas siguen sin validarse.'; L='Validar cantidad positiva y total no negativo.' },
    @{ R=10; M='Precision monetaria'; A=5; B=5; K='Precio y total usan decimal; SQL usa decimal(18,2); Wompi recibe centavos enteros y moneda COP.'; L='Anadir pruebas de limites y redondeo monetario.' },
    @{ R=11; M='Intercambio de datos'; A=2; B=2; K='La API usa JSON REST y el pago recibe eventos JSON de Wompi; ambos se observaron en el flujo Sandbox.'; L='Versionar ejemplos de contrato para ambos formatos.' },
    @{ R=12; M='Compatibilidad de contratos'; A=4; B=5; K='Contratos de acceso, catalogo, carrito y direcciones se usaron. Frontend de pedidos usa Confirmed y omite Paid.'; L='Alinear estados y DTO de creacion de pedido.' },
    @{ R=13; M='Deteccion de fallas'; A=3; B=5; K='Hay logs, pruebas y Swagger. Falta health check de BD y hay ocho pruebas de integracion omitidas.'; L='Agregar salud de SQL y activar pruebas de integracion.' },
    @{ R=14; M='Pruebas backend completadas'; A=150; B=158; K='dotnet test sin LoadTests: 150 aprobadas, 8 omitidas, 0 fallidas. Las omitidas cuentan en el denominador.'; L='Habilitar las ocho pruebas de integracion y ejecutarlas en CI.' },
    @{ R=15; M='Prevencion de fallas'; A=4; B=6; K='Controles presentes: autorizacion, firma webhook, monto/moneda y referencia. Faltan validacion de pedido y control de concurrencia de stock.'; L='Validar entradas y actualizar pago, pedido y stock en transaccion.' },
    @{ R=16; M='Vistas principales disponibles'; A=6; B=7; K='Catalogo, detalle, carrito, direcciones, checkout y pedidos disponibles. Falta vista de pedidos administrativos.'; L='Agregar vista administrativa de pedidos.' },
    @{ R=17; M='Funciones evidentes'; A=5; B=6; K='Cinco recorridos tienen acciones visibles; la vista de pedidos no presenta correctamente el estado Paid.'; L='Mostrar etiqueta e icono correctos para Paid.' },
    @{ R=18; M='Comprensibilidad de estados'; A=4; B=5; K='Login, catalogo, carrito y checkout comunican sus estados. La lista de pedidos usa Confirmed en vez de Paid.'; L='Alinear mensajes de estado con el backend.' },
    @{ R=19; M='Informacion del flujo de compra'; A=4; B=5; K='Se muestran producto, total, referencia y pantalla de resultado; falta explicar el estado mientras llega el webhook.'; L='Anadir mensaje de procesamiento y actualizacion automatica del pedido.' },
    @{ R=20; M='Tiempo de respuesta p95'; A=$null; B=$null; K='No medido en esta entrega. Las pruebas de carga requieren un servidor y datos controlados; no se inventa un valor.'; L='Medir p95 con carga reproducible y registrar umbral y ambiente.' },
    @{ R=21; M='Rendimiento de procesamiento'; A=$null; B=$null; K='No se ejecuto carga de pedidos porque las pruebas actuales crean datos reales en SQL local.'; L='Ejecutar carga en BD aislada y registrar RPS y errores.' },
    @{ R=22; M='Preparacion de entrega tecnica'; A=2; B=2; K='Compilaron backend .NET y frontend Angular. Esto mide capacidad de compilacion, no rendimiento.'; L='Agregar compilacion automatica a CI.' },
    @{ R=23; M='Preparacion del diagnostico'; A=3; B=5; K='Disponibles: logs, Swagger y /health. Faltan salud de BD y fallo de arranque cuando no migran tablas.'; L='Hacer fallar el arranque y comprobar SQL en /health.' },
    @{ R=24; M='Registro de cambios'; A=2; B=2; K='El proyecto usa Git y siete migraciones EF versionadas para el esquema.'; L='Mantener cambios de codigo y BD en la misma revision.' },
    @{ R=25; M='Control de cambios'; A=2; B=4; K='Hay historial Git y migraciones. No se observo CHANGELOG ni politica de versionado por tags.'; L='Crear CHANGELOG y tags de versiones entregables.' },
    @{ R=26; M='Localizacion del impacto'; A=4; B=5; K='Controladores, servicios, repositorios y DTO separan cambios. El estado Paid no esta alineado en la interfaz.'; L='Centralizar contrato de estados y agregar prueba de contrato.' },
    @{ R=27; M='Cobertura de lineas backend'; A=1296; B=3914; K='Coverlet Cobertura de esta revision: 1296/3914 lineas = 33,11 %. Este dato real esta por debajo de la meta 0,80.'; L='Agregar pruebas de ramas de pedidos, pagos, stock y controladores; volver a medir.' },
    @{ R=28; M='Migraciones de base aplicadas'; A=7; B=7; K='Las siete migraciones EF se aplicaron en SQL Server local durante la verificacion del proyecto.'; L='Probar migracion desde base vacia en CI.' },
    @{ R=29; M='Configuracion por ambiente'; A=3; B=5; K='SQL local, User Secrets Wompi y proxy Angular son configurables. Docker omite Wompi y Frontend:Url sigue fija.'; L='Parametrizar Docker, URL frontend y origenes CORS.' },
    @{ R=30; M='Adaptacion del catalogo'; A=4; B=5; K='Roles, categorias, precios y stock se gestionan. El costo de envio se presenta fijo como gratis.'; L='Hacer configurable el envio si es un requisito del proyecto.' },
    @{ R=31; M='Facilidad de instalacion'; A=3; B=5; K='Hay README, perfiles de arranque y compose. Faltan guia de secretos Wompi y configuracion Windows/SQL.'; L='Documentar instalacion y prueba Sandbox paso a paso.' },
    @{ R=32; M='Continuidad de datos'; A=2; B=3; K='Modelo relacional y migraciones preservan el esquema; no existe importacion/exportacion de catalogo.'; L='Agregar exportacion CSV y documentar respaldo.' },
    @{ R=33; M='Cobertura del proceso ecommerce'; A=6; B=7; K='Acceso, catalogo, carrito, direcciones, pedido y pago cubren el proceso principal. Falta administracion de pedidos.'; L='Completar el modulo administrativo de pedidos.' }
)

$excel = $null
$book = $null
try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    $book = $excel.Workbooks.Open($output)
    $sheet = $book.Worksheets.Item(1)
    $sheet.Name = 'Calidad_Ecommerce'
    $sheet.Range('A1').Value2 = 'METRICAS DE CALIDAD - ECOMMERCE'
    $sheet.Range('A2').Value2 = 'Meta de referencia: 0,80 por metrica evaluada. Los resultados son mediciones o listas de verificacion explicitadas.'
    $sheet.Range('A3').Value2 = 'G es resultado real; H es meta. I/J se conservan de la plantilla y no determinan cumplimiento. N/D = no medido.'
    $sheet.Range('G4').Value2 = 'RESULTADO A/B'
    $sheet.Range('H4').Value2 = 'META'
    $sheet.Range('I4').Value2 = 'RESULTADO x META'
    $sheet.Range('J4').Value2 = 'PROMEDIO'
    $sheet.Range('D5:D33').Value2 = 'X = A/B'

    foreach ($entry in $rows) {
        $r = $entry.R
        $sheet.Cells.Item($r,3).Value2 = $entry.M
        if ($null -eq $entry.A) {
            $sheet.Cells.Item($r,5).ClearContents()
            $sheet.Cells.Item($r,6).ClearContents()
            $sheet.Cells.Item($r,7).ClearContents()
            $sheet.Cells.Item($r,9).ClearContents()
            $sheet.Cells.Item($r,10).ClearContents()
        } else {
            $sheet.Cells.Item($r,5).Value2 = [double]$entry.A
            $sheet.Cells.Item($r,6).Value2 = [double]$entry.B
            $sheet.Cells.Item($r,7).Formula = '=IFERROR(E' + $r + '/F' + $r + ',"")'
            $sheet.Cells.Item($r,9).Formula = '=IF(G' + $r + '= "", "", G' + $r + '*H' + $r + ')'
            $sheet.Cells.Item($r,10).Formula = '=IF(G' + $r + '= "", "", AVERAGE(G' + $r + ',H' + $r + '))'
        }
        $sheet.Cells.Item($r,8).Value2 = 0.8
        $sheet.Cells.Item($r,11).Value2 = $entry.K
        $sheet.Cells.Item($r,12).Value2 = $entry.L
    }
    $sheet.Range('G5:J33').NumberFormat = '0.00'
    $sheet.Range('K5:L33').WrapText = $true
    $sheet.Range('C:C').ColumnWidth = 44
    $sheet.Range('K:K').ColumnWidth = 78
    $sheet.Range('L:L').ColumnWidth = 62
    $sheet.Range('N4').Value2 = 'CARACTERISTICA'
    $sheet.Range('O4').Value2 = 'RESULTADO'
    $sheet.Range('P4').Value2 = 'META'
    $groups = @(
        @('Funcionalidad',5,10), @('Interoperabilidad',11,12),
        @('Fiabilidad',13,15), @('Usabilidad',16,19),
        @('Eficiencia',20,22), @('Mantenibilidad',23,27),
        @('Portabilidad',28,33)
    )
    for ($i=0; $i -lt $groups.Count; $i++) {
        $row = $i + 5
        $group = $groups[$i]
        $sheet.Cells.Item($row,14).Value2 = $group[0]
        if ($group[0] -eq 'Eficiencia') {
            $sheet.Cells.Item($row,15).ClearContents()
        } else {
            $sheet.Cells.Item($row,15).Formula = '=AVERAGE(G' + $group[1] + ':G' + $group[2] + ')'
        }
        $sheet.Cells.Item($row,16).Value2 = 0.8
    }
    $sheet.Range('O5:P11').NumberFormat = '0.00'
    $sheet.Range('N:N').ColumnWidth = 23
    $sheet.Range('O:P').ColumnWidth = 16
    $sheet.Range('Q4').Value2 = 'OBSERVACION'
    $sheet.Range('Q9').Value2 = 'Sin p95 ni RPS medidos'
    $sheet.Range('Q:Q').ColumnWidth = 26
    $sheet.Range('N14').Value2 = 'INDICADOR COMPROBADO'
    $sheet.Range('O14').Value2 = 'VALOR'
    $sheet.Range('P14').Value2 = 'META'
    $sheet.Range('N15').Value2 = 'Cobertura backend'
    $sheet.Range('O15').Formula = '=G27'
    $sheet.Range('P15').Value2 = 0.8
    $sheet.Range('N16').Value2 = 'Pruebas backend'
    $sheet.Range('O16').Formula = '=G14'
    $sheet.Range('P16').Value2 = 0.8
    $sheet.Range('N17').Value2 = 'Suites frontend*'
    $sheet.Range('O17').Value2 = 20.0 / 28.0
    $sheet.Range('P17').Value2 = 0.8
    $sheet.Range('N18').Value2 = 'Promedio general'
    $sheet.Range('O18').Formula = '=AVERAGE(G5:G33)'
    $sheet.Range('P18').Value2 = 0.8
    $sheet.Range('N19').Value2 = 'Metricas >= 0,80'
    $sheet.Range('O19').Formula = '=COUNTIF(G5:G33,">="&H5)/COUNT(G5:G33)'
    $sheet.Range('N20').Value2 = '*8 de 28 suites frontend fallaron; 5 pruebas fallidas.'
    $sheet.Range('N21').Value2 = 'Fuente: dotnet test + coverlet y npm test:ci, ' + (Get-Date -Format 'dd/MM/yyyy') + '.'
    $sheet.Range('O15:P19').NumberFormat = '0.00'
    $sheet.Range('N4:P4').Font.Bold = $true
    $sheet.Range('N14:P14').Font.Bold = $true
    $sheet.Range('N4:P11').Borders.LineStyle = 1

    $chart1 = $sheet.ChartObjects(1).Chart
    $chart1.DisplayBlanksAs = 1
    $chart1.Axes(2).MinimumScale = 0
    $chart1.Axes(2).MaximumScale = 1
    $chart1.Axes(2).MajorUnit = 0.2
    $chart1.Axes(2).TickLabelPosition = -4142
    $chart1.HasTitle = $true
    $chart1.ChartTitle.Text = 'Metricas de calidad: resultado vs meta 0,80'
    $s1 = $chart1.SeriesCollection().Item(1)
    $s1.Name = 'Meta 0,80'
    $s1.XValues = $sheet.Range('C5:C33')
    $s1.Values = $sheet.Range('H5:H33')
    $s2 = $chart1.SeriesCollection().Item(2)
    $s2.Name = 'Resultado A/B'
    $s2.XValues = $sheet.Range('C5:C33')
    $s2.Values = $sheet.Range('G5:G33')

    $chart2 = $sheet.ChartObjects(2).Chart
    $chart2.DisplayBlanksAs = 1
    $chart2.Axes(2).MinimumScale = 0
    $chart2.Axes(2).MaximumScale = 1
    $chart2.Axes(2).MajorUnit = 0.2
    $chart2.Axes(2).TickLabelPosition = -4142
    $chart2.HasTitle = $true
    $chart2.ChartTitle.Text = 'Promedio por caracteristica vs meta 0,80'
    $s1 = $chart2.SeriesCollection().Item(1)
    $s1.Name = 'Meta 0,80'
    $s1.XValues = $sheet.Range('N5:N11')
    $s1.Values = $sheet.Range('P5:P11')
    $s2 = $chart2.SeriesCollection().Item(2)
    $s2.Name = 'Promedio real'
    $s2.XValues = $sheet.Range('N5:N11')
    $s2.Values = $sheet.Range('O5:O11')

    $excel.CalculateFullRebuild()
    $book.Save()
    Write-Output "Creado: $output"
    Write-Output ('Metricas evaluadas: ' + ($rows | Where-Object { $null -ne $_.A }).Count)
    Write-Output ('Metricas con resultado >= 0,80: ' + ($rows | Where-Object { $null -ne $_.A -and ($_.A / $_.B) -ge 0.8 }).Count)
} finally {
    if ($book) { $book.Close($false) | Out-Null }
    if ($excel) { $excel.Quit() | Out-Null }
    if ($sheet) { [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($sheet) }
    if ($book) { [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($book) }
    if ($excel) { [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($excel) }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}
