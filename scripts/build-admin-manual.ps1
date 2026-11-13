param(
    [string]$Source = 'docs/manual-administrador.md',
    [string]$Destination = 'docs/manual-administrador.docx'
)

$ErrorActionPreference = 'Stop'
$sourcePath = (Resolve-Path -LiteralPath $Source).Path
$docsDir = Split-Path -Parent $sourcePath
$destinationPath = [IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Destination))
$lines = Get-Content -LiteralPath $sourcePath -Encoding UTF8

function PlainText([string]$value) {
    $value = [regex]::Replace($value, '\[([^\]]+)\]\([^)]+\)', '$1')
    return $value.Replace('**', '').Replace('`', '')
}

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
try {
    $document = $word.Documents.Add()
    try {
        $selection = $word.Selection
        $document.PageSetup.TopMargin = $word.CentimetersToPoints(2)
        $document.PageSetup.BottomMargin = $word.CentimetersToPoints(2)
        $document.PageSetup.LeftMargin = $word.CentimetersToPoints(2)
        $document.PageSetup.RightMargin = $word.CentimetersToPoints(2)

        foreach ($raw in $lines) {
            $line = $raw.Trim()
            if ($line.Length -eq 0) { continue }

            if ($line -match '^!\[([^\]]+)\]\(([^)]+)\)$') {
                $caption = $matches[1]
                $imagePath = [IO.Path]::GetFullPath((Join-Path $docsDir $matches[2]))
                if (-not (Test-Path -LiteralPath $imagePath)) { throw "Falta imagen: $imagePath" }
                $selection.Style = -1
                $shape = $selection.InlineShapes.AddPicture($imagePath, $false, $true)
                $shape.LockAspectRatio = -1
                if ($shape.Width -gt 450) { $shape.Width = 450 }
                $selection.TypeParagraph()
                $selection.Font.Italic = -1
                $selection.Font.Size = 9
                $selection.TypeText($caption)
                $selection.TypeParagraph()
                $selection.Font.Italic = 0
                $selection.Font.Size = 10
                continue
            }

            if ($line -match '^(#{1,3})\s+(.+)$') {
                $level = $matches[1].Length
                $selection.Style = @(-2, -3, -4)[$level - 1]
                $selection.TypeText((PlainText $matches[2]))
                $selection.TypeParagraph()
                continue
            }

            if ($line.StartsWith('|')) {
                if ($line -match '^\|[\s:|-]+\|$') { continue }
                $cells = @($line.Trim('|').Split('|') | ForEach-Object { (PlainText $_.Trim()) })
                if ($cells[0] -in @('Estado mostrado', 'Situación')) { continue }
                $selection.Style = -1
                $selection.Font.Bold = -1
                $selection.TypeText($cells[0] + ': ')
                $selection.Font.Bold = 0
                $selection.TypeText(($cells | Select-Object -Skip 1) -join ' — ')
                $selection.TypeParagraph()
                continue
            }

            $selection.Style = -1
            $selection.Font.Size = 10
            $selection.TypeText((PlainText $line))
            $selection.TypeParagraph()
        }

        $document.SaveAs2($destinationPath, 16)
        Write-Output "Manual creado: $destinationPath"
    } finally {
        $document.Close(0)
    }
} finally {
    $word.Quit()
}
