// Define se o pedido será entregue no endereço do cliente ou retirado no local.

namespace RestaurantOrderSystem.Models;

public enum TipoEntrega
{
    Entrega,   // O restaurante leva até o cliente
    Retirada   // O cliente retira no balcão
}