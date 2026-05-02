// Representa o cliente que realiza um pedido no Restaurante do Bigode.
// Herda os atributos Nome e Telefone da classe abstrata Pessoa,
// demonstrando o uso de herança na Programação Orientada a Objetos.

namespace RestaurantOrderSystem.Models;

public class Cliente : Pessoa
{
    public Cliente(string nome, string telefone) : base(nome, telefone)
    {
    }
}