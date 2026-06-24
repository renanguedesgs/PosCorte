import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.join(__dirname, '..');
const pub = path.join(root, 'public');
const www = path.join(root, 'src', 'PosCorte.Web', 'wwwroot');

function copyIfExists(src, dest) {
  if (!fs.existsSync(src)) return;
  fs.mkdirSync(path.dirname(dest), { recursive: true });
  fs.copyFileSync(src, dest);
}

function copyDirIfExists(srcDir, destDir) {
  if (!fs.existsSync(srcDir)) return;
  fs.mkdirSync(destDir, { recursive: true });
  for (const name of fs.readdirSync(srcDir)) {
    const s = path.join(srcDir, name);
    const d = path.join(destDir, name);
    if (fs.statSync(s).isDirectory()) copyDirIfExists(s, d);
    else fs.copyFileSync(s, d);
  }
}

fs.mkdirSync(pub, { recursive: true });

copyIfExists(path.join(www, 'css', 'site.css'), path.join(pub, 'css', 'site.css'));
copyIfExists(path.join(www, 'favicon.png'), path.join(pub, 'favicon.png'));
copyDirIfExists(path.join(www, 'images'), path.join(pub, 'images'));

const appUrl = (process.env.APP_WEB_URL || process.env.POSCORTE_APP_URL || '').replace(/\/$/, '');
const whatsapp = process.env.CONTACT_WHATSAPP || '';

fs.writeFileSync(
  path.join(pub, 'config.js'),
  `window.POSCORTE_APP_URL=${JSON.stringify(appUrl)};window.POSCORTE_WHATSAPP=${JSON.stringify(whatsapp)};\n`
);

console.log('Vercel build OK — public/ ready');
console.log('APP_WEB_URL:', appUrl || '(não definido — configure na Vercel)');
