from pathlib import Path
import sys
sys.path.insert(0, r'H:\ChatGPT - IT\DocumentTools\python-libs')
import markdown

root = Path(__file__).parent
body = markdown.markdown((root / 'Character_Menu_Asset_Brief.md').read_text(encoding='utf-8'), extensions=['tables'])
html = '<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Character menu asset brief</title><style>body{max-width:960px;margin:40px auto;padding:0 24px;font:17px/1.6 system-ui;color:#202838}h1,h2{line-height:1.2}h2{margin-top:2em}table{border-collapse:collapse;width:100%;font-size:14px}td,th{padding:9px;border:1px solid #ccc;text-align:left}code{background:#eef1f5;padding:2px 4px} @media print{body{font-size:11pt}}</style><main>' + body + '</main></html>'
(root / 'Character_Menu_Asset_Brief.html').write_text(html, encoding='utf-8')
