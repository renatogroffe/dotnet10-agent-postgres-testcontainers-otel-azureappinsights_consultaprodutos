using ConsoleAppChatAIProdutos.Data;
using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace ConsoleAppChatAIProdutos.Plugins;

public static class ProdutosPlugin
{
    [Description("Retorne uma lista com nomes de produtos, além de seus respectivos códigos de barra e preços.")]
    
    [return: Description("Uma lista com nomes, código de barras e preços de produtos")]
    public static async Task<List<Produto>> GetProdutos()
    {
        return await Task.Run(() => ProdutosRepository.ListAll());
    }

    [Description("Retorne o nome de um Produto a partir de seu Código de Barras.")]
    [return: Description("Nome do Produto")]
    public static async Task<string?> GetProdutoByCodigoBarras(
        [Description("Código de Barras")] string codigoBarras)
    {
        return await Task.Run(() => ProdutosRepository.GetProdutoByCodigoBarras(codigoBarras)?.Nome);
    }

    [Description("Retorne o Preço Médio dos Produtos que compõem o catálogo.")]
    [return: Description("Preço Médio dos Produtos")]
    public static async Task<decimal> GetPrecoMedioProdutos()
    {
        return await Task.Run(() => ProdutosRepository.GetPrecoMedio());
    }

    [Description("Retorne informações do(s) Produto(s) com o menor Preço.")]
    [return: Description("Uma lista com nomes, código de barras e valores dos produtos de menor preço")]
    public static async Task<List<Produto>> GetProdutosComMenorPreco()
    {
        return await Task.Run(() => ProdutosRepository.GetProdutosComMenorPreco());
    }

    [Description("Retorne informações do(s) Produto(s) com o maior Preço.")]
    [return: Description("Uma lista com nomes, código de barras e valores dos produtos de maior preço")]
    public static async Task<List<Produto>> GetProdutosComMaiorPreco()
    {
        return await Task.Run(() => ProdutosRepository.GetProdutosComMaiorPreco());
    }

    public static AIFunction[] GetFunctions() => [
        AIFunctionFactory.Create(GetProdutos),
        AIFunctionFactory.Create(GetProdutoByCodigoBarras),
        AIFunctionFactory.Create(GetPrecoMedioProdutos),
        AIFunctionFactory.Create(GetProdutosComMenorPreco),
        AIFunctionFactory.Create(GetProdutosComMaiorPreco)
    ];
}