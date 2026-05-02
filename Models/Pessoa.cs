// Classe base abstrata que representa uma pessoa no sistema.
// Aplica o conceito de herança da Programação Orientada a Objetos:
// outras classes como Cliente podem herdar seus atributos comuns.

namespace RestaurantOrderSystem.Models;

public abstract class Pessoa
{
    public string Nome { get; protected set; }
    public string Telefone { get; protected set; }

    protected Pessoa(string nome, string telefone)
    {
        // Valida que nome e telefone não sejam vazios antes de criar o objeto
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone é obrigatório.");

        Nome     = nome.Trim();
        Telefone = telefone.Trim();
    }
}