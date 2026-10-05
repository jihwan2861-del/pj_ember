const assert=require('node:assert/strict');
const {connectBrowser,collectDiagnostics,writeReport}=require('./browser-utils.cjs');

async function main(){
  const browser=await connectBrowser();const page=await browser.newPage({viewport:{width:1440,height:1000}});
  const diagnostics=collectDiagnostics(page);
  await page.goto('http://127.0.0.1:8791/?qa=1',{waitUntil:'networkidle'});
  await page.waitForFunction(()=>EmberDemo.getState().assetsReady);
  const result=await page.evaluate(()=>{
    const a=EmberDemo,s=()=>a.getState(),p=()=>s().player;
    function step(n){a.step(n);}
    function clean(){for(const k of ['KeyW','KeyA','KeyS','KeyD','Space','ShiftLeft'])a.input.release(k);a.input.mouse(0,0,'right',false);a.input.mouse(0,0,'left',false);step(1);}
    function until(test,max=400){let n=0;while(!test()&&s().mode==='playing'&&n++<max)step(1);if(!test())throw Error('timeout '+JSON.stringify(s()));}
    function absorb(x,y){a.input.mouse(x,y,'right',true);step(1);a.input.mouse(x,y,'right',false);until(()=>p().state==='anchor',100);clean();}
    function launch(x,y){clean();if(x)a.input.press('KeyD');if(y<0)a.input.press('KeyW');a.input.press('Space');step(1);a.input.release('Space');a.input.release('KeyW');}
    a.loadLevel(5);a.input.press('ShiftLeft');step(1);a.input.release('ShiftLeft');
    const groundedRing=s();step(20);const waitingRing=s();
    clean();a.restart();a.input.press('KeyD');until(()=>p().x>=150);a.input.press('Space');step(35);absorb(310,360);
    launch(0,-1);until(()=>p().y<=245,80);a.input.press('ShiftLeft');step(1);a.input.release('ShiftLeft');
    const airRing=s();a.input.press('KeyD');a.input.press('Space');step(1);a.input.release('Space');until(()=>p().state==='free',60);
    const dashEnd=s();absorb(530,235);const recovered=s();launch(1,-1);until(()=>s().mode==='won');const room6=s();
    clean();a.loadLevel(7);absorb(280,365);step(330);const chaseWarning=s();launch(1,-1);absorb(530,285);launch(1,-1);absorb(760,220);launch(1,0);until(()=>s().mode==='finished');
    return {groundedRing,waitingRing,airRing,dashEnd,recovered,room6,chaseWarning,room8:s()};
  });
  assert.equal(result.groundedRing.player.ringReady,false);
  assert.equal(result.waitingRing.player.ringReady,false);
  assert.equal(result.airRing.player.ringReady,false);
  assert.equal(result.dashEnd.player.grounded,false);
  assert.equal(result.dashEnd.player.ringReady,false);
  assert.equal(result.recovered.player.ringReady,true);
  assert.equal(result.room6.mode,'won');assert.equal(result.room8.mode,'finished');
  await page.waitForTimeout(200);
  assert.deepEqual(diagnostics,{pageErrors:[],consoleErrors:[],failedRequests:[],badResponses:[]});
  const output=writeReport('regressions-report.json',{result,diagnostics});
  console.log(JSON.stringify({output,ringWaiting:result.waitingRing.player.ringReady,dashRingReady:result.dashEnd.player.ringReady,absorbedRingReady:result.recovered.player.ringReady,room6:result.room6.mode,room8:result.room8.mode,diagnostics},null,2));
  await browser.close();
}
main().then(()=>process.exit(0)).catch(e=>{console.error(e);process.exit(1);});
