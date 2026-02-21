async function postJson(url, data) {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });
  return res.json();
}

function showToast(message, type = 'success') {
  const container = document.getElementById('toastContainer');
  const colors = { success: 'bg-success', danger: 'bg-danger', warning: 'bg-warning text-dark' };
  const id = 'toast-' + Date.now();
  container.insertAdjacentHTML('beforeend', `
    <div id="${id}" class="toast align-items-center text-white border-0 ${colors[type] ?? colors.success}" role="alert">
      <div class="d-flex">
        <div class="toast-body small">${message}</div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
      </div>
    </div>`);
  const el = new bootstrap.Toast(document.getElementById(id), { delay: 3500 });
  el.show();
  document.getElementById(id).addEventListener('hidden.bs.toast', () => document.getElementById(id)?.remove());
}
