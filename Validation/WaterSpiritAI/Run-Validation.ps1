$ErrorActionPreference = 'Stop'
$taskProject = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$taskQa = Join-Path $env:LOCALAPPDATA ('Temp/EmberWaterSpiritAI-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskQa -Force | Out-Null
foreach ($taskFolder in @('Assets', 'Packages', 'ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $taskProject $taskFolder) -Destination $taskQa -Recurse -Force
}
$taskTests = Join-Path $taskQa 'Assets/WaterSpiritAIValidation'
New-Item -ItemType Directory -Path $taskTests -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'WaterSpiritAITests.cs') -Destination $taskTests
Copy-Item -LiteralPath (Join-Path $taskProject 'Validation/WaterSpirit/WaterSpiritTests.cs') -Destination $taskTests
Copy-Item -LiteralPath (Join-Path $taskProject 'Validation/Wave/WaterWaveTests.cs') -Destination $taskTests
Copy-Item -LiteralPath (Join-Path $taskProject 'Validation/Movement/EmberMovement.Tests.asmdef') -Destination $taskTests
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'WaterSpiritTestBehavior.cs') -Destination (Join-Path $taskQa 'Assets/EmberPrototype/Runtime')
# Historical scene/prefab builders compile only inside the isolated QA project.
$taskSupport = Join-Path $taskQa 'Assets/Editor/ValidationSupport'
New-Item -ItemType Directory -Path $taskSupport -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $taskProject 'Validation/EditorSupport') -File | Where-Object { $_.Extension -eq '.cs' -or $_.Name.EndsWith('.cs.meta') } | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $taskSupport $_.Name) -Force
}
$taskArtifacts = Join-Path $taskProject 'ValidationArtifacts/WaterAmbientParticles'
New-Item -ItemType Directory -Path $taskArtifacts -Force | Out-Null
$env:EMBER_SPIRIT_AI_ARTIFACTS = $taskArtifacts
$taskVersion = ((Get-Content (Join-Path $taskProject 'ProjectSettings/ProjectVersion.txt') -TotalCount 1) -split ': ')[1]
$taskEditor = 'C:/Program Files/Unity/Hub/Editor/' + $taskVersion + '/Editor/Unity.exe'
$taskArgs = @('-batchmode', '-projectPath', ('"' + $taskQa + '"'), '-runTests', '-testPlatform', 'EditMode', '-testFilter', 'WaterSpiritAIValidation|WaterSpiritValidation|WaveValidation', '-testResults', ('"' + (Join-Path $taskArtifacts 'UnityResults.xml') + '"'), '-logFile', ('"' + (Join-Path $taskArtifacts 'unity.log') + '"'))
$taskProcess = Start-Process -FilePath $taskEditor -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
Write-Output ('QA Unity PID: ' + $taskProcess.Id + '; project: ' + $taskQa)







