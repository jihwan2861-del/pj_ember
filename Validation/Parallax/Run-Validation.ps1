$ErrorActionPreference = 'Stop'
$taskProject = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$taskArtifacts = Join-Path $taskProject 'ValidationArtifacts/Parallax'
$taskQa = Join-Path ([System.IO.Path]::GetTempPath()) ('EmberParallaxQA-' + [Guid]::NewGuid().ToString('N'))
if (Test-Path -LiteralPath $taskQa) { throw 'QA destination already exists; preserving it.' }
New-Item -ItemType Directory -Path $taskArtifacts -Force | Out-Null
foreach ($taskFolder in @('Assets/Runtime','Assets/ParallaxValidation','Packages','ProjectSettings')) {
    New-Item -ItemType Directory -Path (Join-Path $taskQa $taskFolder) -Force | Out-Null
}
Copy-Item -LiteralPath (Join-Path $taskProject 'Assets/EmberPrototype/Runtime/ParallaxLayer.cs') -Destination (Join-Path $taskQa 'Assets/Runtime')
Copy-Item -LiteralPath (Join-Path $taskProject 'Assets/EmberPrototype/Runtime/ParallaxLayer.cs.meta') -Destination (Join-Path $taskQa 'Assets/Runtime')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ParallaxTests.cs') -Destination (Join-Path $taskQa 'Assets/ParallaxValidation')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Parallax.Tests.asmdef') -Destination (Join-Path $taskQa 'Assets/ParallaxValidation')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA/manifest.json') -Destination (Join-Path $taskQa 'Packages/manifest.json')
Copy-Item -LiteralPath (Join-Path $taskProject 'ProjectSettings/ProjectVersion.txt') -Destination (Join-Path $taskQa 'ProjectSettings')

# Reuse installed test packages in the isolated project; never edit the user's packages.
foreach ($taskPackage in @('com.unity.test-framework','com.unity.ext.nunit')) {
    $taskSource = Get-ChildItem -LiteralPath (Join-Path $taskProject 'Library/PackageCache') -Directory |
        Where-Object { $_.Name.StartsWith($taskPackage + '@') } | Select-Object -First 1
    if ($null -eq $taskSource) { throw ('Missing cached test package: ' + $taskPackage) }
    & robocopy $taskSource.FullName (Join-Path $taskQa ('Packages/' + $taskPackage)) /E /R:0 /W:0 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw ('Package copy failed: ' + $taskPackage) }
}

$taskVersion = ((Get-Content -LiteralPath (Join-Path $taskProject 'ProjectSettings/ProjectVersion.txt') -TotalCount 1) -split ': ')[1]
$taskEditor = 'C:/Program Files/Unity/Hub/Editor/' + $taskVersion + '/Editor/Unity.exe'
$taskResults = Join-Path $taskArtifacts 'ParallaxResults.xml'
$taskLog = Join-Path $taskArtifacts 'parallax.log'
$taskArgs = @('-batchmode','-nographics','-projectPath',('"' + $taskQa + '"'),'-runTests','-testPlatform','EditMode',
    '-testFilter','ParallaxValidation','-testResults',('"' + $taskResults + '"'),'-logFile',('"' + $taskLog + '"'))
$taskProcess = Start-Process -FilePath $taskEditor -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
[PSCustomObject]@{ PID = $taskProcess.Id; QAProject = $taskQa; Results = $taskResults; Log = $taskLog }
