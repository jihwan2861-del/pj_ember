param(
    [string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/6000.5.2f1/Editor/Unity.exe',
    [string]$QaProject = "$env:LOCALAPPDATA/Temp/EmberMovementQA-20261004",
    [string]$TestFilter = 'EmberMovementRegression',
    [string]$ArtifactSubdirectory = '',
    [switch]$EnableGraphics
)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$artifactRoot = Join-Path $projectRoot 'ValidationArtifacts/MovementFeel'
if ($ArtifactSubdirectory) { $artifactRoot = Join-Path $artifactRoot $ArtifactSubdirectory }
$qaResolved = [IO.Path]::GetFullPath($QaProject)
$tempRoot = [IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA 'Temp')) + [IO.Path]::DirectorySeparatorChar
if (-not $qaResolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The QA project must live inside LocalAppData/Temp.'
}
if (-not (Test-Path -LiteralPath $UnityEditor)) { throw 'Unity Editor was not found.' }
$folders = @($artifactRoot, (Join-Path $qaResolved 'Assets/Runtime'), (Join-Path $qaResolved 'Assets/Tests'), (Join-Path $qaResolved 'Packages'), (Join-Path $qaResolved 'ProjectSettings'))
foreach ($folder in $folders) { New-Item -ItemType Directory -Path $folder -Force | Out-Null }
$sourceRoot = Join-Path $projectRoot 'Assets/EmberPrototype/Runtime'
$snapshot = @()
foreach ($source in Get-ChildItem -LiteralPath $sourceRoot -File | Where-Object { $_.Extension -eq '.cs' -or $_.Name.EndsWith('.cs.meta') }) {
    $destination = Join-Path $qaResolved ('Assets/Runtime/' + $source.Name)
    Copy-Item -LiteralPath $source.FullName -Destination $destination -Force
    if ($source.Extension -eq '.cs') {
        $snapshot += [ordered]@{ name = $source.Name; sha256 = (Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash; copySha256 = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash }
    }
}
$ropeBuilder = Join-Path $projectRoot 'Assets/EmberPrototype/Editor/RopeValidationSceneBuilder.cs'
if (Test-Path -LiteralPath $ropeBuilder) {
    $editorTarget = Join-Path $qaResolved 'Assets/Editor'
    New-Item -ItemType Directory -Path $editorTarget -Force | Out-Null
    Copy-Item -LiteralPath $ropeBuilder -Destination (Join-Path $editorTarget 'RopeValidationSceneBuilder.cs') -Force
    Copy-Item -LiteralPath ($ropeBuilder + '.meta') -Destination (Join-Path $editorTarget 'RopeValidationSceneBuilder.cs.meta') -Force
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'EmberMovementRegressionTests.cs') -Destination (Join-Path $qaResolved 'Assets/Tests/EmberMovementRegressionTests.cs') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'EmberMovement.Tests.asmdef') -Destination (Join-Path $qaResolved 'Assets/Tests/EmberMovement.Tests.asmdef') -Force
foreach ($test in Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*Tests.cs' -File) {
    Copy-Item -LiteralPath $test.FullName -Destination (Join-Path $qaResolved ('Assets/Tests/' + $test.Name)) -Force
}
foreach ($name in @('manifest.json', 'packages-lock.json')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot ('Packages/' + $name)) -Destination (Join-Path $qaResolved ('Packages/' + $name)) -Force
}
foreach ($name in @('ProjectVersion.txt', 'ProjectSettings.asset', 'Physics2DSettings.asset', 'TimeManager.asset')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot ('ProjectSettings/' + $name)) -Destination (Join-Path $qaResolved ('ProjectSettings/' + $name)) -Force
}
if ($EnableGraphics) {
    foreach ($name in @('GraphicsSettings.asset', 'QualitySettings.asset')) {
        Copy-Item -LiteralPath (Join-Path $projectRoot ('ProjectSettings/' + $name)) -Destination (Join-Path $qaResolved ('ProjectSettings/' + $name)) -Force
    }
    $renderTarget = Join-Path $qaResolved 'Assets/Settings'
    New-Item -ItemType Directory -Path $renderTarget -Force | Out-Null
    foreach ($renderAsset in Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets/Settings') -File | Where-Object { $_.Name.EndsWith('.asset') -or $_.Name.EndsWith('.asset.meta') }) {
        Copy-Item -LiteralPath $renderAsset.FullName -Destination (Join-Path $renderTarget $renderAsset.Name) -Force
    }
}
$metadata = [ordered]@{
    createdAt = (Get-Date -Format o)
    editor = $UnityEditor
    qaProject = $qaResolved
    sourceProject = $projectRoot
    fixture = 'EmberMovementRegression.InputAndPhysicsTests'
    fixtureSha256 = (Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'EmberMovementRegressionTests.cs') -Algorithm SHA256).Hash
    scene2Sha256 = (Get-FileHash -LiteralPath (Join-Path $projectRoot 'Assets/Scenes/2.unity') -Algorithm SHA256).Hash
    scope = 'Unchanged runtime-source copy; movement tests use KeyboardState and manually stepped native Physics2D. RopeScenePlayTests enters actual Play Mode for normal lifecycle and physics. Graphics opt-in copies the source render settings. No standalone player build validation.'
    graphicsEnabled = [bool]$EnableGraphics
    ropeSceneBuilderSha256 = if (Test-Path -LiteralPath $ropeBuilder) { (Get-FileHash -LiteralPath $ropeBuilder -Algorithm SHA256).Hash } else { $null }
    files = $snapshot
}
$metadata | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $artifactRoot 'source-snapshot.json') -Encoding UTF8
$testResults = Join-Path $artifactRoot 'UnityResults.xml'
if (Test-Path -LiteralPath $testResults) { Remove-Item -LiteralPath $testResults -Force }
$unityLog = Join-Path $artifactRoot 'isolated-unity.log'
$arguments = @('-batchmode', '-projectPath', ('"' + $qaResolved + '"'), '-runTests', '-testPlatform', 'EditMode', '-testFilter', $TestFilter, '-testResults', ('"' + $testResults + '"'), '-logFile', ('"' + $unityLog + '"'))
if (-not $EnableGraphics) { $arguments += '-nographics' }
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru
[ordered]@{ pid = $process.Id; startedAt = (Get-Date -Format o); log = $unityLog; results = $testResults; qaProject = $qaResolved } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $artifactRoot 'run.json') -Encoding UTF8
Write-Output ('QA Unity PID: ' + $process.Id)
Write-Output ('Results: ' + $testResults)
Write-Output ('Runtime snapshots: ' + $snapshot.Count)
