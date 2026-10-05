const path=require('node:path');
const {connectBrowser,collectDiagnostics,writeReport}=require('./browser-utils.cjs');

async function main(){
  const url=process.argv[2]||'http://127.0.0.1:8791/?qa=1';
  const browser=await connectBrowser();
  const context=await browser.newContext({viewport:{width:1440,height:1000}});
  const page=await context.newPage();const diagnostics=collectDiagnostics(page);
  await page.goto(url,{waitUntil:'networkidle'});
  await page.waitForFunction(()=>EmberDemo.getState().assetsReady);
  await page.locator('#overlayButton').click();
  const report=[];
  for(let index=0;index<8;index++){
    const result=await page.evaluate(index=>{
      const a=EmberDemo,trace=[];
      const s=()=>a.getState(),p=()=>s().player;
      function mark(name){trace.push({name,...s()});}
      function step(n){a.step(n);}
      function clean(){for(const k of ['KeyW','KeyA','KeyS','KeyD','Space','ShiftLeft'])a.input.release(k);a.input.mouse(0,0,'right',false);a.input.mouse(0,0,'left',false);step(1);}
      function until(test,max=400){let n=0;while(!test()&&s().mode==='playing'&&n++<max)step(1);if(!test())throw new Error('condition timeout '+JSON.stringify(s()));}
      function walk(x){a.input.press('KeyD');until(()=>p().x>=x||s().mode==='won'||s().mode==='finished');a.input.release('KeyD');}
      function jump(){a.input.press('KeyD');a.input.press('Space');step(1);}
      function absorb(x,y){a.input.mouse(x,y,'right',true);step(1);a.input.mouse(x,y,'right',false);until(()=>p().state==='anchor',100);clean();mark('anchor '+p().anchorId);}
      function igniteAbsorb(x,y){a.input.mouse(x,y,undefined,false);a.input.press('ShiftLeft');a.input.mouse(x,y,'right',true);step(1);a.input.release('ShiftLeft');a.input.mouse(x,y,'right',false);until(()=>p().state==='anchor',100);clean();mark('ignite-anchor '+p().anchorId);}
      function launch(dx,dy){clean();if(dx>0)a.input.press('KeyD');if(dx<0)a.input.press('KeyA');if(dy<0)a.input.press('KeyW');if(dy>0)a.input.press('KeyS');a.input.press('Space');step(1);a.input.release('Space');a.input.release('KeyW');a.input.release('KeyS');mark('launch');}
      function closeTo(x,y,r){until(()=>Math.hypot(p().x-x,p().y-y)<r,300);}
      try{
        clean();step(3);mark('start');
        if(index===0){walk(110);jump();step(65);a.input.release('Space');step(1);a.input.press('Space');step(90);a.input.release('Space');walk(480);step(60);jump();step(65);a.input.release('Space');step(1);a.input.press('Space');step(90);a.input.release('Space');walk(888);step(30);}
        if(index===1){walk(155);jump();step(85);a.input.release('Space');a.input.release('KeyD');step(25);mark('step');jump();step(43);igniteAbsorb(485,325);launch(1,-1);walk(888);step(30);}
        if(index===2){walk(235);jump();closeTo(390,290,123);igniteAbsorb(390,290);launch(1,-1);closeTo(610,190,123);igniteAbsorb(610,190);launch(1,0);walk(888);step(30);}
        if(index===3){walk(150);jump();step(40);absorb(248,285);launch(1,-1);closeTo(626,200,247);absorb(626,200);launch(1,0);walk(888);step(30);}
        if(index===4){walk(150);jump();closeTo(305,330,123);igniteAbsorb(305,330);launch(1,-1);closeTo(650,235,123);igniteAbsorb(650,235);launch(1,0);walk(888);step(30);if(s().mode==='playing'){a.input.press('KeyA');until(()=>s().mode==='won',120);a.input.release('KeyA');}}
        if(index===5){walk(150);jump();step(35);absorb(310,360);launch(0,-1);until(()=>p().y<=245,80);a.input.press('ShiftLeft');step(1);a.input.release('ShiftLeft');a.input.press('KeyD');a.input.press('Space');step(1);a.input.release('Space');until(()=>p().state==='free',60);mark('dash done');absorb(530,235);}
        if(index===6){walk(150);absorb(300,350);launch(1,-1);absorb(550,285);launch(1,-1);absorb(750,220);launch(1,0);walk(888);step(30);}
        if(index===7){absorb(280,365);step(330);mark('chase warning');launch(1,-1);absorb(530,285);launch(1,-1);absorb(760,220);launch(1,0);walk(888);step(30);}
        mark('end');return {index,ok:index===5?p().anchorId==='turn-end':['won','finished'].includes(s().mode),trace};
      }catch(error){mark('failed');return {index,ok:false,error:error.message,trace};}
    },index);
    report.push(result);
    console.log(JSON.stringify({index,ok:result.ok,error:result.error,end:result.trace.at(-1)},null,2));
    if(!result.ok)break;
    if(index===5){
      await page.evaluate(()=>window.scrollTo(0,210));
      await page.screenshot({path:path.join(__dirname,'playable-scene.png')});
      await page.evaluate(()=>{const a=EmberDemo;a.input.press('KeyD');a.input.press('KeyW');a.input.press('Space');a.step(1);a.input.release('Space');a.input.release('KeyW');for(let n=0;n<400&&a.getState().mode==='playing';n++)a.step(1);a.input.release('KeyD');});
      const finish=await page.evaluate(()=>EmberDemo.getState());
      if(finish.mode!=='won'){report.push({index,error:'sixth room exit failed',end:finish});break;}
    }
    if(index<7)await page.locator('#overlayButton').click();
  }
  writeReport('play-routes-report.json',{url,report,diagnostics});
  await browser.close();
}
main().then(()=>process.exit(0)).catch(e=>{console.error(e);process.exit(1);});
