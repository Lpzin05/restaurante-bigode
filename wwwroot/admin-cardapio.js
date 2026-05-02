// admin-cardapio.js — gerenciamento do cardápio do Restaurante do Bigode
// Permite ao administrador cadastrar, editar e excluir produtos do cardápio
// através de uma interface integrada com a API REST do backend.

// Lista de produtos carregada da API — usada para preencher o formulário ao editar
let produtos = [];

// Lê o valor de um campo do formulário e remove espaços desnecessários
function valor(id) {
  return document.getElementById(id).value.trim();
}

// Limpa todos os campos do formulário e volta ao modo "Novo item"
function limparFormulario() {
  document.getElementById("produtoId").value = "";
  document.getElementById("categoria").value = "";
  document.getElementById("nomeProduto").value = "";
  document.getElementById("preco").value = "";
  document.getElementById("descricao").value = "";
  document.getElementById("ativo").checked = true;
  document.getElementById("formTitulo").innerText = "Novo item";
}

// Busca todos os produtos da API (incluindo ocultos) e renderiza na tela
async function carregarProdutos() {
  const response = await fetch("/admin/cardapio");

  // Redireciona para login se a sessão expirou
  if (response.status === 401) {
    window.location.href = "/admin.html";
    return;
  }

  produtos = await response.json();
  const container = document.getElementById("produtos");
  container.innerHTML = "";

  // Cria um card para cada produto com botões de editar e excluir
  produtos.forEach(produto => {
    const item = document.createElement("article");
    item.className = "order-card";
    item.innerHTML = `
      <strong>${produto.nome}</strong>
      <div class="small-text">${produto.categoria}</div>
      <div class="small-text">${produto.descricao}</div>
      <div class="total-line"><span>Preço</span><span>R$ ${Number(produto.preco).toFixed(2)}</span></div>
      <div class="small-text">${produto.ativo ? "Disponível" : "Oculto"}</div>
      <div class="actions">
        <button class="status-button prep" data-edit="${produto.id}" type="button">Editar</button>
        <button class="danger-button" data-delete-product="${produto.id}" type="button">Excluir</button>
      </div>
    `;
    container.appendChild(item);
  });
}

// Salva um produto — cria (POST) se for novo, atualiza (PUT) se já existir
async function salvarProduto() {
  const id = document.getElementById("produtoId").value;
  const produto = {
    categoria: valor("categoria"),
    nome:      valor("nomeProduto"),
    preco:     Number(document.getElementById("preco").value),
    descricao: valor("descricao"),
    ativo:     document.getElementById("ativo").checked
  };

  // Valida os campos obrigatórios antes de enviar
  if (!produto.categoria || !produto.nome || produto.preco <= 0) {
    alert("Preencha categoria, nome e preço.");
    return;
  }

  // Se houver id, é edição (PUT); caso contrário, é criação (POST)
  const url    = id ? `/admin/cardapio/${id}` : "/admin/cardapio";
  const method = id ? "PUT" : "POST";

  const response = await fetch(url, {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(produto)
  });

  if (!response.ok) {
    alert("Não foi possível salvar o item.");
    return;
  }

  limparFormulario();
  carregarProdutos();
}

// Remove um produto do cardápio após confirmação do administrador
async function excluirProduto(id) {
  if (!confirm("Excluir item do cardápio?")) return;
  await fetch(`/admin/cardapio/${id}`, { method: "DELETE" });
  carregarProdutos();
}

document.getElementById("salvarProduto").addEventListener("click", salvarProduto);
document.getElementById("limparFormulario").addEventListener("click", limparFormulario);

// Listener delegado para botões dinâmicos (editar e excluir nos cards)
document.addEventListener("click", event => {
  // Clique em "Editar" — preenche o formulário com os dados do produto
  const edit = event.target.closest("button[data-edit]");
  if (edit) {
    const produto = produtos.find(item => item.id == edit.dataset.edit);
    document.getElementById("produtoId").value      = produto.id;
    document.getElementById("categoria").value      = produto.categoria;
    document.getElementById("nomeProduto").value    = produto.nome;
    document.getElementById("preco").value          = produto.preco;
    document.getElementById("descricao").value      = produto.descricao;
    document.getElementById("ativo").checked        = produto.ativo;
    document.getElementById("formTitulo").innerText = "Editar item";
  }

  // Clique em "Excluir" — solicita confirmação e remove o produto
  const deleteButton = event.target.closest("button[data-delete-product]");
  if (deleteButton) excluirProduto(deleteButton.dataset.deleteProduct);
});

carregarProdutos();