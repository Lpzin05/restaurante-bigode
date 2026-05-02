// Representa um produto disponível no cardápio do Restaurante do Bigode.
// Ativo = true significa que o item aparece na tela de pedido do cliente.

namespace RestaurantOrderSystem.Models;

public class ProdutoCardapio
{
    public int Id { get; set; }
    public string Categoria { get; set; } = "";
    public string Nome { get; set; } = "";
    public decimal Preco { get; set; }
    public string Descricao { get; set; } = "";

    // Quando falso, o item fica oculto para o cliente mas não é excluído do banco
    public bool Ativo { get; set; } = true;
}