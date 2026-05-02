// DTOs (Data Transfer Objects) — estruturas usadas para receber dados via API.
// Separam os dados de entrada da lógica interna do sistema,
// evitando que o front-end precise conhecer os modelos internos.

using System.Text.Json.Serialization;

namespace RestaurantOrderSystem.Dtos;

// Dados necessários para criar um novo pedido
public record CriarPedidoRequest(
    string Nome,
    string Telefone,
    [property: JsonPropertyName("tipo_entrega")]    string TipoEntrega,
    string? Endereco,
    [property: JsonPropertyName("forma_pagamento")] string FormaPagamento,
    string? Observacao,
    List<ItemPedidoRequest> Itens);

// Representa um item dentro da requisição de pedido
public record ItemPedidoRequest(string Nome, int Quantidade, decimal Preco);

// Dados para atualizar o status de um pedido
public record AtualizarStatusRequest(string Status);

// Dados de login do administrador
public record LoginRequest(string Usuario, string Senha);

// Dados para criar ou atualizar um produto do cardápio
public record ProdutoCardapioRequest(string Categoria, string Nome, decimal Preco, string Descricao, bool Ativo);