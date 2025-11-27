using System;

public class GenerateHash
{
    public static void Main()
    {
        string adminHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        string bankHash = BCrypt.Net.BCrypt.HashPassword("Bank123!");

        Console.WriteLine("=== SQL Insert Statements ===");
        Console.WriteLine($"INSERT INTO Users (Username, PasswordHash, Role, CreatedAt) VALUES ('admin', '{adminHash}', 'Admin', GETUTCDATE());");
        Console.WriteLine($"INSERT INTO Users (Username, PasswordHash, Role, CreatedAt) VALUES ('bankapi', '{bankHash}', 'BankingSystem', GETUTCDATE());");
    }
}
