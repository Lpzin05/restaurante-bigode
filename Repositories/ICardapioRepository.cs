// Interface que define o contrato de acesso aos dados do cardápio.
// Usar interfaces permite trocar a implementação (ex: SQLite por outro banco)
// sem precisar alterar o restante do sistema — princípio da inversão de dependência.

using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Repositories;

public interface ICardapioRepository
{
    // Retorna todos os produtos; se apenasAtivos = true, filtra somente os disponíveis
    IReadOnlyCollection<ProdutoCardapio> Listar(bool apenasAtivos);

    // Busca um produto específico pelo seu Id
    ProdutoCardapio? Obter(int id);

    // Cadastra um novo produto e retorna o objeto com o Id gerado pelo banco
    ProdutoCardapio Criar(ProdutoCardapio produto);

    // Atualiza os dados de um produto existente; retorna false se não encontrar
    bool Atualizar(int id, ProdutoCardapio produto);

    // Remove um produto pelo Id; retorna false se não encontrar
    bool Excluir(int id);
}