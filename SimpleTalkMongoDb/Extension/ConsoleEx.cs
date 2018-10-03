using System;

public static class ConsoleEx
{
    public static void WriteLine(string message, ConsoleColor cc = ConsoleColor.White,
        ConsoleColor bc = ConsoleColor.Black)
    {

        var oldColor = Console.ForegroundColor;
        var oldBack = Console.BackgroundColor;
        Console.ForegroundColor = cc;
        Console.BackgroundColor = bc;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;
        Console.BackgroundColor = oldBack;
    }
}