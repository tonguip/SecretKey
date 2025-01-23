using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace SecretKey
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*const string KeyFileName = "secret.key";
            const int KeySize = 256;  // in bits, so 256 bits = 32 bytes*/

            int choice = 0;

            do
            {
                Console.Clear(); // Clear the console.

                // Display the menu.
                Console.WriteLine("Console Menu:");
                Console.WriteLine("1. GenerateSecretFile");
                Console.WriteLine("2. ReadSecretFile");
                Console.WriteLine("3. GenerateKey");
                Console.WriteLine("4. ValidateSerial");
                Console.WriteLine("5. Exit");

                Console.Write("\nEnter your choice: ");

                // Get the choice from the user.
                if (int.TryParse(Console.ReadLine(), out choice))
                {
                    // Execute the action based on the choice.
                    switch (choice)
                    {
                        case 1:
                            GenerateSecretFile();
                            break;

                        case 2:
                            ReadSecretFile();
                            break;

                        case 3:
                            GenerateKey();
                            break;

                        case 4:
                            ValidateSerial();
                            break;

                        case 5:
                            Console.WriteLine("\nExiting...");
                            break;

                        default:
                            Console.WriteLine("\nInvalid choice. Please select a valid option.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("\nInvalid input. Please enter a number.");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();

            } while (choice != 5);


            /*var secretKey = GenerateSecretKey.LoadSecretKeyFromFile(KeyFileName);

            string obfuscatedString = GenerateSecretKey.Obfuscate(secretKey);
            Console.WriteLine($"Obfuscated: {obfuscatedString}");

            string deObfuscatedString = GenerateSecretKey.DeObfuscate(obfuscatedString);
            Console.WriteLine($"De-Obfuscated: {deObfuscatedString}");

            Console.ReadLine();*/

            string ReadNotNullString(string text)
            {
                while (true)
                {
                    Console.WriteLine(text);
                    var line = Console.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                        return line;
                }
            }

            void GenerateSecretFile()
            {
                Console.Clear();
                Console.WriteLine("This program will generate a secret key file that can be used to encrypt and decrypt files.\n" +
                                  "The secret key file will be saved in the same directory as this program.\n" +
                                  "The secret key file should be kept secret and should not be shared with anyone.\n");
                int KeySize;
                string KeyFileName;

                int.TryParse(ReadNotNullString("Enter the key size in bits (128, 192, 256, etc): "), out KeySize);
                KeyFileName = ReadNotNullString("Enter the key file path and file name (C:\\Users\\keys\\MyKey.key) : ");

                try
                {
                    var secretKey = GenerateSecretKey.GenerateSecretKeyKUS(KeySize);

                    var obfuscatedKey = GenerateSecretKey.Obfuscate(secretKey);
                    File.WriteAllText(KeyFileName, obfuscatedKey);
                    Console.WriteLine($"Secret key file {KeyFileName} successfully created.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            void ReadSecretFile()
            {
                Console.Clear();
                Console.WriteLine("This program will read a secret key file that can be used to encrypt and decrypt files.\n" +
                                  "The secret key file will be saved in the same directory as this program.\n" +
                                  "The secret key file should be kept secret and should not be shared with anyone.\n" +
                                  "If the secret key file is lost, any files that are encrypted with the key file cannot be decrypted.\n");

                string KeyFileName;

                KeyFileName = ReadNotNullString("Enter the key file path and file name (C:\\Users\\keys\\MyKey.key) : ");

                try
                {
                    var obfuscatedKey = File.ReadAllText(KeyFileName);
                    var secretKey = GenerateSecretKey.DeObfuscate(obfuscatedKey);
                    Console.WriteLine($"\nSecret key file {KeyFileName} successfully read.");
                    Console.WriteLine($"Secret key: {secretKey}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
               
            }

            void GenerateKey()
            {
                Console.Clear();
                Console.WriteLine("This program will generate a secret key file that can be used to encrypt and decrypt files.\n" +
                                    "The secret key file will be saved in the same directory as this program.\n" +
                                    "The secret key file should be kept secret and should not be shared with anyone.\n" +
                                    "If the secret key file is lost, any files that are encrypted with the key file cannot be decrypted.\n");

                string KeyFileName;
                string secretKey;

                KeyFileName = ReadNotNullString("Enter the key file path and file name (C:\\Users\\keys\\MyKey.key) : ");

                try
                {
                    var obfuscatedKey = File.ReadAllText(KeyFileName);
                    secretKey = GenerateSecretKey.DeObfuscate(obfuscatedKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return;
                }

                // Get the UTC+7 time zone
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");

                // Convert the current UTC time to UTC+7
                DateTime utcNow = DateTime.UtcNow;
                DateTime utcPlus7 = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);

                // Add two days to the converted time
                int daysToAdd;
                string orgName;
                string programName;
                int.TryParse(ReadNotNullString("Enter the number of days to add to the current date (30,90,120,365 or etc) : "), out daysToAdd);
                DateTime resultDay = utcPlus7.AddDays(daysToAdd);
                orgName = ReadNotNullString("Enter the organization name (MyCompany) : ");
                programName = ReadNotNullString("Enter the program name (MyProgram) : ");
                string serialNumber = ReadNotNullString("Enter the serial number to validate : ");


                try {
                    string serial = GenerateSecretKey.GenerateSerial(resultDay, orgName, programName, serialNumber, secretKey);
                    Console.WriteLine($"Generated Serial: {serial}");
                } catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            void ValidateSerial()
            {
                Console.Clear();
                Console.WriteLine("This program will validate a serial number.\n");
                Console.WriteLine(
                    "The serial number must be generated using the same secret key file that was used to generate the serial number.\n" +
                    "The secret key file should be kept secret and should not be shared with anyone.\n" +
                    "If the secret key file is lost, any files that are encrypted with the key file cannot be decrypted.\n");

                string KeyFileName;
                string secretKey;

                KeyFileName = ReadNotNullString("Enter the key file path and file name (C:\\Users\\keys\\MyKey.key) : ");

                try
                {
                    var obfuscatedKey = File.ReadAllText(KeyFileName);
                    secretKey = GenerateSecretKey.DeObfuscate(obfuscatedKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return;
                }

                string serial = ReadNotNullString("Enter the serial number to validate : ");

                TimeZoneInfo timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
                DateTime utcNow = DateTime.UtcNow;
                DateTime utcPlus7 = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);

                try
                {
                    var result = GenerateSecretKey.ValidateSerial(serial,out var payload, secretKey);
                    Console.WriteLine($"\nSerial number is valid: {result}");
                    if (result)
                    {
                        Console.WriteLine($"\nExpirationDate: {payload.ExpirationDate} let: {(payload.ExpirationDate - utcPlus7).Days}");
                        Console.WriteLine($"OrganizationName: {payload.OrganizationName}");
                        Console.WriteLine($"ProgramName: {payload.ProgramName}");
                        Console.WriteLine($"SerialNumber: {payload.SerialNumber}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
