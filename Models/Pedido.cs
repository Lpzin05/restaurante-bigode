// Representa um pedido realizado no Restaurante do Bigode.
// Agrupa os dados do cliente, itens escolhidos, forma de pagamento e status atual.

namespace RestaurantOrderSystem.Models;

public class Pedido
{
    // Lista interna de itens — não exposta diretamente para proteger a integridade dos dados
    private readonly List<ItemPedido> _itens = [];

    public int Id { get; private set; }
    public Cliente Cliente { get; private set; }
    public TipoEntrega TipoEntrega { get; private set; }
    public string? Endereco { get; private set; }
    public FormaPagamento FormaPagamento { get; private set; }
    public string? Observacao { get; private set; }
    public StatusPedido Status { get; private set; }
    public DateTime CriadoEm { get; private set; }

    // Expõe os itens como somente leitura para evitar alterações externas
    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

    // Calcula o total do pedido somando os subtotais de cada item
    public decimal Total => _itens.Sum(item => item.Subtotal);

    // Construtor principal — usado ao criar um novo pedido vindo do cliente
    public Pedido(
        int id,
        Cliente cliente,
        TipoEntrega tipoEntrega,
        FormaPagamento formaPagamento,
        string? endereco,
        string? observacao)
    {
        // Endereço é obrigatório quando o tipo de entrega é domicílio
        if (tipoEntrega == TipoEntrega.Entrega && string.IsNullOrWhiteSpace(endereco))
        {
            throw new ArgumentException("Endereço é obrigatório para entrega.");
        }

        Id             = id;
        Cliente        = cliente;
        TipoEntrega    = tipoEntrega;
        FormaPagamento = formaPagamento;
        Endereco       = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();
        Observacao     = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        Status         = StatusPedido.Novo;
        CriadoEm      = DateTime.Now;
    }

    // Construtor secundário — usado ao reconstruir um pedido já salvo no banco de dados
    public Pedido(
        int id,
        Cliente cliente,
        TipoEntrega tipoEntrega,
        FormaPagamento formaPagamento,
        string? endereco,
        string? observacao,
        DateTime criadoEm)
    {
        Id             = id;
        Cliente        = cliente;
        TipoEntrega    = tipoEntrega;
        FormaPagamento = formaPagamento;
        Endereco       = endereco;
        Observacao     = observacao;
        Status         = StatusPedido.Novo;
        CriadoEm      = criadoEm;
    }

    // Adiciona um item à lista do pedido
    public void AdicionarItem(ItemPedido item) => _itens.Add(item);

    // Altera o status do pedido (ex: de Novo para Preparando)
    public void AlterarStatus(StatusPedido status) => Status = status;
}