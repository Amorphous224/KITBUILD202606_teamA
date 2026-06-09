// game.js - Main game logic, card definitions, and state machine for 単位獲得バトル (Credit Clash)

import sound from './sound.js';

// ==========================================================================
// CARD DATABASE
// ==========================================================================
const CARD_POOL = [
  // --- 単位取得カード (Credit Acquisition Cards) ---
  {
    id: "attendance",
    name: "出席 (Attendance)",
    category: "credit",
    cost: 1,
    power: 2, // Credits gained
    emoji: "🙋",
    ability: "On Reveal: このターン自分のデスクに他カードがなければ、獲得単位+1",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      // Check if this card was the only one played on owner's desk this turn
      const ownerDeskCards = deskCardsThisTurn.filter(c => c.owner === owner.id);
      if (ownerDeskCards.length === 1) {
        card.power += 1;
        game.log(`[効果] ${owner.name} は真面目に出席！単独ボーナスで獲得単位が +3 に増加。`, owner);
        game.createFloatingNumber(card.instanceId, "+1", "buff");
      } else {
        game.log(`[報告] ${owner.name} が出席しました。(+2単位)`, owner);
      }
    }
  },
  {
    id: "report-submission",
    name: "レポート提出 (Report)",
    category: "credit",
    cost: 2,
    power: 4,
    emoji: "📝",
    ability: "期日通りのレポート提出。特殊効果なし。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      game.log(`[報告] ${owner.name} がレポートを期限内に提出しました。(+4単位)`, owner);
    }
  },
  {
    id: "quiz",
    name: "小テスト (Quiz)",
    category: "credit",
    cost: 2,
    power: 3,
    emoji: "✍️",
    ability: "On Reveal: デッキからカードを1枚ドローする。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      game.log(`[効果] ${owner.name} は小テストをクリア。カードを1枚追加ドロー！`, owner);
      game.drawCard(owner);
    }
  },
  {
    id: "term-exam",
    name: "定期試験 (Term Exam)",
    category: "credit",
    cost: 4,
    power: 8,
    emoji: "✍️",
    ability: "一発逆転の定期試験。膨大な単位を獲得する。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      game.log(`[報告] ${owner.name} が定期試験を突破！大量の単位を獲得しました。(+8単位)`, owner);
    }
  },
  {
    id: "group-work",
    name: "グループワーク (GW)",
    category: "credit",
    cost: 3,
    power: 5,
    emoji: "👥",
    ability: "On Reveal: 相手プレイヤーの獲得単位も +1 する。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      // Give opponent +1 credit
      targetPlayer.credits = Math.max(0, targetPlayer.credits + 1);
      game.log(`[効果] ${owner.name} は協調性を発揮！共同作業で相手 (${targetPlayer.name}) にも +1 単位。`, owner);
      game.createFloatingNumber(`desk-${targetPlayer.id}`, "+1", "buff");
    }
  },

  // --- 妨害カード (Interference Cards) ---
  {
    id: "oversleeping",
    name: "寝坊 (Oversleeping)",
    category: "interfere",
    cost: 1,
    power: -2, // Credits reduced
    emoji: "⏰",
    ability: "相手の獲得単位を -2 する。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      game.log(`[妨害] ${owner.name} の仕掛け：${targetPlayer.name} は寝坊して講義をサボった。(-2単位)`, targetPlayer);
    }
  },
  {
    id: "missed-deadline",
    name: "締切忘れ (Missed DL)",
    category: "interfere",
    cost: 2,
    power: -4,
    emoji: "⏳",
    ability: "On Reveal: 相手がこのターン「レポート提出」をプレイしていた場合、-6 に強化。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      // Check if target played report submission on their desk this turn
      const reportPlayed = deskCardsThisTurn.some(c => c.owner === targetPlayer.id && c.id === 'report-submission');
      if (reportPlayed) {
        card.power = -6;
        game.log(`[妨害] ${owner.name} の仕掛け：${targetPlayer.name} は提出予定のレポートを出し忘れた！痛恨の -6 単位。`, targetPlayer);
        game.createFloatingNumber(card.instanceId, "-2", "nerf");
      } else {
        game.log(`[妨害] ${owner.name} の仕掛け：${targetPlayer.name} は課題の締切を忘れていた。(-4単位)`, targetPlayer);
      }
    }
  },
  {
    id: "cold",
    name: "風邪 (Cold)",
    category: "interfere",
    cost: 2,
    power: -3,
    emoji: "😷",
    ability: "On Reveal: 自分がカードを1枚ドローする。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      game.log(`[妨害] ${owner.name} の仕掛け：${targetPlayer.name} は風邪で体調を崩した。(-3単位) ${owner.name} はカードを1枚ドロー！`, targetPlayer);
      game.drawCard(owner);
    }
  },
  {
    id: "flu",
    name: "インフルエンザ (Flu)",
    category: "interfere",
    cost: 3,
    power: -5,
    emoji: "🌡️",
    ability: "相手にインフルエンザを感染させ、重大な単位ロスを与える。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      game.log(`[妨害] ${owner.name} の仕掛け：${targetPlayer.name} がインフルエンザを発症！強制出席停止。(-5単位)`, targetPlayer);
    }
  },
  {
    id: "registration-error",
    name: "履修登録ミス (Miss)",
    category: "interfere",
    cost: 4,
    power: -7,
    emoji: "🚫",
    ability: "On Reveal: このターン相手がプレイした最もコストの高い単位取得カードを無効化する。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      // Find the highest cost credit card played by opponent on their desk this turn
      const opponentCredits = deskCardsThisTurn.filter(c => c.owner === targetPlayer.id && c.category === 'credit');
      if (opponentCredits.length > 0) {
        let highest = opponentCredits[0];
        for (let c of opponentCredits) {
          if (c.cost > highest.cost) highest = c;
        }
        
        // Disable its unit addition by setting its power to 0
        highest.power = 0;
        highest.abilityTextOverride = "履修登録ミスにより無効化";
        game.log(`[妨害] ${owner.name} の仕掛け：${targetPlayer.name} の「${highest.name}」が履修登録エラーで無効化された！(-7単位ダメージ併発)`, targetPlayer);
        game.createFloatingNumber(highest.instanceId, "無効", "nerf");
      } else {
        game.log(`[妨害] ${owner.name} の仕掛け：${targetPlayer.name} は登録ミスに気付かず冷や汗を流した。(-7単位)`, targetPlayer);
      }
    }
  },
  {
    id: "overworked",
    name: "バイト入れすぎ (Job)",
    category: "interfere",
    cost: 3,
    power: -4,
    emoji: "💸",
    ability: "On Reveal: 次のターン、相手のカード使用コストを +1 する。",
    effect: (game, card, owner, targetPlayer, deskCardsThisTurn) => {
      targetPlayer.costPenaltyNextTurn += 1;
      game.log(`[妨害] ${owner.name} の仕掛け：${targetPlayer.name} は深夜バイトを入れすぎてヘトヘトだ。次のターンの全カードコスト +1。(-4単位)`, targetPlayer);
      game.createFloatingNumber(`desk-${targetPlayer.id}`, "コスト+1", "nerf");
    }
  }
];

