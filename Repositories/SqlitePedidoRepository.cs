// Implementação do repositório de pedidos usando SQLite.
// Utiliza transação ao salvar para garantir que pedido e itens
// sejam gravados juntos — se um falhar, nenhum é salvo.

using Microsoft.Data.Sqlite;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Repositories;

public class SqlitePedidoRepository : IPedidoRepository
{
    private readonly string _connectionString;

    public SqlitePedidoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // O Id é gerado automaticamente pelo SQLite (AUTOINCREMENT)
    public int ProximoId() => 0;

    // Salva o pedido e seus itens em uma única transação
    // Isso garante consistência: ou tudo é salvo ou nada é salvo
    public void Salvar(Pedido pedido)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // Insere o pedido principal e obtém o Id gerado pelo banco
        using var cmdPedido = connection.CreateCommand();
        cmdPedido.CommandText = """
            INSERT INTO Pedidos (ClienteNome, ClienteTelefone, TipoEntrega, FormaPagamento, Endereco, Observacao, Status, CriadoEm)
            VALUES ($nome, $telefone, $tipoEntrega, $formaPagamento, $endereco, $observacao, $status, $criadoEm);
            SELECT last_insert_rowid();
        """;
        cmdPedido.Parameters.AddWithValue("$nome",           pedido.Cliente.Nome);
        cmdPedido.Parameters.AddWithValue("$telefone",       pedido.Cliente.Telefone);
        cmdPedido.Parameters.AddWithValue("$tipoEntrega",    pedido.TipoEntrega.ToString());
        cmdPedido.Parameters.AddWithValue("$formaPagamento", pedido.FormaPagamento.ToString());
        cmdPedido.Parameters.AddWithValue("$endereco",       pedido.Endereco ?? (object)DBNull.Value);
        cmdPedido.Parameters.AddWithValue("$observacao",     pedido.Observacao ?? (object)DBNull.Value);
        cmdPedido.Parameters.AddWithValue("$status",         pedido.Status.ToString());
        cmdPedido.Parameters.AddWithValue("$criadoEm",       pedido.CriadoEm.ToString("o"));

        var pedidoId = Convert.ToInt32(cmdPedido.ExecuteScalar());

        // Insere cada item do pedido vinculado ao Id do pedido
        foreach (var item in pedido.Itens)
        {
            using var cmdItem = connection.CreateCommand();
            cmdItem.CommandText = """
                INSERT INTO ItensPedido (PedidoId, Produto, Quantidade, PrecoUnitario)
                VALUES ($pedidoId, $produto, $quantidade, $preco);
            """;
            cmdItem.Parameters.AddWithValue("$pedidoId",   pedidoId);
            cmdItem.Parameters.AddWithValue("$produto",    item.Produto);
            cmdItem.Parameters.AddWithValue("$quantidade", item.Quantidade);
            cmdItem.Parameters.AddWithValue("$preco",      (double)item.PrecoUnitario);
            cmdItem.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    // Busca todos os pedidos e seus itens; ordena do mais recente para o mais antigo
    public IReadOnlyCollection<Pedido> Listar()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmdPedidos = connection.CreateCommand();
        cmdPedidos.CommandText = """
            SELECT Id, ClienteNome, ClienteTelefone, TipoEntrega, FormaPagamento,
                   Endereco, Observacao, Status, CriadoEm
            FROM Pedidos
            ORDER BY CriadoEm DESC;
        """;

        var pedidos = new List<Pedido>();
        using var reader = cmdPedidos.ExecuteReader();

        while (reader.Read())
        {
            var id             = reader.GetInt32(0);
            var nome           = reader.GetString(1);
            var telefone       = reader.GetString(2);
            var tipoEntrega    = Enum.Parse<TipoEntrega>(reader.GetString(3));
            var formaPagamento = Enum.Parse<FormaPagamento>(reader.GetString(4));
            var endereco       = reader.IsDBNull(5) ? null : reader.GetString(5);
            var observacao     = reader.IsDBNull(6) ? null : reader.GetString(6);
            var status         = Enum.Parse<StatusPedido>(reader.GetString(7));
            var criadoEm       = DateTime.Parse(reader.GetString(8));

            // Reconstrói o objeto Pedido a partir dos dados do banco
            var pedido = new Pedido(id, new Cliente(nome, telefone), tipoEntrega, formaPagamento, endereco, observacao, criadoEm);
            pedido.AlterarStatus(status);

            // Busca os itens relacionados ao pedido atual
            using var cmdItens = connection.CreateCommand();
            cmdItens.CommandText = "SELECT Produto, Quantidade, PrecoUnitario FROM ItensPedido WHERE PedidoId = $id;";
            cmdItens.Parameters.AddWithValue("$id", id);

            using var readerItens = cmdItens.ExecuteReader();
            while (readerItens.Read())
            {
                pedido.AdicionarItem(new ItemPedido(
                    readerItens.GetString(0),
                    readerItens.GetInt32(1),
                    (decimal)readerItens.GetDouble(2)
                ));
            }

            pedidos.Add(pedido);
        }

        return pedidos;
    }

    // Atualiza apenas o campo Status do pedido
    public void AtualizarStatus(int id, StatusPedido status)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE Pedidos SET Status = $status WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$status", status.ToString());
        cmd.Parameters.AddWithValue("$id",     id);
        cmd.ExecuteNonQuery();
    }

    // Remove o pedido e todos os seus itens do banco
    public void Excluir(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Exclui primeiro os itens (chave estrangeira) antes de excluir o pedido
        using var cmdItens = connection.CreateCommand();
        cmdItens.CommandText = "DELETE FROM ItensPedido WHERE PedidoId = $id;";
        cmdItens.Parameters.AddWithValue("$id", id);
        cmdItens.ExecuteNonQuery();

        using var cmdPedido = connection.CreateCommand();
        cmdPedido.CommandText = "DELETE FROM Pedidos WHERE Id = $id;";
        cmdPedido.Parameters.AddWithValue("$id", id);
        cmdPedido.ExecuteNonQuery();
    }
}