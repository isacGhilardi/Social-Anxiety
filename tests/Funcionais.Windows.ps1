param(
    [Parameter(Mandatory = $true)][string]$AppPath,
    [string]$ReportPath = 'artifacts/funcionais-windows.json',
    [string]$Filter = ''
)

$ErrorActionPreference = 'Stop'
$marker = Join-Path (Split-Path -Parent (Resolve-Path $AppPath)) 'functional-test-build.txt'
if (!(Test-Path -LiteralPath $marker) -or (Get-Content -LiteralPath $marker -Raw).Trim() -ne 'com.calme.functionaltests') {
    throw 'Use a versão isolada gerada por tests/Testar.Windows.ps1. O aplicativo normal não pode ser usado nesta suíte.'
}
$dataDirectory = Join-Path $env:LOCALAPPDATA 'Calme Tests/com.calme.functionaltests/Data'
$dataPath = Join-Path $dataDirectory 'calme.db3'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$results = [Collections.Generic.List[object]]::new()
$app = $null
$window = $null

function Start-Calme {
    $script:app = Start-Process -FilePath (Resolve-Path $AppPath) -WindowStyle Hidden -PassThru
    $deadline = (Get-Date).AddSeconds(25)
    do {
        if ($app.HasExited) { throw "Aplicativo encerrou: $($app.ExitCode)" }
        $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $app.Id)
        $script:window = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children, $condition)
        if ($window) { return }
        Start-Sleep -Milliseconds 200
    } while ((Get-Date) -lt $deadline)
    throw 'A janela do aplicativo não abriu em 25 segundos.'
}

function Find-Control([string]$value) {
    $deadline = (Get-Date).AddSeconds(8)
    do {
        $id = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $value)
        $name = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $value)
        $element = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.OrCondition]::new($id, $name))
        if ($element) { return $element }
        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)
    throw "Controle ausente: $value"
}

function Click-Control([string]$value) {
    Find-Control $value | Out-Null
    $id = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $value)
    $name = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $value)
    $matches = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.OrCondition]::new($id, $name))
    foreach ($element in @($matches | Sort-Object { $_.Current.IsOffscreen })) {
        $pattern = $null
        if ($element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
            if (!$element.Current.IsEnabled) { throw "Controle desabilitado: $value" }
            $pattern.Invoke()
            Start-Sleep -Milliseconds 450
            return
        }
    }
    throw "Controle não pode ser acionado: $value"
}

function Assert-Equal($actual, $expected, [string]$message) {
    if ($actual -ne $expected) { throw "$message. Esperado: $expected; recebido: $actual" }
}

function Set-Switch([string]$id, [bool]$value) {
    $pattern = (Find-Control $id).GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
    if (($pattern.Current.ToggleState -eq [System.Windows.Automation.ToggleState]::On) -ne $value) {
        $pattern.Toggle()
        Start-Sleep -Milliseconds 150
    }
}

function Wait-Text([string]$id, [string]$expected, [int]$seconds = 7) {
    $element = Find-Control $id
    $deadline = (Get-Date).AddSeconds($seconds)
    do {
        if ($element.Current.Name -eq $expected) { return }
        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)
    throw "O controle $id não apresentou $expected dentro do prazo."
}

function Test-Flow([string]$name, [scriptblock]$body) {
    if ($Filter -and $name -notmatch $Filter) { return }
    try {
        & $body
        $results.Add([pscustomobject]@{ Name = $name; Passed = $true; Error = $null })
        Write-Output "PASS: $name"
    } catch {
        $visible = @()
        if ($window) {
            try {
                $visible = @($window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name } | Where-Object { $_ })
            } catch { $visible = @('Janela indisponível') }
        }
        $results.Add([pscustomobject]@{ Name = $name; Passed = $false; Error = $_.Exception.Message; Visible = $visible })
        Write-Output ($visible -join ' | ')
        if ($app.HasExited) { Write-Output "Aplicativo encerrou com código $($app.ExitCode)" }
        throw
    }
}

