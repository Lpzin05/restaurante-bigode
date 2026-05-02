// cliente.js — tela de pedido online do Restaurante do Bigode
// Monta o cardápio dinamicamente, gerencia o carrinho e envia o pedido para a API.

const cardapio = {
  "Todos os dias": [
    ["Bife",            23, "Arroz, feijão e batata frita"],
    ["Bisteca",         23, "Arroz, feijão e batata frita"],
    ["Calabresa",       23, "Arroz, feijão e batata frita"],
    ["Filé de frango",  23, "Arroz, feijão e batata frita"],
    ["Frango cozido",   23, "Arroz, feijão e batata frita"],
    ["Picadinho",       23, "Arroz, feijão e batata frita"],
    ["Steak",           23, "Arroz, feijão e batata frita"]
  ],
  "Pratos do dia": [
    ["Segunda — Virada à Paulista",        26, "Arroz, tutu, couve, torresmo, ovo e bisteca"],
    ["Terça — Costela cozida",             26, "Arroz, feijão e farofa"],
    ["Quarta — Strogonoff de frango",      26, "Arroz e batata frita"],
    ["Quinta — Macarrão com frango assado",26, "Macarrão penne e frango"],
    ["Sexta — Filé de peixe",              26, "Arroz, feijão e purê"]
  ],
  "Especiais": [
    ["Bife com ovo",              26, "Arroz, feijão e batata frita"],
    ["Bife com queijo",           26, "Arroz, feijão e batata frita"],
    ["Bife à milanesa",           26, "Arroz, feijão e batata frita"],
    ["Bife à parmegiana",         26, "Arroz, feijão e batata frita"],
    ["Calabresa com ovo",         26, "Arroz, feijão e batata frita"],
    ["Filé de frango à milanesa", 26, "Arroz, feijão e batata frita"],
    ["Filé à parmegiana",         26, "Arroz, feijão e batata frita"],
    ["Panqueca de carne",         26, "Arroz, feijão e batata frita"],
    ["Panqueca de frango",        26, "Arroz, feijão e batata frita"]
  ],
  "Feijoada light": [
    ["Prato feito feijoada light", 30, "Feijoada light completa"],
    ["Feijoada light pequena",     45, "Serve 1 pessoa"],
    ["Feijoada light grande",      60, "Serve 2 pessoas"]
  ],
  "Vegetarianos": [
    ["Carne de soja",        26, "Arroz, feijão e batata frita"],
    ["Hambúrguer de soja",   26, "Arroz, feijão e batata frita"],
    ["Legumes com fritas",   26, "Arroz, feijão, legumes e batata frita"],
    ["Omelete",              26, "Arroz, feijão e batata frita"],
    ["Panqueca de queijo",   26, "Arroz, feijão e batata frita"]
  ],
  "Porções e bebidas": [
    ["Fritas pequena",        15, "Porção individual"],
    ["Fritas grande",         20, "Porção grande"],
    ["Refrigerante lata",      6, "Lata 350ml"],
    ["Refrigerante 2L",       15, "Garrafa 2 litros"],
    ["Água 500ml",             5, "Sem gás"],
    ["Água com gás 500ml",     5, "Com gás"]
  ]
};

const carrinho = new Map();
const produtoIds = new Map();
const cardapioElement = document.getElementById("cardapio");

function dinheiro(valor) {
  return valor.toFixed(2);
}

function criarIdProduto(nome) {
  return nome
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^a-zA-Z0-9]+/g, "-")
    .replace(/^-|-$/g, "")
    .toLowerCase();
}

