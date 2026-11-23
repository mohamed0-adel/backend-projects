using System.Security.Cryptography;

namespace UrlShortener.Api.Services;

public static class ShortCodeGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Create(int length)
    {
        return string.Create(length, Alphabet, static (characters, alphabet) =>
        {
            for (var index = 0; index < characters.Length; index++)
                characters[index] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        });
    }
}
