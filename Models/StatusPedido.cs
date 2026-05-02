// Define os possíveis estados de um pedido ao longo do seu ciclo de vida.
// O painel do administrador usa esses valores para mover os pedidos entre colunas.

namespace RestaurantOrderSystem.Models;

public enum StatusPedido
{
    Novo,        // Pedido recém recebido, aguardando preparo
    Preparando,  // Pedido em preparo na cozinha
    Finalizado   // Pedido pronto para entrega ou retirada
}