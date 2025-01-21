// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        string passwordToMatch = "!97132977Michaei";

        byte[] salt = RandomNumberGenerator.GetBytes(16); 
        string hashedPassword = HashPassword(passwordToMatch, salt);

        Console.WriteLine($"Hash generado para la contraseña '!97132977Michaei': {hashedPassword}");

        Thread verificationThread = new Thread(() => VerifyPasswords("C:\\Users\\dani0\\RiderProjects\\ConsoleApp1\\FuerzaBrutaPrueba\\2151220-passwords.txt", hashedPassword, salt));
        verificationThread.Start();
        verificationThread.Join();
    }

    static string HashPassword(string password, byte[] salt)
    {
        int iterations = 10000;

        using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
        {
            byte[] hash = deriveBytes.GetBytes(32);
            return Convert.ToBase64String(hash);
        }
    }

    static void VerifyPasswords(string filePath, string hashedPassword, byte[] salt)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"El archivo {filePath} no existe.");
                return;
            }

            string[] passwords = File.ReadAllLines(filePath);

            foreach (string password in passwords)
            {
                Console.WriteLine(password);
                string hashedInput = HashPassword(password, salt);
                Console.WriteLine(hashedInput);
                if (hashedInput == hashedPassword)
                {
                    Console.WriteLine($"\nContraseña encontrada: {password}");
                    return;
                }
            }

            Console.WriteLine("Ninguna contraseña coincide con el hash.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar contraseñas: {ex.Message}");
        }
    }
}


