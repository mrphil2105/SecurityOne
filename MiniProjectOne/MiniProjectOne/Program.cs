using MiniProjectOne;

const int generator = 666;
const int prime = 6661;
const int bobPublicKey = 2227;
const int message = 2000;

var alice = new ElGamal(prime, generator);
var bob = new ElGamal(prime, generator);

alice.GenerateKeys();
bob.PublicKey = bobPublicKey;
// At this point bob.PrivateKey is still 0.

// Step 1: Alice
var ciphertext = alice.Encrypt(message, bob.PublicKey);
Console.WriteLine("Alice's Ciphertext: {0}", ciphertext);

// Step 2: Eve
// We need to guess the private key.
bob.PrivateKey = BruteForce.GuessPrivateKey(bob);
Console.WriteLine("Bob's Private Key: {0}", bob.PrivateKey);
var plaintext = bob.Decrypt(ciphertext, alice.PublicKey);
Console.WriteLine("Decrypted Plaintext: {0}", plaintext);

// Step 3: Weave
var newCiphertext = ciphertext * 2;
// Bob decrypts the modified ciphertext.
var newPlaintext = bob.Decrypt(newCiphertext, alice.PublicKey);
Console.WriteLine("Modified Ciphertext: {0}", newCiphertext);
Console.WriteLine("Modified Plaintext: {0}", newPlaintext);
