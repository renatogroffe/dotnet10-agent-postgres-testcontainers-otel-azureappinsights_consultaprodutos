using Sharprompt;

namespace ConsoleAppChatAIProdutos.Inputs;

public class InputHelper
{
    public static int GetNumberOfNewProducts()
    {
        var answer = Prompt.Select<int>(options =>
        {
            options.Message = "Selecione a quantidade de produtos novos a serem criados";
            options.Items = [5, 10, 15, 20];
        });
        Console.WriteLine();
        return answer;
    }
}