// ==========================================================================
// GAME ENGINE
// ==========================================================================
class GameEngine {
  constructor() {
    this.turn = 1;
    this.phase = 'MENU'; // MENU, PLANNING_P1, TRANSITION_TO_P2, PLANNING_P2, CLASH, GAMEOVER
    
    this.p1 = this.initPlayer('p1', 'PLAYER 1 [AETHER]');
    this.p2 = this.initPlayer('p2', 'PLAYER 2 [NEXUS]');
    this.activePlayer = this.p1;
    
    this.selectedCard = null; // Card selected in hand
    
    this.clashLog = [];
    this.clashQueue = [];
    this.instanceCounter = 0;
  }

  initPlayer(id, name) {
    return {
      id: id,
      name: name,
      credits: 0, // Current units/credits
      deck: [],
      hand: [],
      energy: 1, // Action Points
      maxEnergy: 1,
      costPenaltyNextTurn: 0, // AP penalty from "バイト入れすぎ"
      boardCards: [], // Cards currently placed on this desk
      plannedPlays: [] // Plays made this turn: { card, deskOwnerId, costPaid }
    };
  }

  buildDecks() {
    [this.p1, this.p2].forEach(player => {
      player.deck = [];
      player.credits = 0;
      player.costPenaltyNextTurn = 0;
      player.boardCards = [];
      player.plannedPlays = [];

      // Construct a balanced 12-card deck
      const deckComposition = [
        "attendance", "attendance",
        "report-submission", "report-submission",
        "quiz",
        "term-exam",
        "group-work",
        "oversleeping", "oversleeping",
        "missed-deadline",
        "cold",
        "flu"
      ];
      
      const cards = deckComposition.map(id => {
        const def = CARD_POOL.find(c => c.id === id);
        return this.createCardInstance(def, player.id);
      });

      player.deck = this.shuffleArray(cards);
      player.hand = [];
    });
  }

