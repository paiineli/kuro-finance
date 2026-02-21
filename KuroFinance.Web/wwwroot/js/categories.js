let editingId  = null;
let deletingId = null;
let modalType  = 'Expense';
let allCategories = [];
let catModal;
let deleteModal;

function initCategories(categories) {
  allCategories = categories;
  catModal    = new bootstrap.Modal(document.getElementById('catModal'));
  deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
  renderList();
}

function renderList() {
  const container = document.getElementById('catTableContainer');

  if (!allCategories.length) {
    container.innerHTML = '<p class="text-secondary small text-center py-4 mb-0">Nenhuma categoria cadastrada ainda.</p>';
    return;
  }

  const rows = allCategories.map(cat => `
    <tr>
      <td>
        <span class="${cat.type === 'Income' ? 'badge-income' : 'badge-expense'}">
          ${cat.type === 'Income' ? 'Receita' : 'Despesa'}
        </span>
      </td>
      <td class="fw-medium">${escHtml(cat.name)}</td>
      <td class="text-end">
        <div class="d-flex gap-1 justify-content-end">
          <button class="btn btn-sm btn-outline-secondary py-0 px-1" onclick="openEdit('${cat.id}')">
            <i class="bi bi-pencil"></i>
          </button>
          <button class="btn btn-sm btn-outline-danger py-0 px-1" onclick="openDelete('${cat.id}', ${JSON.stringify(cat.name)})">
            <i class="bi bi-trash"></i>
          </button>
        </div>
      </td>
    </tr>`).join('');

  container.innerHTML = `
    <div class="card">
      <div class="card-body p-0">
        <table class="table table-hover mb-0">
          <thead>
            <tr>
              <th class="small text-secondary fw-normal ps-3" style="width:110px">Tipo</th>
              <th class="small text-secondary fw-normal">Nome</th>
              <th></th>
            </tr>
          </thead>
          <tbody>${rows}</tbody>
        </table>
      </div>
    </div>`;
}

function escHtml(str) {
  return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function setModalType(type, btn) {
  modalType = type;
  document.querySelectorAll('#fTypeExpense, #fTypeIncome').forEach(b => b.classList.remove('active'));
  btn.classList.add('active');
}

function openCreate() {
  editingId = null;
  document.getElementById('catModalTitle').textContent = 'Nova categoria';
  document.getElementById('catSubmitBtn').textContent  = 'Criar';
  document.getElementById('fName').value = '';
  document.getElementById('catError').classList.add('d-none');
  modalType = 'Expense';
  document.getElementById('fTypeExpense').classList.add('active');
  document.getElementById('fTypeIncome').classList.remove('active');
}

function openEdit(id) {
  const cat = allCategories.find(c => c.id === id);
  if (!cat) return;
  editingId = id;
  document.getElementById('catModalTitle').textContent = 'Editar categoria';
  document.getElementById('catSubmitBtn').textContent  = 'Salvar';
  document.getElementById('fName').value = cat.name;
  document.getElementById('catError').classList.add('d-none');
  modalType = cat.type;
  document.getElementById('fTypeExpense').classList.toggle('active', cat.type === 'Expense');
  document.getElementById('fTypeIncome').classList.toggle('active',  cat.type === 'Income');
  catModal.show();
}

async function submitCategory() {
  const name = document.getElementById('fName').value.trim();
  const btn  = document.getElementById('catSubmitBtn');

  if (!name) { showCatError('Nome é obrigatório.'); return; }

  btn.disabled    = true;
  btn.textContent = 'Salvando...';

  const url = editingId ? `/Category/Update/${editingId}` : '/Category/Create';

  try {
    const res = await postJson(url, { name, type: modalType });
    if (!res.success) { showCatError(res.error); return; }

    if (editingId) {
      const cat = allCategories.find(c => c.id === editingId);
      cat.name = res.name;
      cat.type = res.type;
    } else {
      allCategories.push({ id: res.id, name: res.name, type: res.type });
    }

    allCategories.sort((a, b) => a.type.localeCompare(b.type) || a.name.localeCompare(b.name, 'pt'));
    catModal.hide();
    renderList();
    showToast(editingId ? 'Categoria atualizada!' : 'Categoria criada!');
  } catch {
    showCatError('Erro ao salvar. Tente novamente.');
  } finally {
    btn.disabled    = false;
    btn.textContent = editingId ? 'Salvar' : 'Criar';
  }
}

function showCatError(msg) {
  const el = document.getElementById('catError');
  el.textContent = msg;
  el.classList.remove('d-none');
}

function openDelete(id, name) {
  deletingId = id;
  deleteModal.show();
}

async function confirmDelete() {
  const btn = document.getElementById('confirmDeleteBtn');
  btn.disabled    = true;
  btn.textContent = 'Excluindo...';
  try {
    const res = await postJson(`/Category/Delete/${deletingId}`, {});
    if (res.success) {
      allCategories = allCategories.filter(c => c.id !== deletingId);
      deleteModal.hide();
      renderList();
      showToast('Categoria excluída!');
    } else {
      deleteModal.hide();
      showToast(res.error, 'danger');
    }
  } catch {
    showToast('Erro ao excluir.', 'danger');
  } finally {
    btn.disabled    = false;
    btn.textContent = 'Excluir';
  }
}
