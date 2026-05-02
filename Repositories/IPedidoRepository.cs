// Interface que define o contrato de acesso aos dados de pedidos.
// A separação em interface permite que o sistema não dependa diretamente
// de um banco específico, facilitando manutenção e testes futuros.

using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Repositories;

public interface IPedidoRepository
{
    // Retorna o próximo Id disponível (usado como placeholder antes de salvar)
    int ProximoId();

    // Persiste um novo pedido e seus itens no banco de dados
    void Salvar(Pedido pedido);

    // Retorna todos os pedidos ordenados do mais recente para o mais antigo
    IReadOnlyCollection<Pedido> Listar();

    // Atualiza o status de um pedido (Novo, Preparando, Finalizado)
    void AtualizarStatus(int id, StatusPedido status);

    // Remove um pedido e seus itens do banco de dados
    void Excluir(int id);
}