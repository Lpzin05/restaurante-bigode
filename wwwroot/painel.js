// painel.js — painel de pedidos em tempo real do Restaurante do Bigode
// Exibe os pedidos organizados por status (Novo, Preparando, Finalizado)
// e atualiza automaticamente a cada 5 segundos para detectar novos pedidos.

let totalNovosAnterior = 0; // controla quantos pedidos novos havia na última atualização
let somAtivado = false;     // indica se o administrador ativou o alerta sonoro

// Áudio tocado quando um novo pedido chega
const audio = new Audio("/bell.mp3");
audio.volume = 1;

// Toca o som de alerta — trata erros pois navegadores podem bloquear áudio automático
async function tocarSom() {
  try {
    audio.pause();
    audio.currentTime = 0;
    await audio.play();
  } catch {
    console.warn("Som bloqueado pelo navegador.");
  }
}

// Formata um valor numérico como preço com duas casas decimais
function formatarTotal(valor) {
  return Number(valor).toFixed(2);
}

// Cria o card HTML de um pedido com seus itens e botões de ação
function criarPedidoCard(pedido) {
  const card = document.createElement("article");
  card.className = `order-card ${pedido.status}`;

  // Monta a lista de itens do pedido
  const itens = pedido.itens?.length
    ? pedido.itens.map(item => `<div class="small-text">${item.quantidade}x ${item.produto}</div>`).join("")
    : "<div class=\"small-text\">Sem itens</div>";

  card.innerHTML = `
    <strong>Pedido #${pedido.id} - ${pedido.nome}</strong>
    <div class="small-text">${pedido.telefone}</div>
    <div class="small-text">${pedido.tipo_entrega.toUpperCase()} ${pedido.endereco ? `- ${pedido.endereco}` : ""}</div>
    <div class="small-text">Pagamento: ${pedido.forma_pagamento.toUpperCase()}</div>
    ${itens}
    ${pedido.observacao ? `<div class="small-text">Obs.: ${pedido.observacao}</div>` : ""}
    <div class="total-line"><span>Total</span><span>R$ ${formatarTotal(pedido.total)}</span></div>
    <div class="actions">
      <button class="status-button new"  type="button" data-status="novo"       data-id="${pedido.id}">Novo</button>
      <button class="status-button prep" type="button" data-status="preparando" data-id="${pedido.id}">Preparando</button>
      <button class="status-button done" type="button" data-status="finalizado" data-id="${pedido.id}">Finalizado</button>
      <button class="danger-button"      type="button" data-delete="${pedido.id}">Excluir</button>
    </div>
  `;

  return card;
}

// Busca todos os pedidos da API e distribui nas três colunas do painel
async function carregarPedidos() {
  const response = await fetch("/pedidos");

  // Redireciona para login se a sessão expirou
  if (response.status === 401) {
    window.location.href = "/admin.html";
    return;
  }

  const pedidos = await response.json();

  // Limpa as colunas antes de renderizar novamente
  document.getElementById("novos").innerHTML = "";
  document.getElementById("preparando").innerHTML = "";
  document.getElementById("finalizados").innerHTML = "";

  // Toca o som se chegaram novos pedidos desde a última atualização
  const novosAgora = pedidos.filter(pedido => pedido.status === "novo").length;
  if (somAtivado && novosAgora > totalNovosAnterior) tocarSom();
  totalNovosAnterior = novosAgora;

  // Distribui cada pedido na coluna correta conforme seu status
  pedidos.forEach(pedido => {
    const destino = pedido.status === "novo"
      ? "novos"
      : pedido.status === "preparando"
        ? "preparando"
        : "finalizados";
    document.getElementById(destino).appendChild(criarPedidoCard(pedido));
  });

  // Remove o loading após o primeiro carregamento
  const loading = document.getElementById("loading");
  if (loading) loading.classList.add("oculto");
}

// Envia para a API a mudança de status de um pedido
async function atualizarStatus(id, status) {
  await fetch(`/pedido/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ status })
  });
  await carregarPedidos();
}

// Remove um pedido após confirmação do administrador
async function excluirPedido(id) {
  if (!confirm("Excluir pedido?")) return;
  await fetch(`/pedido/${id}`, { method: "DELETE" });
  await carregarPedidos();
}

// Ativa o som de alerta ao clicar no botão
document.getElementById("ativarSom").addEventListener("click", () => {
  somAtivado = true;
  document.getElementById("ativarSom").innerText = "Som ativado ✓";
  tocarSom();
});

// Botão de sair — encerra a sessão e volta para a tela de login
document.getElementById("sair").addEventListener("click", async () => {
  await fetch("/admin/logout", { method: "POST" });
  window.location.href = "/admin.html";
});

// Listener delegado para os botões de status e excluir nos cards
document.addEventListener("click", event => {
  const statusButton = event.target.closest("button[data-status]");
  if (statusButton) {
    atualizarStatus(statusButton.dataset.id, statusButton.dataset.status);
    return;
  }

  const deleteButton = event.target.closest("button[data-delete]");
  if (deleteButton) excluirPedido(deleteButton.dataset.delete);
});

// Carrega os pedidos ao abrir a página e atualiza a cada 5 segundos
carregarPedidos();
setInterval(carregarPedidos, 5000);