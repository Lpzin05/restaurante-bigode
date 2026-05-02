// Define as formas de pagamento aceitas pelo Restaurante do Bigode.

namespace RestaurantOrderSystem.Models;

public enum FormaPagamento
{
    Pix,      // Pagamento via chave PIX
    Dinheiro, // Pagamento em dinheiro na entrega ou balcão
    Cartao    // Pagamento com cartão de débito ou crédito
}