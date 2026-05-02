// Implementação do repositório de cardápio usando SQLite.
// Realiza as operações de leitura e escrita na tabela Cardapio do banco de dados.

using Microsoft.Data.Sqlite;
using RestaurantOrderSystem.Models;

namespace RestaurantOrderSystem.Repositories;

public class SqliteCardapioRepository : ICardapioRepository
{
    private readonly string _connectionString;

    public SqliteCardapioRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Lista os produtos do cardápio; filtra apenas ativos se solicitado
    public IReadOnlyCollection<ProdutoCardapio> Listar(bool apenasAtivos)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = apenasAtivos
            ? "SELECT Id, Categoria, Nome, Preco, Descricao, Ativo FROM Cardapio WHERE Ativo = 1 ORDER BY Categoria, Nome;"
            : "SELECT Id, Categoria, Nome, Preco, Descricao, Ativo FROM Cardapio ORDER BY Categoria, Nome;";

        var lista = new List<ProdutoCardapio>();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new ProdutoCardapio
            {
                Id        = reader.GetInt32(0),
                Categoria = reader.GetString(1),
                Nome      = reader.GetString(2),
                Preco     = (decimal)reader.GetDouble(3),
                Descricao = reader.GetString(4),
                Ativo     = reader.GetInt32(5) == 1
            });
        }

        return lista;
    }

    // Busca um produto pelo Id; retorna null se não existir
    public ProdutoCardapio? Obter(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Categoria, Nome, Preco, Descricao, Ativo FROM Cardapio WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return new ProdutoCardapio
        {
            Id        = reader.GetInt32(0),
            Categoria = reader.GetString(1),
            Nome      = reader.GetString(2),
            Preco     = (decimal)reader.GetDouble(3),
            Descricao = reader.GetString(4),
            Ativo     = reader.GetInt32(5) == 1
        };
    }

    // Insere um novo produto e retorna o objeto com o Id gerado pelo banco
    public ProdutoCardapio Criar(ProdutoCardapio produto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Cardapio (Categoria, Nome, Preco, Descricao, Ativo)
            VALUES ($categoria, $nome, $preco, $descricao, $ativo);
            SELECT last_insert_rowid();
        """;
        cmd.Parameters.AddWithValue("$categoria", produto.Categoria);
        cmd.Parameters.AddWithValue("$nome",      produto.Nome);
        cmd.Parameters.AddWithValue("$preco",     (double)produto.Preco);
        cmd.Parameters.AddWithValue("$descricao", produto.Descricao);
        cmd.Parameters.AddWithValue("$ativo",     produto.Ativo ? 1 : 0);

        produto.Id = Convert.ToInt32(cmd.ExecuteScalar());
        return produto;
    }

    // Atualiza os dados de um produto existente; retorna false se não encontrar
    public bool Atualizar(int id, ProdutoCardapio produto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Cardapio
            SET Categoria = $categoria, Nome = $nome, Preco = $preco,
                Descricao = $descricao, Ativo = $ativo
            WHERE Id = $id;
        """;
        cmd.Parameters.AddWithValue("$id",        id);
        cmd.Parameters.AddWithValue("$categoria", produto.Categoria);
        cmd.Parameters.AddWithValue("$nome",      produto.Nome);
        cmd.Parameters.AddWithValue("$preco",     (double)produto.Preco);
        cmd.Parameters.AddWithValue("$descricao", produto.Descricao);
        cmd.Parameters.AddWithValue("$ativo",     produto.Ativo ? 1 : 0);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Remove um produto pelo Id; retorna false se não encontrar
    public bool Excluir(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Cardapio WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        return cmd.ExecuteNonQuery() > 0;
    }
}