  createCardInstance(cardDef, ownerId) {
    this.instanceCounter++;
    return {
      ...cardDef,
      instanceId: `card-${ownerId}-${this.instanceCounter}`,
      owner: ownerId,
      revealed: false,
      basePower: cardDef.power,
      power: cardDef.power,
      permanentBuff: 0,
      abilityTextOverride: null
    };
  }

  shuffleArray(array) {
    for (let i = array.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [array[i], array[j]] = [array[j], array[i]];
    }
    return array;
  }

  drawCard(player) {
    if (player.deck.length > 0 && player.hand.length < 7) {
      const card = player.deck.pop();
      player.hand.push(card);
      sound.playDraw();
      return card;
    }
    return null;
  }

  startMatch() {
    this.turn = 1;
    this.buildDecks();
    
    // Draw initial hand of 4 cards
    for (let i = 0; i < 4; i++) {
      this.drawCard(this.p1);
      this.drawCard(this.p2);
    }
    
    this.p1.maxEnergy = 1;
    this.p1.energy = 1;
    this.p2.maxEnergy = 1;
    this.p2.energy = 1;
    
    this.phase = 'PLANNING_P1';
    this.activePlayer = this.p1;
    this.selectedCard = null;
    
    this.render();
  }

  startNextTurn() {
    this.turn++;
    
    // Reset board cards from previous turn (they have already resolved and clear out)
    this.p1.boardCards = [];
    this.p2.boardCards = [];

    // Increment Action Points (AP)
    [this.p1, this.p2].forEach(p => {
      // Energy scales: Turn 1 = 1 AP, Turn 2 = 2 AP, Turn 3 = 3 AP, Turn 4 = 4 AP, then stays at 4 AP for fast paced matches
      const maxAp = Math.min(4, this.turn);
      p.maxEnergy = maxAp;
      p.energy = Math.max(1, p.maxEnergy - p.costPenaltyNextTurn);
      p.costPenaltyNextTurn = 0; // reset
      
      this.drawCard(p);
    });

    this.phase = 'PLANNING_P1';
    this.activePlayer = this.p1;
    this.selectedCard = null;
    this.render();
  }

  lockPlanning() {
    sound.playClick();
    if (this.phase === 'PLANNING_P1') {
      this.phase = 'TRANSITION_TO_P2';
      this.activePlayer = this.p2;
      this.selectedCard = null;
      this.renderTransitionScreen();
    } else if (this.phase === 'PLANNING_P2') {
      this.phase = 'CLASH';
      this.selectedCard = null;
      this.startClash();
    }
  }

  // ==========================================================================
  // CARD PLACEMENT / DESK TARGETING
  // ==========================================================================

  selectCardInHand(cardId) {
    if (this.phase !== 'PLANNING_P1' && this.phase !== 'PLANNING_P2') return;
    
    const card = this.activePlayer.hand.find(c => c.instanceId === cardId);
    if (!card) return;

    sound.playHover();

    if (this.selectedCard && this.selectedCard.instanceId === cardId) {
      this.selectedCard = null; // Toggle selection off
    } else {
      this.selectedCard = card;
    }
    this.render();
  }

  getCardPlayCost(card) {
    // If Overworked was played last turn, active player pays +1
    return card.cost;
  }

