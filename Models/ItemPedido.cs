// Representa um item dentro de um pedido.
// Encapsula produto, quantidade e preço unitário,
// calculando o subtotal automaticamente.

namespace RestaurantOrderSystem.Models;

public class ItemPedido
{
    public string Produto { get; private set; }
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }

    // Subtotal calculado automaticamente — evita inconsistências manuais
    public decimal Subtotal => Quantidade * PrecoUnitario;

    public ItemPedido(string produto, int quantidade, decimal precoUnitario)
    {
        if (string.IsNullOrWhiteSpace(produto))
            throw new ArgumentException("Produto é obrigatório.");
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.");
        if (precoUnitario <= 0)
            throw new ArgumentException("Preço deve ser maior que zero.");

        Produto        = produto.Trim();
        Quantidade     = quantidade;
        PrecoUnitario  = precoUnitario;
    }
}