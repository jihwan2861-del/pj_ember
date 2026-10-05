/* Ember: a dependency-free browser movement prototype. World coordinates: 960 x 540. */
(() => {
  'use strict';
  const canvas = document.getElementById('gameCanvas');
  const ctx = canvas.getContext('2d');
  const $ = id => document.getElementById(id);
  const W = 960, H = 540, DT = 1 / 120;
  const QA = new URLSearchParams(location.search).has('qa');
  const C = { move:220, jump:420, launch:580, gravity:1000, fall:1250,
    ignite:125, absorb:250, travel:950, dash:650, dashTime:.16, ringTime:.42 };
  const keys = new Set(), pressed = new Set(), released = new Set();
  const mouse = { x:480,y:240,left:false,right:false,leftPressed:false,leftReleased:false,rightPressed:false };
  const art = new Image(); art.src = 'assets/ember-atlas.png';
  const backdrop = new Image(); backdrop.src = 'assets/ember-scene.png';
  // The generated atlas preserves alpha. Per-cell visible baselines prevent feet from sinking into ledges.
  const atlasFoot=[1,.994,1,1,.963,.957,.963,.969,.910,.957,.848,.874,.858,.858,.858,.858];
  let levelIndex=0, level, p, mode='ready', deaths=0, runTime=0, visualTime=0;
  let particles=[], shots=[], trail=[], enemy=null, shake=0, ringFlash=0;
  let deadTimer=0, launchProtect=0, jumpBuffer=0, coyote=0, shotCooldown=0;
  let charging=false, chargeTime=0, toast='', toastTime=0, furthest=0;
  let muted=true, audio=null, uiClock=0;
  try { furthest=Math.min(7,Math.max(0,Number(localStorage.getItem('ember.unlocked')||0))); } catch (_) {}

  const clamp=(n,a,b)=>Math.max(a,Math.min(b,n));
  const approach=(a,b,d)=>a<b?Math.min(a+d,b):Math.max(a-d,b);
  const dist=(a,b)=>Math.hypot(a.x-b.x,a.y-b.y);
  const down=(...codes)=>codes.some(code=>keys.has(code));
  function direction(defaultUp=false) {
    let x=Number(down('KeyD','ArrowRight'))-Number(down('KeyA','ArrowLeft'));
    let y=Number(down('KeyS','ArrowDown'))-Number(down('KeyW','ArrowUp'));
    if(!x&&!y) { if(defaultUp)y=-1; else x=p.facing||1; }
    const n=Math.hypot(x,y); return {x:x/n,y:y/n};
  }
  function bounds(x=p.x,y=p.y) { return {x:x-p.w/2,y:y-p.h/2,w:p.w,h:p.h}; }
  function overlap(a,b) { return a.x<b.x+b.w&&a.x+a.w>b.x&&a.y<b.y+b.h&&a.y+a.h>b.y; }
  function doorOpen() { return !level.door || level.door.requires.every(id=>level.torches.find(t=>t.id===id)?.lit); }
  function solids() { return level.door&&!doorOpen()?[...level.platforms,level.door]:level.platforms; }
  function segmentRect(a,b,r) {
    let lo=0,hi=1;
    for(const [axis,size] of [['x','w'],['y','h']]) {
      const delta=b[axis]-a[axis];
      if(Math.abs(delta)<.00001) { if(a[axis]<=r[axis]||a[axis]>=r[axis]+r[size])return false; }
      else {
        let t0=(r[axis]-a[axis])/delta,t1=(r[axis]+r[size]-a[axis])/delta;
        if(t0>t1)[t0,t1]=[t1,t0];
        lo=Math.max(lo,t0);hi=Math.min(hi,t1);if(lo>hi)return false;
      }
    }
    return hi>.0001 && lo<.9999;
  }
  function clearPath(a,b) {
    return !solids().some(r=>segmentRect(a,b,{x:r.x-p.w/2+.08,y:r.y-p.h/2+.08,w:r.w+p.w-.16,h:r.h+p.h-.16}));
  }
  function candidate() {
    if(!level.abilities.absorb)return null;
    return level.torches.filter(t=>t.lit&&t.id!==p.anchorId&&dist(p,t)<=C.absorb&&clearPath(p,t))
      .sort((a,b)=>(dist(a,mouse)+dist(a,p)*.12)-(dist(b,mouse)+dist(b,p)*.12))[0]||null;
  }
  function tell(text,seconds=2) { toast=text;toastTime=seconds; $('statusMessage').textContent=text; }
  function sound(type) {
    if(muted)return;
    try {
      audio ||= new (window.AudioContext||window.webkitAudioContext)();
      if(audio.state==='suspended')audio.resume();
      const settings={jump:[510,260,.09],ignite:[180,680,.18],absorb:[260,920,.16],launch:[740,240,.17],dash:[880,330,.1],death:[180,55,.2],win:[440,880,.28],shot:[640,180,.1],warn:[100,160,.16]};
      const [start,end,length]=settings[type]||settings.jump;
      const osc=audio.createOscillator(),gain=audio.createGain(),now=audio.currentTime;
      osc.type=type==='death'||type==='warn'?'triangle':'sine';
      osc.frequency.setValueAtTime(start,now);osc.frequency.exponentialRampToValueAtTime(end,now+length);
      gain.gain.setValueAtTime(.0001,now);gain.gain.exponentialRampToValueAtTime(.09,now+.01);
      gain.gain.exponentialRampToValueAtTime(.0001,now+length);
      osc.connect(gain);gain.connect(audio.destination);osc.start(now);osc.stop(now+length+.02);
    } catch (_) {}
  }
  function sparks(x,y,n=15,color='#ffc774',power=100) {
    for(let i=0;i<n;i++) {
      const a=Math.random()*Math.PI*2,v=power*(.25+Math.random());
      particles.push({x,y,vx:Math.cos(a)*v,vy:Math.sin(a)*v,life:.25+Math.random()*.45,max:.7,color,size:1+Math.random()*2.4});
    }
  }
  function resetTransient() {
    particles=[];shots=[];trail=[];shake=0;charging=false;chargeTime=0;
    launchProtect=jumpBuffer=coyote=shotCooldown=0;pressed.clear();released.clear();
    mouse.leftPressed=mouse.leftReleased=mouse.rightPressed=false;
  }
  function loadLevel(index,playing=false) {
    levelIndex=clamp(index,0,window.EMBER_LEVELS.length-1);
    level=JSON.parse(JSON.stringify(window.EMBER_LEVELS[levelIndex]));
    resetTransient();
    p={x:level.spawn.x,y:level.spawn.y,w:18,h:26,vx:0,vy:0,state:'free',grounded:false,
      airJump:1,ringReady:true,ringTimer:0,dashTimer:0,anchorId:null,targetId:null,facing:1,jumping:false};
    p.grounded=solids().some(r=>p.x+p.w/2>r.x&&p.x-p.w/2<r.x+r.w&&Math.abs(p.y+p.h/2-r.y)<.1);
    enemy=level.chase?{phase:'grace',timer:2.1,x:70,y:145,vx:0,vy:0,aimX:p.x,aimY:p.y,anchorTarget:null,locked:false,cycle:0}:null;
    mode=playing?'playing':'ready';toast='';toastTime=0;
    buildProgress();updateUI();showOverlay();
  }
  function buildProgress() {
    const box=$('progressDots');box.replaceChildren();
    window.EMBER_LEVELS.forEach((l,i)=>{
      const button=document.createElement('button');button.type='button';button.className='progress-dot';
      button.classList.toggle('active',i===levelIndex);button.classList.toggle('complete',i<furthest);
      button.disabled=i>furthest;button.setAttribute('aria-label',`${i+1}번 방: ${l.title}`);
      if(i===levelIndex)button.setAttribute('aria-current','step');
      button.textContent=String(i+1).padStart(2,'0');button.addEventListener('click',()=>{loadLevel(i,true);canvas.focus();});
      box.appendChild(button);
    });
  }
  function showOverlay() {
    const visible=['ready','paused','won','finished','dead'].includes(mode);
    $('gameOverlay').hidden=!visible;
    if(!visible)return;
    const content={
      ready:['작은 불에서, 새로운 길로.','횃불을 깨우고 불꽃 사이를 이동하세요. 여덟 개의 방 끝에 어둠이 기다립니다.','회랑에 들어가기'],
      paused:['잠시 숨을 고르세요.','길은 그대로 기다리고 있습니다. Esc를 눌러 돌아갈 수 있어요.','계속하기'],
      dead:['불꽃은 다시 피어납니다.','잠시 후 이 방의 시작점에서 다시 이어집니다.','바로 재시도'],
      won:['다음 불빛을 향해.','방을 통과했습니다. 다음 방에서 새로운 길을 찾아보세요.','다음 방'],
      finished:['회랑이 다시 숨 쉽니다.',`완주 ${formatTime(runTime)} · 다시 피어난 불꽃 ${deaths}회. 마지막 불이 정원을 깨웠습니다.`,'처음부터 다시하기']
    }[mode];
    $('overlayTitle').textContent=content[0];$('overlayText').textContent=content[1];$('overlayButton').textContent=content[2];
  }
  function primary() {
    if(mode==='won')loadLevel(levelIndex+1,true);
    else if(mode==='finished') { deaths=0;runTime=0;loadLevel(0,true); }
    else if(mode==='dead')restart();
    else if(mode==='paused') { mode='playing';pressed.clear();showOverlay(); }
    else if(mode==='ready') { mode='playing';pressed.clear();showOverlay(); }
    canvas.focus();
  }
  function restart() { loadLevel(levelIndex,true);canvas.focus(); }
  function pause() {
    if(mode==='playing')mode='paused';else if(mode==='paused')mode='playing';else return;
    pressed.clear();released.clear();charging=false;mouse.left=false;showOverlay();updateUI();
  }
  function die(reason) {
    if(mode!=='playing')return;
    deaths++;mode='dead';deadTimer=.62;sparks(p.x,p.y,30,'#ff9d65',190);shake=5;sound('death');
    charging=false;pressed.clear();released.clear();showOverlay();updateUI();
    $('overlayText').textContent=reason||'작은 실패가 다음 길을 알려줍니다.';
  }
  function win() {
    if(mode!=='playing')return;
    furthest=Math.max(furthest,Math.min(window.EMBER_LEVELS.length-1,levelIndex+1));
    try {localStorage.setItem('ember.unlocked',String(furthest));}catch(_){}
    mode=levelIndex===window.EMBER_LEVELS.length-1?'finished':'won';
    sparks(p.x,p.y,50,'#ffe3a2',170);sound('win');charging=false;keys.clear();buildProgress();showOverlay();updateUI();
  }
  function ignite() {
    if(!level.abilities.ignite) {tell('이 방에서는 점프부터 익혀보세요.');return;}
    if(p.state!=='free'||!p.ringReady) {if(!p.ringReady)tell('착지하거나 불에 들어가면 점화를 다시 쓸 수 있어요.');return;}
    p.ringReady=false;p.state='ring';p.ringTimer=C.ringTime;p.vx=p.vy=0;p.jumping=false;ringFlash=.3;
    level.torches.forEach(t=>{if(dist(p,t)<=C.ignite&&!t.lit){t.lit=true;sparks(t.x,t.y,22);}});
    sparks(p.x,p.y,18);sound('ignite');
  }
  function absorb() {
    if(!level.abilities.absorb)return false;
    const target=candidate();
    if(!target) {tell('켜진 횃불 가까이에서, 벽에 막히지 않은 길을 골라주세요.');return false;}
    p.state='travel';p.targetId=target.id;p.anchorId=null;p.vx=p.vy=0;p.jumping=false;
    charging=false;sparks(p.x,p.y,10);sound('absorb');return true;
  }
  function launch() {
    const d=direction(true),destination={x:p.x+d.x*28,y:p.y+d.y*28};
    if(!clearPath(p,destination)||destination.x<10||destination.x>W-10) {tell('이 방향은 막혀 있어요. 다른 방향을 골라주세요.');return;}
    p.x=destination.x;p.y=destination.y;p.anchorId=null;p.state='free';p.vx=d.x*C.launch;p.vy=d.y*C.launch;
    p.grounded=false;p.jumping=false;launchProtect=.18;jumpBuffer=0;coyote=0;
    if(d.x)p.facing=Math.sign(d.x);sparks(p.x,p.y,20,'#ffd48c',180);sound('launch');
  }
  function dash() {
    if(!level.abilities.dash) {tell('링 대시는 여섯 번째 방에서 배웁니다.');return;}
    const d=direction();p.state='dash';p.dashTimer=C.dashTime;p.vx=d.x*C.dash;p.vy=d.y*C.dash;
    p.grounded=false;p.jumping=false;jumpBuffer=0;sparks(p.x,p.y,12);sound('dash');
  }
  function fireShot() {
    if(!level.abilities.shot||shotCooldown>0||!['free','ring'].includes(p.state))return;
    const dx=mouse.x-p.x,dy=mouse.y-p.y,n=Math.hypot(dx,dy)||1,speed=630+Math.min(chargeTime,.8)*180;
    shots.push({x:p.x,y:p.y,vx:dx/n*speed,vy:dy/n*speed,life:520/speed});
    if(p.state==='ring')p.state='free';shotCooldown=.28;sparks(p.x,p.y,8);sound('shot');
  }
  function actions() {
    if(pressed.has('ShiftLeft')||pressed.has('ShiftRight'))ignite();
    if(mouse.rightPressed && ['free','ring','anchor'].includes(p.state) && absorb())return;
    if(pressed.has('Space')) {
      if(p.state==='anchor')launch();
      else if(p.state==='ring')dash();
      else if(p.state==='free')jumpBuffer=.12;
    }
    if(mouse.leftPressed&&level.abilities.shot&&['free','ring'].includes(p.state)){charging=true;chargeTime=0;}
    if(mouse.leftReleased&&charging){fireShot();charging=false;chargeTime=0;}
    if(released.has('Space')&&p.jumping&&p.vy<0){p.vy*=.5;p.jumping=false;}
  }
  function moveBody(dt) {
    let hit=false;p.x+=p.vx*dt;
    for(const r of solids())if(overlap(bounds(),r)){p.x=p.vx>0?r.x-p.w/2:r.x+r.w+p.w/2;p.vx=0;hit=true;}
    p.x=clamp(p.x,p.w/2,W-p.w/2);
    p.y+=p.vy*dt;p.grounded=false;
    for(const r of solids())if(overlap(bounds(),r)) {
      if(p.vy>=0){p.y=r.y-p.h/2;p.grounded=true;}else p.y=r.y+r.h+p.h/2;
      p.vy=0;hit=true;
    }
    return hit;
  }
  function updateEnemy(dt) {
    if(!enemy)return;
    enemy.timer-=dt;
    if(enemy.phase==='grace'&&enemy.timer<=0)prepareEnemy();
    else if(enemy.phase==='warn') {
      if(enemy.timer>.25){enemy.aimX=p.x;enemy.aimY=p.y;enemy.anchorTarget=p.state==='anchor'?p.anchorId:null;}
      else if(!enemy.locked){enemy.locked=true;sound('warn');}
      if(enemy.timer<=0){
        if(enemy.anchorTarget&&p.state==='anchor'&&p.anchorId===enemy.anchorTarget){die('어둠이 이 불을 노렸어요. 예고가 끝나기 전에 발사하거나 다른 불로 이동하세요.');return;}
        const dx=enemy.aimX-enemy.x,dy=enemy.aimY-enemy.y,n=Math.hypot(dx,dy)||1;
        enemy.vx=dx/n*1100;enemy.vy=dy/n*1100;enemy.phase='attack';enemy.timer=.8;sound('dash');
      }
    } else if(enemy.phase==='attack') {
      enemy.x+=enemy.vx*dt;enemy.y+=enemy.vy*dt;
      if(dist(enemy,p)<27){die('돌진이 지나갈 방향을 보고, 잠긴 순간에 다른 길로 피하세요.');return;}
      if(enemy.timer<=0){enemy.phase='recover';enemy.timer=1.45;}
    } else if(enemy.phase==='recover'&&enemy.timer<=0)prepareEnemy();
  }
  function prepareEnemy() {
    enemy.cycle++;enemy.x=p.x<W/2?875:85;enemy.y=[145,245,100][enemy.cycle%3];
    enemy.phase='warn';enemy.timer=1.35;enemy.locked=false;enemy.aimX=p.x;enemy.aimY=p.y;enemy.anchorTarget=null;
    sparks(enemy.x,enemy.y,12,'#afa5f4',80);sound('warn');
  }
  function update(dt) {
    visualTime+=dt;toastTime=Math.max(0,toastTime-dt);ringFlash=Math.max(0,ringFlash-dt);shake=Math.max(0,shake-dt*20);
    particles.forEach(a=>{a.x+=a.vx*dt;a.y+=a.vy*dt;a.vy-=60*dt;a.life-=dt;});particles=particles.filter(a=>a.life>0);
    trail.forEach(a=>a.life-=dt);trail=trail.filter(a=>a.life>0);
    if($('helpDialog').open||$('artDialog').open){clearEdges();return;}
    if(mode==='dead'){runTime+=dt;deadTimer-=dt;if(deadTimer<=0)restart();clearEdges();return;}
    if(mode!=='playing'){clearEdges();return;}
    runTime+=dt;shotCooldown=Math.max(0,shotCooldown-dt);launchProtect=Math.max(0,launchProtect-dt);
    // Recover on normal grounded movement, before consuming an ability. A grounded ring must not refund itself.
    if(p.grounded&&p.state==='free'){coyote=.12;p.airJump=1;p.ringReady=true;}else coyote=Math.max(0,coyote-dt);
    actions();if(charging)chargeTime=Math.min(1,chargeTime+dt);
    jumpBuffer=Math.max(0,jumpBuffer-dt);
    if(p.state==='anchor') {
      const t=level.torches.find(t=>t.id===p.anchorId);p.x=t.x;p.y=t.y;p.vx=p.vy=0;p.airJump=1;p.ringReady=true;
    } else if(p.state==='travel') {
      const t=level.torches.find(t=>t.id===p.targetId),dx=t.x-p.x,dy=t.y-p.y,d=Math.hypot(dx,dy);
      trail.push({x:p.x,y:p.y,life:.2});
      if(d<C.travel*dt+1){p.x=t.x;p.y=t.y;p.state='anchor';p.anchorId=t.id;p.targetId=null;p.airJump=1;p.ringReady=true;sparks(t.x,t.y,14);}
      else {p.x+=dx/d*C.travel*dt;p.y+=dy/d*C.travel*dt;}
    } else if(p.state==='ring') {
      p.ringTimer-=dt;if(p.ringTimer<=0)p.state='free';
    } else if(p.state==='dash') {
      trail.push({x:p.x,y:p.y,life:.18});p.dashTimer-=dt;
      const hit=moveBody(dt);
      if(hit||p.dashTimer<=0){p.state='free';p.vx*=.45;p.vy=p.vy<0?Math.max(p.vy*.3,-140):p.vy*.3;launchProtect=.08;}
    } else {
      const x=Number(down('KeyD','ArrowRight'))-Number(down('KeyA','ArrowLeft'));
      if(x)p.facing=x;
      if(launchProtect<=0)p.vx=approach(p.vx,x*C.move,(x?(p.grounded?1550:850):(p.grounded?1900:450))*dt);
      if(jumpBuffer>0 && (coyote>0||p.grounded||p.airJump>0)) {
        if(!p.grounded&&coyote<=0)p.airJump--;
        p.vy=-C.jump;p.grounded=false;p.jumping=true;coyote=0;jumpBuffer=0;sparks(p.x,p.y+12,5);sound('jump');
      }
      let g=p.vy>0?C.fall:(p.jumping&&down('Space')?900:C.gravity);
      if(Math.abs(p.vy)<60&&p.jumping&&down('Space'))g*=.65;
      p.vy=Math.min(p.vy+g*dt,800);moveBody(dt);
    }
    for(const shot of shots) {
      const before={x:shot.x,y:shot.y};shot.x+=shot.vx*dt;shot.y+=shot.vy*dt;shot.life-=dt;
      const hitTorch=level.torches.find(t=>Math.hypot(t.x-shot.x,t.y-shot.y)<21);
      if(hitTorch){hitTorch.lit=true;shot.life=0;sparks(hitTorch.x,hitTorch.y,18);}
      else if(solids().some(r=>segmentRect(before,shot,r)))shot.life=0;
    }
    shots=shots.filter(s=>s.life>0);
    if(p.state!=='anchor' && (p.y>H+35||level.hazards.some(h=>overlap(bounds(),h))))die('다음 착지점과 불의 위치를 보고 다시 이어보세요.');
    if(mode==='playing')updateEnemy(dt);
    if(mode==='playing' && doorOpen() && overlap(bounds(),level.goal))win();
    clearEdges();
  }
  function clearEdges(){pressed.clear();released.clear();mouse.leftPressed=mouse.leftReleased=mouse.rightPressed=false;}
  function formatTime(s){return `${Math.floor(s/60).toString().padStart(2,'0')}:${Math.floor(s%60).toString().padStart(2,'0')}`;}
  function updateUI() {
    $('levelName').textContent=level.title;$('levelSubtitle').textContent=level.subtitle;
    $('levelCounter').textContent=`${String(levelIndex+1).padStart(2,'0')} / ${String(window.EMBER_LEVELS.length).padStart(2,'0')}`;
    $('deathCount').textContent=String(deaths).padStart(2,'0');$('timerValue').textContent=formatTime(runTime);
    $('hintText').textContent=toastTime>0?toast:level.hint;
    $('abilityStatus').textContent=level.abilities.ignite?`${p.ringReady?'●':'○'} 점화  ·  ${p.airJump?'●':'○'} 공중 점프`:`${p.airJump?'●':'○'} 공중 점프`;
    $('pauseButton').setAttribute('aria-label',mode==='paused'?'계속하기':'일시정지');
    $('pauseButton').setAttribute('aria-pressed',String(mode==='paused'));
    $('soundButton').setAttribute('aria-label',muted?'소리 켜기':'소리 끄기');$('soundButton').setAttribute('aria-pressed',String(!muted));
    $('soundButton').classList.toggle('is-on',!muted);
  }
  function gradient(x,y,r,color) {
    const g=ctx.createRadialGradient(x,y,0,x,y,r);g.addColorStop(0,color);g.addColorStop(1,'transparent');ctx.fillStyle=g;ctx.fillRect(x-r,y-r,r*2,r*2);
  }
  function background() {
    const g=ctx.createLinearGradient(0,0,0,H);g.addColorStop(0,'#152d39');g.addColorStop(.6,'#0e202a');g.addColorStop(1,'#070f17');ctx.fillStyle=g;ctx.fillRect(0,0,W,H);
    if(backdrop.complete&&backdrop.naturalWidth){
      // Sample distant architecture only: no painted platforms or character silhouettes become false terrain.
      ctx.drawImage(backdrop,backdrop.naturalWidth*.47,0,backdrop.naturalWidth*.27,backdrop.naturalHeight*.40,0,0,W,H);
      ctx.fillStyle='#07121aa3';ctx.fillRect(0,0,W,H);
    }
    for(let layer=0;layer<3;layer++) {
      ctx.strokeStyle=['#264350','#213a45','#162c36'][layer];ctx.lineWidth=20+layer*4;ctx.globalAlpha=.25;
      for(let i=0;i<4;i++){let x=i*300-40+layer*25,y=280+layer*60;ctx.beginPath();ctx.moveTo(x,520);ctx.lineTo(x,y);ctx.bezierCurveTo(x,y-175,x+210,y-175,x+210,y);ctx.lineTo(x+210,520);ctx.stroke();}
    }
    ctx.globalAlpha=1;gradient(620,100,280,'#284a5236');
    for(let i=0;i<30;i++){const x=(i*137+Math.sin(visualTime*.2+i)*25)%W,y=(i*71-visualTime*(4+i%3)+5400)%H;ctx.fillStyle=i%4===0?'#e8bc6355':'#8aaaa726';ctx.fillRect(x,y,i%3===0?2:1,2);}
    const fog=ctx.createLinearGradient(0,430,0,H);fog.addColorStop(0,'transparent');fog.addColorStop(1,'#1d394c77');ctx.fillStyle=fog;ctx.fillRect(0,420,W,120);
  }
  function platform(r) {
    ctx.fillStyle='#182b34';ctx.fillRect(r.x,r.y,r.w,r.h);ctx.fillStyle='#35484a';ctx.fillRect(r.x,r.y,r.w,6);
    ctx.fillStyle='#74978c';ctx.fillRect(r.x,r.y,r.w,2);ctx.fillStyle='#477062';ctx.fillRect(r.x,r.y+2,r.w,3);
    ctx.strokeStyle='#0b1c2499';ctx.lineWidth=1;
    for(let y=r.y+19;y<Math.min(r.y+r.h,H);y+=22){ctx.beginPath();ctx.moveTo(r.x,y);ctx.lineTo(r.x+r.w,y);ctx.stroke();}
    for(let x=r.x+30;x<r.x+r.w;x+=42){ctx.beginPath();ctx.moveTo(x,r.y+7);ctx.lineTo(x,r.y+Math.min(r.h,20));ctx.stroke();}
    ctx.fillStyle='#70967b';
    for(let x=r.x+8;x<r.x+r.w-4;x+=19){ctx.fillRect(x,r.y-2,2,3);ctx.fillRect(x+2,r.y-4,1,4);}
    ctx.fillStyle='#06101844';ctx.fillRect(r.x,r.y+8,r.w,6);
  }
  function sprite(frame,x,foot,width=60,flip=false,alpha=1) {
    if(!art.complete||!art.naturalWidth)return false;
    const sw=art.naturalWidth/4,sh=art.naturalHeight/4;
    ctx.save();ctx.globalAlpha=alpha;ctx.translate(x,foot);if(flip)ctx.scale(-1,1);
    ctx.drawImage(art,(frame%4)*sw,Math.floor(frame/4)*sh,sw,sh,-width/2,-width*atlasFoot[frame],width,width);ctx.restore();return true;
  }
  function torch(t,i) {
    if(t.lit){ctx.save();ctx.globalCompositeOperation='screen';gradient(t.x,t.y,110,'#e69a2336');gradient(t.x,t.y,36,'#ffc7634d');ctx.restore();}
    const frame=t.lit?13+Math.floor(visualTime*8+i)%3:12;
    if(!sprite(frame,t.x,t.y+38,68)){
      ctx.fillStyle='#51666a';ctx.fillRect(t.x-6,t.y+5,12,20);ctx.fillRect(t.x-12,t.y+3,24,6);
      if(t.lit){ctx.fillStyle='#ffc766';ctx.beginPath();ctx.ellipse(t.x,t.y,7,13,0,0,Math.PI*2);ctx.fill();}
    }
    ctx.font='10px sans-serif';ctx.textAlign='center';ctx.fillStyle=t.lit?'#fbd898':'#64818a';ctx.fillText(String(i+1).padStart(2,'0'),t.x,t.y+37);
    if(enemy?.phase==='warn'&&enemy.anchorTarget===t.id){ctx.strokeStyle='#d4a1ff';ctx.lineWidth=2;ctx.beginPath();ctx.arc(t.x,t.y,29+Math.sin(visualTime*15)*4,0,Math.PI*2);ctx.stroke();}
  }
  function arrow(x,y,dx,dy,length,color) {
    ctx.save();ctx.strokeStyle=color;ctx.fillStyle=color;ctx.lineWidth=2;
    const ex=x+dx*length,ey=y+dy*length;ctx.beginPath();ctx.moveTo(x,y);ctx.lineTo(ex,ey);ctx.stroke();
    const a=Math.atan2(dy,dx);ctx.beginPath();ctx.moveTo(ex,ey);ctx.lineTo(ex-Math.cos(a-.5)*9,ey-Math.sin(a-.5)*9);ctx.lineTo(ex-Math.cos(a+.5)*9,ey-Math.sin(a+.5)*9);ctx.closePath();ctx.fill();ctx.restore();
  }
  function render() {
    ctx.save();if(shake>0)ctx.translate((Math.random()-.5)*shake,(Math.random()-.5)*shake);
    background();
    if(level.door) {
      ctx.save();ctx.setLineDash([3,7]);ctx.lineWidth=1;
      for(const id of level.door.requires){const t=level.torches.find(t=>t.id===id);ctx.strokeStyle=t.lit?'#e6b86866':'#41667166';ctx.beginPath();ctx.moveTo(t.x,t.y);ctx.lineTo(level.door.x+level.door.w/2,level.door.y+level.door.h/2);ctx.stroke();}
      ctx.restore();
    }
    level.platforms.forEach(platform);
    for(const h of level.hazards){
      ctx.fillStyle='#111a24';ctx.fillRect(h.x,h.y,h.w,h.h);ctx.fillStyle='#806578';
      for(let x=h.x;x<h.x+h.w;x+=15){ctx.beginPath();ctx.moveTo(x,h.y+h.h);ctx.lineTo(x+7,h.y);ctx.lineTo(Math.min(x+14,h.x+h.w),h.y+h.h);ctx.fill();}
      ctx.fillStyle='#dc947355';ctx.fillRect(h.x,h.y+h.h-2,h.w,2);
    }
    if(level.door&&!doorOpen()){
      const d=level.door;ctx.fillStyle='#23343d';ctx.fillRect(d.x,d.y,d.w,d.h);ctx.strokeStyle='#7f7960';ctx.lineWidth=2;ctx.strokeRect(d.x+3,d.y+3,d.w-6,d.h-6);
      level.door.requires.forEach((id,i)=>{ctx.fillStyle=level.torches.find(t=>t.id===id).lit?'#ffca76':'#4c5355';ctx.beginPath();ctx.arc(d.x+d.w/2,d.y+22+i*22,4,0,Math.PI*2);ctx.fill();});
    }
    const g=level.goal;
    gradient(g.x+g.w/2,g.y+g.h/2,90,doorOpen()?'#83d6b323':'#42687920');
    ctx.strokeStyle=doorOpen()?'#9cc9ac':'#547f85';ctx.lineWidth=2;ctx.beginPath();ctx.roundRect(g.x,g.y,g.w,g.h,[g.w/2,g.w/2,0,0]);ctx.stroke();
    ctx.fillStyle=doorOpen()?'#82afaa33':'#1c323c';ctx.fill();ctx.font='10px sans-serif';ctx.textAlign='center';ctx.fillStyle='#acc9bc';ctx.fillText(levelIndex===7?'정원':'다음 길',g.x+g.w/2,g.y-10);
    level.torches.forEach(torch);
    const selected=candidate();
    if(selected&&['free','ring','anchor'].includes(p.state)&&mode==='playing') {
      ctx.save();ctx.strokeStyle='#f4c57488';ctx.setLineDash([3,6]);ctx.lineWidth=1;ctx.beginPath();ctx.moveTo(p.x,p.y);ctx.lineTo(selected.x,selected.y);ctx.stroke();ctx.setLineDash([]);
      ctx.strokeStyle='#ffdb96';ctx.beginPath();ctx.arc(selected.x,selected.y,25,0,Math.PI*2);ctx.stroke();ctx.restore();
    }
    trail.forEach(t=>{ctx.fillStyle=`rgba(255,192,94,${t.life*1.8})`;ctx.beginPath();ctx.arc(t.x,t.y,8*t.life/.2,0,Math.PI*2);ctx.fill();});
    shots.forEach(s=>{gradient(s.x,s.y,20,'#ffa63865');arrow(s.x-s.vx*.012,s.y-s.vy*.012,s.vx/Math.hypot(s.vx,s.vy),s.vy/Math.hypot(s.vx,s.vy),15,'#ffd68b');});
    if(mode!=='dead') {
      gradient(p.x,p.y,55,'#ffab3324');
      let frame=0;
      if(p.state==='anchor')frame=11;else if(p.state==='dash'||p.state==='travel')frame=10;
      else if(!p.grounded&&p.state!=='ring')frame=p.vy<0?8:9;
      else frame=Math.abs(p.vx)>25?4+Math.floor(visualTime*12)%4:Math.floor(visualTime*7)%4;
      const anchored=p.state==='anchor';
      if(!sprite(frame,p.x,p.y+(anchored?16:p.h/2),anchored?45:60,p.facing<0)){
        ctx.fillStyle='#ffc979';ctx.beginPath();ctx.ellipse(p.x,p.y,9,15,0,0,Math.PI*2);ctx.fill();ctx.fillStyle='#4a2b25';ctx.fillRect(p.x-4,p.y-4,2,4);ctx.fillRect(p.x+3,p.y-4,2,4);
      }
      if(p.state==='anchor') {const d=direction(true);arrow(p.x,p.y,d.x,d.y,52,'#ffe2a4');}
      if(p.state==='ring'||ringFlash>0){ctx.save();ctx.strokeStyle='#ffc669';ctx.lineWidth=1.5;ctx.globalAlpha=p.state==='ring'?.6:ringFlash*2;ctx.beginPath();ctx.arc(p.x,p.y,C.ignite*(p.state==='ring'?1:1+(1-ringFlash/.3)*.16),0,Math.PI*2);ctx.stroke();ctx.restore();}
      if(p.state==='ring'&&level.abilities.dash){const d=direction();arrow(p.x,p.y,d.x,d.y,40,'#ffd694');}
      if(charging){const dx=mouse.x-p.x,dy=mouse.y-p.y,n=Math.hypot(dx,dy)||1;arrow(p.x,p.y,dx/n,dy/n,42+chargeTime*30,'#ffdca5');}
    }
    if(enemy&&['warn','attack','recover'].includes(enemy.phase)) {
      ctx.save();
      if(enemy.phase==='warn') {
        ctx.strokeStyle=enemy.locked?'#c2a2ffcc':'#a9a1d669';ctx.setLineDash([8,9]);ctx.lineWidth=enemy.locked?2:1;ctx.beginPath();ctx.moveTo(enemy.x,enemy.y);ctx.lineTo(enemy.aimX,enemy.aimY);ctx.stroke();ctx.setLineDash([]);
        ctx.strokeStyle='#c0a5ff';ctx.lineWidth=2;ctx.beginPath();ctx.arc(enemy.x,enemy.y,35,-Math.PI/2,-Math.PI/2+(1-enemy.timer/1.35)*Math.PI*2);ctx.stroke();
      }
      ctx.globalAlpha=enemy.phase==='recover'?.35:1;gradient(enemy.x,enemy.y,60,'#9277cf35');
      ctx.fillStyle='#070c17';ctx.beginPath();ctx.ellipse(enemy.x,enemy.y,25,30,Math.sin(visualTime)*.1,0,Math.PI*2);ctx.fill();
      ctx.fillStyle='#d9ceff';ctx.beginPath();ctx.ellipse(enemy.x-8,enemy.y-4,3,5,0,0,Math.PI*2);ctx.ellipse(enemy.x+8,enemy.y-4,3,5,0,0,Math.PI*2);ctx.fill();ctx.restore();
    }
    particles.forEach(a=>{ctx.globalAlpha=Math.min(1,a.life*3);ctx.fillStyle=a.color;ctx.fillRect(a.x,a.y,a.size,a.size);});ctx.globalAlpha=1;
    ctx.restore();
    if(mode==='playing'&&level.abilities.shot){ctx.strokeStyle='#ffe0a47a';ctx.lineWidth=1;ctx.beginPath();ctx.arc(mouse.x,mouse.y,6,0,Math.PI*2);ctx.moveTo(mouse.x-10,mouse.y);ctx.lineTo(mouse.x-3,mouse.y);ctx.moveTo(mouse.x+3,mouse.y);ctx.lineTo(mouse.x+10,mouse.y);ctx.stroke();}
  }
  function keyPress(code){if(!keys.has(code))pressed.add(code);keys.add(code);}
  function keyRelease(code){keys.delete(code);released.add(code);}
  function pointer(x,y,button,isDown=true) {
    mouse.x=clamp(x,0,W);mouse.y=clamp(y,0,H);
    if(button==='right'){if(isDown&&!mouse.right)mouse.rightPressed=true;mouse.right=isDown;}
    else if(button==='left'){if(isDown&&!mouse.left)mouse.leftPressed=true;if(!isDown&&mouse.left)mouse.leftReleased=true;mouse.left=isDown;}
  }
  document.addEventListener('keydown',e=>{
    if($('helpDialog').open||$('artDialog').open)return;
    if(e.code==='Escape'){e.preventDefault();pause();return;}
    if(e.code==='KeyR'&&['playing','paused','dead','won'].includes(mode)){e.preventDefault();restart();return;}
    if(['Space','KeyW','KeyA','KeyS','KeyD','ArrowUp','ArrowLeft','ArrowDown','ArrowRight','ShiftLeft','ShiftRight'].includes(e.code)){
      if(e.target.tagName==='BUTTON'&&e.code==='Space')return;
      e.preventDefault();if(mode==='ready'&&e.code==='Space')primary();else keyPress(e.code);
    }
  });
  document.addEventListener('keyup',e=>keyRelease(e.code));
  function mousePosition(e){const r=canvas.getBoundingClientRect();return {x:(e.clientX-r.left)*W/r.width,y:(e.clientY-r.top)*H/r.height};}
  canvas.addEventListener('mousemove',e=>Object.assign(mouse,mousePosition(e)));
  canvas.addEventListener('mousedown',e=>{e.preventDefault();canvas.focus();const q=mousePosition(e);pointer(q.x,q.y,e.button===2?'right':'left',true);});
  document.addEventListener('mouseup',e=>{if(e.button===2)mouse.right=false;else if(e.button===0&&mouse.left){mouse.leftReleased=true;mouse.left=false;}});
  canvas.addEventListener('contextmenu',e=>e.preventDefault());
  window.addEventListener('blur',()=>{keys.clear();clearEdges();mouse.left=mouse.right=false;if(mode==='playing')pause();});
  document.addEventListener('visibilitychange',()=>{if(document.hidden&&mode==='playing')pause();});
  $('startButton').addEventListener('click',primary);$('overlayButton').addEventListener('click',primary);
  $('restartButton').addEventListener('click',restart);$('pauseButton').addEventListener('click',pause);
  $('soundButton').addEventListener('click',()=>{muted=!muted;if(!muted)sound('ignite');updateUI();});
  function openDialog(id){if(mode==='playing')pause();$(id).showModal();}
  $('helpButton').addEventListener('click',()=>openDialog('helpDialog'));
  $('artButton').addEventListener('click',()=>openDialog('artDialog'));
  for(const [close,id] of [['closeHelpButton','helpDialog'],['closeArtButton','artDialog']])$(close).addEventListener('click',()=>$(id).close());
  for(const id of ['helpDialog','artDialog'])$(id).addEventListener('click',e=>{if(e.target===$(id))$(id).close();});
  window.EmberDemo={
    getState:()=>({levelIndex,levelId:level.id,mode,deaths,time:runTime,player:{...p},torches:level.torches.map(t=>({...t})),doorOpen:doorOpen(),shots:shots.map(s=>({...s})),enemy:enemy?{...enemy}:null,assetsReady:art.complete&&art.naturalWidth>0,physics:C}),
    loadLevel:index=>loadLevel(index,true),restart,step:frames=>{for(let i=0;i<frames;i++)update(DT);updateUI();render();},
    input:{press:keyPress,release:keyRelease,mouse:pointer},
    debugTeleport:(x,y)=>{p.x=x;p.y=y;p.vx=p.vy=0;p.state='free';p.anchorId=null;p.targetId=null;p.grounded=false;launchProtect=0;}
  };
  loadLevel(0);
  let previous=performance.now(),accumulator=0;
  function frame(now){const elapsed=Math.min((now-previous)/1000,.08);previous=now;
    if(!QA){accumulator+=elapsed;while(accumulator>=DT){update(DT);accumulator-=DT;}}
    else visualTime+=elapsed;
    uiClock+=elapsed;if(uiClock>.1){updateUI();uiClock=0;}render();requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);
})();