  playCardToDesk(deskOwnerId) {
    if (!this.selectedCard) return;
    const card = this.selectedCard;
    const player = this.activePlayer;

    // Rules verification
    // 1. Correct desk targeting
    if (card.category === 'credit' && deskOwnerId !== player.id) {
      alert("単位取得カードは自分のデスクにのみ配置できます。");
      return;
    }
    if (card.category === 'interfere' && deskOwnerId === player.id) {
      alert("妨害カードは相手のデスクにのみ配置できます。");
      return;
    }

    // 2. AP Check
    const cost = this.getCardPlayCost(card);
    if (player.energy < cost) {
      alert("AP（アクションポイント）が不足しています。");
      return;
    }

    // 3. Desk Capacity check (max 4 cards)
    const targetDeskCards = deskOwnerId === 'p1' ? this.p1.boardCards : this.p2.boardCards;
    if (targetDeskCards.length >= 4) {
      alert("これ以上カードを配置できません。");
      return;
    }

    sound.playPlay();
    player.energy -= cost;
    player.hand = player.hand.filter(c => c.instanceId !== card.instanceId);
    
    card.revealed = false;
    
    // Add to board cards of targeted desk
    const targetPlayer = deskOwnerId === 'p1' ? this.p1 : this.p2;
    targetPlayer.boardCards.push(card);
    
    // Track planned play to rollback
    player.plannedPlays.push({
      card: card,
      deskOwnerId: deskOwnerId,
      costPaid: cost
    });

    this.selectedCard = null;
    this.render();
  }

  cancelPlay(cardInstanceId, deskOwnerId) {
    if (this.phase !== 'PLANNING_P1' && this.phase !== 'PLANNING_P2') return;
    
    const player = this.activePlayer;
    const playIndex = player.plannedPlays.findIndex(p => p.card.instanceId === cardInstanceId);
    if (playIndex === -1) return;

    sound.playClick();
    const play = player.plannedPlays[playIndex];
    player.plannedPlays.splice(playIndex, 1);
    
    // Remove from board cards
    const targetPlayer = deskOwnerId === 'p1' ? this.p1 : this.p2;
    targetPlayer.boardCards = targetPlayer.boardCards.filter(c => c.instanceId !== cardInstanceId);
    
    // Return to hand and refund AP
    player.hand.push(play.card);
    player.energy += play.costPaid;

    this.render();
  }

  // ==========================================================================
  // CLASH SEQUENCE RESOLUTION (ACADEMIC AUDIT)
  // ==========================================================================
  
  startClash() {
    this.clashQueue = [];
    this.clashLog = [];
    
    const overlay = document.getElementById('clash-overlay');
    overlay.classList.add('active');
    
    const logEl = document.getElementById('clash-log');
    logEl.innerHTML = `<div class="log-line text-yellow">> サイクル ${this.turn} 単位判定シーケンスを開始...</div>`;
    
    // Gather all cards played on both desks this turn for evaluation
    // Keep reference to the cards played this turn so card abilities can check them
    this.deskCardsThisTurn = [
      ...this.p1.boardCards,
      ...this.p2.boardCards
    ];

    // Build reveal schedule
    // Alternate reveals: P1's desk card 1, P2's desk card 1, P1's card 2, etc.
    const p1PlaysCount = this.p1.plannedPlays.length;
    const p2PlaysCount = this.p2.plannedPlays.length;
    const maxPlays = Math.max(p1PlaysCount, p2PlaysCount);

    for (let i = 0; i < maxPlays; i++) {
      if (i < p1PlaysCount) {
        const play = this.p1.plannedPlays[i];
        this.clashQueue.push({ player: this.p1, play: play });
      }
      if (i < p2PlaysCount) {
        const play = this.p2.plannedPlays[i];
        this.clashQueue.push({ player: this.p2, play: play });
      }
    }

    // Reset planning lists
    this.p1.plannedPlays = [];
    this.p2.plannedPlays = [];

    this.processNextClashStep();
  }

  processNextClashStep() {
    if (this.clashQueue.length === 0) {
      this.finishClashPhase();
      return;
    }

    const { player, play } = this.clashQueue.shift();
    const card = play.card;
    const deskOwnerId = play.deskOwnerId;
    const deskOwner = deskOwnerId === 'p1' ? this.p1 : this.p2;

    // Verify it is still on the board (Registration Error could have destroyed/negated it)
    const isStillOnBoard = deskOwner.boardCards.some(c => c.instanceId === card.instanceId);
    if (!isStillOnBoard) {
      setTimeout(() => this.processNextClashStep(), 800);
      return;
    }

    sound.playReveal();
    card.revealed = true;
    this.render();

    // Visual pop
    const cardEl = document.getElementById(card.instanceId);
    if (cardEl) {
      cardEl.classList.add('selected-to-play');
      setTimeout(() => cardEl.classList.remove('selected-to-play'), 500);
    }

    // Execute effect (if any)
    const opponent = player.id === 'p1' ? this.p2 : this.p1;
    
    setTimeout(() => {
      // Trigger card logic
      if (card.effect) {
        card.effect(this, card, player, opponent, this.deskCardsThisTurn);
      }

      // Add/Subtract Credits from the desk owner
      // Credits cards add power (positive), Interference cards add power (negative)
      const change = card.power;
      deskOwner.credits = Math.max(0, deskOwner.credits + change);
      
      const floatType = change >= 0 ? 'buff' : 'nerf';
      const floatText = change >= 0 ? `+${change}` : `${change}`;
      
      this.createFloatingNumber(card.instanceId, floatText, floatType);
      
      this.render();
      sound.playReveal(); // Small notification sound

      setTimeout(() => this.processNextClashStep(), 1200);
    }, 600);
  }

