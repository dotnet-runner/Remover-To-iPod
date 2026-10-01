using System.Security.Cryptography;
using SpotifyApiWorker.Services.Contracts;
using SpotifyApiWorker.ValueObjects;

namespace SpotifyApiWorker.Services.Implementations;

public class ServerSessionKeyGenerator: IServerSessionKeyGenerator
{
    public const int ServerSessionKeyLength = 32;
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    
    public ServerSessionKey Generate()
    {
        var chars = new char[ServerSessionKeyLength];
        
        for (int i = 0; i < ServerSessionKeyLength; i++)
        {
            var randomIndex = RandomNumberGenerator.GetInt32(Alphabet.Length);
            chars[i] = Alphabet[randomIndex];
        }
        
        var key = new string(chars);
        return new ServerSessionKey(key);
    }
}