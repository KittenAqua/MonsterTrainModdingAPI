using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using HarmonyLib;
using System.Reflection;

namespace Trainworks.Utilities
{
    public class GUIDGenerator
    {
        private static SHA256CryptoServiceProvider provider = new SHA256CryptoServiceProvider();
        public static void ResetProvider()
        {
            provider = new SHA256CryptoServiceProvider();
        }
        public static string GenerateDeterministicGUID(string key)
        {
            if (key == null)
            {
                Trainworks.Log(BepInEx.Logging.LogLevel.Warning, "GUIDGenerator cannot generate determinstic GUID for null key. Will Crash.");
                return key;
            }

            //Generate a byte array from a string using default encoding
            byte[] inputBytes = Encoding.Default.GetBytes(key);

            //Use bytes to generate a byte array of length 16 using MD5CryptoServiceProvider
            byte[] hashBytes = provider.ComputeHash(inputBytes);

            //Set the first nibble of the byte to be 0100
            hashBytes[7] = (byte)(0x40 | ((int)hashBytes[7] & 0xf));

            //Set the first crumb of the byte to be 10
            hashBytes[8] = (byte)(0x80 | ((int)hashBytes[8] & 0x3f));

            //Resize the array to get the first 16 bytes
            Array.Resize<byte>(ref hashBytes, 16);

            Guid guid = new Guid(hashBytes);
            return guid.ToString();
        }
    }
}
