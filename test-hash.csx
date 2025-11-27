#!/usr/bin/env dotnet script
#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

string adminHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
string bankHash = BCrypt.Net.BCrypt.HashPassword("Bank123!");

Console.WriteLine("=== Correct BCrypt Hashes ===");
Console.WriteLine($"Admin hash: {adminHash}");
Console.WriteLine($"Bank hash: {bankHash}");
Console.WriteLine();
Console.WriteLine("=== SQL Update Statements ===");
Console.WriteLine($"UPDATE Users SET PasswordHash = '{adminHash}' WHERE Username = 'admin';");
Console.WriteLine($"UPDATE Users SET PasswordHash = '{bankHash}' WHERE Username = 'bankapi';");
