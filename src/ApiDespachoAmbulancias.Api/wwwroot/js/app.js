import { api } from './api.js';
import {
  GRAVIDADES, TIPOS, TIPOS_VEICULO,
  GRAVIDADE_BADGE, GRAVIDADE_LABEL, TIPO_LABEL,
  TIPO_VEICULO_LABEL, STATUS_VEICULO_LABEL, STATUS_VEICULO_BADGE
} from './constants.js';

const state = {
  page: 1,
  size: 20,
  totalPages: 1,
  histPage: 1,
  histSize: 20,
  histTotalPages: 1,
  editId: null,
  despacharId: null
};

const $ = (sel) => document.querySelector(sel);
const $$ = (sel) => document.querySelectorAll(sel);

function toast(msg, ok = true) {
  const el = document.createElement('div');
  el.className = `toast ${ok ? 'ok' : 'err'}`;
  el.textContent = msg;
  $('#toasts').appendChild(el);
  setTimeout(() => el.remove(), 4000);
}

function formatEspera(o) {
  if (o.tempoEsperaMinutos === 0) return '&lt; 1 min';
  return `${o.tempoEsperaMinutos} min`;
}

function badgeGravidade(v) {
  return `<span class="badge ${GRAVIDADE_BADGE[v] || 'badge-mod'}">${GRAVIDADE_LABEL[v] || v}</span>`;
}

function badgeStatusVeiculo(v) {
  return `<span class="badge ${STATUS_VEICULO_BADGE[v] || 'badge-mod'}">${STATUS_VEICULO_LABEL[v] || v}</span>`;
}

function fillSelect(select, options, placeholder = 'Selecione...') {
  select.innerHTML = `<option value="">${placeholder}</option>` +
    options.map(o => `<option value="${o.value}">${o.label}</option>`).join('');
}

function formData(form) {
  const data = Object.fromEntries(new FormData(form));
  data.pacientesEnvolvidos = Number(data.pacientesEnvolvidos);
  return data;
}

function formatData(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('pt-BR');
}

function renderProxima(o) {
  const card = $('#proxima-card');
  if (!o) {
    card.className = 'highlight-card empty';
    card.innerHTML = '<div class="empty-state">Nenhuma ocorrência ativa na fila.</div>';
    return;
  }

  card.className = 'highlight-card';
  card.innerHTML = `
    <div class="highlight-label">Próximo despacho — topo do Heap</div>
    <h3>${o.descricao}</h3>
    <div class="highlight-meta">
      <span>${badgeGravidade(o.gravidade)} ${TIPO_LABEL[o.tipoEmergencia]}</span>
      <span>Score: <strong>${o.scorePrioridade}</strong> (${o.resumoPrioridade || ''})</span>
      <span>Espera: ${formatEspera(o)}</span>
      <span>${o.endereco}</span>
    </div>
    <div style="margin-top:1rem">
      <button class="btn btn-success" id="btn-despachar-proxima" data-id="${o.id}">🚑 Despachar ambulância</button>
    </div>
  `;
  $('#btn-despachar-proxima')?.addEventListener('click', () => abrirDespacho(o.id, o.descricao));
}

function renderTable(itens) {
  const tbody = $('#fila-body');
  if (!itens?.length) {
    tbody.innerHTML = '<tr><td colspan="9" class="empty-state">Nenhuma ocorrência na fila.</td></tr>';
    return;
  }

  tbody.innerHTML = itens.map(o => `
    <tr>
      <td><span class="pos-badge ${o.posicaoFila === 1 ? 'top' : ''}">${o.posicaoFila}</span></td>
      <td>${o.descricao}<br><small style="color:var(--muted)">${o.endereco}</small></td>
      <td>${badgeGravidade(o.gravidade)}</td>
      <td>${TIPO_LABEL[o.tipoEmergencia]}</td>
      <td>${o.pacientesEnvolvidos}</td>
      <td>${formatEspera(o)}</td>
      <td><strong>${o.scorePrioridade}</strong></td>
      <td><small>${o.pontosGravidade}+${o.pontosTipo}+${o.pontosPacientes}+${o.pontosEspera}</small></td>
      <td>
        <div class="actions">
          <button class="btn btn-success btn-sm" data-despachar="${o.id}" data-desc="${encodeURIComponent(o.descricao)}">🚑 Despachar</button>
          <button class="btn btn-secondary btn-sm" data-edit="${o.id}">Editar</button>
          <button class="btn btn-danger btn-sm" data-del="${o.id}">Excluir</button>
        </div>
      </td>
    </tr>
  `).join('');

  tbody.querySelectorAll('[data-despachar]').forEach(btn =>
    btn.addEventListener('click', () =>
      abrirDespacho(btn.dataset.despachar, decodeURIComponent(btn.dataset.desc))));
  tbody.querySelectorAll('[data-edit]').forEach(btn =>
    btn.addEventListener('click', () => abrirEdicao(btn.dataset.edit)));
  tbody.querySelectorAll('[data-del]').forEach(btn =>
    btn.addEventListener('click', () => excluir(btn.dataset.del)));
}

