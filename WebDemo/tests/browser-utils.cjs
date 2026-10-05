const fs = require('node:fs');
const path = require('node:path');

function loadPlaywright() {
  try { return require('playwright'); }
  catch {
    return require('C:/Users/admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
  }
}

async function connectBrowser(cdpUrl) {
  const { chromium } = loadPlaywright();
  if (cdpUrl) return chromium.connectOverCDP(cdpUrl);
  return chromium.launch({
    executablePath: process.env.CHROME_PATH || 'C:/Program Files/Google/Chrome/Application/chrome.exe',
    headless: true,
  });
}

function collectDiagnostics(page) {
  const diagnostics = { pageErrors: [], consoleErrors: [], failedRequests: [], badResponses: [] };
  page.on('pageerror', error => diagnostics.pageErrors.push(error.message));
  page.on('console', message => {
    if (message.type() === 'error') diagnostics.consoleErrors.push(message.text());
  });
  page.on('requestfailed', request => diagnostics.failedRequests.push({
    url: request.url(), error: request.failure()?.errorText,
  }));
  page.on('response', response => {
    if (response.status() >= 400) diagnostics.badResponses.push({ url: response.url(), status: response.status() });
  });
  return diagnostics;
}

function writeReport(name, report) {
  const target = path.join(__dirname, name);
  fs.writeFileSync(target, JSON.stringify(report, null, 2));
  return target;
}

module.exports = { connectBrowser, collectDiagnostics, writeReport };
