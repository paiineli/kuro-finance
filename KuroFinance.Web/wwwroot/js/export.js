function initExport(currentMonth) {
  document.getElementById('exportMonth').value = currentMonth;
}

function toggleFilters(cb) {
  document.getElementById('exportFilters').style.display = cb.checked ? 'none' : '';
}

async function downloadExcel() {
  const btn   = document.getElementById('exportBtn');
  const all   = document.getElementById('exportAll').checked;
  const month = document.getElementById('exportMonth').value;
  const year  = document.getElementById('exportYear').value;

  btn.disabled  = true;
  btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Gerando...';

  try {
    const qs  = all ? '' : `?month=${month}&year=${year}`;
    const res = await fetch(`/Export/Excel${qs}`);
    if (!res.ok) throw new Error('Erro ao exportar');

    const cd       = res.headers.get('Content-Disposition') ?? '';
    const match    = cd.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
    const filename = match ? match[1].replace(/['"]/g, '') : 'kurofinance.xlsx';

    const blob = await res.blob();
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href = url; a.download = filename; a.click();
    URL.revokeObjectURL(url);

    showToast('Arquivo Excel baixado!');
  } catch {
    showToast('Erro ao exportar.', 'danger');
  } finally {
    btn.disabled  = false;
    btn.innerHTML = '<i class="bi bi-download me-1"></i> Baixar Excel';
  }
}