async function carregarDashboard() {
  const main = $('#sec-dashboard');
  main.classList.add('loading');

  try {
    const [lista, proxima] = await Promise.all([
      api.listar(state.page, state.size),
      api.proxima().catch(() => null)
    ]);

    $('#stat-total').textContent = lista.totalItens;
    $('#stat-paginas').textContent = lista.totalPaginas || 1;
    state.totalPages = lista.totalPaginas || 1;

    renderProxima(proxima);
    renderTable(lista.itens);

    $('#pag-info').textContent = `Página ${lista.pagina} de ${lista.totalPaginas || 1} — ${lista.totalItens} ativas`;
    $('#btn-prev').disabled = lista.pagina <= 1;
    $('#btn-next').disabled = lista.pagina >= (lista.totalPaginas || 1);
  } catch (e) {
    toast(e.message, false);
  } finally {
    main.classList.remove('loading');
  }
}

async function abrirDespacho(id, descricao) {
  state.despacharId = id;
  $('#despachar-descricao').textContent = descricao || 'Selecione uma viatura para enviar ao local.';
  const select = $('#select-veiculo-despacho');
  select.innerHTML = '<option value="">Carregando...</option>';
  $('#modal-despachar').classList.add('open');

  try {
    const veiculos = await api.veiculos.disponiveis();
    if (!veiculos.length) {
      select.innerHTML = '<option value="">Nenhuma viatura disponível</option>';
      toast('Cadastre ou libere uma viatura antes de despachar.', false);
      return;
    }
    select.innerHTML = veiculos.map(v =>
      `<option value="${v.id}">${v.identificacao} — ${TIPO_VEICULO_LABEL[v.tipo] || v.tipo}</option>`
    ).join('');
  } catch (e) {
    select.innerHTML = '<option value="">Erro ao carregar</option>';
    toast(e.message, false);
  }
}

function fecharDespacho() {
  $('#modal-despachar').classList.remove('open');
  state.despacharId = null;
}

async function confirmarDespacho(veiculoId) {
  try {
    const result = await api.despachar(state.despacharId, veiculoId);
    toast(`Ambulância ${result.veiculoIdentificacao} a caminho!`);
    fecharDespacho();
    await carregarDashboard();
    if ($('#sec-acaminho').classList.contains('active')) await carregarACaminho();
    if ($('#sec-veiculos').classList.contains('active')) await carregarVeiculos();
  } catch (e) {
    toast(e.message, false);
  }
}

async function concluir(id) {
  if (!confirm('Confirmar chegada ao hospital e dar baixa no atendimento?')) return;
  try {
    await api.concluir(id);
    toast('Baixa registrada — viatura liberada para próximo despacho.');
    await carregarACaminho();
    if ($('#sec-veiculos').classList.contains('active')) await carregarVeiculos();
    if ($('#sec-historico').classList.contains('active')) await carregarHistorico();
  } catch (e) {
    toast(e.message, false);
  }
}

async function carregarACaminho() {
  const sec = $('#sec-acaminho');
  sec.classList.add('loading');
  try {
    const itens = await api.aCaminho();
    $('#stat-caminho').textContent = `${itens.length} ambulância(s) em deslocamento`;
    const tbody = $('#caminho-body');
    if (!itens.length) {
      tbody.innerHTML = '<tr><td colspan="6" class="empty-state">Nenhuma ambulância a caminho no momento.</td></tr>';
      return;
    }
    tbody.innerHTML = itens.map(o => `
      <tr>
        <td>${o.descricao}<br><small style="color:var(--muted)">${TIPO_LABEL[o.tipoEmergencia]}</small></td>
        <td>${o.endereco}</td>
        <td>${badgeGravidade(o.gravidade)}</td>
        <td><strong>${o.veiculoIdentificacao || '—'}</strong></td>
        <td>${formatData(o.dataDespacho)}</td>
        <td><button class="btn btn-primary btn-sm" data-concluir="${o.id}">🏥 Chegou ao hospital</button></td>
      </tr>
    `).join('');
    tbody.querySelectorAll('[data-concluir]').forEach(btn =>
      btn.addEventListener('click', () => concluir(btn.dataset.concluir)));
  } catch (e) {
    toast(e.message, false);
  } finally {
    sec.classList.remove('loading');
  }
}