function Set-Text([string]$id, [string]$value) {
    (Find-Control $id).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($value)
    Start-Sleep -Milliseconds 150
}

function Restart-Calme {
    $app.Kill()
    $app.WaitForExit()
    Start-Calme
}

# Fecha o app e apaga só a sessão salva da identidade de teste: reabre na tela de acesso.
function Clear-Session {
    $app.Kill()
    $app.WaitForExit()
    $session = Join-Path (Split-Path -Parent $dataDirectory) 'Settings/securestorage.dat'
    if (Test-Path -LiteralPath $session) { Remove-Item -LiteralPath $session }
    Start-Calme
    Find-Control 'Criar minha conta' | Out-Null
}

# Grava dados fictícios direto no banco da identidade de teste, com o SQLite que acompanha o Windows.
Add-Type -Namespace CalmeTests -Name Sqlite -MemberDefinition @'
[DllImport("winsqlite3.dll", CharSet = CharSet.Ansi)] public static extern int sqlite3_open(string filename, out IntPtr db);
[DllImport("winsqlite3.dll", CharSet = CharSet.Ansi)] public static extern int sqlite3_exec(IntPtr db, string sql, IntPtr callback, IntPtr arg, IntPtr errmsg);
[DllImport("winsqlite3.dll")] public static extern int sqlite3_close(IntPtr db);
'@

function Invoke-Sqlite([string]$sql) {
    $db = [IntPtr]::Zero
    if ([CalmeTests.Sqlite]::sqlite3_open($dataPath, [ref]$db) -ne 0) { throw "Não foi possível abrir $dataPath" }
    try {
        $code = [CalmeTests.Sqlite]::sqlite3_exec($db, $sql, [IntPtr]::Zero, [IntPtr]::Zero, [IntPtr]::Zero)
        if ($code -ne 0) { throw "SQLite retornou $code" }
    } finally { [CalmeTests.Sqlite]::sqlite3_close($db) | Out-Null }
}

