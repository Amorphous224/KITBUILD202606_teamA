// sound.js - Synthesizer module for custom procedural game sound effects using Web Audio API

class SoundSynth {
  constructor() {
    this.ctx = null;
    this.enabled = true;
  }

  init() {
    if (this.ctx) return;
    try {
      const AudioContext = window.AudioContext || window.webkitAudioContext;
      this.ctx = new AudioContext();
    } catch (e) {
      console.warn("Web Audio API is not supported in this browser", e);
      this.enabled = false;
    }
  }

  resume() {
    if (this.ctx && this.ctx.state === "suspended") {
      this.ctx.resume();
    }
  }

  playClick() {
    if (!this.enabled) return;
    this.init();
    this.resume();
    const ctx = this.ctx;
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();

    osc.type = "sine";
    osc.frequency.setValueAtTime(800, ctx.currentTime);
    osc.frequency.exponentialRampToValueAtTime(1200, ctx.currentTime + 0.05);

    gain.gain.setValueAtTime(0.08, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.08);

    osc.connect(gain);
    gain.connect(ctx.destination);

    osc.start();
    osc.stop(ctx.currentTime + 0.08);
  }

  playHover() {
    if (!this.enabled) return;
    this.init();
    this.resume();
    const ctx = this.ctx;
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();

    osc.type = "triangle";
    osc.frequency.setValueAtTime(300, ctx.currentTime);
    osc.frequency.setValueAtTime(310, ctx.currentTime + 0.02);

    gain.gain.setValueAtTime(0.02, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.03);

    osc.connect(gain);
    gain.connect(ctx.destination);

    osc.start();
    osc.stop(ctx.currentTime + 0.03);
  }

  playDraw() {
    if (!this.enabled) return;
    this.init();
    this.resume();
    const ctx = this.ctx;
    const osc = ctx.createOscillator();
    const filter = ctx.createBiquadFilter();
    const gain = ctx.createGain();

    osc.type = "sawtooth";
    osc.frequency.setValueAtTime(150, ctx.currentTime);
    osc.frequency.exponentialRampToValueAtTime(600, ctx.currentTime + 0.25);

    filter.type = "lowpass";
    filter.Q.setValueAtTime(4, ctx.currentTime);
    filter.frequency.setValueAtTime(200, ctx.currentTime);
    filter.frequency.exponentialRampToValueAtTime(2000, ctx.currentTime + 0.25);

    gain.gain.setValueAtTime(0.05, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.28);

    osc.connect(filter);
    filter.connect(gain);
    gain.connect(ctx.destination);

    osc.start();
    osc.stop(ctx.currentTime + 0.28);
  }

  playPlay() {
    if (!this.enabled) return;
    this.init();
    this.resume();
    const ctx = this.ctx;
    const osc1 = ctx.createOscillator();
    const osc2 = ctx.createOscillator();
    const gain = ctx.createGain();

    osc1.type = "triangle";
    osc1.frequency.setValueAtTime(440, ctx.currentTime);
    osc1.frequency.exponentialRampToValueAtTime(110, ctx.currentTime + 0.35);

    osc2.type = "sawtooth";
    osc2.frequency.setValueAtTime(445, ctx.currentTime);
    osc2.frequency.exponentialRampToValueAtTime(108, ctx.currentTime + 0.35);

    gain.gain.setValueAtTime(0.07, ctx.currentTime);
    gain.gain.setValueAtTime(0.05, ctx.currentTime + 0.05);
    gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.38);

    osc1.connect(gain);
    osc2.connect(gain);
    gain.connect(ctx.destination);

