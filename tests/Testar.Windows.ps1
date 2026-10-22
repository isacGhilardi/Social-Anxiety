$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repo 'Social-Anxiety/Social-Anxiety.csproj'
$targets = Join-Path $PSScriptRoot 'FunctionalBuild.targets'
$output = Join-Path $repo 'artifacts/e2e/app'
$intermediate = Join-Path $repo 'artifacts/e2e/obj/'
& dotnet build $project -f net10.0-windows10.0.19041.0 "-p:CustomAfterMicrosoftCommonTargets=$targets" "-p:IntermediateOutputPath=$intermediate" -o $output --no-restore
if ($LASTEXITCODE -ne 0) { throw 'A compilação para os testes falhou.' }
& (Join-Path $PSScriptRoot 'Funcionais.Windows.ps1') -AppPath (Join-Path $output 'Social-Anxiety.exe') -ReportPath (Join-Path $repo 'artifacts/funcionais-windows.json')
