export class Chart {
  constructor(ctx, config) {
    // Basic Chart.js wrapper (minimal implementation for this context)
    this.chart = new window.Chart(ctx, config)
  }
}
