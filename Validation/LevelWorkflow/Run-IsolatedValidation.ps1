param(
    [string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/6000.5.2f1/Editor/Unity.exe',
    [string]$QaProject = "$env:LOCALAPPDATA/Temp/EmberMovementQA-20261004"
)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$artifactRoot = Join-Path $projectRoot 'ValidationArtifacts/LevelWorkflow'
$qaResolved = [IO.Path]::GetFullPath($QaProject)
$taskTempRoot = [IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA 'Temp')) + [IO.Path]::DirectorySeparatorChar
if (-not $qaResolved.StartsWith($taskTempRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'QA project must be under LocalAppData/Temp.' }
foreach ($folder in @($artifactRoot, (Join-Path $qaResolved 'Assets/Runtime'), (Join-Path $qaResolved 'Assets/LevelWorkflowEditor/Editor'), (Join-Path $qaResolved 'Assets/Tests'), (Join-Path $qaResolved 'Packages'), (Join-Path $qaResolved 'ProjectSettings'))) {
    New-Item -ItemType Directory -Path $folder -Force | Out-Null
}
$snapshots = @()
$sourceFiles = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets/EmberPrototype/Runtime') -File | Where-Object { $_.Extension -eq '.cs' -or $_.Name.EndsWith('.cs.meta') })
foreach ($source in $sourceFiles) {
    $destination = Join-Path $qaResolved ('Assets/Runtime/' + $source.Name)
    Copy-Item -LiteralPath $source.FullName -Destination $destination -Force
    if ($source.Extension -eq '.cs') { $snapshots += [ordered]@{ kind='runtime'; name=$source.Name; sha256=(Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash; copySha256=(Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash } }
}
foreach ($name in @('EmberLevelWorkflowWindow.cs', 'CameraRoomEditor.cs', 'MovementPathEditor.cs', 'PathMovingBlockEditor.cs')) {
    $source = Join-Path $projectRoot ('Assets/EmberPrototype/Editor/' + $name)
    if (-not (Test-Path -LiteralPath $source)) { throw ('Missing editor implementation: ' + $name) }
    $destination = Join-Path $qaResolved ('Assets/LevelWorkflowEditor/Editor/' + $name)
    Copy-Item -LiteralPath $source -Destination $destination -Force
    $snapshots += [ordered]@{ kind='editor'; name=$name; sha256=(Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash; copySha256=(Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash }
}
foreach ($source in Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*Tests.cs' -File) {
    $destination = Join-Path $qaResolved ('Assets/Tests/' + $source.Name)
    Copy-Item -LiteralPath $source.FullName -Destination $destination -Force
    $snapshots += [ordered]@{ kind='test'; name=$source.Name; sha256=(Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash; copySha256=(Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash }
}
Copy-Item -LiteralPath (Join-Path $projectRoot 'Validation/Movement/EmberMovement.Tests.asmdef') -Destination (Join-Path $qaResolved 'Assets/Tests/EmberMovement.Tests.asmdef') -Force
foreach ($name in @('manifest.json', 'packages-lock.json')) { Copy-Item -LiteralPath (Join-Path $projectRoot ('Packages/' + $name)) -Destination (Join-Path $qaResolved ('Packages/' + $name)) -Force }
foreach ($name in @('ProjectVersion.txt', 'ProjectSettings.asset', 'Physics2DSettings.asset', 'TimeManager.asset')) { Copy-Item -LiteralPath (Join-Path $projectRoot ('ProjectSettings/' + $name)) -Destination (Join-Path $qaResolved ('ProjectSettings/' + $name)) -Force }
[ordered]@{ capturedAt=(Get-Date -Format o); qaProject=$qaResolved; files=$snapshots; scene2Sha256=(Get-FileHash -LiteralPath (Join-Path $projectRoot 'Assets/Scenes/2.unity') -Algorithm SHA256).Hash; scope='Unchanged source snapshots; native EditMode creator, Undo, camera bounds, tilemap/hazard and path movement validation. No production scenes or settings edited by runner.' } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $artifactRoot 'source-snapshot.json') -Encoding UTF8
$results = Join-Path $artifactRoot 'UnityResults.xml'
$log = Join-Path $artifactRoot 'isolated-unity.log'
if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Force }
$arguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $qaResolved + '"'), '-runTests', '-testPlatform', 'EditMode', '-testFilter', 'EmberLevelWorkflow', '-testResults', ('"' + $results + '"'), '-logFile', ('"' + $log + '"'))
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru
[ordered]@{pid=$process.Id;startedAt=(Get-Date -Format o);qaProject=$qaResolved;results=$results;log=$log} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $artifactRoot 'run.json') -Encoding UTF8
Write-Output ('QA Unity PID: ' + $process.Id)
Write-Output ('Results: ' + $results)
