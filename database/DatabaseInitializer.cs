// Responsável por inicializar o banco de dados SQLite do Restaurante do Bigode.
// Cria as tabelas necessárias caso não existam e popula o cardápio inicial.

using Microsoft.Data.Sqlite;

namespace RestaurantOrderSystem.Database;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();

        // Tabela de produtos do cardápio
        // Ativo = 1 significa disponível; Ativo = 0 significa oculto no pedido online
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Cardapio (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Categoria   TEXT    NOT NULL,
                Nome        TEXT    NOT NULL,
                Preco       REAL    NOT NULL,
                Descricao   TEXT    NOT NULL DEFAULT '',
                Ativo       INTEGER NOT NULL DEFAULT 1
            );
        """;
        cmd.ExecuteNonQuery();

        // Tabela de pedidos — armazena os dados do cliente e do pedido
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Pedidos (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                ClienteNome     TEXT    NOT NULL,
                ClienteTelefone TEXT    NOT NULL,
                TipoEntrega     TEXT    NOT NULL,
                FormaPagamento  TEXT    NOT NULL,
                Endereco        TEXT,
                Observacao      TEXT,
                Status          TEXT    NOT NULL DEFAULT 'Novo',
                CriadoEm        TEXT    NOT NULL
            );
        """;
        cmd.ExecuteNonQuery();

        // Tabela de itens do pedido — relacionada à tabela Pedidos via chave estrangeira
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS ItensPedido (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                PedidoId        INTEGER NOT NULL,
                Produto         TEXT    NOT NULL,
                Quantidade      INTEGER NOT NULL,
                PrecoUnitario   REAL    NOT NULL,
                FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id)
            );
        """;
        cmd.ExecuteNonQuery();

        // Verifica se o cardápio já foi populado para não duplicar os dados
        cmd.CommandText = "SELECT COUNT(*) FROM Cardapio;";
        var count = (long)(cmd.ExecuteScalar() ?? 0);

        if (count == 0)
        {
            // Insere o cardápio inicial do Restaurante do Bigode
            cmd.CommandText = """
                INSERT INTO Cardapio (Categoria, Nome, Preco, Descricao) VALUES
                ('Todos os dias', 'Bife',                 23, 'Arroz, feijão e batata frita'),
                ('Todos os dias', 'Bisteca',              23, 'Arroz, feijão e batata frita'),
                ('Todos os dias', 'Calabresa',            23, 'Arroz, feijão e batata frita'),
                ('Todos os dias', 'Filé de frango',       23, 'Arroz, feijão e batata frita'),
                ('Pratos do dia', 'Virada à Paulista',    26, 'Arroz, tutu, couve, torresmo, ovo e bisteca'),
                ('Pratos do dia', 'Costela cozida',       26, 'Arroz, feijão e farofa'),
                ('Pratos do dia', 'Strogonoff de frango', 26, 'Arroz e batata frita'),
                ('Especiais',     'Bife com ovo',         26, 'Arroz, feijão e batata frita'),
                ('Especiais',     'Filé à parmegiana',    26, 'Arroz, feijão e batata frita'),
                ('Vegetarianos',  'Omelete',              26, 'Arroz, feijão e batata frita'),
                ('Bebidas',       'Refrigerante lata',     6, 'Lata 350ml'),
                ('Bebidas',       'Refrigerante 2L',      15, 'Garrafa 2 litros');
            """;
            cmd.ExecuteNonQuery();
        }
    }
}