async function carregarVeiculos() {
  const sec = $('#sec-veiculos');
  sec.classList.add('loading');
  try {
    const veiculos = await api.veiculos.listar();
    const disp = veiculos.filter(v => v.status === 'Disponivel').length;
    const atend = veiculos.filter(v => v.status === 'EmAtendimento').length;

    $('#stat-veiculos-total').textContent = veiculos.length;
    $('#stat-veiculos-disp').textContent = disp;
    $('#stat-veiculos-atend').textContent = atend;

    const tbody = $('#veiculos-body');
    if (!veiculos.length) {
      tbody.innerHTML = '<tr><td colspan="4" class="empty-state">Nenhuma viatura cadastrada.</td></tr>';
      return;
    }

    tbody.innerHTML = veiculos.map(v => `
      <tr>
        <td><strong>${v.identificacao}</strong></td>
        <td>${TIPO_VEICULO_LABEL[v.tipo] || v.tipo}</td>
        <td>${badgeStatusVeiculo(v.status)}</td>
        <td>
          ${v.status === 'Disponivel'
            ? `<button class="btn btn-danger btn-sm" data-del-veiculo="${v.id}" data-nome="${v.identificacao}">Excluir</button>`
            : '<small style="color:var(--muted)">—</small>'}
        </td>
      </tr>
    `).join('');

    tbody.querySelectorAll('[data-del-veiculo]').forEach(btn =>
      btn.addEventListener('click', () => excluirVeiculo(btn.dataset.delVeiculo, btn.dataset.nome)));
  } catch (e) {
    toast(e.message, false);
  } finally {
    sec.classList.remove('loading');
  }
}

async function carregarHistorico() {
  const sec = $('#sec-historico');
  sec.classList.add('loading');
  try {
    const lista = await api.concluidas(state.histPage, state.histSize);
    state.histTotalPages = lista.totalPaginas || 1;

    $('#stat-historico').textContent = `${lista.totalItens} atendimento(s) concluído(s)`;
    $('#hist-pag-info').textContent = `Página ${lista.pagina} de ${lista.totalPaginas || 1}`;
    $('#btn-hist-prev').disabled = lista.pagina <= 1;
    $('#btn-hist-next').disabled = lista.pagina >= (lista.totalPaginas || 1);

    const tbody = $('#historico-body');
    if (!lista.itens?.length) {
      tbody.innerHTML = '<tr><td colspan="5" class="empty-state">Nenhum atendimento concluído ainda.</td></tr>';
      return;
    }

    tbody.innerHTML = lista.itens.map(o => `
      <tr>
        <td>${o.descricao}<br><small style="color:var(--muted)">${TIPO_LABEL[o.tipoEmergencia]}</small></td>
        <td>${o.endereco}</td>
        <td>${o.veiculoIdentificacao || '—'}</td>
        <td>${formatData(o.dataDespacho)}</td>
        <td>${formatData(o.dataConclusao)}</td>
      </tr>
    `).join('');
  } catch (e) {
    toast(e.message, false);
  } finally {
    sec.classList.remove('loading');
  }
}

async function excluirVeiculo(id, nome) {
  if (!confirm(`Excluir viatura ${nome}? (Status → Inativo)`)) return;
  try {
    await api.veiculos.excluir(id);
    toast('Viatura excluída.');
    await carregarVeiculos();
  } catch (e) {
    toast(e.message, false);
  }
}

async function abrirEdicao(id) {
  try {
    const o = await api.obter(id);
    state.editId = id;
    const form = $('#form-editar');
    form.cpf.value = o.cpf;
    form.descricao.value = o.descricao;
    form.endereco.value = o.endereco;
    form.gravidade.value = o.gravidade;
    form.tipoEmergencia.value = o.tipoEmergencia;
    form.pacientesEnvolvidos.value = o.pacientesEnvolvidos;
    $('#modal-editar').classList.add('open');
  } catch (e) {
    toast(e.message, false);
  }
}

async function excluir(id) {
  if (!confirm('Excluir logicamente esta ocorrência? (Status → Inativo)')) return;
  try {
    await api.excluir(id);
    toast('Ocorrência inativada com sucesso.');
    await carregarDashboard();
  } catch (e) {
    toast(e.message, false);
  }
}

function navegar(sec) {
  $$('.section').forEach(s => s.classList.remove('active'));
  $$('.nav-btn').forEach(b => b.classList.remove('active'));
  $(`#sec-${sec}`).classList.add('active');
  $(`.nav-btn[data-sec="${sec}"]`).classList.add('active');
  if (sec === 'dashboard') carregarDashboard();
  if (sec === 'acaminho') carregarACaminho();
  if (sec === 'veiculos') carregarVeiculos();
  if (sec === 'historico') carregarHistorico();
}

