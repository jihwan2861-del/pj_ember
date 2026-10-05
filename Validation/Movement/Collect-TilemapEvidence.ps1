param([string]$QaProject = "$env:LOCALAPPDATA/Temp/EmberMovementQA-20261004")
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$artifactRoot = Join-Path $projectRoot 'ValidationArtifacts/MovementFeel/TilemapSticking'
$run = Get-Content -LiteralPath (Join-Path $artifactRoot 'run.json') -Raw | ConvertFrom-Json
if (Get-Process -Id $run.pid -ErrorAction SilentlyContinue) { throw 'Wait for the isolated QA Unity process to exit first.' }
[xml]$results = Get-Content -LiteralPath (Join-Path $artifactRoot 'UnityResults.xml') -Raw
$caseNames = @('walk-before','walk-after','walk-after-extrusion01','landing-before','landing-after')
$metrics = @()
$rows = foreach ($name in $caseNames) {
    $data = Get-Content -LiteralPath (Join-Path $artifactRoot ($name + '.json')) -Raw | ConvertFrom-Json
    $metrics += [ordered]@{ case = $name; distance = $data.distance; stalledTicks = $data.stalledTicks; horizontalContactTicks = @($data.frames | Where-Object horizontalContact).Count; firstHorizontalContact = @($data.frames | Where-Object horizontalContact | Select-Object -First 1); endY = $data.endY; extrusion = $data.colliderExtrusion }
    foreach ($frame in $data.frames) {
        [pscustomobject]@{ case = $name; tick = $frame.tick; timeSeconds = ([double]$frame.tick + 1) * .02; x = $frame.x; y = $frame.y; vx = $frame.vx; vy = $frame.vy; horizontalContact = $frame.horizontalContact; sideNormalX = $frame.sideNormal.x; sideNormalY = $frame.sideNormal.y; sidePointX = $frame.sidePoint.x; sidePointY = $frame.sidePoint.y }
    }
}
$rows | Export-Csv -LiteralPath (Join-Path $artifactRoot 'simulation.csv') -NoTypeInformation -Encoding UTF8
$manifest = Get-Content -LiteralPath (Join-Path $artifactRoot 'terrain-snapshot.json') -Raw | ConvertFrom-Json
$entries = @()
$paths = @(
    @('original-before', (Join-Path $artifactRoot 'scene2-before.unity.txt')),
    @('original-after', (Join-Path $projectRoot 'Assets/Scenes/2.unity')),
    @('native-extracted-before', (Join-Path $QaProject 'Assets/TileFixtures/Scene2TerrainBefore.unity')),
    @('native-extracted-after', (Join-Path $QaProject 'Assets/TileFixtures/Scene2TerrainAfter.unity')),
    @('rule-tile', (Join-Path $projectRoot 'Assets/tile/Mossy Tileset/Autotile/Mossy Terrain RuleTile.asset')),
    @('rule-tile-meta', (Join-Path $projectRoot 'Assets/tile/Mossy Tileset/Autotile/Mossy Terrain RuleTile.asset.meta')),
    @('atlas', (Join-Path $projectRoot 'Assets/tile/Mossy Tileset/Mossy - TileSet.png')),
    @('atlas-meta', (Join-Path $projectRoot 'Assets/tile/Mossy Tileset/Mossy - TileSet.png.meta')),
    @('tilemap-fixture', (Join-Path $PSScriptRoot 'Scene2TilemapTests.cs'))
)
foreach ($pair in $paths) { $entries += [ordered]@{ kind = $pair[0]; path = $pair[1]; sha256 = (Get-FileHash -LiteralPath $pair[1] -Algorithm SHA256).Hash } }
foreach ($variant in @('Before','After')) {
    $nativePath = Join-Path $QaProject ('Assets/TileFixtures/Scene2Terrain' + $variant + '.unity')
    $savedPath = Join-Path $artifactRoot ('Scene2Terrain' + $variant + '.unity.txt')
    Copy-Item -LiteralPath $nativePath -Destination $savedPath -Force
    $entries += [ordered]@{ kind = ('preserved-native-extracted-' + $variant.ToLowerInvariant()); path = $savedPath; sha256 = (Get-FileHash -LiteralPath $savedPath -Algorithm SHA256).Hash }
}
$manifest | Add-Member -NotePropertyName finalCheckedAt -NotePropertyValue (Get-Date -Format o) -Force
$manifest | Add-Member -NotePropertyName files -NotePropertyValue $entries -Force
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $artifactRoot 'terrain-snapshot.json') -Encoding UTF8
$sourceSnapshot = Get-Content -LiteralPath (Join-Path $artifactRoot 'source-snapshot.json') -Raw | ConvertFrom-Json
$sourceMismatches = @()
foreach ($source in $sourceSnapshot.files) {
    $currentHash = (Get-FileHash -LiteralPath (Join-Path $projectRoot ('Assets/EmberPrototype/Runtime/' + $source.name)) -Algorithm SHA256).Hash
    if ($currentHash -ne $source.sha256 -or $source.sha256 -ne $source.copySha256) { $sourceMismatches += $source.name }
}
$summary = [ordered]@{
    unityVersion = '6000.5.2f1'
    result = $results.DocumentElement.GetAttribute('result')
    total = [int]$results.DocumentElement.GetAttribute('total')
    passed = [int]$results.DocumentElement.GetAttribute('passed')
    failed = [int]$results.DocumentElement.GetAttribute('failed')
    physicsStepSeconds = .02
    testNames = @($results.SelectNodes('//test-case') | ForEach-Object { $_.fullname })
    beforeGeometry = (Get-Content -LiteralPath (Join-Path $artifactRoot 'before-geometry.json') -Raw | ConvertFrom-Json)
    afterGeometry = (Get-Content -LiteralPath (Join-Path $artifactRoot 'after-geometry.json') -Raw | ConvertFrom-Json)
    metrics = $metrics
    csvRows = $rows.Count
    runtimeSourceMismatches = $sourceMismatches
    forcedCompositeGenerateGeometry = $false
    cause = 'Horizontal contact and stalled velocity reproduced at an internal tile seam on actual flat cell row -7; disappears after native Composite merge with identical cells and player code.'
    scope = 'Four new native-terrain tests, separate from the previous 17 generic-floor regressions. Original active editor not driven; full route/render/player build not exercised.'
}
$summary | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath (Join-Path $artifactRoot 'summary.json') -Encoding UTF8
Write-Output ('Tilemap tests: ' + $summary.passed + '/' + $summary.total)
Write-Output ('CSV rows: ' + $rows.Count)
Write-Output ('Runtime source mismatches: ' + $sourceMismatches.Count)
