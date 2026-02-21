let editingId  = null;
let deletingId = null;
let filterType = '';
let filterMonth = 0;
let filterYear  = 0;
let txModal;
let deleteModal;

function initTransactions(month, year) {
  filterMonth = month;
  filterYear  = year;
  txModal     = new bootstrap.Modal(document.getElementById('txModal'));
  deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
}

function setType(type, btn) {
  filterType = type;
  document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
  btn.classList.add('active');
  loadTable();
}

async function loadTable() {
  const qs = new URLSearchParams({ month: filterMonth, year: filterYear });
  if (filterType) qs.append('type', filterType);
  try {
    const res  = await fetch(`/Transaction/Filtered?${qs}`);
    const html = await res.text();
    document.getElementById('txTableContainer').innerHTML = html;
  } catch {
    showToast('Erro ao carregar transações.', 'danger');
  }
}

function filterCategoriesByType(type) {
  document.querySelectorAll('#fCategory option[data-type]').forEach(opt => {
    opt.hidden = opt.dataset.type !== type;
  });
  const sel = document.getElementById('fCategory');
  if (sel.selectedOptions[0]?.hidden) sel.value = '';
}

function openCreate() {
  editingId = null;
  document.getElementById('txModalTitle').textContent = 'Nova transação';
  document.getElementById('txSubmitBtn').textContent  = 'Criar';
  document.getElementById('fDesc').value     = '';
  document.getElementById('fAmount').value   = '';
  document.getElementById('fType').value     = 'Expense';
  document.getElementById('fDate').value     = new Date().toISOString().split('T')[0];
  document.getElementById('fCategory').value = '';
  document.getElementById('txError').classList.add('d-none');
  filterCategoriesByType('Expense');
}

function openEdit(id, desc, amount, type, date, categoryId) {
  editingId = id;
  document.getElementById('txModalTitle').textContent = 'Editar transação';
  document.getElementById('txSubmitBtn').textContent  = 'Salvar';
  document.getElementById('fDesc').value     = desc;
  document.getElementById('fAmount').value   = amount;
  document.getElementById('fType').value     = type;
  document.getElementById('fDate').value     = date;
  filterCategoriesByType(type);
  document.getElementById('fCategory').value = categoryId;
  document.getElementById('txError').classList.add('d-none');
  txModal.show();
}

async function submitTransaction() {
  const desc   = document.getElementById('fDesc').value.trim();
  const amount = parseFloat(document.getElementById('fAmount').value);
  const type   = document.getElementById('fType').value;
  const date   = document.getElementById('fDate').value;
  const catId  = document.getElementById('fCategory').value;
  const btn    = document.getElementById('txSubmitBtn');

  if (!desc)                  { showTxError('Descrição é obrigatória.'); return; }
  if (!amount || amount <= 0) { showTxError('Valor deve ser maior que zero.'); return; }
  if (!date)                  { showTxError('Data é obrigatória.'); return; }
  if (!catId)                 { showTxError('Selecione uma categoria.'); return; }

  btn.disabled    = true;
  btn.textContent = 'Salvando...';

  const url     = editingId ? `/Transaction/Update/${editingId}` : '/Transaction/Create';
  const payload = { description: desc, amount, type, date: date + 'T12:00:00', categoryId: catId };

  try {
    const res = await postJson(url, payload);
    if (!res.success) { showTxError(res.error); return; }
    txModal.hide();
    showToast(editingId ? 'Transação atualizada!' : 'Transação criada!');
    await loadTable();
  } catch {
    showTxError('Erro ao salvar. Tente novamente.');
  } finally {
    btn.disabled    = false;
    btn.textContent = editingId ? 'Salvar' : 'Criar';
  }
}

function showTxError(msg) {
  const el = document.getElementById('txError');
  el.textContent = msg;
  el.classList.remove('d-none');
}

function confirmDelete(id) {
  deletingId = id;
  deleteModal.show();
}

async function deleteTransaction() {
  const btn = document.getElementById('confirmDeleteBtn');
  btn.disabled    = true;
  btn.textContent = 'Excluindo...';
  try {
    const res = await postJson(`/Transaction/Delete/${deletingId}`, {});
    if (res.success) {
      deleteModal.hide();
      showToast('Transação excluída!');
      await loadTable();
    } else showToast(res.error, 'danger');
  } catch {
    showToast('Erro ao excluir.', 'danger');
  } finally {
    btn.disabled    = false;
    btn.textContent = 'Excluir';
  }
}
