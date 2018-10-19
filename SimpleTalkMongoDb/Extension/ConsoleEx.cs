using System;

public static class ConsoleEx
{
    public static void WriteLine(string message, ConsoleColor cc = ConsoleColor.White,
        ConsoleColor bc = ConsoleColor.Black)
    {

        Console.ForegroundColor = cc;
        Console.BackgroundColor = bc;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}