function init() {
  fillSelect($('#form-cadastro select[name=gravidade]'), GRAVIDADES);
  fillSelect($('#form-cadastro select[name=tipoEmergencia]'), TIPOS);
  fillSelect($('#form-editar select[name=gravidade]'), GRAVIDADES);
  fillSelect($('#form-editar select[name=tipoEmergencia]'), TIPOS);
  fillSelect($('#form-veiculo select[name=tipo]'), TIPOS_VEICULO);

  $$('.nav-btn').forEach(btn =>
    btn.addEventListener('click', () => navegar(btn.dataset.sec)));

  $('#btn-refresh').addEventListener('click', carregarDashboard);
  $('#btn-refresh-caminho').addEventListener('click', carregarACaminho);
  $('#btn-refresh-veiculos').addEventListener('click', carregarVeiculos);
  $('#btn-refresh-historico').addEventListener('click', carregarHistorico);
  $('#btn-prev').addEventListener('click', () => { state.page--; carregarDashboard(); });
  $('#btn-next').addEventListener('click', () => { state.page++; carregarDashboard(); });
  $('#btn-hist-prev').addEventListener('click', () => { state.histPage--; carregarHistorico(); });
  $('#btn-hist-next').addEventListener('click', () => { state.histPage++; carregarHistorico(); });

  $('#form-cadastro').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
      const result = await api.criar(formData(e.target));
      toast(`Cadastrado! ${result.resumoPrioridade} — posição ${result.posicaoFila}`);
      e.target.reset();
      e.target.pacientesEnvolvidos.value = 1;
      navegar('dashboard');
    } catch (err) {
      toast(err.message, false);
    }
  });

  $('#form-veiculo').addEventListener('submit', async (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    try {
      const result = await api.veiculos.criar({
        identificacao: fd.get('identificacao'),
        tipo: fd.get('tipo')
      });
      toast(`Viatura ${result.identificacao} cadastrada!`);
      e.target.reset();
      await carregarVeiculos();
    } catch (err) {
      toast(err.message, false);
    }
  });

  $('#form-despachar').addEventListener('submit', async (e) => {
    e.preventDefault();
    const veiculoId = e.target.veiculoId.value;
    if (!veiculoId) {
      toast('Selecione uma viatura disponível.', false);
      return;
    }
    await confirmarDespacho(veiculoId);
  });

  $('#form-buscar').addEventListener('submit', async (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    try {
      const itens = await api.buscar(fd.get('cpf'), fd.get('descricao'));
      const tbody = $('#busca-body');
      if (!itens.length) {
        tbody.innerHTML = '<tr><td colspan="8" class="empty-state">Nenhum resultado.</td></tr>';
      } else {
        tbody.innerHTML = itens.map(o => `
          <tr>
            <td><span class="pos-badge ${o.posicaoFila === 1 ? 'top' : ''}">${o.posicaoFila}</span></td>
            <td>${o.descricao}</td>
            <td>${badgeGravidade(o.gravidade)}</td>
            <td>${TIPO_LABEL[o.tipoEmergencia]}</td>
            <td>${o.pacientesEnvolvidos}</td>
            <td>${formatEspera(o)}</td>
            <td>${o.scorePrioridade}</td>
            <td>${o.cpf}</td>
          </tr>`).join('');
      }
      toast(`${itens.length} resultado(s) encontrado(s).`);
    } catch (err) {
      toast(err.message, false);
    }
  });

  $('#form-editar').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
      const result = await api.atualizar(state.editId, formData(e.target));
      toast(`Atualizado! Nova posição: ${result.posicaoFila}`);
      $('#modal-editar').classList.remove('open');
      await carregarDashboard();
    } catch (err) {
      toast(err.message, false);
    }
  });

  $('#btn-cancelar-edit').addEventListener('click', () => $('#modal-editar').classList.remove('open'));
  $('#btn-cancelar-edit2').addEventListener('click', () => $('#modal-editar').classList.remove('open'));
  $('#btn-cancelar-desp').addEventListener('click', fecharDespacho);
  $('#btn-cancelar-desp2').addEventListener('click', fecharDespacho);
  $('#modal-editar').addEventListener('click', (e) => {
    if (e.target.id === 'modal-editar') $('#modal-editar').classList.remove('open');
  });
  $('#modal-despachar').addEventListener('click', (e) => {
    if (e.target.id === 'modal-despachar') fecharDespacho();
  });

  carregarDashboard();

  setInterval(() => {
    if ($('#sec-dashboard').classList.contains('active')) carregarDashboard();
    if ($('#sec-acaminho').classList.contains('active')) carregarACaminho();
    if ($('#sec-veiculos').classList.contains('active')) carregarVeiculos();
  }, 60_000);
}

init();
