/* Ember WebDemo: world is 960 × 540; player and torch positions are centres. */
window.EMBER_LEVELS = [
  {
    id: "01-footsteps",
    title: "01 · 작은 불꽃의 발걸음",
    subtitle: "짧게 누르고, 길게 누르고, 한 번 더 뛰어 보세요.",
    hint: "A/D 이동 · Space 점프 · 공중에서 Space 한 번 더. 마지막 틈에는 이단점프를 남겨 보세요.",
    spawn: { x: 72, y: 467 },
    platforms: [
      { x: 0, y: 480, w: 230, h: 60 },
      { x: 310, y: 480, w: 220, h: 60 },
      { x: 640, y: 480, w: 320, h: 60 },
      { x: 150, y: 415, w: 80, h: 20 },
      { x: 340, y: 405, w: 95, h: 20 },
      { x: 690, y: 395, w: 100, h: 20 }
    ],
    hazards: [{ x: 0, y: 525, w: 960, h: 15 }],
    torches: [],
    goal: { x: 885, y: 416, w: 38, h: 64 },
    abilities: { ignite: false, absorb: false, dash: false, shot: false }
  },
  {
    id: "02-first-flame",
    title: "02 · 첫 횃불",
    subtitle: "불은 위험물이 아니라, 들어가서 출발할 장소입니다.",
    hint: "횃불 가까이 Shift로 점화 → 마우스로 겨누고 우클릭 흡수 → W+D와 Space로 발사.",
    spawn: { x: 72, y: 467 },
    platforms: [
      { x: 0, y: 480, w: 260, h: 60 },
      { x: 260, y: 415, w: 80, h: 20 },
      { x: 352, y: 380, w: 88, h: 20 },
      { x: 650, y: 280, w: 310, h: 260 }
    ],
    hazards: [{ x: 0, y: 525, w: 960, h: 15 }],
    torches: [{ id: "first", x: 485, y: 325, lit: false }],
    goal: { x: 885, y: 216, w: 38, h: 64 },
    abilities: { ignite: true, absorb: true, dash: false, shot: false }
  },
  {
    id: "03-light-in-flight",
    title: "03 · 공중에서 길 만들기",
    subtitle: "땅에서 닿지 않던 불도, 뛰어오르면 켤 수 있습니다.",
    hint: "오른쪽으로 점프하며 Shift → 우클릭 흡수. 발사 중 다음 횃불 가까이 가면 다시 점화하세요.",
    spawn: { x: 72, y: 467 },
    platforms: [
      { x: 0, y: 480, w: 300, h: 60 },
      { x: 580, y: 260, w: 380, h: 280 }
    ],
    hazards: [{ x: 0, y: 525, w: 960, h: 15 }],
    torches: [
      { id: "air-low", x: 390, y: 290, lit: false },
      { id: "air-high", x: 610, y: 190, lit: false }
    ],
    goal: { x: 885, y: 196, w: 38, h: 64 },
    abilities: { ignite: true, absorb: true, dash: false, shot: false }
  },
  {
    id: "04-read-the-route",
    title: "04 · 뒤로 가야 열리는 길",
    subtitle: "가장 앞에 있는 불이 항상 좋은 출발점은 아닙니다.",
    hint: "낮은 천장 아래 거점보다, 뒤쪽의 높은 거점에서 우상향으로 출발해 보세요. 우클릭은 벽을 통과하지 않습니다.",
    spawn: { x: 72, y: 467 },
    platforms: [
      { x: 0, y: 480, w: 265, h: 60 },
      { x: 185, y: 420, w: 100, h: 20 },
      { x: 392, y: 260, w: 185, h: 28 },
      { x: 400, y: 420, w: 150, h: 20 },
      { x: 730, y: 250, w: 230, h: 290 }
    ],
    hazards: [{ x: 0, y: 525, w: 960, h: 15 }],
    torches: [
      { id: "choice-back", x: 248, y: 285, lit: true },
      { id: "choice-front", x: 445, y: 323, lit: true },
      { id: "choice-exit", x: 626, y: 200, lit: true }
    ],
    goal: { x: 885, y: 186, w: 38, h: 64 },
    abilities: { ignite: true, absorb: true, dash: false, shot: false }
  },
  {
    id: "05-the-sealed-hall",
    title: "05 · 닫힌 회랑",
    subtitle: "두 불을 깨우면 봉인이 열립니다.",
    hint: "문과 연결된 두 횃불을 Shift로 켜세요. 왼쪽 거점 발사를 이용하면 높은 두 번째 거점에 접근하기 쉽습니다.",
    spawn: { x: 72, y: 467 },
    platforms: [
      { x: 0, y: 480, w: 960, h: 60 },
      { x: 180, y: 400, w: 140, h: 20 },
      { x: 480, y: 330, w: 190, h: 20 }
    ],
    hazards: [{ x: 0, y: 525, w: 960, h: 15 }],
    torches: [
      { id: "seal-left", x: 305, y: 330, lit: false },
      { id: "seal-right", x: 650, y: 235, lit: false }
    ],
    door: { x: 826, y: 0, w: 28, h: 480, requires: ["seal-left", "seal-right"] },
    goal: { x: 885, y: 416, w: 38, h: 64 },
    abilities: { ignite: true, absorb: true, dash: false, shot: false }
  },
  {
    id: "06-turn-in-midair",
    title: "06 · 한 번의 방향 전환",
    subtitle: "위로 나갔다가, 링 안에서 오른쪽으로 방향을 바꿔 보세요.",
    hint: "첫 불에서 W+Space 발사 → 천장 가시 전에 Shift → D+Space 대시 → 오른쪽 불로 흡수. 착지나 불 진입으로 링이 회복됩니다.",
    spawn: { x: 72, y: 467 },
    platforms: [
      { x: 0, y: 480, w: 260, h: 60 },
      { x: 200, y: 420, w: 150, h: 20 },
      { x: 470, y: 415, w: 100, h: 20 },
      { x: 730, y: 260, w: 230, h: 280 }
    ],
    hazards: [
      { x: 0, y: 525, w: 960, h: 15 },
      { x: 268, y: 175, w: 136, h: 18 }
    ],
    torches: [
      { id: "turn-start", x: 310, y: 360, lit: true },
      { id: "turn-end", x: 530, y: 235, lit: true }
    ],
    goal: { x: 885, y: 196, w: 38, h: 64 },
    abilities: { ignite: true, absorb: true, dash: true, shot: false }
  },
  {
    id: "07-flame-relay",
    title: "07 · 불꽃 릴레이",
    subtitle: "발사, 흡수, 회복. 연결한 불이 다음 길이 됩니다.",
    hint: "우상향 발사 → 다음 불로 우클릭 흡수 → 다시 발사. 위쪽의 꺼진 불은 마우스 왼클릭 불씨로 켜는 선택 지름길입니다.",
    spawn: { x: 72, y: 467 },
    platforms: [
      { x: 0, y: 480, w: 220, h: 60 },
      { x: 435, y: 455, w: 105, h: 20 },
      { x: 720, y: 300, w: 240, h: 240 }
    ],
    hazards: [{ x: 0, y: 525, w: 960, h: 15 }],
    torches: [
      { id: "relay-one", x: 300, y: 350, lit: true },
      { id: "relay-two", x: 550, y: 285, lit: true },
      { id: "relay-three", x: 750, y: 220, lit: true },
      { id: "relay-distant", x: 770, y: 115, lit: false }
    ],
    goal: { x: 885, y: 236, w: 38, h: 64 },
    abilities: { ignite: true, absorb: true, dash: true, shot: true }
  },
  {
    id: "08-the-last-hall",
    title: "08 · 마지막 회랑",
    subtitle: "뒤따르는 어둠을 피하며, 익힌 동작을 연결하세요.",
    hint: "어둠의 돌진 예고를 보고 떠나세요. 우클릭으로 다음 거점에 들어가면 링과 공중 점프가 회복됩니다. 마지막에는 오른쪽으로 발사!",
    spawn: { x: 72, y: 467 },
    platforms: [
      { x: 0, y: 480, w: 230, h: 60 },
      { x: 365, y: 420, w: 115, h: 20 },
      { x: 620, y: 345, w: 90, h: 20 },
      { x: 800, y: 285, w: 160, h: 255 }
    ],
    hazards: [
      { x: 0, y: 525, w: 960, h: 15 },
      { x: 760, y: 125, w: 130, h: 18 }
    ],
    torches: [
      { id: "chase-one", x: 280, y: 365, lit: true },
      { id: "chase-two", x: 530, y: 285, lit: true },
      { id: "chase-three", x: 760, y: 220, lit: true }
    ],
    goal: { x: 885, y: 221, w: 38, h: 64 },
    abilities: { ignite: true, absorb: true, dash: true, shot: false },
    chase: true
  }
];