  finishClashPhase() {
    // Check Victory
    const p1Win = this.p1.credits >= 20;
    const p2Win = this.p2.credits >= 20;

    if (p1Win || p2Win) {
      this.log(`[判定] 20単位到達を検知！セメスターを終了します。`, null, 'text-yellow');
      
      const btn = document.getElementById('clash-continue-btn');
      btn.querySelector('.btn-text').textContent = "RESULT CHECK";
      btn.classList.remove('hidden');
      btn.onclick = () => {
        sound.playClick();
        btn.classList.add('hidden');
        document.getElementById('clash-overlay').classList.remove('active');
        this.endMatch();
      };
    } else {
      this.log(`[SYS] 判定処理完了。次の学期 (CYCLE) へ移行します。`, null, 'text-green');
      
      const btn = document.getElementById('clash-continue-btn');
      btn.querySelector('.btn-text').textContent = "START NEXT CYCLE";
      btn.classList.remove('hidden');
      btn.onclick = () => {
        sound.playClick();
        btn.classList.add('hidden');
        document.getElementById('clash-overlay').classList.remove('active');
        this.startNextTurn();
      };
    }
  }

  // Destruction logic (can be triggered by Registration Error or custom effects)
  destroyCard(card, deskOwner, deskId) {
    sound.playDestroy();
    const cardEl = document.getElementById(card.instanceId);
    if (cardEl) {
      cardEl.style.transform = 'scale(0.1) rotate(90deg)';
      cardEl.style.opacity = '0';
    }

    // Remove from board
    deskOwner.boardCards = deskOwner.boardCards.filter(c => c.instanceId !== card.instanceId);
    this.log(`[破棄] ${deskOwner.name} の「${card.name}」が破棄されました。`, deskOwner, 'text-red');
  }

  createFloatingNumber(elementId, text, type) {
    const el = document.getElementById(elementId);
    if (!el) return;
    
    const rect = el.getBoundingClientRect();
    const floatEl = document.createElement('div');
    floatEl.className = `floating-status-number ${type}`;
    floatEl.textContent = text;
    floatEl.style.left = `${rect.left + rect.width / 2}px`;
    floatEl.style.top = `${rect.top}px`;
    
    document.body.appendChild(floatEl);
    
    setTimeout(() => {
      floatEl.remove();
    }, 1200);
  }

  log(msg, player = null, textClass = '') {
    let coloredClass = textClass;
    if (!coloredClass && player) {
      coloredClass = player.id === 'p1' ? 'text-cyan' : 'text-magenta';
    }
    
    const logEl = document.getElementById('clash-log');
    if (logEl) {
      const line = document.createElement('div');
      line.className = `log-line ${coloredClass}`;
      line.textContent = `> ${msg}`;
      logEl.appendChild(line);
      logEl.scrollTop = logEl.scrollHeight;
    }
  }

  // ==========================================================================
  // GAME OVER SCREEN
  // ==========================================================================
  
  endMatch() {
    this.phase = 'GAMEOVER';
    
    const banner = document.getElementById('winner-banner');
    const finalP1 = document.getElementById('final-p1-score');
    const finalP2 = document.getElementById('final-p2-score');
    
    finalP1.textContent = `${this.p1.credits} 単位`;
    finalP2.textContent = `${this.p2.credits} 単位`;

    if (this.p1.credits > this.p2.credits) {
      banner.textContent = "PLAYER 1 GRADUATES! (AETHER DEPT)";
      banner.className = "winner-neon p1-win";
      sound.playWin();
    } else if (this.p2.credits > this.p1.credits) {
      banner.textContent = "PLAYER 2 GRADUATES! (NEXUS DEPT)";
      banner.className = "winner-neon p2-win";
      sound.playLose();
    } else {
      banner.textContent = "CRITICAL CO-GRADUATION TIE";
      banner.className = "winner-neon tie-game";
      sound.playClick();
    }

    document.getElementById('game-board').classList.remove('active');
    document.getElementById('game-over').classList.add('active');
  }

