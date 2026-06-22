const API = '/ocorrencias';
const VEICULOS = '/veiculos';

async function request(url, options = {}) {
  const res = await fetch(url, {
    headers: { Accept: 'application/json', ...options.headers },
    ...options
  });

  if (res.status === 204) return null;

  const body = await res.json().catch(() => ({}));

  if (!res.ok) {
    const msg = body.mensagem || body.title || `Erro HTTP ${res.status}`;
    throw new Error(msg);
  }

  return body;
}

export const api = {
  listar: (page = 1, size = 20) => request(`${API}?page=${page}&size=${size}`),
  proxima: () => request(`${API}/proxima`),
  aCaminho: () => request(`${API}/a-caminho`),
  concluidas: (page = 1, size = 20) => request(`${API}/concluidas?page=${page}&size=${size}`),
  buscar: (cpf, descricao) => {
    const params = new URLSearchParams();
    if (cpf) params.set('cpf', cpf);
    if (descricao) params.set('descricao', descricao);
    return request(`${API}/buscar?${params}`);
  },
  obter: (id) => request(`${API}/${id}`),
  criar: (data) => request(API, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  }),
  atualizar: (id, data) => request(`${API}/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  }),
  excluir: (id) => request(`${API}/${id}`, { method: 'DELETE' }),
  despachar: (id, veiculoId) => request(`${API}/${id}/despachar`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ veiculoId })
  }),
  concluir: (id) => request(`${API}/${id}/concluir`, { method: 'POST' }),

  veiculos: {
    listar: () => request(VEICULOS),
    disponiveis: () => request(`${VEICULOS}/disponiveis`),
    criar: (data) => request(VEICULOS, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    }),
    excluir: (id) => request(`${VEICULOS}/${id}`, { method: 'DELETE' })
  }
};
