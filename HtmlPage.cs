namespace MRYAN;

public static class HtmlPage
{
    public const string Content = """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Mr. YAN</title>
<style>
*{box-sizing:border-box;margin:0;padding:0}
:root{
  --bg0:#0e1117;--bg1:#161b24;--bg2:#1d2433;--bg3:#252d3d;
  --border:rgba(255,255,255,.08);--border2:rgba(255,255,255,.14);
  --text:#e2e8f4;--muted:#7a8599;
  --accent:#4a9eff;--green:#34d399;--amber:#fbbf24;--red:#f87171;--purple:#a78bfa;
  --mono:'Consolas','Courier New',monospace;
  --sans:'Segoe UI',system-ui,-apple-system,sans-serif;
}
html,body{height:100%;background:var(--bg0)}
body{color:var(--text);font-family:var(--sans);font-size:13px;display:flex;flex-direction:column;overflow:hidden}
.titlebar{background:var(--bg1);border-bottom:1px solid var(--border);padding:0 16px;height:48px;display:flex;align-items:center;justify-content:space-between;flex-shrink:0}
.title-left{display:flex;align-items:center;gap:10px}
.title-icon{width:28px;height:28px;background:var(--accent);border-radius:6px;display:flex;align-items:center;justify-content:center;font-size:15px}
.title-name{font-weight:600;font-size:14px}
.title-sub{font-size:11px;color:var(--muted);letter-spacing:.03em;margin-top:1px}
.pill{display:flex;align-items:center;gap:6px;padding:4px 10px;border-radius:20px;font-size:11px;font-weight:500;letter-spacing:.04em;border:1px solid}
.pill-running{background:rgba(52,211,153,.12);border-color:rgba(52,211,153,.3);color:var(--green)}
.pill-stopped{background:rgba(248,113,113,.12);border-color:rgba(248,113,113,.3);color:var(--red)}
.dot{width:6px;height:6px;border-radius:50%}
.dot-green{background:var(--green);animation:blink 1.4s ease-in-out infinite}
.dot-red{background:var(--red)}
@keyframes blink{0%,100%{opacity:1}50%{opacity:.3}}
.toolbar{background:var(--bg1);border-bottom:1px solid var(--border);padding:8px 16px;display:flex;align-items:center;gap:8px;flex-shrink:0}
.btn{padding:5px 14px;border-radius:5px;font-family:var(--sans);font-size:12px;font-weight:500;cursor:pointer;border:1px solid;transition:opacity .15s;letter-spacing:.02em;background:transparent}
.btn:hover{opacity:.8}.btn:active{opacity:.6}.btn:disabled{opacity:.35;cursor:default}
.btn-start{background:rgba(52,211,153,.15);border-color:rgba(52,211,153,.35);color:var(--green)}
.btn-stop{background:rgba(248,113,113,.15);border-color:rgba(248,113,113,.35);color:var(--red)}
.btn-poll{background:rgba(74,158,255,.12);border-color:rgba(74,158,255,.3);color:var(--accent)}
.btn-muted{background:var(--bg2);border-color:var(--border2);color:var(--muted)}
.sep{width:1px;height:20px;background:var(--border2);margin:0 2px}
.toolbar-info{margin-left:auto;font-size:11px;color:var(--muted);font-family:var(--mono)}
.main{flex:1;display:grid;grid-template-columns:1fr 340px;grid-template-rows:1fr 230px;gap:1px;background:var(--border);overflow:hidden;min-height:0}
.panel{background:var(--bg0);display:flex;flex-direction:column;overflow:hidden;min-height:0}
.panel-header{background:var(--bg1);border-bottom:1px solid var(--border);padding:7px 14px;display:flex;align-items:center;gap:8px;flex-shrink:0}
.panel-title{font-size:11px;font-weight:500;letter-spacing:.07em;color:var(--muted);text-transform:uppercase}
.badge{padding:1px 7px;border-radius:20px;font-size:10px;font-weight:500;font-family:var(--mono)}
.badge-alert{background:rgba(248,113,113,.18);color:var(--red)}
.badge-ok{background:rgba(52,211,153,.15);color:var(--green)}
.badge-log{background:rgba(74,158,255,.12);color:var(--accent)}
.panel-body{flex:1;overflow-y:auto;padding:10px;min-height:0}
.panel-body::-webkit-scrollbar{width:5px}
.panel-body::-webkit-scrollbar-thumb{background:var(--bg3);border-radius:3px}
.alert-card{background:var(--bg1);border:1px solid var(--border);border-radius:8px;padding:12px 14px;margin-bottom:8px}
.alert-hdr{display:flex;align-items:flex-start;gap:10px;margin-bottom:8px}
.alert-icon{font-size:20px;line-height:1;flex-shrink:0}
.alert-event{font-size:13px;font-weight:600;line-height:1.3}
.alert-sender{font-size:11px;color:var(--muted);margin-top:2px}
.chips{display:flex;flex-wrap:wrap;gap:5px;margin-bottom:8px}
.chip{padding:2px 8px;border-radius:4px;font-size:10px;font-weight:500;letter-spacing:.03em;border:1px solid}
.chip-ext{background:rgba(248,113,113,.15);border-color:rgba(248,113,113,.3);color:var(--red)}
.chip-sev{background:rgba(251,191,36,.12);border-color:rgba(251,191,36,.3);color:var(--amber)}
.chip-mod{background:rgba(74,158,255,.12);border-color:rgba(74,158,255,.3);color:var(--accent)}
.chip-dim{background:var(--bg3);border-color:var(--border2);color:var(--muted)}
.alert-times{font-size:11px;color:var(--muted);font-family:var(--mono);display:flex;flex-direction:column;gap:2px}
.alert-times span{color:var(--text)}
.alert-desc{font-size:11px;color:var(--muted);line-height:1.5;border-top:1px solid var(--border);margin-top:8px;padding-top:8px;display:-webkit-box;-webkit-line-clamp:3;-webkit-box-orient:vertical;overflow:hidden}
.no-alerts{height:100%;display:flex;flex-direction:column;align-items:center;justify-content:center;color:var(--muted);gap:8px}
.stats-grid{display:grid;grid-template-columns:1fr 1fr;gap:8px;margin-bottom:14px}
.stat-card{background:var(--bg1);border:1px solid var(--border);border-radius:7px;padding:10px 12px}
.stat-lbl{font-size:10px;color:var(--muted);letter-spacing:.05em;text-transform:uppercase;margin-bottom:4px}
.stat-val{font-size:18px;font-weight:600;font-family:var(--mono)}
.stat-sub{font-size:10px;color:var(--muted);margin-top:2px}
.cg{color:var(--green)}.cr{color:var(--red)}.cb{color:var(--accent)}.cp{color:var(--purple)}
.sec-lbl{font-size:10px;letter-spacing:.07em;text-transform:uppercase;color:var(--muted);margin:12px 0 8px;font-weight:500}
.interval-row{display:flex;align-items:center;gap:8px;margin-bottom:8px}
.interval-lbl{font-size:11px;color:var(--muted);flex:1}
.spin-wrap{display:flex;align-items:center;gap:4px}
.spin-btn{width:24px;height:24px;border-radius:4px;border:1px solid var(--border2);background:var(--bg2);color:var(--text);font-size:15px;cursor:pointer;display:flex;align-items:center;justify-content:center}
.spin-btn:hover{background:var(--bg3)}
.spin-val{font-family:var(--mono);font-size:12px;font-weight:500;min-width:54px;text-align:center;color:var(--text);background:var(--bg2);border:1px solid var(--border2);border-radius:4px;padding:3px 0}
.info-row{display:flex;justify-content:space-between;align-items:center;padding:5px 0;border-bottom:1px solid var(--border);font-size:11px}
.info-row:last-child{border-bottom:none}
.ik{color:var(--muted)}.iv{font-family:var(--mono);color:var(--text)}
.log-entry{font-family:var(--mono);font-size:11px;line-height:1.7;padding:1px 0;display:flex;gap:12px;border-bottom:1px solid rgba(255,255,255,.03)}
.lt{color:var(--muted);flex-shrink:0;width:56px}
.ll{flex-shrink:0;width:52px;font-weight:500}
.lm{color:var(--text);word-break:break-word}
.li{color:var(--accent)}.ls{color:var(--green)}.lw{color:var(--amber)}.le{color:var(--red)}.lc{color:var(--purple)}
.composer{border-top:1px solid var(--border);padding:10px;flex-shrink:0;background:var(--bg0)}
.composer-lbl{font-size:10px;letter-spacing:.07em;text-transform:uppercase;color:var(--muted);margin-bottom:6px}
.composer-row{display:flex;gap:6px;align-items:flex-end}
textarea.msg-input{flex:1;background:var(--bg1);border:1px solid var(--border2);border-radius:6px;color:var(--text);font-family:var(--sans);font-size:12px;padding:7px 10px;resize:none;outline:none;min-height:54px;line-height:1.5;transition:border-color .15s}
textarea.msg-input:focus{border-color:var(--accent)}
textarea.msg-input::placeholder{color:var(--muted)}
.btn-send{padding:0 16px;border-radius:5px;font-family:var(--sans);font-size:12px;font-weight:500;cursor:pointer;border:1px solid rgba(167,139,250,.35);background:rgba(167,139,250,.15);color:var(--purple);height:54px;white-space:nowrap}
.btn-send:hover{opacity:.8}.btn-send:disabled{opacity:.35;cursor:default}
.toast{position:fixed;bottom:32px;left:50%;transform:translateX(-50%) translateY(20px);background:var(--bg2);border:1px solid var(--border2);border-radius:8px;padding:8px 18px;font-size:12px;color:var(--text);opacity:0;transition:opacity .2s,transform .2s;pointer-events:none;z-index:99;white-space:nowrap}
.toast.show{opacity:1;transform:translateX(-50%) translateY(0)}
.statusbar{background:var(--bg1);border-top:1px solid var(--border);padding:4px 16px;display:flex;align-items:center;gap:16px;flex-shrink:0;font-size:10px;font-family:var(--mono);color:var(--muted)}
.sb-dot{width:5px;height:5px;border-radius:50%;display:inline-block;margin-right:5px}
</style>
</head>
<body>
<div class="titlebar">
  <div class="title-left">
    <div class="title-icon">&#x1F324;&#xFE0F;</div>
    <div>
      <div class="title-name">Mr. YAN</div>
      <div class="title-sub">METEOROLOGICAL RESPONSES YOUR ACCURATE NOTIFICATION</div>
    </div>
  </div>
  <div id="pill" class="pill pill-running">
    <div id="dot" class="dot dot-green"></div>
    <span id="pill-txt">RUNNING</span>
  </div>
</div>
<div class="toolbar">
  <button id="btn-toggle" class="btn btn-stop" onclick="toggleMonitor()">&#9209; Stop Monitor</button>
  <div class="sep"></div>
  <button class="btn btn-poll" onclick="pollNow()">&#8635; Poll Now</button>
  <button class="btn btn-muted" onclick="clearLog()">&#10005; Clear Log</button>
  <div class="toolbar-info" id="poll-info">Connecting&#8230;</div>
</div>
<div class="main">
  <div class="panel">
    <div class="panel-header">
      <span class="panel-title">Active Alerts</span>
      <span id="alert-badge" class="badge badge-ok">NONE</span>
    </div>
    <div class="panel-body" id="alerts-body">
      <div class="no-alerts"><div style="font-size:28px">&#8987;</div><div style="font-size:12px">Loading&#8230;</div></div>
    </div>
  </div>
  <div class="panel">
    <div class="panel-header"><span class="panel-title">Status &amp; Config</span></div>
    <div class="panel-body">
      <div class="stats-grid">
        <div class="stat-card"><div class="stat-lbl">Active Alerts</div><div id="s-active" class="stat-val cg">&#8212;</div><div class="stat-sub">Monroe Co. IN</div></div>
        <div class="stat-card"><div class="stat-lbl">Posted Today</div><div id="s-posted" class="stat-val cb">&#8212;</div><div class="stat-sub">to Google Chat</div></div>
      </div>
      <div class="sec-lbl">Intervals</div>
      <div class="interval-row">
        <span class="interval-lbl">Poll interval</span>
        <div class="spin-wrap">
          <button class="spin-btn" onclick="adj('poll',-1)">&#8722;</button>
          <div class="spin-val" id="poll-disp">5 min</div>
          <button class="spin-btn" onclick="adj('poll',1)">+</button>
        </div>
      </div>
      <div class="interval-row">
        <span class="interval-lbl">Repost interval</span>
        <div class="spin-wrap">
          <button class="spin-btn" onclick="adj('repost',-5)">&#8722;</button>
          <div class="spin-val" id="repost-disp">60 min</div>
          <button class="spin-btn" onclick="adj('repost',5)">+</button>
        </div>
      </div>
      <div style="margin-top:6px;margin-bottom:4px">
        <button class="btn btn-poll" style="font-size:11px;padding:4px 12px;width:100%" onclick="applyIntervals()">Apply &amp; Restart Monitor</button>
      </div>
      <div class="sec-lbl">Runtime</div>
      <div class="info-row"><span class="ik">Uptime</span><span id="r-uptime" class="iv">&#8212;</span></div>
      <div class="info-row"><span class="ik">Polls run</span><span id="r-polls" class="iv">&#8212;</span></div>
      <div class="info-row"><span class="ik">Poll interval</span><span id="r-poll" class="iv">&#8212;</span></div>
      <div class="info-row"><span class="ik">Repost interval</span><span id="r-repost" class="iv">&#8212;</span></div>
      <div class="info-row"><span class="ik">Webhook</span><span id="r-webhook" class="iv">&#8212;</span></div>
    </div>
  </div>
  <div class="panel" style="grid-column:1/-1">
    <div class="panel-header">
      <span class="panel-title">Activity Log</span>
      <span id="log-badge" class="badge badge-log">0 ENTRIES</span>
    </div>
    <div class="panel-body" id="log-body"></div>
    <div class="composer">
      <div class="composer-lbl">Send custom message to Google Chat</div>
      <div class="composer-row">
        <textarea class="msg-input" id="msg-input" placeholder="Type a one-off message&#8230; (Ctrl+Enter to send)" onkeydown="msgKey(event)"></textarea>
        <button class="btn-send" id="send-btn" onclick="sendMsg()">&#128232; Send</button>
      </div>
    </div>
  </div>
</div>
<div class="statusbar">
  <div><span class="sb-dot" id="conn-dot" style="background:var(--amber)"></span><span id="conn-txt">Connecting&#8230;</span></div>
  <div><span class="sb-dot" style="background:var(--purple)"></span>Monroe County, IN &#183; INZ027</div>
  <div style="margin-left:auto"><span id="sb-time"></span></div>
</div>
<div class="toast" id="toast"></div>
<script>
var logEntries=[], pollMin=5, repostMin=60;

// ── Mode: WebView2 IPC vs browser HTTP ────────────────────────────────────────
var ipc = (function(){
  try{ return !!(window.chrome && window.chrome.webview); }catch(e){ return false; }
})();

function ipcSend(obj){ if(ipc) window.chrome.webview.postMessage(JSON.stringify(obj)); }

// ── Toast ─────────────────────────────────────────────────────────────────────
function toast(msg){
  var el=document.getElementById('toast');
  el.textContent=msg; el.classList.add('show');
  setTimeout(function(){el.classList.remove('show');},2400);
}

// ── Log ───────────────────────────────────────────────────────────────────────
function lvCls(l){ return l==='Success'?'ls':l==='Warning'?'lw':l==='Error'?'le':l==='Custom'?'lc':'li'; }
function lvLbl(l){ return l==='Success'?'SUCCESS':l==='Warning'?'WARN':l==='Error'?'ERROR':l==='Custom'?'CUSTOM':'INFO'; }

function addLog(e){
  logEntries.unshift(e);
  if(logEntries.length>1000) logEntries.pop();
  renderLog();
}
function renderLog(){
  var body=document.getElementById('log-body');
  document.getElementById('log-badge').textContent=logEntries.length+' ENTRIES';
  body.innerHTML=logEntries.map(function(e){
    return '<div class="log-entry"><span class="lt">'+e.time+'</span><span class="ll '+lvCls(e.level)+'">'+lvLbl(e.level)+'</span><span class="lm">'+e.message+'</span></div>';
  }).join('');
}

// ── State ─────────────────────────────────────────────────────────────────────
function applyState(s){
  if(s.pollMinutes)  pollMin   = s.pollMinutes;
  if(s.repostMinutes) repostMin = s.repostMinutes;

  var pill=document.getElementById('pill'), dot=document.getElementById('dot'),
      ptxt=document.getElementById('pill-txt'), btn=document.getElementById('btn-toggle');
  if(s.running){
    pill.className='pill pill-running'; dot.className='dot dot-green';
    ptxt.textContent='RUNNING'; btn.className='btn btn-stop'; btn.textContent='\u23F9 Stop Monitor';
  } else {
    pill.className='pill pill-stopped'; dot.className='dot dot-red';
    ptxt.textContent='STOPPED'; btn.className='btn btn-start'; btn.textContent='\u25B6 Start Monitor';
  }

  var info=document.getElementById('poll-info');
  if(s.running && s.lastPollAt) info.textContent='Last poll: '+s.lastPollAt+' \u00B7 Next: '+(s.nextPollAt||'\u2026');
  else if(!s.running) info.textContent='Monitor stopped';

  document.getElementById('poll-disp').textContent  = (s.pollMinutes||pollMin)+' min';
  document.getElementById('repost-disp').textContent= (s.repostMinutes||repostMin)+' min';

  var ac=s.alerts?s.alerts.length:0;
  document.getElementById('s-active').textContent=ac;
  document.getElementById('s-active').className='stat-val '+(ac>0?'cr':'cg');
  document.getElementById('s-posted').textContent=s.postedToday!=null?s.postedToday:'\u2014';
  document.getElementById('r-uptime').textContent =s.uptime||'\u2014';
  document.getElementById('r-polls').textContent  =s.pollsRun!=null?s.pollsRun:'\u2014';
  document.getElementById('r-poll').textContent   =(s.pollMinutes||pollMin)+' min';
  document.getElementById('r-repost').textContent =(s.repostMinutes||repostMin)+' min';
  document.getElementById('r-webhook').textContent=s.webhookOk?'Configured \u2713':'NOT SET \u2717';
  document.getElementById('r-webhook').className  ='iv '+(s.webhookOk?'cg':'cr');

  renderAlerts(s.alerts, s.running);
  document.getElementById('conn-dot').style.background='var(--green)';
  document.getElementById('conn-txt').textContent=ipc?'IPC connected':'Live log connected';
}

// ── Alerts ────────────────────────────────────────────────────────────────────
function chipCls(sv){ return sv==='Extreme'?'chip chip-ext':sv==='Severe'?'chip chip-sev':sv==='Moderate'?'chip chip-mod':'chip chip-dim'; }

function renderAlerts(alerts, isRunning){
  var body=document.getElementById('alerts-body'), badge=document.getElementById('alert-badge');
  if(!isRunning||!alerts||!alerts.length){
    badge.textContent='NONE'; badge.className='badge badge-ok';
    body.innerHTML='<div class="no-alerts"><div style="font-size:28px">\u2705</div><div style="font-size:12px">No active alerts for Monroe County, IN</div></div>';
    return;
  }
  badge.textContent=alerts.length+' ACTIVE'; badge.className='badge badge-alert';
  body.innerHTML=alerts.map(function(a){
    var p=a.properties||a;
    var icon=p.severityIcon||p.SeverityIcon||'\u26A0\uFE0F';
    var ev=p.event||p.Event||'', sn=p.senderName||p.SenderName||'';
    var sv=p.severity||p.Severity||'', ce=p.certainty||p.Certainty||'', ur=p.urgency||p.Urgency||'';
    var on=p.onsetLocal||p.OnsetLocal||'', ex=p.expiresLocal||p.ExpiresLocal||'';
    var desc=p.description||p.Description||'';
    return '<div class="alert-card">'+
      '<div class="alert-hdr"><div class="alert-icon">'+icon+'</div>'+
      '<div><div class="alert-event">'+ev+'</div><div class="alert-sender">'+sn+'</div></div></div>'+
      '<div class="chips"><span class="'+chipCls(sv)+'">'+sv+'</span>'+
      '<span class="chip chip-dim">'+ce+'</span><span class="chip chip-dim">'+ur+'</span></div>'+
      '<div class="alert-times"><div>Onset: <span>'+on+'</span></div><div>Expires: <span>'+ex+'</span></div></div>'+
      (desc?'<div class="alert-desc">'+desc.slice(0,300)+'</div>':'')+
      '</div>';
  }).join('');
}

// ── Actions ───────────────────────────────────────────────────────────────────
function toggleMonitor(){
  if(ipc){ ipcSend({action:'toggle'}); return; }
  fetch('/api/toggle',{method:'POST'}).then(function(){fetchState();});
}
function pollNow(){
  if(ipc){ ipcSend({action:'poll'}); return; }
  fetch('/api/poll',{method:'POST'}).then(function(){setTimeout(fetchState,1500);});
}
function clearLog(){ logEntries=[]; renderLog(); }

function adj(which,delta){
  if(which==='poll'){ pollMin=Math.max(1,Math.min(60,pollMin+delta)); document.getElementById('poll-disp').textContent=pollMin+' min'; }
  else { repostMin=Math.max(5,Math.min(480,repostMin+delta)); document.getElementById('repost-disp').textContent=repostMin+' min'; }
}

function applyIntervals(){
  if(ipc){ ipcSend({action:'intervals',pollMinutes:pollMin,repostMinutes:repostMin}); return; }
  fetch('/api/intervals',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({pollMinutes:pollMin,repostMinutes:repostMin})})
    .then(function(r){return r.json();}).then(function(s){toast('Applied: poll '+s.pollMinutes+' min \u00B7 repost '+s.repostMinutes+' min'); fetchState();});
}

function sendMsg(){
  var ta=document.getElementById('msg-input'), btn=document.getElementById('send-btn');
  var text=ta.value.trim();
  if(!text){ toast('Message is empty'); return; }
  btn.disabled=true; btn.textContent='Sending\u2026';
  if(ipc){
    ipcSend({action:'message',text:text}); ta.value=''; btn.disabled=false; btn.textContent='\u{1F4E8} Send';
  } else {
    fetch('/api/message',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({text:text})})
      .then(function(r){return r.json();}).then(function(s){
        if(s.success){toast('Posted to Google Chat \u2713'); ta.value='';} else toast('Post failed \u2014 check log');
        fetchState(); btn.disabled=false; btn.textContent='\u{1F4E8} Send';
      });
  }
}
function msgKey(e){ if(e.key==='Enter'&&(e.ctrlKey||e.metaKey)){e.preventDefault();sendMsg();} }

// ── HTTP mode ─────────────────────────────────────────────────────────────────
function fetchState(){
  fetch('/api/state').then(function(r){return r.json();}).then(applyState).catch(function(){});
}
function connectSSE(){
  var es=new EventSource('/api/events');
  es.onopen=function(){document.getElementById('conn-dot').style.background='var(--green)';document.getElementById('conn-txt').textContent='Live log connected';};
  es.onmessage=function(e){addLog(JSON.parse(e.data));};
  es.onerror=function(){document.getElementById('conn-dot').style.background='var(--red)';document.getElementById('conn-txt').textContent='Reconnecting\u2026';};
}

// ── IPC mode (WebView2) ───────────────────────────────────────────────────────
if(ipc){
  window.chrome.webview.addEventListener('message', function(event){
    try{
      var msg=JSON.parse(event.data);
      if(msg.type==='log')        addLog(msg);
      else if(msg.type==='state') applyState(msg);
      else if(msg.type==='toast') toast(msg.message);
    }catch(e){}
  });
}

// ── Clock ─────────────────────────────────────────────────────────────────────
function clock(){
  var n=new Date();
  document.getElementById('sb-time').textContent=
    n.toLocaleDateString('en-US',{weekday:'short',month:'short',day:'numeric'})+
    ' \u00B7 '+n.toTimeString().slice(0,8);
}

// ── Boot ──────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function(){
  if(ipc){
    ipcSend({action:'ready'});
  } else {
    connectSSE();
    fetchState();
    setInterval(fetchState, 3000);
  }
  setInterval(clock, 1000);
  clock();
});
</script>
</body>
</html>
""";
}
