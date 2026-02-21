const PIE_COLORS  = ['#4ade80','#60a5fa','#f97316','#f472b6','#a78bfa','#34d399','#fb923c','#818cf8'];
const tickColor   = '#6b7280';
const gridColor   = 'rgba(255,255,255,0.06)';
const legendColor = '#e5e7eb';
const FONT        = { family: 'JetBrains Mono', size: 11 };
const MONTH_NAMES = ['Janeiro','Fevereiro','Março','Abril','Maio','Junho',
                     'Julho','Agosto','Setembro','Outubro','Novembro','Dezembro'];

let dashMonth;
let dashYear;
let pieChart = null;

function initDashboard(month, year, pieData, barData) {
  dashMonth = month;
  dashYear  = year;
  if (pieData.length > 0) pieChart = buildPieChart(pieData);
  buildBarChart(barData);
}

function navigatePrev() {
  if (dashMonth === 1) { dashMonth = 12; dashYear--; }
  else dashMonth--;
  loadDashboard();
}

function navigateNext() {
  const now = new Date();
  if (dashYear === now.getFullYear() && dashMonth === now.getMonth() + 1) return;
  if (dashMonth === 12) { dashMonth = 1; dashYear++; }
  else dashMonth++;
  loadDashboard();
}

function fmtBRL(val) {
  return val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

async function loadDashboard() {
  const now = new Date();
  const isCurrentMonth = dashMonth === now.getMonth() + 1 && dashYear === now.getFullYear();

  document.getElementById('dashMonthLabel').textContent = MONTH_NAMES[dashMonth - 1] + ' ' + dashYear;
  document.getElementById('dashNextBtn').disabled = isCurrentMonth;

  try {
    const res  = await fetch(`/Dashboard/Filtered?month=${dashMonth}&year=${dashYear}`);
    const data = await res.json();

    document.getElementById('cardIncome').textContent   = fmtBRL(data.totalIncome);
    document.getElementById('cardExpenses').textContent = fmtBRL(data.totalExpenses);

    const balEl = document.getElementById('cardBalance');
    balEl.textContent = fmtBRL(data.balance);
    balEl.className   = 'card-value ' + (data.balance >= 0 ? 'text-green' : 'text-red');

    if (pieChart) { pieChart.destroy(); pieChart = null; }

    const pieWrap  = document.getElementById('pieWrap');
    const pieEmpty = document.getElementById('pieEmpty');

    if (data.expensesByCategory.length > 0) {
      pieWrap.classList.remove('d-none');
      pieEmpty.classList.add('d-none');
      pieChart = buildPieChart(data.expensesByCategory);
    } else {
      pieWrap.classList.add('d-none');
      pieEmpty.classList.remove('d-none');
    }
  } catch { }
}

function buildPieChart(data) {
  return new Chart(document.getElementById('pieChart'), {
    type: 'doughnut',
    data: {
      labels:   data.map(d => d.label),
      datasets: [{ data: data.map(d => d.value), backgroundColor: PIE_COLORS, borderWidth: 0 }],
    },
    options: {
      cutout: '60%',
      plugins: {
        legend: { position: 'right', labels: { color: legendColor, font: FONT, padding: 14, boxWidth: 12 } },
        tooltip: { callbacks: { label: ctx => ` R$ ${ctx.parsed.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}` } },
      },
    },
  });
}

function buildBarChart(data) {
  new Chart(document.getElementById('barChart'), {
    type: 'bar',
    data: {
      labels: data.map(d => d.label),
      datasets: [
        { label: 'Receita',  data: data.map(d => d.income),   backgroundColor: '#4ade80', borderRadius: 4, maxBarThickness: 30 },
        { label: 'Despesas', data: data.map(d => d.expenses), backgroundColor: '#f87171', borderRadius: 4, maxBarThickness: 30 },
      ],
    },
    options: {
      responsive: true,
      plugins: {
        legend: { labels: { color: legendColor, font: FONT } },
        tooltip: { callbacks: { label: ctx => ` ${ctx.dataset.label}: R$ ${ctx.parsed.y.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}` } },
      },
      scales: {
        x: { grid: { display: false }, ticks: { color: tickColor, font: FONT } },
        y: {
          border: { display: false },
          grid: { color: gridColor },
          ticks: { color: tickColor, font: FONT, callback: v => v >= 1000 ? `${(v/1000).toFixed(0)}k` : v },
        },
      },
    },
  });
}
