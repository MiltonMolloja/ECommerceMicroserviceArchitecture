using Microsoft.AspNetCore.Identity;

class Program
{
    static void Main(string[] args)
    {
        var password = "Pa$$w0rd!";
        
        if (args.Length > 0)
        {
            password = args[0];
        }

        var hasher = new PasswordHasher<object>();
        var hash = hasher.HashPassword(null!, password);
        
        Console.WriteLine("===========================================");
        Console.WriteLine("  PASSWORD HASH GENERATOR");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        Console.WriteLine($"Password: {password}");
        Console.WriteLine();
        Console.WriteLine("Hash generado:");
        Console.WriteLine(hash);
        Console.WriteLine();
        Console.WriteLine("===========================================");
    }
}
