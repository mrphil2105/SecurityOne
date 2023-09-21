namespace MiniProjectOne;

public static class BruteForce
{
    public static int GuessPrivateKey(ElGamal gamal)
    {
        if (gamal.PublicKey == 0)
        {
            throw new ArgumentException("The Gamal must have a public key set.", nameof(gamal));
        }

        var publicKey = gamal.PublicKey;

        for (var privateKey = 1; privateKey < gamal.Prime; privateKey++)
        {
            gamal.PrivateKey = privateKey;
            gamal.CalculatePublicKey();

            if (gamal.PublicKey == publicKey)
            {
                return privateKey;
            }
        }

        throw new Exception("Unable to guess private key.");
    }
}
