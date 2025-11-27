using System;

class Program
{
    static void Main()
    {
        // Generate BCrypt hashes for the passwords
        string adminPassword = "Admin123!";
        string bankPassword = "Bank123!";

        string adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
        string bankHash = BCrypt.Net.BCrypt.HashPassword(bankPassword);

        Console.WriteLine("Admin password hash:");
        Console.WriteLine(adminHash);
        Console.WriteLine();
        Console.WriteLine("Bank password hash:");
        Console.WriteLine(bankHash);
        Console.WriteLine();
        Console.WriteLine("SQL Insert statements:");
        Console.WriteLine($"INSERT INTO Users (Username, PasswordHash, Role, CreatedAt) VALUES ('admin', '{adminHash}', 'Admin', GETUTCDATE());");
        Console.WriteLine($"INSERT INTO Users (Username, PasswordHash, Role, CreatedAt) VALUES ('bankapi', '{bankHash}', 'BankingSystem', GETUTCDATE());");
    }
}
