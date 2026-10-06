$ErrorActionPreference = 'Stop'
$taskProject=(Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$taskQa=Join-Path $env:LOCALAPPDATA 'Temp/EmberWaterSpiritQA-20261006'
foreach ($taskFolder in @('Assets/Tests','Assets/Editor')) { New-Item -ItemType Directory -Path (Join-Path $taskQa $taskFolder) -Force | Out-Null }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'WaterWaveTests.cs') -Destination (Join-Path $taskQa 'Assets/Tests/WaterWaveTests.cs') -Force
Copy-Item -LiteralPath (Join-Path $taskProject 'Assets') -Destination $taskQa -Recurse -Force
$taskArtifacts=Join-Path $taskProject 'ValidationArtifacts/Wave'
New-Item -ItemType Directory -Path $taskArtifacts -Force | Out-Null
$taskResults=Join-Path $taskArtifacts 'UnityResults.xml'
if (Test-Path -LiteralPath $taskResults) { Remove-Item -LiteralPath $taskResults }
$taskLog=Join-Path $taskArtifacts 'unity.log'
$taskArguments=@('-batchmode','-projectPath',('"'+$taskQa+'"'),'-runTests','-testPlatform','EditMode','-testFilter','WaveValidation','-testResults',('"'+$taskResults+'"'),'-logFile',('"'+$taskLog+'"'))
$taskProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.5.2f1/Editor/Unity.exe' -ArgumentList $taskArguments -WindowStyle Hidden -PassThru
Write-Output ('QA Unity PID: '+$taskProcess.Id)
