
    const params = new URLSearchParams(location.search);
    const sessionId = (params.get('session') || 'ORBIT').toUpperCase();
    const status = document.getElementById('status');
    const votePanel = document.getElementById('votePanel');
    const racePanel = document.getElementById('racePanel');
    const buttons = [...document.querySelectorAll('button[data-champion]')];
    const boostButton = document.getElementById('boostButton');
    const raceStatus = document.getElementById('raceStatus');
    const raceResult = document.getElementById('raceResult');
    const tapCount = document.getElementById('tapCount');
    const teamName = document.getElementById('teamName');
    const participantId = localStorage.getItem('orbit-participant') || crypto.randomUUID();
    localStorage.setItem('orbit-participant', participantId);
    document.getElementById('session').textContent = `Session: ${sessionId}`;

    let latestSnapshot = null;
    let latestRace = null;
    let taps = 0;
    let pendingTaps = 0;
    let selectedChampion = localStorage.getItem(`orbit-vote-${sessionId}`);

    const labels = {
      RacingDriver: '🏎️ THE DRIVER',
      Charioteer: '🛡️ THE CHARIOTEER',
      SpacePilot: '🚀 THE SPACE PILOT'
    };

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/adventureHub')
      .withAutomaticReconnect()
      .build();

    function phaseName(phase) {
      if (typeof phase === 'string') return phase;
      return ['Ready', 'Open', 'Closed', 'Revealed'][phase] || 'Ready';
    }

    function racePhaseName(phase) {
      if (typeof phase === 'string') return phase;
      return ['Ready', 'Racing', 'Finished'][phase] || 'Ready';
    }

    function championName(value) {
      if (value == null) return null;
      if (typeof value === 'string') return value;
      return ['RacingDriver', 'Charioteer', 'SpacePilot'][value] || null;
    }

    function applySnapshot(snapshot) {
      latestSnapshot = snapshot;
      document.getElementById('driverVotes').textContent = `${snapshot.racingDriver} vote${snapshot.racingDriver === 1 ? '' : 's'}`;
      document.getElementById('chariotVotes').textContent = `${snapshot.charioteer} vote${snapshot.charioteer === 1 ? '' : 's'}`;
      document.getElementById('pilotVotes').textContent = `${snapshot.spacePilot} vote${snapshot.spacePilot === 1 ? '' : 's'}`;

      const phase = phaseName(snapshot.phase);
      const revealed = championName(snapshot.revealedChampion);

      buttons.forEach(button => {
        button.disabled = phase !== 'Open';
        button.classList.toggle('selected', button.dataset.champion === selectedChampion && phase !== 'Revealed');
        button.classList.toggle('winner', phase === 'Revealed' && revealed === button.dataset.champion);
        button.classList.toggle('loser', phase === 'Revealed' && revealed && revealed !== button.dataset.champion);
      });

      if (phase === 'Ready') {
        status.textContent = 'You’re in! Waiting for the presenter to open voting…';
      } else if (phase === 'Open') {
        status.textContent = selectedChampion
          ? 'Voting is open — your vote is in, but you can still change it.'
          : 'Voting is open — choose your champion!';
      } else if (phase === 'Closed') {
        status.textContent = 'Voting is closed. The result is locked…';
      } else if (phase === 'Revealed' && revealed) {
        status.textContent = revealed === selectedChampion
          ? '🎉 Your champion won! Keep this page open — you’ll race for them later.'
          : 'The audience has chosen. Keep this page open — your champion still races later!';
      } else if (phase === 'Revealed') {
        status.textContent = 'It’s a tie! Waiting for one more round…';
      }
    }

    function applyRace(snapshot) {
      latestRace = snapshot;
      const phase = racePhaseName(snapshot.phase);
      const winner = championName(snapshot.winner);

      if (phase === 'Ready') {
        // Keep the voting/result screen visible until the presenter actually starts the race.
        racePanel.style.display = 'none';
        votePanel.style.display = 'block';
        return;
      }

      votePanel.style.display = 'none';
      racePanel.style.display = 'block';
      const label = selectedChampion ? labels[selectedChampion] : 'YOUR ORIGINAL CHAMPION';
      teamName.textContent = label;

      if (phase === 'Racing') {
        boostButton.disabled = false;
        raceStatus.textContent = 'GO! Tap as fast as you can!';
        raceResult.textContent = '';
      } else {
        boostButton.disabled = true;
        raceStatus.textContent = 'FINISH!';
        raceResult.textContent = winner === selectedChampion
          ? '🏆 YOUR TEAM WON!'
          : `${labels[winner] || 'A champion'} wins the race!`;
      }
    }

    connection.on('VoteUpdated', applySnapshot);
    connection.on('RaceUpdated', applyRace);
    connection.onreconnecting(() => {
      status.textContent = 'Reconnecting…';
      raceStatus.textContent = 'Reconnecting…';
      boostButton.disabled = true;
    });
    connection.onreconnected(async () => {
      await connection.invoke('JoinSession', sessionId);
    });

    buttons.forEach(button => {
      button.addEventListener('click', async () => {
        if (connection.state !== signalR.HubConnectionState.Connected) return;
        if (!latestSnapshot || phaseName(latestSnapshot.phase) !== 'Open') return;

        selectedChampion = button.dataset.champion;
        localStorage.setItem(`orbit-vote-${sessionId}`, selectedChampion);
        buttons.forEach(x => x.classList.toggle('selected', x === button));
        await connection.invoke('Vote', sessionId, participantId, selectedChampion);
        status.textContent = 'Vote received — you can change it while voting remains open.';
      });
    });

    boostButton.addEventListener('click', async () => {
      if (connection.state !== signalR.HubConnectionState.Connected) return;
      if (!latestRace || racePhaseName(latestRace.phase) !== 'Racing') return;

      taps += 1;
      pendingTaps += 1;
      tapCount.textContent = `${taps} tap${taps === 1 ? '' : 's'}`;
    });

    // Batch frantic tapping into at most ten SignalR calls per second per browser.
    // The local tap count still responds immediately, while the server remains authoritative.
    setInterval(async () => {
      if (pendingTaps <= 0) return;
      if (connection.state !== signalR.HubConnectionState.Connected) return;
      if (!latestRace || racePhaseName(latestRace.phase) !== 'Racing') return;

      const batch = Math.min(pendingTaps, 20);
      pendingTaps -= batch;
      try {
        await connection.invoke('Boost', sessionId, participantId, batch);
      } catch {
        pendingTaps += batch;
      }
    }, 100);

    async function start() {
      try {
        await connection.start();
        await connection.invoke('JoinSession', sessionId);
      } catch {
        status.textContent = 'Unable to connect. Retrying…';
        setTimeout(start, 1500);
      }
    }

    start();
  