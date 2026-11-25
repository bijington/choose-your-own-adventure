// Exercise the actual audience script with a small DOM/SignalR test double.
// This checks UI state logic, not visual layout or real browser rendering.
const { readFileSync } = require('node:fs');
const { join } = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const html = readFileSync(join(__dirname, '../../src/Adventure.Web/wwwroot/index.html'), 'utf8');
const source = html.split('  <script>')[1].split('</script>')[0];
function element(dataset = {}) {
  const listeners = new Map();
  return { dataset, style: {}, disabled: false, textContent: '', value: 0,
    classList: { toggle() {} }, addEventListener: (name, callback) => listeners.set(name, callback),
    click() { listeners.get('click')?.(); } };
}
const elements = new Map([...html.matchAll(/id="([^"]+)"/g)].map(match => [match[1], element()]));
const championButtons = ['RacingDriver', 'Charioteer', 'SpacePilot'].map(champion => element({ champion }));
const modeButtons = ['Steady', 'ChargeAndRelease'].map(mode => element({ mode }));
const events = new Map(), intervals = [], calls = [];
let assignedSide = 1;
const connection = { state: 'Connected', async start() {},
  on: (name, callback) => events.set(name, callback),
  onreconnecting: callback => events.set('reconnecting', callback),
  onreconnected: callback => events.set('reconnected', callback),
  async invoke(...args) { calls.push(args); if (args[0] === 'SteeringSide') return assignedSide; } };
class Builder { withUrl() { return this; } withAutomaticReconnect() { return this; } build() { return connection; } }
vm.runInNewContext(source, {
  document: { getElementById: id => { assert(elements.has(id), `Unknown element ${id}`); return elements.get(id); },
    querySelectorAll: selector => selector.includes('data-champion') ? championButtons : modeButtons },
  localStorage: { getItem: key => key === 'orbit-participant' ? 'p0' : key === 'orbit-vote-UI' ? 'RacingDriver' : null, setItem() {} },
  crypto: { randomUUID: () => 'p0' }, location: { search: '?session=UI' }, URLSearchParams,
  signalR: { HubConnectionBuilder: Builder, HubConnectionState: { Connected: 'Connected' } },
  setInterval: callback => intervals.push(callback), setTimeout() {}, console
});
(async () => {
  await Promise.resolve();
  events.get('RaceUpdated')({ phase: 2, winner: 20, mode: 0 });
  const vehicle = { champion: 20, progress: .1, offset: 0, targetOffset: .1 };
  const ready = { sessionId: 'UI', roundId: 'r1', phase: 1, vehicles: [vehicle], winner: null };
  await events.get('SteeringUpdated')(ready);
  assert.equal(elements.get('steeringPanel').style.display, 'block');
  assert.equal(elements.get('racePanel').style.display, 'none');
  assert.equal(elements.get('leftPower').disabled, true);
  assert.equal(elements.get('rightPower').style.display, 'none');
  await events.get('SteeringUpdated')({ ...ready, phase: 2 });
  assert.equal(elements.get('leftPower').disabled, false);
  elements.get('leftPower').click();
  for (const callback of intervals) await callback();
  assert(calls.some(x => x[0] === 'SteeringPower' && x[2] === 'r1' && x[4] === 1 && x[5] === 1));
  connection.state = 'Reconnecting';
  events.get('reconnecting')();
  assert.equal(elements.get('leftPower').disabled, true);
  connection.state = 'Connected';
  await events.get('reconnected')();
  await events.get('SteeringUpdated')({ ...ready, phase: 2 });
  assert.equal(elements.get('leftPower').disabled, false);
  await events.get('SteeringUpdated')({ ...ready, phase: 0, vehicles: [] });
  assert.equal(elements.get('steeringPanel').style.display, 'none');
  assert.equal(elements.get('racePanel').style.display, 'block');
  await events.get('SteeringUpdated')({ ...ready, phase: 2 });
  assert.equal(elements.get('steeringPanel').style.display, 'none', 'Late tick reopened the closed round');
  assignedSide = 3;
  await events.get('SteeringUpdated')({ ...ready, roundId: 'r2' });
  assert.equal(elements.get('leftPower').style.display, 'block');
  assert.equal(elements.get('rightPower').style.display, 'block');
  console.log('PASS: audience bonus panel, role controls, power batch, reconnect, normal-race restoration and stale-round guard.');
})().catch(error => { console.error(error); process.exitCode = 1; });