  // ==========================================================================
  // TRANSITION SHIELD
  // ==========================================================================
  
  renderTransitionScreen() {
    const screen = document.getElementById('transition-screen');
    const title = document.getElementById('transition-title');
    const nameEl = document.getElementById('next-player-name');
    
    title.textContent = `⚡ セメスター操作引き渡し ⚡`;
    nameEl.textContent = this.activePlayer.id === 'p1' ? 'PLAYER 1 [AETHER]' : 'PLAYER 2 [NEXUS]';
    nameEl.className = this.activePlayer.id === 'p1' ? 'highlight p1-text' : 'highlight p2-text';

    screen.classList.add('active');

    const decryptBtn = document.getElementById('decrypt-btn');
    decryptBtn.onclick = () => {
      sound.playClick();
      screen.classList.remove('active');
      
      if (this.phase === 'TRANSITION_TO_P2') {
        this.phase = 'PLANNING_P2';
      }
      this.render();
    };
  }

  // ==========================================================================
  // DOM RENDERING ENGINE
  // ==========================================================================
  
  render() {
    const boardEl = document.getElementById('game-board');
    if (this.activePlayer.id === 'p1') {
      boardEl.className = "screen active p1-active";
    } else {
      boardEl.className = "screen active p2-active";
    }

    // Current turn
    document.getElementById('current-turn').textContent = this.turn;

    // Deck counts
    document.getElementById('p1-deck-count').textContent = this.p1.deck.length;
    document.getElementById('p2-deck-count').textContent = this.p2.deck.length;

    // Active player badge
    document.getElementById('active-player-badge').textContent = this.activePlayer.name;

    // Credit Scores on Desks
    document.getElementById('p1-credits').textContent = `${this.p1.credits} / 20`;
    document.getElementById('p2-credits').textContent = `${this.p2.credits} / 20`;

    // Render Action Point (AP) Nodes
    const energyBar = document.getElementById('energy-bar');
    energyBar.innerHTML = '';
    for (let i = 0; i < this.activePlayer.maxEnergy; i++) {
      const node = document.createElement('div');
      node.className = `energy-node ${i < this.activePlayer.energy ? 'filled' : ''}`;
      energyBar.appendChild(node);
    }
    document.getElementById('energy-text').textContent = `${this.activePlayer.energy} / ${this.activePlayer.maxEnergy} AP`;

    // Render Desk Zones
    const p1Desk = document.getElementById('desk-p1');
    const p2Desk = document.getElementById('desk-p2');
    
    p1Desk.className = "desk-zone";
    p2Desk.className = "desk-zone";

    // Highlight valid targeting desks based on selected card type
    if (this.selectedCard) {
      if (this.selectedCard.category === 'credit') {
        // Credit cards played on own desk
        if (this.activePlayer.id === 'p1') p1Desk.classList.add('highlight-place-self');
        else p2Desk.classList.add('highlight-place-self');
      } else if (this.selectedCard.category === 'interfere') {
        // Interference cards played on enemy desk
        if (this.activePlayer.id === 'p1') p2Desk.classList.add('highlight-place-enemy');
        else p1Desk.classList.add('highlight-place-enemy');
      }
    }

    // Render P1 board cards
    const p1CardsEl = document.getElementById('desk-p1-cards');
    p1CardsEl.innerHTML = '';
    this.p1.boardCards.forEach(card => {
      p1CardsEl.appendChild(this.renderCardElement(card, 'p1', 'p1'));
    });

    // Render P2 board cards
    const p2CardsEl = document.getElementById('desk-p2-cards');
    p2CardsEl.innerHTML = '';
    this.p2.boardCards.forEach(card => {
      p2CardsEl.appendChild(this.renderCardElement(card, 'p2', 'p2'));
    });

    // Render Active Player Hand
    const handEl = document.getElementById('player-hand');
    handEl.innerHTML = '';
    this.activePlayer.hand.forEach(card => {
      handEl.appendChild(this.renderCardElement(card, this.activePlayer.id, null));
    });

    // Confirm button styling
    const confirmBtn = document.getElementById('end-planning-btn');
    confirmBtn.querySelector('.btn-text').textContent = 
      this.activePlayer.id === 'p1' ? "PLAYER 1: LOCK" : "PLAYER 2: CLASH";
  }

