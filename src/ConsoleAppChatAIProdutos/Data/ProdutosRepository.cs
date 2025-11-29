namespace ConsoleAppChatAIProdutos.Data;

public static class ProdutosRepository
{
    public static List<Produto> ListAll()
    {
        var context = new CatalogoContext();
        return context.Produtos!.ToList();
    }

    public static Produto? GetProdutoByCodigoBarras(string codigoBarras)
    {
        var context = new CatalogoContext();
        return context.Produtos!.Where(p => p.CodigoBarras == codigoBarras).FirstOrDefault();
    }

    public static decimal GetPrecoMedio()
    {
        var context = new CatalogoContext();
        return context.Produtos!.Average(p => p.Preco);
    }

    public static List<Produto> GetProdutosComMenorPreco()
    {
        var context = new CatalogoContext();
        if (!context.Produtos!.Any())
            return new List<Produto>();

        var menorPreco = context.Produtos!.Min(p => p.Preco);
        return context.Produtos!.Where(p => p.Preco == menorPreco).ToList();
    }

    public static List<Produto> GetProdutosComMaiorPreco()
    {
        var context = new CatalogoContext();
        if (!context.Produtos!.Any())
            return new List<Produto>();

        var maiorPreco = context.Produtos!.Max(p => p.Preco);
        return context.Produtos!.Where(p => p.Preco == maiorPreco).ToList();
    }
}