try {
    # Pasta exclusiva da identidade de teste: banco, sessão e preferências começam do zero.
    $testAppDirectory = Split-Path -Parent $dataDirectory
    if (Test-Path -LiteralPath $testAppDirectory) { Remove-Item -LiteralPath $testAppDirectory -Recurse -Force }
    [IO.Directory]::CreateDirectory($dataDirectory) | Out-Null
    Start-Calme
    Test-Flow 'Sem sessão, reabrir vai direto ao acesso' {
        Click-Control 'Conhecer o Calme'
        Click-Control 'Continuar'
        Click-Control 'Entendi, vamos continuar'
        Click-Control 'Prefiro explorar primeiro'
        Find-Control 'Criar minha conta' | Out-Null
        Restart-Calme
        Find-Control 'Criar minha conta' | Out-Null
    }
    Test-Flow 'Cadastro valida campos, abre a sessão e continua após reabrir' {
        Click-Control 'Criar minha conta'
        Set-Text 'signup-name' 'Pessoa Teste'
        Set-Text 'signup-email' 'Pessoa.Teste@Exemplo.com'
        Set-Text 'signup-password' '12345'
        Click-Control 'signup-submit'
        Assert-Equal (Find-Control 'signup-error').Current.Name 'Confira seu nome, e-mail e senha. A senha precisa ter pelo menos 6 caracteres.' 'Validação do cadastro não apareceu'
        Set-Text 'signup-password' 'senha-ficticia'
        Click-Control 'signup-submit'
        Wait-Text 'home-greeting' 'Olá, Pessoa Teste.'
        Restart-Calme
        Wait-Text 'home-greeting' 'Olá, Pessoa Teste.'
    }
    Test-Flow 'Login errado e e-mail repetido mostram mensagens, login certo entra' {
        Clear-Session
        Click-Control 'Já tenho uma conta'
        Set-Text 'login-email' 'pessoa.teste@exemplo.com'
        Set-Text 'login-password' 'senha-errada'
        Click-Control 'login-submit'
        Assert-Equal (Find-Control 'login-error').Current.Name 'E-mail ou senha incorretos.' 'Mensagem de login incorreta'
        Click-Control 'Criar uma conta'
        Set-Text 'signup-name' 'Outra Pessoa'
        Set-Text 'signup-email' ' PESSOA.TESTE@exemplo.com '
        Set-Text 'signup-password' 'outra-senha'
        Click-Control 'signup-submit'
        Assert-Equal (Find-Control 'signup-error').Current.Name 'Este e-mail já tem uma conta. Que tal entrar?' 'E-mail repetido foi aceito'
        Click-Control 'Já tenho uma conta'
        Set-Text 'login-email' 'pessoa.teste@exemplo.com'
        Set-Text 'login-password' 'senha-ficticia'
        Click-Control 'login-submit'
        Wait-Text 'home-greeting' 'Olá, Pessoa Teste.'
    }
    Test-Flow 'Convidado chega ao início' {
        Clear-Session
        Click-Control 'Continuar como convidado'
        Find-Control 'home-checkin' | Out-Null
        Assert-Equal (Find-Control 'home-greeting').Current.Name 'Olá, que bom ter você.' 'Saudação de convidado incorreta'
    }
    Test-Flow 'Convidado reabre o aplicativo direto no início' {
        Restart-Calme
        Find-Control 'home-checkin' | Out-Null
        Assert-Equal (Find-Control 'home-greeting').Current.Name 'Olá, que bom ter você.' 'Sessão de convidado não foi restaurada'
    }
    Test-Flow 'Histórico vazio não apresenta dados de exemplo' {
        Click-Control 'home-history'
        Assert-Equal (Find-Control 'history-count').Current.Name '0 registros no período' 'Histórico deveria estar vazio'
        Find-Control 'Nenhum registro neste período.' | Out-Null
        Click-Control 'history-home'
    }
    Test-Flow 'Voltar do check-in cancela sem gravar' {
        Click-Control 'home-checkin'
        Click-Control 'score-3'
        Click-Control 'Voltar'
        Click-Control 'home-history'
        Assert-Equal (Find-Control 'history-count').Current.Name '0 registros no período' 'Cancelamento gravou um registro'
        Click-Control 'history-home'
    }
    Test-Flow 'Check-in exige seleção e salva o valor escolhido' {
        Click-Control 'home-checkin'
        Assert-Equal (Find-Control 'checkin-save').Current.IsEnabled $false 'Salvar deve estar bloqueado'
        Click-Control 'score-2'
        Click-Control 'score-4'
        Click-Control 'open-support'
        Click-Control 'Voltar ao que eu estava fazendo'
        Assert-Equal (Find-Control 'checkin-selected').Current.Name '4 de 5' 'Escolha perdida após o apoio'
        Click-Control 'checkin-save'
        Find-Control 'feedback-title' | Out-Null
        Assert-Equal (Find-Control 'feedback-score').Current.Name '4 de 5 · Intensa' 'Valor salvo incorreto'
    }
    Test-Flow 'Histórico abre o registro gravado' {
        Click-Control 'feedback-history'
        Click-Control ((Get-Date).ToString("dd 'de' MMMM 'de' yyyy", [Globalization.CultureInfo]::GetCultureInfo('pt-BR')))
        Assert-Equal (Find-Control 'record-score').Current.Name '4 de 5 · Intensa' 'Detalhe não corresponde ao check-in'
        Click-Control 'record-history'
        Click-Control 'history-month'
        Find-Control 'history-graph' | Out-Null
        Click-Control 'history-week'
        Click-Control 'history-home'
        Assert-Equal (Find-Control 'home-latest').Current.Name '4 de 5 · Intensa' 'Resumo não atualizou'
    }
    Test-Flow 'Novo check-in no mesmo dia atualiza o registro sem duplicar' {
        Click-Control 'home-checkin'
        Click-Control 'score-1'
        Click-Control 'checkin-save'
        Click-Control 'feedback-history'
        Assert-Equal (Find-Control 'history-count').Current.Name '1 registro no período' 'Registro duplicado'
        Click-Control ((Get-Date).ToString("dd 'de' MMMM 'de' yyyy", [Globalization.CultureInfo]::GetCultureInfo('pt-BR')))
        Assert-Equal (Find-Control 'record-score').Current.Name '1 de 5 · Pouca' 'Valor não foi atualizado'
    }
    Test-Flow 'Registro continua disponível após reiniciar o aplicativo' {
        $app.Kill()
        $app.WaitForExit()
        Start-Calme
        Find-Control 'home-checkin' | Out-Null
        Assert-Equal (Find-Control 'home-latest').Current.Name '1 de 5 · Pouca' 'Persistência falhou'
    }
    Test-Flow 'Todos os níveis da escala chegam corretamente ao histórico e detalhe' {
        $labels = @('Pouca', 'Leve', 'Moderada', 'Intensa', 'Muito intensa')
        foreach ($score in @(2, 3, 4, 5, 1)) {
            Click-Control 'home-checkin'
            Click-Control "score-$score"
            Click-Control 'checkin-save'
            Assert-Equal (Find-Control 'feedback-score').Current.Name "$score de 5 · $($labels[$score - 1])" 'Confirmação incorreta'
            Click-Control 'feedback-history'
            Click-Control ((Get-Date).ToString("dd 'de' MMMM 'de' yyyy", [Globalization.CultureInfo]::GetCultureInfo('pt-BR')))
            Assert-Equal (Find-Control 'record-score').Current.Name "$score de 5 · $($labels[$score - 1])" 'Detalhe incorreto'
            Click-Control 'record-history'
            Click-Control 'history-home'
        }
    }
    Test-Flow 'Semana e mês filtram as datas na interface' {
        $app.Kill()
        $app.WaitForExit()
        $rows = @(0, 6, 7, 29, 30) | ForEach-Object {
            "('$([Guid]::NewGuid())', 2, $([DateTime]::UtcNow.Ticks), '$((Get-Date).Date.AddDays(-$_).ToString('yyyy-MM-dd'))')"
        }
        Invoke-Sqlite "delete from checkins; insert into checkins (Id, Score, RecordedAtUtc, LocalDay) values $($rows -join ', ');"
        Start-Calme
        Find-Control 'home-checkin' | Out-Null
        Click-Control 'home-history'
        Assert-Equal (Find-Control 'history-count').Current.Name '2 registros no período' 'Filtro de semana incorreto'
        Click-Control 'history-month'
        Assert-Equal (Find-Control 'history-count').Current.Name '4 registros no período' 'Filtro de mês incorreto'
        Click-Control 'history-home'
    }
    Test-Flow 'Respiração inicia, pausa e conclui' {
        Click-Control 'home-breathe'
        Click-Control 'breathe-toggle'
        Find-Control 'breathe-phase' | Out-Null
        Click-Control 'breathe-toggle'
        Assert-Equal (Find-Control 'breathe-phase').Current.Name 'Pausa' 'Prática não pausou'
        Click-Control 'breathe-toggle'
        Click-Control 'breathe-finish'
        Find-Control 'breathe-finish-title' | Out-Null
        Click-Control 'breathe-return'
        Find-Control 'home-checkin' | Out-Null
    }
    Test-Flow 'Respiração pode ser cancelada antes de iniciar' {
        Click-Control 'home-breathe'
        Click-Control 'Voltar'
        Find-Control 'home-checkin' | Out-Null
    }
    Test-Flow 'Fases, pausa, retomada e opções da respiração funcionam na interface' {
        Click-Control 'home-breathe'
        Set-Switch 'breathe-skip-hold' $false
        Set-Switch 'breathe-reduced-motion' $false
        Click-Control 'breathe-toggle'
        Wait-Text 'breathe-phase' 'Inspire'
        Wait-Text 'breathe-phase' 'Segure'
        Wait-Text 'breathe-phase' 'Expire'
        Click-Control 'breathe-toggle'
        $elapsed = (Find-Control 'breathe-elapsed').Current.Name
        Start-Sleep -Milliseconds 500
        Assert-Equal (Find-Control 'breathe-elapsed').Current.Name $elapsed 'A contagem avançou durante a pausa'
        Set-Switch 'breathe-reduced-motion' $true
        Set-Switch 'breathe-skip-hold' $true
        Click-Control 'breathe-toggle'
        Wait-Text 'breathe-phase' 'Expire'
        Click-Control 'breathe-finish'
        Click-Control 'breathe-again'
        Assert-Equal (Find-Control 'breathe-phase').Current.Name 'Quando quiser, comece.' 'Nova prática não reiniciou'
        Click-Control 'Voltar'
        Find-Control 'home-checkin' | Out-Null
    }
    Test-Flow 'Minimizar pausa a respiração sem retomar automaticamente' {
        Click-Control 'home-breathe'
        Click-Control 'breathe-toggle'
        $native = $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern)
        $native.SetWindowVisualState([System.Windows.Automation.WindowVisualState]::Minimized)
        Start-Sleep -Milliseconds 500
        $native.SetWindowVisualState([System.Windows.Automation.WindowVisualState]::Normal)
        Wait-Text 'breathe-phase' 'Pausa'
        Click-Control 'Voltar'
        Find-Control 'home-checkin' | Out-Null
    }
    Test-Flow 'Conclusão iniciada no apoio retorna ao apoio e depois à origem' {
        Click-Control 'open-support'
        Click-Control 'Respiração guiada. Acompanhe um ritmo suave'
        Click-Control 'breathe-toggle'
        Click-Control 'breathe-finish'
        Click-Control 'breathe-return'
        Click-Control 'Voltar ao que eu estava fazendo'
        Find-Control 'home-checkin' | Out-Null
    }
    Test-Flow 'Arquivo inválido apresenta erro e permanece intacto' {
        $app.Kill()
        $app.WaitForExit()
        Remove-Item -LiteralPath "$dataPath-wal", "$dataPath-shm" -ErrorAction SilentlyContinue
        [IO.File]::WriteAllText($dataPath, 'dados inválidos de teste')
        Start-Calme
        Find-Control 'home-checkin' | Out-Null
        Assert-Equal (Find-Control 'home-error').Current.Name 'Não foi possível ler os registros. Seus dados foram preservados.' 'Erro de leitura não foi mostrado'
        Click-Control 'home-checkin'
        Click-Control 'score-2'
        Click-Control 'checkin-save'
        Assert-Equal (Find-Control 'checkin-error').Current.Name 'Não foi possível salvar. Os registros anteriores foram preservados. Tente novamente.' 'Erro ao salvar não foi mostrado'
        Click-Control 'Voltar'
        Click-Control 'home-history'
        Assert-Equal (Find-Control 'history-error').Current.Name 'Não foi possível ler o histórico. Seus dados foram preservados.' 'Erro no histórico não foi mostrado'
        $app.Kill()
        $app.WaitForExit()
        Assert-Equal ([IO.File]::ReadAllText($dataPath)) 'dados inválidos de teste' 'Arquivo inválido foi sobrescrito'
    }
} finally {
    if ($app -and !$app.HasExited) { $app.Kill(); $app.WaitForExit() }
    $directory = Split-Path -Parent $ReportPath
    [IO.Directory]::CreateDirectory($directory) | Out-Null
    $results | ConvertTo-Json -Depth 4 | Set-Content -Path $ReportPath -Encoding UTF8
}
