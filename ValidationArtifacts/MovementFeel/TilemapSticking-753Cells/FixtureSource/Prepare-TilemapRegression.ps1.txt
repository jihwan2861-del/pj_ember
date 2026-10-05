param([string]$QaProject = "$env:LOCALAPPDATA/Temp/EmberMovementQA-20261004")
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$artifactRoot = Join-Path $projectRoot 'ValidationArtifacts/MovementFeel/TilemapSticking'
New-Item -ItemType Directory -Path (Join-Path $QaProject 'Assets/TileFixtures') -Force | Out-Null
$beforePath = Join-Path $artifactRoot 'scene2-before.unity.txt'
if (-not (Test-Path -LiteralPath $beforePath)) { throw 'The pre-fix scene snapshot must exist.' }
$afterPath = Join-Path $projectRoot 'Assets/Scenes/2.unity'
Copy-Item -LiteralPath $afterPath -Destination (Join-Path $artifactRoot 'scene2-after.unity.txt') -Force
$terrainIds = @('218775023','218775024','218775025','1924296175','1924296176','1924296177','1924296178','1924296179','1924296180','1924296181')
function Export-NativeTerrainScene([string]$sourcePath, [string]$destination) {
    $sourceText = Get-Content -LiteralPath $sourcePath -Raw
    $parts = [regex]::Split($sourceText, '(?m)(?=^--- !u!)')
    $output = "%YAML 1.1`n%TAG !u! tag:unity3d.com,2011:`n"
    foreach ($part in $parts) {
        $header = [regex]::Match($part, '^--- !u!(\d+) &(\d+)')
        if (-not $header.Success) { continue }
        $typeId = $header.Groups[1].Value
        $objectId = $header.Groups[2].Value
        if ($terrainIds -contains $objectId -or @('29','104','157','196') -contains $typeId) { $output += $part }
    }
    $output += "--- !u!1660057539 &9223372036854775807`nSceneRoots:`n  m_ObjectHideFlags: 0`n  m_Roots:`n  - {fileID: 218775025}`n"
    [IO.File]::WriteAllText($destination, $output, [Text.UTF8Encoding]::new($false))
}
Export-NativeTerrainScene $beforePath (Join-Path $QaProject 'Assets/TileFixtures/Scene2TerrainBefore.unity')
Export-NativeTerrainScene $afterPath (Join-Path $QaProject 'Assets/TileFixtures/Scene2TerrainAfter.unity')
foreach ($sourceName in @('Assets/tile/Mossy Tileset/Mossy - TileSet.png','Assets/tile/Mossy Tileset/Mossy - TileSet.png.meta','Assets/tile/Mossy Tileset/Autotile/Mossy Terrain RuleTile.asset','Assets/tile/Mossy Tileset/Autotile/Mossy Terrain RuleTile.asset.meta')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $sourceName) -Destination (Join-Path $QaProject ('Assets/TileFixtures/' + [IO.Path]::GetFileName($sourceName))) -Force
}
[ordered]@{
    capturedAt = (Get-Date -Format o)
    beforeSceneSha256 = (Get-FileHash -LiteralPath $beforePath -Algorithm SHA256).Hash
    afterSceneSha256 = (Get-FileHash -LiteralPath $afterPath -Algorithm SHA256).Hash
    terrainObjectIds = $terrainIds
    sceneExport = 'Original native scene documents for Grid, Tilemap, renderer, collider and added Rigidbody/Composite; original cells, tile GUIDs and transforms preserved. Other gameplay objects excluded.'
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $artifactRoot 'terrain-snapshot.json') -Encoding UTF8
& (Join-Path $PSScriptRoot 'Run-IsolatedRegression.ps1') -QaProject $QaProject -TestFilter 'EmberMovementRegression.InputAndPhysicsTests.Tilemap' -ArtifactSubdirectory 'TilemapSticking'
