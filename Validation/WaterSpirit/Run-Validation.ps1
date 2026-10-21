$ErrorActionPreference = 'Stop'
$taskProject = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$taskQa = Join-Path $env:LOCALAPPDATA ('Temp/EmberWaterSpirit-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskQa -Force | Out-Null
foreach ($taskFolder in @('Assets', 'Packages', 'ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $taskProject $taskFolder) -Destination $taskQa -Recurse -Force
}
$taskTests = Join-Path $taskQa 'Assets/WaterSpiritValidation'
New-Item -ItemType Directory -Path $taskTests -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'WaterSpiritTests.cs') -Destination $taskTests
Copy-Item -LiteralPath (Join-Path $taskProject 'Validation/Movement/EmberMovement.Tests.asmdef') -Destination $taskTests
# Historical scene/prefab builders compile only inside the isolated QA project.
$taskSupport = Join-Path $taskQa 'Assets/Editor/ValidationSupport'
New-Item -ItemType Directory -Path $taskSupport -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $taskProject 'Validation/EditorSupport') -File | Where-Object { $_.Extension -eq '.cs' -or $_.Name.EndsWith('.cs.meta') } | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $taskSupport $_.Name) -Force
}
$taskArtifacts = Join-Path $taskProject 'ValidationArtifacts/WaterSpirit'
New-Item -ItemType Directory -Path $taskArtifacts -Force | Out-Null
$taskVersion = ((Get-Content (Join-Path $taskProject 'ProjectSettings/ProjectVersion.txt') -TotalCount 1) -split ': ')[1]
$taskEditor = 'C:/Program Files/Unity/Hub/Editor/' + $taskVersion + '/Editor/Unity.exe'
$taskArgs = @('-batchmode', '-projectPath', ('"' + $taskQa + '"'), '-runTests', '-testPlatform', 'EditMode', '-testFilter', 'WaterSpiritValidation', '-testResults', ('"' + (Join-Path $taskArtifacts 'UnityResults.xml') + '"'), '-logFile', ('"' + (Join-Path $taskArtifacts 'unity.log') + '"'))
$taskProcess = Start-Process -FilePath $taskEditor -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
Write-Output ('QA Unity PID: ' + $taskProcess.Id + '; project: ' + $taskQa)
