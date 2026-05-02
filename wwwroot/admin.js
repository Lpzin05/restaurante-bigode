// admin.js — tela de login do administrador do Restaurante do Bigode
// Realiza autenticação via API e redireciona para o painel em caso de sucesso.

async function fazerLogin() {
  const usuario = document.getElementById("usuario").value.trim();
  const senha   = document.getElementById("senha").value;

  const response = await fetch("/admin/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ usuario, senha })
  });

  if (!response.ok) {
    document.getElementById("mensagemLogin").innerText = "Usuário ou senha inválidos.";
    return;
  }

  window.location.href = "/painel.html";
}

// Clique no botão Entrar
document.getElementById("entrar").addEventListener("click", fazerLogin);

// Pressionar Enter em qualquer campo do formulário também faz login
document.getElementById("usuario").addEventListener("keydown", e => {
  if (e.key === "Enter") fazerLogin();
});

document.getElementById("senha").addEventListener("keydown", e => {
  if (e.key === "Enter") fazerLogin();
});