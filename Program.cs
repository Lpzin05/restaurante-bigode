// Ponto de entrada da aplicação web do Restaurante do Bigode.
// Configura os serviços, middlewares e define todas as rotas da API REST.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RestaurantOrderSystem.Database;
using RestaurantOrderSystem.Dtos;
using RestaurantOrderSystem.Models;
using RestaurantOrderSystem.Repositories;

// Caminho do banco de dados SQLite gerado na raiz do projeto
var connectionString = "Data Source=restaurante-bigode.db";

// Inicializa o banco: cria as tabelas e insere o cardápio inicial se estiver vazio
DatabaseInitializer.Initialize(connectionString);

var builder = WebApplication.CreateBuilder(args);

// Configura autenticação por cookie de sessão
// Usuários não autenticados que tentarem acessar rotas protegidas recebem 401
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin.html";
        options.AccessDeniedPath = "/admin.html";
        options.Events.OnRedirectToLogin = context =>
        {
            // Rotas de API retornam 401 em vez de redirecionar para o login
            if (context.Request.Path.StartsWithSegments("/admin") ||
                context.Request.Path.StartsWithSegments("/pedido") ||
                context.Request.Path.StartsWithSegments("/pedidos"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// Registra os repositórios SQLite como singletons (uma instância por aplicação)
builder.Services.AddSingleton<IPedidoRepository>(_ => new SqlitePedidoRepository(connectionString));
builder.Services.AddSingleton<ICardapioRepository>(_ => new SqliteCardapioRepository(connectionString));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Middleware que protege páginas HTML administrativas de acesso sem login
app.Use(async (context, next) =>
{
    var protectedPages = new[] { "/painel.html", "/admin-cardapio.html", "/relatorios.html" };
    if (protectedPages.Contains(context.Request.Path.Value?.ToLowerInvariant()) &&
        context.User.Identity?.IsAuthenticated != true)
    {
        context.Response.Redirect("/admin.html");
        return;
    }

    await next();
});

// Serve os arquivos estáticos da pasta wwwroot (HTML, CSS, JS)
app.UseDefaultFiles();
app.UseStaticFiles();

// ──────────────────────────────────────────────
// ROTAS DE AUTENTICAÇÃO
// ──────────────────────────────────────────────

// POST /admin/login — valida usuário e senha e cria a sessão do administrador
app.MapPost("/admin/login", async (LoginRequest login, HttpContext context) =>
{
    const string usuarioAdmin = "admin";
    const string senhaAdmin = "admin123";

    if (!string.Equals(login.Usuario, usuarioAdmin, StringComparison.OrdinalIgnoreCase) || login.Senha != senhaAdmin)
    {
        return Results.Unauthorized();
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, "Administrador"),
        new(ClaimTypes.Role, "Admin")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Ok(new { mensagem = "Login realizado com sucesso." });
});

// POST /admin/logout — encerra a sessão do administrador
app.MapPost("/admin/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { mensagem = "Logout realizado." });
}).RequireAuthorization();

// GET /auth/status — informa se o usuário atual está autenticado
app.MapGet("/auth/status", (HttpContext context) =>
{
    return Results.Ok(new { autenticado = context.User.Identity?.IsAuthenticated == true });
});

// ──────────────────────────────────────────────
// ROTAS DO CARDÁPIO
// ──────────────────────────────────────────────

// GET /cardapio — retorna os itens ativos agrupados por categoria (acesso público)
app.MapGet("/cardapio", (ICardapioRepository repository) =>
{
    return Results.Ok(repository.Listar(apenasAtivos: true)
        .GroupBy(produto => produto.Categoria)
        .Select(grupo => new { categoria = grupo.Key, produtos = grupo }));
});

// GET /admin/cardapio — retorna todos os itens incluindo os ocultos (requer login)
app.MapGet("/admin/cardapio", (ICardapioRepository repository) =>
{
    return Results.Ok(repository.Listar(apenasAtivos: false));
}).RequireAuthorization();

// POST /admin/cardapio — cadastra um novo item no cardápio
app.MapPost("/admin/cardapio", (ProdutoCardapioRequest request, ICardapioRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(request.Categoria) ||
        string.IsNullOrWhiteSpace(request.Nome) ||
        request.Preco <= 0)
    {
        return Results.BadRequest(new { erro = "Categoria, nome e preço são obrigatórios." });
    }

    var produto = repository.Criar(new ProdutoCardapio
    {
        Categoria = request.Categoria.Trim(),
        Nome      = request.Nome.Trim(),
        Preco     = request.Preco,
        Descricao = request.Descricao?.Trim() ?? "",
        Ativo     = request.Ativo
    });

    return Results.Created($"/admin/cardapio/{produto.Id}", produto);
}).RequireAuthorization();

// PUT /admin/cardapio/{id} — atualiza os dados de um item existente
app.MapPut("/admin/cardapio/{id:int}", (int id, ProdutoCardapioRequest request, ICardapioRepository repository) =>
{
    var atualizado = repository.Atualizar(id, new ProdutoCardapio
    {
        Categoria = request.Categoria.Trim(),
        Nome      = request.Nome.Trim(),
        Preco     = request.Preco,
        Descricao = request.Descricao?.Trim() ?? "",
        Ativo     = request.Ativo
    });

    return atualizado ? Results.Ok(new { mensagem = "Produto atualizado." }) : Results.NotFound();
}).RequireAuthorization();

// DELETE /admin/cardapio/{id} — remove um item do cardápio
app.MapDelete("/admin/cardapio/{id:int}", (int id, ICardapioRepository repository) =>
{
    return repository.Excluir(id)
        ? Results.Ok(new { mensagem = "Produto excluído." })
        : Results.NotFound();
}).RequireAuthorization();

// ──────────────────────────────────────────────
// ROTAS DE PEDIDOS
// ──────────────────────────────────────────────

// POST /pedido — recebe um novo pedido do cliente (acesso público)
app.MapPost("/pedido", (CriarPedidoRequest request, IPedidoRepository pedidos, ICardapioRepository cardapio) =>
{
    try
    {
        if (request.Itens.Count == 0)
        {
            return Results.BadRequest(new { erro = "Selecione pelo menos um item." });
        }

        // Cria o pedido com os dados do cliente
        var pedido = new Pedido(
            0,
            new Cliente(request.Nome, request.Telefone),
            ParseTipoEntrega(request.TipoEntrega),
            ParseFormaPagamento(request.FormaPagamento),
            request.Endereco,
            request.Observacao);

        // Valida os itens contra o cardápio ativo para evitar itens inválidos
        var produtosAtivos = cardapio.Listar(apenasAtivos: true)
            .ToDictionary(p => p.Nome, StringComparer.OrdinalIgnoreCase);

        foreach (var item in request.Itens)
        {
            if (!produtosAtivos.TryGetValue(item.Nome, out var produto)) continue;
            pedido.AdicionarItem(new ItemPedido(produto.Nome, item.Quantidade, produto.Preco));
        }

        if (!pedido.Itens.Any())
        {
            return Results.BadRequest(new { erro = "Os itens selecionados não estão disponíveis." });
        }

        pedidos.Salvar(pedido);
        return Results.Created($"/pedido/{pedido.Id}", new { mensagem = "Pedido salvo com sucesso!", id = pedido.Id });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
});

// GET /pedidos — lista todos os pedidos para o painel do administrador
app.MapGet("/pedidos", (IPedidoRepository repository) =>
{
    return Results.Ok(repository.Listar().Select(pedido => new
    {
        id             = pedido.Id,
        nome           = pedido.Cliente.Nome,
        telefone       = pedido.Cliente.Telefone,
        total          = pedido.Total,
        observacao     = pedido.Observacao,
        endereco       = pedido.Endereco,
        tipo_entrega   = ToApiValueTipoEntrega(pedido.TipoEntrega),
        forma_pagamento = ToApiValueFormaPagamento(pedido.FormaPagamento),
        status         = ToApiValueStatus(pedido.Status),
        criado_em      = pedido.CriadoEm,
        itens          = pedido.Itens.Select(item => new
        {
            produto    = item.Produto,
            quantidade = item.Quantidade,
            preco_unit = item.PrecoUnitario
        })
    }));
}).RequireAuthorization();

// PUT /pedido/{id} — atualiza o status de um pedido (novo, preparando, finalizado)
app.MapPut("/pedido/{id:int}", (int id, AtualizarStatusRequest request, IPedidoRepository repository) =>
{
    if (!Enum.TryParse<StatusPedido>(request.Status, ignoreCase: true, out var status))
    {
        return Results.BadRequest(new { erro = "Status inválido." });
    }

    repository.AtualizarStatus(id, status);
    return Results.Ok(new { mensagem = "Status atualizado." });
}).RequireAuthorization();

// DELETE /pedido/{id} — remove um pedido do sistema
app.MapDelete("/pedido/{id:int}", (int id, IPedidoRepository repository) =>
{
    repository.Excluir(id);
    return Results.Ok(new { mensagem = "Pedido excluído." });
}).RequireAuthorization();

app.Run();

// ──────────────────────────────────────────────
// FUNÇÕES AUXILIARES DE CONVERSÃO
// ──────────────────────────────────────────────

// Converte a string recebida pela API para o enum TipoEntrega
static TipoEntrega ParseTipoEntrega(string value) => value.ToLowerInvariant() switch
{
    "entrega"  => TipoEntrega.Entrega,
    "retirada" => TipoEntrega.Retirada,
    _ => throw new ArgumentException("Tipo de pedido inválido.")
};

// Converte a string recebida pela API para o enum FormaPagamento
static FormaPagamento ParseFormaPagamento(string value) => value.ToLowerInvariant() switch
{
    "pix"      => FormaPagamento.Pix,
    "dinheiro" => FormaPagamento.Dinheiro,
    "cartao"   => FormaPagamento.Cartao,
    _ => throw new ArgumentException("Forma de pagamento inválida.")
};

// Converte StatusPedido para string usada na API e no front-end
static string ToApiValueStatus(StatusPedido status) => status switch
{
    StatusPedido.Novo       => "novo",
    StatusPedido.Preparando => "preparando",
    StatusPedido.Finalizado => "finalizado",
    _ => "novo"
};

// Converte TipoEntrega para string usada na API
static string ToApiValueTipoEntrega(TipoEntrega tipo) =>
    tipo == TipoEntrega.Entrega ? "entrega" : "retirada";

// Converte FormaPagamento para string usada na API
static string ToApiValueFormaPagamento(FormaPagamento forma) => forma switch
{
    FormaPagamento.Pix      => "pix",
    FormaPagamento.Dinheiro => "dinheiro",
    FormaPagamento.Cartao   => "cartao",
    _ => "pix"
};