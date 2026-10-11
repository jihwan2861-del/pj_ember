param([ValidateSet('Generate','Test')][string]$Mode = 'Generate', [string]$QAProject)
$ErrorActionPreference = 'Stop'
$taskProject = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$taskArtifacts = Join-Path $taskProject 'ValidationArtifacts/MainMenu'
New-Item -ItemType Directory -Path $taskArtifacts -Force | Out-Null
if ($Mode -eq 'Generate') {
    $taskQa = Join-Path ([System.IO.Path]::GetTempPath()) ('EmberMainMenuQA-' + [Guid]::NewGuid().ToString('N'))
    if (Test-Path -LiteralPath $taskQa) { throw 'Preserving existing QA destination.' }
    foreach ($taskFolder in @('Assets/Runtime','Assets/Editor','Assets/MainMenuValidation','Assets/Scenes','Assets/Settings','Assets/image','Assets/EmberPrototype/UI','Packages','ProjectSettings')) {
        New-Item -ItemType Directory -Path (Join-Path $taskQa $taskFolder) -Force | Out-Null
    }
    foreach ($taskScript in @('EmberMainMenu','MainMenuButtonFeedback','MainMenuBackgroundCover','MainMenuAmbientEmbers')) {
        foreach ($taskSuffix in @('.cs','.cs.meta')) {
            Copy-Item -LiteralPath (Join-Path $taskProject ('Assets/EmberPrototype/Runtime/' + $taskScript + $taskSuffix)) -Destination (Join-Path $taskQa 'Assets/Runtime')
        }
    }
    # Keep the same predefined Assembly-CSharp as production when serializing the scene.
    # Tests use reflection so no validation-only runtime assembly identity leaks into assets.
    foreach ($taskSuffix in @('.cs','.cs.meta')) {
        Copy-Item -LiteralPath (Join-Path $taskProject ('Assets/EmberPrototype/Editor/EmberMainMenuSceneBuilder' + $taskSuffix)) -Destination (Join-Path $taskQa 'Assets/Editor')
    }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MainMenuValidationBatch.cs') -Destination (Join-Path $taskQa 'Assets/Editor')
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MainMenuTests.cs') -Destination (Join-Path $taskQa 'Assets/MainMenuValidation')
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MainMenu.Tests.asmdef') -Destination (Join-Path $taskQa 'Assets/MainMenuValidation')
    Copy-Item -LiteralPath (Join-Path $taskProject 'Assets/EmberPrototype/UI/MainMenu') -Destination (Join-Path $taskQa 'Assets/EmberPrototype/UI') -Recurse
    foreach ($taskFile in @('Assets/image/ProjectEmber_TitleBackground.png','Assets/image/ProjectEmber_TitleBackground.png.meta',
        'Assets/Scenes/game 1.unity','Assets/Scenes/game 1.unity.meta',
        'Assets/Settings/InputSystem_Actions.inputactions','Assets/Settings/InputSystem_Actions.inputactions.meta')) {
        Copy-Item -LiteralPath (Join-Path $taskProject $taskFile) -Destination (Join-Path $taskQa $taskFile)
    }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA/manifest.json') -Destination (Join-Path $taskQa 'Packages/manifest.json')
    foreach ($taskSetting in @('ProjectVersion.txt','EditorBuildSettings.asset','ProjectSettings.asset')) {
        Copy-Item -LiteralPath (Join-Path $taskProject ('ProjectSettings/' + $taskSetting)) -Destination (Join-Path $taskQa 'ProjectSettings')
    }
    foreach ($taskPackage in @('com.unity.inputsystem','com.unity.ugui','com.unity.test-framework','com.unity.ext.nunit')) {
        $taskSource = Get-ChildItem -LiteralPath (Join-Path $taskProject 'Library/PackageCache') -Directory |
            Where-Object { $_.Name.StartsWith($taskPackage + '@') } | Select-Object -First 1
        if ($null -eq $taskSource) { throw ('Missing cached package: ' + $taskPackage) }
        & robocopy $taskSource.FullName (Join-Path $taskQa ('Packages/' + $taskPackage)) /E /R:0 /W:0 /NFL /NDL /NJH /NJS /NP | Out-Null
        if ($LASTEXITCODE -ge 8) { throw ('Package copy failed: ' + $taskPackage) }
    }
} else {
    if ([string]::IsNullOrWhiteSpace($QAProject)) { throw 'Specify the existing isolated QA project.' }
    $taskQa = (Resolve-Path -LiteralPath $QAProject).Path
    if (-not (Test-Path -LiteralPath (Join-Path $taskQa 'Assets/Scenes/EmberMainMenu.unity'))) { throw 'Generate the menu in QA first.' }
}
$taskVersion = ((Get-Content -LiteralPath (Join-Path $taskProject 'ProjectSettings/ProjectVersion.txt') -TotalCount 1) -split ': ')[1]
$taskEditor = 'C:/Program Files/Unity/Hub/Editor/' + $taskVersion + '/Editor/Unity.exe'
$env:EMBER_MENU_ARTIFACTS = $taskArtifacts
$taskLog = Join-Path $taskArtifacts ($Mode.ToLowerInvariant() + '.log')
$taskResults = Join-Path $taskArtifacts 'MainMenuResults.xml'
$taskArgs = @('-batchmode','-projectPath',('"' + $taskQa + '"'),'-logFile',('"' + $taskLog + '"'))
if ($Mode -eq 'Generate') {
    $taskArgs += @('-executeMethod','MainMenuValidationBatch.Generate')
} else {
    $taskArgs += @('-runTests','-testPlatform','PlayMode','-testFilter','MainMenuValidation','-testResults',('"' + $taskResults + '"'))
}
$taskProcess = Start-Process -FilePath $taskEditor -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
[PSCustomObject]@{ PID = $taskProcess.Id; QAProject = $taskQa; Results = $taskResults; Log = $taskLog; Artifacts = $taskArtifacts }
