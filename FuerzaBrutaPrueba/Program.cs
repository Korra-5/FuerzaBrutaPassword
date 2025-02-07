using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

class Program
{
    //Variables estaticas
    static Thread[] threads;
    static volatile bool found = false;
    static byte[] salt = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

    static void Main(string[] args)
    {
        string passwordToMatch = "";
        string hashedPassword = "";

        string opcion;
        do
        {
            Console.WriteLine("¿Va a insertar la contraseña hasheada o sin hashear?");
            Console.WriteLine("1- Sin hashear");
            Console.WriteLine("2- Hasheada");
            opcion = Console.ReadLine().Trim();

            if (opcion != "1" && opcion != "2")
            {
                Console.WriteLine("Opción no válida. Intente de nuevo.");
            }

        } while (opcion != "1" && opcion != "2");

        do
        {
            Console.Write("Ingrese la contraseña: ");
            passwordToMatch = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(passwordToMatch))
            {
                Console.WriteLine("La contraseña no puede estar vacía.");
            }

        } while (string.IsNullOrEmpty(passwordToMatch));

        if (opcion == "1")
        {
            //Hashea la contraseña en caso de que no este hasheada
            hashedPassword = HashPassword(passwordToMatch, salt);
            Console.WriteLine($"Hash generado para la contraseña: {hashedPassword}");
        }
        else
        {
            hashedPassword = passwordToMatch;
        }

        int numThreads;
        do
        {
            Console.Write("Ingrese el número de hilos a usar: ");
            string input = Console.ReadLine();

            //Comprobación de que el dato sea un Int
            if (!int.TryParse(input, out numThreads) || numThreads <= 0)
            {
                Console.WriteLine("Debe ingresar un número entero positivo.");
            }

        } while (numThreads <= 0);
        
        threads = new Thread[numThreads];

        string filePath = "2151220-passwords.txt";
        string[][] dividedPasswords = DividePasswords(filePath, numThreads);

        //Inicializar los hilos
        for (int i = 0; i < numThreads; i++)
        {
            int num = i;
            threads[i] = new Thread(() => VerifyPasswords(dividedPasswords[num], hashedPassword, salt));
            threads[i].Start();
        }

        //Esperar a que acaben antes de seguir el codigo
        foreach (var thread in threads)
            thread.Join();
    }

    //Metodo para hashear la contraseña
    static string HashPassword(string password, byte[] salt)
    {
        int iterations = 10000;
        using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
        {
            byte[] hash = deriveBytes.GetBytes(32);
            return Convert.ToBase64String(hash);
        }
    }

    //Metodo que divide las contraseñas
    static string[][] DividePasswords(string filePath, int numThreads)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"El archivo {filePath} no existe.");
            return new string[numThreads][];
        }

        string[] passwords = File.ReadAllLines(filePath);
        int total = passwords.Length;
        int partSize = total / numThreads;

        string[][] dividedPasswords = new string[numThreads][];
        for (int i = 0; i < numThreads - 1; i++)
            dividedPasswords[i] = passwords[(i * partSize)..((i + 1) * partSize)];

        dividedPasswords[numThreads - 1] = passwords[((numThreads - 1) * partSize)..];

        return dividedPasswords;
    }

    //Metodo para hashar las contraseñas
    static void VerifyPasswords(string[] passwords, string hashedPassword, byte[] salt)
    {
        foreach (string password in passwords)
        {
            if (found) return;

            string hashedInput = HashPassword(password, salt);
            Console.WriteLine(password);

            if (hashedInput == hashedPassword)
            {
                Console.WriteLine($"\nContraseña encontrada: {password}");
                StopAllThreads();
                return;
            }
        }
    }

    static void StopAllThreads()
    {
        found = true;
        foreach (var thread in threads)
        {
            if (thread.IsAlive)
                thread.Interrupt();
        }
    }
}
