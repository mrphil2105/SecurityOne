using System.Numerics;
using System.Security.Cryptography;

namespace MiniProjectOne;

public class ElGamal
{
    public ElGamal(int prime, int generator)
    {
        if (prime <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(prime));
        }

        if (generator <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(generator));
        }

        Prime = prime;
        Generator = generator;
    }

    // The prime p.
    public int Prime { get; }

    // The base g.
    public int Generator { get; }

    public int PrivateKey { get; set; }

    public int PublicKey { get; set; }

    public int Encrypt(int message, int otherPublicKey)
    {
        if (PrivateKey == 0)
        {
            throw new InvalidOperationException("You must generate a private key before encrypting.");
        }

        var sharedKey = CalculateSharedKey(otherPublicKey);
        var ciphertext = sharedKey * message % Prime;

        return ciphertext;
    }

    public int Decrypt(int ciphertext, int otherPublicKey)
    {
        if (PrivateKey == 0)
        {
            throw new InvalidOperationException("You must generate a private key before encrypting.");
        }

        var sharedKey = CalculateSharedKey(otherPublicKey);
        var inverseSharedKey = ModInverse(sharedKey, Prime);
        var plaintext = ciphertext * inverseSharedKey % Prime;

        return plaintext;
    }

    private int CalculateSharedKey(int otherPublicKey)
    {
        return (int)BigInteger.ModPow(otherPublicKey, PrivateKey, Prime);
    }

    public void GenerateKeys()
    {
        if (Prime == 0)
        {
            throw new InvalidOperationException($"You must set property '{Prime}' before generating keys.");
        }

        if (Generator == 0)
        {
            throw new InvalidOperationException($"You must set property '{Generator}' before generating keys.");
        }

        PrivateKey = RandomNumberGenerator.GetInt32(Prime);
        CalculatePublicKey();
    }

    public void CalculatePublicKey()
    {
        PublicKey = (int)BigInteger.ModPow(Generator, PrivateKey, Prime);
    }

    // Credit: ChatGPT
    private static int ModInverse(int a, int m)
    {
        // Calculate the modular multiplicative inverse of a modulo m.

        var m0 = m;
        var x0 = 0;
        var x1 = 1;

        while (a > 1)
        {
            var q = a / m;
            var temp = m;

            m = a % m;
            a = temp;
            temp = x0;

            x0 = x1 - q * x0;
            x1 = temp;
        }

        if (x1 < 0)
        {
            x1 += m0;
        }

        return x1;
    }
}
