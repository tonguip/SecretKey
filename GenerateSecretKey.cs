using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecretKey
{
    public class GenerateSecretKey
    {
        private const char DataSeparator = '|';
        public class Payload
        {
            public DateTime ExpirationDate { get; set; }
            public string OrganizationName { get; set; }
            public string ProgramName { get; set; }
            public string SerialNumber { get; set; }

            public override string ToString()
            {
                return $"{ExpirationDate:O}{DataSeparator}{OrganizationName}{DataSeparator}{ProgramName}{DataSeparator}{SerialNumber}";
            }

            public static Payload FromString(string data)
            {
                var parts = data.Split(DataSeparator);
                if (parts.Length != 4)
                    throw new FormatException("Invalid payload format");

                return new Payload
                {
                    ExpirationDate = DateTime.Parse(parts[0]),
                    OrganizationName = parts[1],
                    ProgramName = parts[2],
                    SerialNumber = parts[3]
                };
            }
        }

        public static string GenerateSecretKeyKUS(int KeySize)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[KeySize / 8];  // Convert bits to bytes
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);  // Convert to base64 for easier storage and reading
            }
        }

        public static string LoadSecretKeyFromFile(string keyFilePath)
        {
            if (!File.Exists(keyFilePath))
            {
                throw new FileNotFoundException($"Secret key file {keyFilePath} not found.");
            }

            return File.ReadAllText(keyFilePath).Trim();
        }

        public static string Obfuscate(string input)
        {
            char key = 'X';  // Example simple key for XOR operation
            var result = new StringBuilder(input.Length);

            foreach (var c in input)
            {
                result.Append((char)(c ^ key));
            }

            return result.ToString();
        }
        public static string DeObfuscate(string input)
        {
            return Obfuscate(input);  // Since XOR is its own inverse
        }
        public static string GenerateSerial(DateTime expirationDate, string orgName, string programName,string serialNumber, string key)
        {
            var payload = new Payload
            {
                ExpirationDate = expirationDate,
                OrganizationName = orgName,
                ProgramName = programName,
                SerialNumber = serialNumber
            };

            Console.WriteLine(payload);

            string serializedPayload = payload.ToString();
            string signature = ComputeSignature(serializedPayload, key);

            return $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedPayload))}.{signature}";
        }

        public static bool ValidateSerial(string serial, out Payload payloadData, string Secret)
        {
            payloadData = null;

            var parts = serial.Split('.');
            if (parts.Length != 2) return false;

            string serializedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
            string providedSignature = parts[1];

            if (ComputeSignature(serializedPayload, Secret) != providedSignature) return false;

            try
            {
                payloadData = Payload.FromString(serializedPayload);

                if (DateTime.Now <= payloadData.ExpirationDate)
                {
                    return true;
                }
            }
            catch (FormatException)
            {
                return false;
            }

            return false;
        }

        private static string ComputeSignature(string data, string Secret)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