  renderCardElement(card, ownerId, deskId) {
    const el = document.createElement('div');
    el.id = card.instanceId;
    
    let cssClasses = ['card'];
    cssClasses.push(card.category === 'credit' ? 'credit-card-type' : 'interfere-card-type');
    cssClasses.push(ownerId === 'p1' ? 'p1-card' : 'p2-card');
    cssClasses.push(card.owner === 'p1' ? 'p1-card-owner' : 'p2-card-owner');

    let shouldBeFaceDown = !card.revealed;
    if (deskId !== null) {
      // Board view: active player can see what they played this turn to rollback
      const playedThisTurnByActive = this.activePlayer.plannedPlays.some(p => p.card.instanceId === card.instanceId);
      if (playedThisTurnByActive) {
        shouldBeFaceDown = false;
      }
    } else {
      // Hand view
      shouldBeFaceDown = false;
    }

    if (shouldBeFaceDown) {
      cssClasses.push('face-down');
    }

    if (this.selectedCard && this.selectedCard.instanceId === card.instanceId) {
      cssClasses.push('selected-to-play');
    }

    el.className = cssClasses.join(' ');

    if (!shouldBeFaceDown) {
      const actualCost = this.getCardPlayCost(card);
      const isBuffed = card.power !== card.basePower;
      let powerClass = '';
      if (isBuffed) {
        powerClass = card.power > card.basePower ? 'buffed' : 'nerfed';
      }

      const displayPower = card.power >= 0 ? `+${card.power}` : `${card.power}`;
      const abilityText = card.abilityTextOverride || card.ability;

      el.innerHTML = `
        <div class="card-top">
          <span class="card-cost">${actualCost}</span>
          <span class="card-owner-tag">${card.owner.toUpperCase()}</span>
        </div>
        <div class="card-art">${card.emoji}</div>
        <div class="card-info">
          <span class="card-name">${card.name}</span>
          <p class="card-ability">${abilityText}</p>
        </div>
        <div class="card-bottom">
          <span class="card-power ${powerClass}">${displayPower}</span>
        </div>
      `;
    }

    // Interactivity
    if (deskId === null) {
      // Hand Cards
      el.addEventListener('click', (e) => {
        e.stopPropagation();
        this.selectCardInHand(card.instanceId);
      });
      el.addEventListener('mouseenter', () => {
        sound.playHover();
      });
    } else {
      // Board Cards
      const playedThisTurnByActive = this.activePlayer.plannedPlays.some(p => p.card.instanceId === card.instanceId);
      if (playedThisTurnByActive) {
        el.addEventListener('click', (e) => {
          e.stopPropagation();
          this.cancelPlay(card.instanceId, deskId);
        });
      }
    }

    return el;
  }
}

// ==========================================================================
// BOOTSTRAP INITIALIZATION
// ==========================================================================

const game = new GameEngine();

window.addEventListener('DOMContentLoaded', () => {
  // Main Menu button
  const startBtn = document.getElementById('start-btn');
  startBtn.addEventListener('click', () => {
    sound.playClick();
    document.getElementById('main-menu').classList.remove('active');
    document.getElementById('game-board').classList.add('active');
    game.startMatch();
  });

  // End planning
  const endPlanningBtn = document.getElementById('end-planning-btn');
  endPlanningBtn.addEventListener('click', () => {
    game.lockPlanning();
  });

  // Rematch
  const rematchBtn = document.getElementById('rematch-btn');
  rematchBtn.addEventListener('click', () => {
    sound.playClick();
    document.getElementById('game-over').classList.remove('active');
    document.getElementById('game-board').classList.add('active');
    game.startMatch();
  });

  // Setup Desk click listeners
  const deskP1El = document.getElementById('desk-p1');
  const deskP2El = document.getElementById('desk-p2');

  deskP1El.addEventListener('click', () => {
    game.playCardToDesk('p1');
  });

  deskP2El.addEventListener('click', () => {
    game.playCardToDesk('p2');
  });
});
