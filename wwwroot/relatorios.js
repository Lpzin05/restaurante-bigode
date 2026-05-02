// relatorios.js — Análise de dados do Restaurante do Bigode
// Busca os pedidos salvos e gera indicadores, rankings e previsões simples
// para apoiar a tomada de decisão do administrador.

async function carregarRelatorios() {
  const response = await fetch("/pedidos");

  // Redireciona para login se não estiver autenticado
  if (response.status === 401) {
    window.location.href = "/admin.html";
    return;
  }

  const pedidos = await response.json();

  if (pedidos.length === 0) {
    document.querySelector(".relatorios-layout").innerHTML +=
      '<p class="small-text" style="text-align:center;padding:40px">Nenhum pedido registrado ainda.</p>';
    return;
  }

  calcularIndicadores(pedidos);
  rankingProdutos(pedidos);
  rankingPorCampo(pedidos, "forma_pagamento", "rankingPagamento", formatarPagamento);
  rankingPorCampo(pedidos, "tipo_entrega",    "rankingEntrega",   formatarEntrega);
  rankingPorCampo(pedidos, "status",          "rankingStatus",    formatarStatus);
  calcularPrevisao(pedidos);
}

// ── Indicadores gerais ────────────────────────────────────────────────────────

function calcularIndicadores(pedidos) {
  const total        = pedidos.length;
  const faturamento  = pedidos.reduce((soma, p) => soma + Number(p.total), 0);
  const ticketMedio  = faturamento / total;
  const finalizados  = pedidos.filter(p => p.status === "finalizado").length;

  document.getElementById("totalPedidos").innerText       = total;
  document.getElementById("faturamentoTotal").innerText   = "R$ " + faturamento.toFixed(2);
  document.getElementById("ticketMedio").innerText        = "R$ " + ticketMedio.toFixed(2);
  document.getElementById("pedidosFinalizados").innerText = finalizados;
}

// ── Ranking de produtos mais pedidos ─────────────────────────────────────────

function rankingProdutos(pedidos) {
  // Conta quantas vezes cada produto aparece somando as quantidades
  const contagem = {};
  pedidos.forEach(pedido => {
    pedido.itens?.forEach(item => {
      contagem[item.produto] = (contagem[item.produto] || 0) + item.quantidade;
    });
  });

  // Ordena do mais pedido para o menos pedido
  const ordenado = Object.entries(contagem).sort((a, b) => b[1] - a[1]);
  const total    = ordenado.reduce((soma, [, qtd]) => soma + qtd, 0);

  const container = document.getElementById("rankingProdutos");
  container.innerHTML = ordenado.map(([nome, qtd], i) => `
    <div class="ranking-item">
      <span class="ranking-pos">${i + 1}º</span>
      <div class="ranking-info">
        <span class="ranking-nome">${nome}</span>
        <div class="ranking-barra-container">
          <div class="ranking-barra" style="width: ${(qtd / total * 100).toFixed(0)}%"></div>
        </div>
      </div>
      <span class="ranking-valor">${qtd} un.</span>
    </div>
  `).join("");
}

// ── Ranking genérico por campo do pedido ──────────────────────────────────────

function rankingPorCampo(pedidos, campo, elementoId, formatarLabel) {
  // Agrupa e conta os pedidos pelo valor do campo informado
  const contagem = {};
  pedidos.forEach(p => {
    const chave = p[campo] || "indefinido";
    contagem[chave] = (contagem[chave] || 0) + 1;
  });

  const ordenado = Object.entries(contagem).sort((a, b) => b[1] - a[1]);
  const total    = pedidos.length;

  const container = document.getElementById(elementoId);
  container.innerHTML = ordenado.map(([chave, qtd]) => `
    <div class="ranking-item">
      <div class="ranking-info">
        <span class="ranking-nome">${formatarLabel(chave)}</span>
        <div class="ranking-barra-container">
          <div class="ranking-barra" style="width: ${(qtd / total * 100).toFixed(0)}%"></div>
        </div>
      </div>
      <span class="ranking-valor">${qtd} pedido${qtd > 1 ? "s" : ""} (${(qtd / total * 100).toFixed(0)}%)</span>
    </div>
  `).join("");
}

// ── Previsão simples de faturamento ──────────────────────────────────────────

function calcularPrevisao(pedidos) {
  // Técnica: média móvel simples — usa a média atual como base de estimativa
  const faturamento = pedidos.reduce((soma, p) => soma + Number(p.total), 0);
  const mediaPorPedido = faturamento / pedidos.length;

  // Estimativas para diferentes volumes de pedidos futuros
  const cenarios = [
    { label: "Previsão (10 pedidos)", qtd: 10 },
    { label: "Previsão (20 pedidos)", qtd: 20 },
    { label: "Previsão (50 pedidos)", qtd: 50 },
  ];

  document.getElementById("previsao").innerHTML = cenarios.map(c => `
    <div class="indicador-card">
      <div class="indicador-titulo">${c.label}</div>
      <div class="indicador-valor">R$ ${(mediaPorPedido * c.qtd).toFixed(2)}</div>
      <div class="small-text">com base no ticket médio de R$ ${mediaPorPedido.toFixed(2)}</div>
    </div>
  `).join("");
}

// ── Funções de formatação dos labels ─────────────────────────────────────────

function formatarPagamento(valor) {
  return { pix: "PIX", dinheiro: "Dinheiro", cartao: "Cartão" }[valor] || valor;
}

function formatarEntrega(valor) {
  return { entrega: "Entrega", retirada: "Retirada" }[valor] || valor;
}

function formatarStatus(valor) {
  return { novo: "Novo", preparando: "Preparando", finalizado: "Finalizado" }[valor] || valor;
}

// Carrega os relatórios ao abrir a página
carregarRelatorios().finally(() => {
  const loading = document.getElementById('loading');
  if (loading) loading.classList.add('oculto');
});