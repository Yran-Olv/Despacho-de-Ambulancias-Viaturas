export const GRAVIDADES = [
  { value: 'Baixa', label: 'Baixa — caso leve' },
  { value: 'Moderada', label: 'Moderada — febre, mal-estar' },
  { value: 'Alta', label: 'Alta — trauma, parto, queimadura' },
  { value: 'Critica', label: 'Crítica — risco de vida' },
  { value: 'EmergenciaMaxima', label: 'Emergência máxima — parada cardíaca' }
];

export const TIPOS = [
  { value: 'Clinica', label: 'Clínica' },
  { value: 'Trauma', label: 'Trauma' },
  { value: 'Queimadura', label: 'Queimadura' },
  { value: 'Obstetrica', label: 'Obstétrica (parto)' },
  { value: 'Cardiaca', label: 'Cardíaca' },
  { value: 'MultiplasVitimas', label: 'Múltiplas vítimas' }
];

export const GRAVIDADE_BADGE = {
  EmergenciaMaxima: 'badge-max',
  Critica: 'badge-critica',
  Alta: 'badge-alta',
  Moderada: 'badge-mod',
  Baixa: 'badge-baixa'
};

export const GRAVIDADE_LABEL = Object.fromEntries(GRAVIDADES.map(g => [g.value, g.label.split(' — ')[0]]));
export const TIPO_LABEL = Object.fromEntries(TIPOS.map(t => [t.value, t.label]));

export const TIPOS_VEICULO = [
  { value: 'Basica', label: 'Básica' },
  { value: 'Avancada', label: 'Avançada' },
  { value: 'UTI', label: 'UTI' }
];

export const TIPO_VEICULO_LABEL = Object.fromEntries(TIPOS_VEICULO.map(t => [t.value, t.label]));

export const STATUS_VEICULO_LABEL = {
  Disponivel: 'Disponível',
  EmAtendimento: 'Em atendimento',
  Inativo: 'Inativo'
};

export const STATUS_VEICULO_BADGE = {
  Disponivel: 'badge-ok',
  EmAtendimento: 'badge-warn',
  Inativo: 'badge-inativo'
};
