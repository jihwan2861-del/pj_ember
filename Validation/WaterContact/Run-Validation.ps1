$ErrorActionPreference = 'Stop'
$taskProject = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$taskArtifacts = Join-Path $taskProject 'ValidationArtifacts/WaterContact'
New-Item -ItemType Directory -Path $taskArtifacts -Force | Out-Null
$taskQa = Join-Path $env:LOCALAPPDATA ('Temp/EmberWaterContactQA-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
if ($taskQa -eq $taskProject) { throw 'Use an isolated QA project, not the source project.' }
New-Item -ItemType Directory -Path $taskQa -Force | Out-Null
foreach ($taskFolder in @('Assets','Packages','ProjectSettings')) {
    & robocopy (Join-Path $taskProject $taskFolder) (Join-Path $taskQa $taskFolder) /E /MT:8 /R:0 /W:0 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw ('Copy failed: ' + $taskFolder) }
}
$taskCache = Join-Path $taskProject 'Library/PackageCache'
if (Test-Path -LiteralPath $taskCache) {
    & robocopy $taskCache (Join-Path $taskQa 'Library/PackageCache') /E /MT:8 /R:0 /W:0 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw 'Package cache copy failed.' }
}
$taskTests = Join-Path $taskQa 'Assets/WaterContactValidation'
New-Item -ItemType Directory -Path $taskTests -Force | Out-Null
$taskFiles = @('Validation/WaterContact/WaterContactTests.cs','Validation/WaterSpiritAI/WaterSpiritAITests.cs','Validation/WaterSpirit/WaterSpiritTests.cs','Validation/Wave/WaterWaveTests.cs')
$taskFiles += @(Get-ChildItem -LiteralPath (Join-Path $taskProject 'Validation/Movement') -Filter '*.cs' | ForEach-Object { 'Validation/Movement/' + $_.Name })
foreach ($taskFile in $taskFiles) { Copy-Item -LiteralPath (Join-Path $taskProject $taskFile) -Destination $taskTests -Force }
Copy-Item -LiteralPath (Join-Path $taskProject 'Validation/Movement/EmberMovement.Tests.asmdef') -Destination $taskTests -Force
Copy-Item -LiteralPath (Join-Path $taskProject 'Validation/WaterSpiritAI/WaterSpiritTestBehavior.cs') -Destination (Join-Path $taskQa 'Assets/EmberPrototype/Runtime') -Force
$env:EMBER_WATER_CONTACT_ARTIFACTS = $taskArtifacts
$env:EMBER_SPIRIT_AI_ARTIFACTS = $taskArtifacts
$taskVersion = ((Get-Content (Join-Path $taskProject 'ProjectSettings/ProjectVersion.txt') -TotalCount 1) -split ': ')[1]
$taskEditor = 'C:/Program Files/Unity/Hub/Editor/' + $taskVersion + '/Editor/Unity.exe'
$taskFilter = 'WaterContactValidation|WaterSpiritAIValidation|WaterSpiritValidation|WaveValidation|EmberMovementRegression.InputAndPhysicsTests.Rope|EmberMovementRegression.InputAndPhysicsTests.AnchorLaunch|EmberMovementRegression.InputAndPhysicsTests.HoldingForwardRetainsMoreFireLaunchSpeed|EmberMovementRegression.InputAndPhysicsTests.ReverseInputBrakesDuringFireLaunch'
$taskArgs = @('-batchmode','-projectPath',('"'+$taskQa+'"'),'-runTests','-testPlatform','EditMode','-testFilter',$taskFilter,'-testResults',('"'+(Join-Path $taskArtifacts 'AllResults.xml')+'"'),'-logFile',('"'+(Join-Path $taskArtifacts 'all.log')+'"'))
$taskProcess = Start-Process -FilePath $taskEditor -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
Write-Output ('QA Unity PID: ' + $taskProcess.Id + '; project: ' + $taskQa)