    osc1.start();
    osc2.start();
    osc1.stop(ctx.currentTime + 0.38);
    osc2.stop(ctx.currentTime + 0.38);
  }

  playReveal() {
    if (!this.enabled) return;
    this.init();
    this.resume();
    const ctx = this.ctx;
    const osc = ctx.createOscillator();
    const biquad = ctx.createBiquadFilter();
    const gain = ctx.createGain();

    osc.type = "sawtooth";
    osc.frequency.setValueAtTime(180, ctx.currentTime);
    osc.frequency.linearRampToValueAtTime(800, ctx.currentTime + 0.15);
    osc.frequency.linearRampToValueAtTime(400, ctx.currentTime + 0.35);

    biquad.type = "bandpass";
    biquad.frequency.setValueAtTime(600, ctx.currentTime);
    biquad.frequency.exponentialRampToValueAtTime(3000, ctx.currentTime + 0.2);

    gain.gain.setValueAtTime(0.0, ctx.currentTime);
    gain.gain.linearRampToValueAtTime(0.12, ctx.currentTime + 0.05);
    gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.4);

    osc.connect(biquad);
    biquad.connect(gain);
    gain.connect(ctx.destination);

    osc.start();
    osc.stop(ctx.currentTime + 0.4);
  }

  playDestroy() {
    if (!this.enabled) return;
    this.init();
    this.resume();
    const ctx = this.ctx;
    
    // Create white noise buffer
    const bufferSize = ctx.sampleRate * 0.5; // 0.5 seconds
    const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < bufferSize; i++) {
      data[i] = Math.random() * 2 - 1;
    }

    const noise = ctx.createBufferSource();
    noise.buffer = buffer;

    const noiseFilter = ctx.createBiquadFilter();
    noiseFilter.type = "lowpass";
    noiseFilter.frequency.setValueAtTime(1000, ctx.currentTime);
    noiseFilter.frequency.exponentialRampToValueAtTime(10, ctx.currentTime + 0.45);

    const noiseGain = ctx.createGain();
    noiseGain.gain.setValueAtTime(0.18, ctx.currentTime);
    noiseGain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.5);

    noise.connect(noiseFilter);
    noiseFilter.connect(noiseGain);
    noiseGain.connect(ctx.destination);

    // Sub rumble
    const subOsc = ctx.createOscillator();
    const subGain = ctx.createGain();
    subOsc.type = "triangle";
    subOsc.frequency.setValueAtTime(120, ctx.currentTime);
    subOsc.frequency.exponentialRampToValueAtTime(20, ctx.currentTime + 0.4);
    
    subGain.gain.setValueAtTime(0.15, ctx.currentTime);
    subGain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.45);
    
    subOsc.connect(subGain);
    subGain.connect(ctx.destination);

    noise.start();
    subOsc.start();
    noise.stop(ctx.currentTime + 0.5);
    subOsc.stop(ctx.currentTime + 0.5);
  }

  playWin() {
    if (!this.enabled) return;
    this.init();
    this.resume();
    const ctx = this.ctx;
    const now = ctx.currentTime;
    
    const notes = [261.63, 329.63, 392.00, 523.25, 659.25, 783.99, 1046.50]; // C Major scale arpeggio
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      
      osc.type = "sine";
      osc.frequency.setValueAtTime(freq, now + idx * 0.1);
      
      gain.gain.setValueAtTime(0.0, now);
      gain.gain.linearRampToValueAtTime(0.05, now + idx * 0.1 + 0.05);
      gain.gain.exponentialRampToValueAtTime(0.001, now + idx * 0.1 + 0.8);
      
      osc.connect(gain);
      gain.connect(ctx.destination);
      
      osc.start(now + idx * 0.1);
      osc.stop(now + idx * 0.1 + 0.8);
    });
  }

  playLose() {
    if (!this.enabled) return;
    this.init();
    this.resume();
    const ctx = this.ctx;
    const now = ctx.currentTime;

    const notes = [220.00, 207.65, 196.00, 164.81]; // Decreasing minor/chromatic descent (A -> Ab -> G -> E)
    notes.forEach((freq, idx) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      
      osc.type = "sawtooth";
      osc.frequency.setValueAtTime(freq, now + idx * 0.15);
      
      gain.gain.setValueAtTime(0.0, now);
      gain.gain.linearRampToValueAtTime(0.06, now + idx * 0.15 + 0.05);
      gain.gain.exponentialRampToValueAtTime(0.001, now + idx * 0.15 + 0.6);
      
      osc.connect(gain);
      gain.connect(ctx.destination);
      
      osc.start(now + idx * 0.15);
      osc.stop(now + idx * 0.15 + 0.6);
    });
  }
}

const sound = new SoundSynth();
export default sound;
