# Carrega os dados de demonstração do Calme (check-ins e desafios) no banco local.
# Uso (com o app FECHADO):
#   .\tests\Carregar-Demonstracao.ps1              carrega os dados
#   .\tests\Carregar-Demonstracao.ps1 -Completo    também conclui o desafio 3
#   .\tests\Carregar-Demonstracao.ps1 -Restaurar   volta o banco ao estado de antes
# O roteiro completo está em docs/demonstracao.md.
param(
    [string]$Banco,
    [switch]$Completo,
    [switch]$Restaurar
)
$ErrorActionPreference = 'Stop'

# Acha o calme.db3 do app (ignora o banco da identidade de testes funcionais).
if (-not $Banco) {
    $achados = Get-ChildItem $env:LOCALAPPDATA -Recurse -Filter calme.db3 -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch 'Calme Tests' } | Sort-Object LastWriteTime -Descending
    if (-not $achados) { throw 'Não achei o calme.db3. Abra o app uma vez e tente de novo, ou passe -Banco <caminho>.' }
    $Banco = $achados[0].FullName
}
Write-Host "Banco: $Banco"
$backup = "$Banco.demo-bak"

if ($Restaurar) {
    if (-not (Test-Path $backup)) { throw 'Não existe cópia para restaurar.' }
    Copy-Item $backup $Banco -Force
    Write-Host 'Banco restaurado.'
    return
}

# Grava direto no banco com o SQLite que acompanha o Windows (mesma técnica de Funcionais.Windows.ps1).
Add-Type -Namespace CalmeDemo -Name Sqlite -MemberDefinition @'
[DllImport("winsqlite3.dll", CharSet = CharSet.Ansi)] public static extern int sqlite3_open(string filename, out IntPtr db);
[DllImport("winsqlite3.dll", CharSet = CharSet.Ansi)] public static extern int sqlite3_exec(IntPtr db, string sql, IntPtr callback, IntPtr arg, IntPtr errmsg);
[DllImport("winsqlite3.dll")] public static extern int sqlite3_close(IntPtr db);
'@
function Invoke-Sqlite([string]$sql) {
    $db = [IntPtr]::Zero
    if ([CalmeDemo.Sqlite]::sqlite3_open($Banco, [ref]$db) -ne 0) { throw "Não foi possível abrir $Banco" }
    try {
        $codigo = [CalmeDemo.Sqlite]::sqlite3_exec($db, $sql, [IntPtr]::Zero, [IntPtr]::Zero, [IntPtr]::Zero)
        if ($codigo -ne 0) { throw "SQLite retornou $codigo (o app está fechado? as tabelas já existem?)" }
    } finally { [CalmeDemo.Sqlite]::sqlite3_close($db) | Out-Null }
}

Copy-Item $Banco $backup -Force

# Check-ins: do dia 1 do mês (ou dos últimos 7 dias, o que for mais antigo) até ONTEM.
# Hoje fica livre para o check-in ao vivo. Há um dia sem registro na semana, para mostrar a lacuna no gráfico.
# A ansiedade cai ao longo do período (de ~4 para ~3), com pequenas oscilações.
$hoje = (Get-Date).Date
$inicio = @((Get-Date -Day 1).Date, $hoje.AddDays(-6)) | Sort-Object | Select-Object -First 1
$total = ($hoje.AddDays(-1) - $inicio).Days + 1
$oscila = @(0, 1, 0, -1, 1, 0, -1)
$linhas = @()
for ($i = 0; $i -lt $total; $i++) {
    $dia = $inicio.AddDays($i)
    $atras = ($hoje - $dia).Days
    if ($atras -eq 5) { continue }                  # lacuna na semana
    if ($atras -gt 7 -and $i % 5 -eq 3) { continue } # alguns dias sem registro no resto do mês
    $progresso = if ($total -gt 1) { $i / ($total - 1) } else { 1 }
    $nota = [int][Math]::Min(5, [Math]::Max(2, [Math]::Floor(4.5 - 2.0 * $progresso + 0.6 * $oscila[$i % 7] + 0.5)))
    $quando = $dia.AddHours(19).ToUniversalTime().Ticks
    $linhas += "('$([Guid]::NewGuid())', $nota, $quando, '$($dia.ToString('yyyy-MM-dd'))')"
}
$sql = "delete from checkins; delete from challenge_progress; delete from achievements; " +
       "insert into checkins (Id, Score, RecordedAtUtc, LocalDay) values $($linhas -join ', ');"

# Desafios: o nível 1 (Perceber) conta sozinho pelos check-ins. O nível 2 já vem concluído;
# o nível 3 fica disponível para concluir ao vivo (ou use -Completo).
$niveis = @(@{ N = 2; Chave = 'pequeno-passo'; Atras = 2 })
if ($Completo) { $niveis += @{ N = 3; Chave = 'uma-conversa'; Atras = 1 } }
foreach ($nivel in $niveis) {
    $quando = $hoje.AddDays(-$nivel.Atras).AddHours(20).ToUniversalTime().Ticks
    $sql += " insert into challenge_progress (Id, Level, CompletedAtUtc) values ('$([Guid]::NewGuid())', $($nivel.N), $quando);"
    $sql += " insert into achievements (Id, [Key], ObtainedAtUtc) values ('$([Guid]::NewGuid())', '$($nivel.Chave)', $quando);"
}

Invoke-Sqlite $sql
Write-Host "Pronto: $($linhas.Count) check-ins e $($niveis.Count) desafio(s) carregados. Cópia do estado anterior em $backup"
