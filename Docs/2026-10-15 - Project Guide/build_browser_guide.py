from pathlib import Path
import sys, base64, re
sys.path.insert(0, r'H:\ChatGPT - IT\DocumentTools\python-libs')
import markdown

folder = Path(__file__).resolve().parent
source = (folder / 'Project_Guide.md').read_text(encoding='utf-8')
source = re.sub(r'## Contents\n.*?(?=## How to read this guide)', '## Contents\n\n[TOC]\n\n', source, flags=re.S)
def embed(match):
    label, target = match.groups()
    path = (folder / target).resolve()
    if target.startswith('diagrams/') and path.exists():
        mime = 'image/png' if path.suffix == '.png' else 'image/svg+xml'
        target = 'data:' + mime + ';base64,' + base64.b64encode(path.read_bytes()).decode('ascii')
    elif target.startswith('../') and path.exists():
        target = path.as_uri()
    return '[' + label + '](' + target + ')'
# SVG download links are redundant in the self-contained browser edition.
source = re.sub(r'\[Open scalable SVG\]\(diagrams/[^)]+\)', '', source)
source = re.sub(r'\[([^\]]*)\]\(([^)]+)\)', embed, source)
body = markdown.markdown(source, extensions=['tables', 'fenced_code', 'toc'], extension_configs={'toc': {'toc_depth': '2'}})
html = '''<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Learning_and_Creating — Project Guide</title>
<style>
:root{color-scheme:light;--ink:#1d2c40;--muted:#52637a;--accent:#215da2}
*{box-sizing:border-box}body{margin:0;background:#edf2f8;color:var(--ink);font:17px/1.7 system-ui,-apple-system,"Segoe UI",sans-serif}
header{background:#152c46;color:#fff;padding:24px max(24px,calc((100vw - 1120px)/2));display:flex;gap:24px;justify-content:space-between;align-items:center}
header strong{font-size:20px}header span{display:block;font-size:14px;color:#c3d7eb}button{font:inherit;border:1px solid #bacde2;border-radius:8px;padding:7px 16px;cursor:pointer;background:white;color:#153956}
main{max-width:1200px;margin:32px auto;padding:42px 48px;background:white;border:1px solid #dce4ee;border-radius:14px;box-shadow:0 10px 35px #132d4610}
h1{font-size:36px;line-height:1.2;letter-spacing:-1px;margin-top:0}h2{font-size:27px;line-height:1.35;margin-top:54px;padding-top:22px;border-top:1px solid #dfe6ee;scroll-margin-top:22px}
a{color:var(--accent);text-underline-offset:3px}p,li{max-width:100%}code{font:0.9em/1.6 Consolas,"Courier New",monospace;background:#edf2f8;padding:2px 5px;border-radius:4px;overflow-wrap:anywhere}
pre{background:#14283e;color:#e8f2ff;padding:22px;border-radius:10px;overflow:auto;font-size:15px;line-height:1.6}pre code{padding:0;background:none;color:inherit;overflow-wrap:normal}
table{width:100%;border-collapse:collapse;margin:24px 0;font-size:15px}th,td{padding:12px 14px;text-align:left;vertical-align:top;border:1px solid #dbe3ee;overflow-wrap:anywhere}th{background:#e8f0fa}tr:nth-child(even){background:#f8fafc}
img{max-width:100%;height:auto;border:1px solid #dbe3ee;border-radius:9px;cursor:zoom-in;display:block;margin:24px auto}
.toc{background:#f1f6fc;border-left:4px solid #4275ab;border-radius:5px;padding:14px 24px}.toc ul{padding-left:22px}.note{padding:14px 20px;background:#fff5da;border-radius:8px;color:#60451d;font-size:15px}
dialog{width:96vw;max-width:none;height:94vh;border:0;border-radius:10px;padding:16px;background:#f7f9fc}dialog::backdrop{background:#081423cc}dialog button{position:sticky;top:0;z-index:2}#zoom-content{overflow:auto;height:calc(100% - 54px)}dialog img{max-width:none;margin:10px auto;border:0;cursor:default}
@media(max-width:700px){main{margin:0;padding:24px 18px;border-radius:0}h1{font-size:28px}h2{font-size:23px}header{padding:16px}table{display:block;overflow:auto}pre{font-size:13px}}
@media print{body{background:white;font-size:11pt}header,.note,dialog{display:none}main{border:0;box-shadow:none;margin:0;padding:0;max-width:none}pre{white-space:pre-wrap;background:#f1f3f5;color:#172332;overflow-wrap:anywhere}h2{break-after:avoid}img,tr{break-inside:avoid}a{color:inherit}table{font-size:9pt}}
</style></head><body>
<header><div><strong>Project documentation</strong><span>Learning_and_Creating · Browser edition · 15 October 2026</span></div><button onclick="window.print()">Print / Save PDF</button></header>
<main><p class="note">All diagrams are embedded: this file works offline. Click a diagram to enlarge it; press Escape to close. Source-code links require the original project files on this PC.</p>
''' + body + '''
</main><dialog id="zoom"><button type="button" id="close-zoom">Close diagram (Esc)</button><div id="zoom-content"></div></dialog>
<script>
const dialog=document.getElementById('zoom');
document.querySelectorAll('main img').forEach(img=>{
img.tabIndex=0;img.setAttribute('role','button');img.setAttribute('aria-label','Enlarge diagram: '+img.alt);
const open=()=>{document.getElementById('zoom-content').replaceChildren(img.cloneNode());dialog.showModal();};
img.addEventListener('click',open);img.addEventListener('keydown',e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();open();}});
});
document.getElementById('close-zoom').addEventListener('click',()=>dialog.close());
dialog.addEventListener('click',e=>{if(e.target===dialog)dialog.close();});
</script></body></html>'''
out=folder/'Project_Guide.html';out.write_text(html,encoding='utf-8')
from html.parser import HTMLParser
class Check(HTMLParser):
    def __init__(self):super().__init__();self.ids=set();self.links=[];self.images=[]
    def handle_starttag(self,tag,attrs):
        a=dict(attrs)
        if 'id' in a:self.ids.add(a['id'])
        if tag=='a':self.links.append(a.get('href',''))
        if tag=='img':self.images.append(a.get('src',''))
check=Check();check.feed(html)
assert len(check.images)==9 and all(x.startswith('data:image/png;base64,') for x in check.images)
assert all(x[1:] in check.ids for x in check.links if x.startswith('#'))
assert not re.search(r'<(?:script|link)[^>]+(?:src|href)="https?://',html)
print(f'Created {out}\nPASS: nine embedded diagrams, working section anchors, no external dependencies. {out.stat().st_size:,} bytes.')
