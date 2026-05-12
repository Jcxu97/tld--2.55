param([string]$Path)
$bytes = [System.IO.File]::ReadAllBytes($Path)
$text = [System.Text.Encoding]::ASCII.GetString($bytes)
[regex]::Matches($text, '[\x20-\x7e]{6,}') | ForEach-Object { $_.Value } | Sort-Object -Unique
