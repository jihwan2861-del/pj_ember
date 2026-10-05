const path = require('node:path');
const assert = require('node:assert/strict');
const { connectBrowser, collectDiagnostics, writeReport } = require('./browser-utils.cjs');

async function main() {
  const url = process.argv[2] || 'http://127.0.0.1:8765/?qa=1';
  const cdp = process.argv[3];
  const browser = await connectBrowser(cdp);
  const context = browser.contexts()[0] || await browser.newContext({viewport:{width:1440,height:1120}});
  const page = await context.newPage();
  await page.setViewportSize({width:1440,height:1120});
  const diagnostics = collectDiagnostics(page);
  await page.goto(url, {waitUntil:'networkidle'});
  await page.waitForFunction(() => window.EmberDemo?.getState().assetsReady);
  await page.screenshot({path:path.join(__dirname,'desktop-ready.png'),fullPage:true});
  assert.equal(await page.locator('#gameCanvas').count(),1);
  assert.equal((await page.evaluate(()=>EmberDemo.getState())).mode,'ready');
  await page.locator('#overlayButton').click();
  await page.keyboard.down('d');
  await page.evaluate(()=>EmberDemo.step(60));
  await page.keyboard.up('d');
  const moved = await page.evaluate(()=>EmberDemo.getState());
  assert.ok(moved.player.x>100,'real DOM keydown must move player');
  await page.keyboard.press('Escape');
  const paused = await page.evaluate(()=>{const a=EmberDemo.getState();EmberDemo.step(120);const b=EmberDemo.getState();return {a,b};});
  assert.equal(paused.a.mode,'paused');
  assert.equal(paused.a.player.x,paused.b.player.x);
  assert.equal(paused.a.time,paused.b.time);
  await page.keyboard.press('Escape');
  assert.equal((await page.evaluate(()=>EmberDemo.getState())).mode,'playing');
  await page.keyboard.press('r');
  const reset=await page.evaluate(()=>EmberDemo.getState());
  assert.equal(reset.player.x,72);
  const contracts=await page.evaluate(()=>{
    const api=EmberDemo,out=[];
    function clean(){for(const code of ['KeyW','KeyA','KeyS','KeyD','Space','ShiftLeft','ShiftRight'])api.input.release(code);api.input.mouse(0,0,'right',false);api.input.mouse(0,0,'left',false);}
    for(let index=0;index<EMBER_LEVELS.length;index++){
      clean();api.loadLevel(index);api.step(3);
      const before=api.getState();
      api.input.press('Space');api.step(15);api.input.release('Space');api.step(1);
      const jumped=api.getState();
      api.debugTeleport(500,600);api.step(1);
      const died=api.getState();api.step(80);
      const respawn=api.getState();
      out.push({index,id:before.levelId,jumpRose:jumped.player.y<before.player.y,deathMode:died.mode,respawnMode:respawn.mode,respawnX:respawn.player.x,torchesReset:JSON.stringify(respawn.torches.map(t=>t.lit))===JSON.stringify(EMBER_LEVELS[index].torches.map(t=>t.lit))});
    }
    clean();return out;
  });
  for(const item of contracts){assert.ok(item.jumpRose,item.id+' jump');assert.equal(item.deathMode,'dead',item.id+' death');assert.equal(item.respawnMode,'playing',item.id+' respawn');assert.equal(item.respawnX,72,item.id+' spawn');assert.ok(item.torchesReset,item.id+' torches reset');}
  await page.setViewportSize({width:390,height:844});
  await page.screenshot({path:path.join(__dirname,'mobile-ready.png'),fullPage:true});
  const mobile=await page.evaluate(()=>({width:innerWidth,scroll:document.documentElement.scrollWidth,canvas:document.querySelector('canvas').getBoundingClientRect().toJSON()}));
  assert.ok(mobile.scroll<=mobile.width+1,'mobile horizontal overflow');
  const report={url,checkedAt:new Date().toISOString(),moved,paused,contracts,mobile,diagnostics};
  const output=writeReport('browser-smoke-report.json',report);
  console.log(JSON.stringify({output,contracts:contracts.length,diagnostics,mobile},null,2));
  await page.close();
  if(!cdp)await browser.close();
}
main().then(()=>process.exit(0)).catch(error=>{console.error(error);process.exit(1);});