function montarCardapio() {
  Object.entries(cardapio).forEach(([categoria, produtos], index) => {
    const section = document.createElement("article");
    section.className = `category ${index === 0 ? "open" : ""}`;

    const button = document.createElement("button");
    button.className = "category-button";
    button.type = "button";
    button.innerHTML = `<span>${categoria}</span><span>+</span>`;
    button.addEventListener("click", () => section.classList.toggle("open"));

    const list = document.createElement("div");
    list.className = "products";

    produtos.forEach(([nome, preco, descricao]) => {
      carrinho.set(nome, { nome, preco, quantidade: 0 });
      produtoIds.set(nome, criarIdProduto(nome));

      const produto = document.createElement("div");
      produto.className = "product";
      produto.innerHTML = `
        <div>
          <div class="product-title">
            <span>${nome}</span>
            <span>R$ ${dinheiro(preco)}</span>
          </div>
          <div class="description">${descricao}</div>
        </div>
        <div class="quantity">
          <button class="icon-button" type="button" data-action="remove" data-product="${nome}">-</button>
          <strong id="qtd-${produtoIds.get(nome)}">0</strong>
          <button class="icon-button" type="button" data-action="add" data-product="${nome}">+</button>
        </div>
      `;
      list.appendChild(produto);
    });

    section.append(button, list);
    cardapioElement.appendChild(section);
  });
}

function alterarQuantidade(nome, delta) {
  const item = carrinho.get(nome);
  item.quantidade = Math.max(0, item.quantidade + delta);
  document.getElementById(`qtd-${produtoIds.get(nome)}`).innerText = item.quantidade;
  atualizarResumo();
}

function atualizarResumo() {
  const selecionados = [...carrinho.values()].filter(item => item.quantidade > 0);
  const total = selecionados.reduce((soma, item) => soma + item.quantidade * item.preco, 0);
  const totalItens = selecionados.reduce((soma, item) => soma + item.quantidade, 0);

  document.getElementById("valorTotal").innerText = dinheiro(total);
  document.getElementById("valorTotalResumo").innerText = dinheiro(total);
  document.getElementById("totalItens").innerText = totalItens;
  document.getElementById("listaCarrinho").innerHTML = selecionados.length
    ? selecionados.map(item => `<div>${item.quantidade}x ${item.nome}</div>`).join("")
    : "Carrinho vazio";
}

function limparPedido() {
  document.querySelectorAll("input, textarea, select").forEach(field => field.value = "");
  carrinho.forEach(item => {
    item.quantidade = 0;
    document.getElementById(`qtd-${produtoIds.get(item.nome)}`).innerText = "0";
  });
  atualizarResumo();
}

async function finalizarPedido() {
  const itens = [...carrinho.values()]
    .filter(item => item.quantidade > 0)
    .map(item => ({ nome: item.nome, quantidade: item.quantidade, preco: item.preco }));

  const pedido = {
    nome:            document.getElementById("nome").value.trim(),
    telefone:        document.getElementById("telefone").value.trim(),
    tipo_entrega:    document.getElementById("tipo_entrega").value,
    endereco:        document.getElementById("endereco").value.trim(),
    forma_pagamento: document.getElementById("forma_pagamento").value,
    observacao:      document.getElementById("observacao").value.trim(),
    itens
  };

  if (!pedido.nome || !pedido.telefone || !pedido.tipo_entrega || !pedido.forma_pagamento) {
    alert("Preencha todos os campos obrigatórios.");
    return;
  }

  if (pedido.tipo_entrega === "entrega" && !pedido.endereco) {
    alert("Informe o endereço para entrega.");
    return;
  }

  if (itens.length === 0) {
    alert("Selecione pelo menos um item.");
    return;
  }

  const response = await fetch("/pedido", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(pedido)
  });

  if (!response.ok) {
    alert("Não foi possível enviar o pedido. Tente novamente.");
    return;
  }

  limparPedido();
  document.getElementById("modalSucesso").classList.add("active");
}

cardapioElement.addEventListener("click", event => {
  const button = event.target.closest("button[data-product]");
  if (!button) return;
  alterarQuantidade(button.dataset.product, button.dataset.action === "add" ? 1 : -1);
});

document.getElementById("enviarPedido").addEventListener("click", finalizarPedido);
document.getElementById("fecharModal").addEventListener("click", () => {
  document.getElementById("modalSucesso").classList.remove("active");
});
document.getElementById("abrirCheckout").addEventListener("click", () => {
  const checkout = document.getElementById("checkout");
  checkout.classList.add("visible");
  checkout.scrollIntoView({ behavior: "smooth", block: "start" });
});

montarCardapio();

// Remove o loading após montar o cardápio
const loading = document.getElementById('loading');
if (loading) loading.classList.add('oculto');