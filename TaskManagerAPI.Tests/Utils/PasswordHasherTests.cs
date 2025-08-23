using System.Security.Cryptography;
using TaskManagerAPI.Infrastructure.Utils;

namespace TaskManagerAPI.Tests.Utils;

public class PasswordHasherTests
{
    [Fact]
    public void CreatePasswordHash_GeneratesNonEmptyHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Assert
        Assert.NotNull(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(hash);
        Assert.NotEmpty(salt);
        Assert.NotEqual(0, hash.Length);
        Assert.NotEqual(0, salt.Length);
    }

    [Fact]
    public void CreatePasswordHash_GeneratesDifferentSaltForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        PasswordHasher.CreatePasswordHash(password, out byte[] hash1, out byte[] salt1);
        PasswordHasher.CreatePasswordHash(password, out byte[] hash2, out byte[] salt2);

        // Assert
        Assert.NotNull(salt1);
        Assert.NotNull(salt2);
        Assert.NotEqual(salt1, salt2);
        Assert.False(salt1.SequenceEqual(salt2));
    }

    [Fact]
    public void CreatePasswordHash_GeneratesDifferentHashForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        PasswordHasher.CreatePasswordHash(password, out byte[] hash1, out byte[] salt1);
        PasswordHasher.CreatePasswordHash(password, out byte[] hash2, out byte[] salt2);

        // Assert
        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.NotEqual(hash1, hash2);
        Assert.False(hash1.SequenceEqual(hash2));
    }

    [Fact]
    public void VerifyPasswordHash_ReturnsTrueForCorrectPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act
        var result = PasswordHasher.VerifyPasswordHash(password, hash, salt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPasswordHash_ReturnsFalseForIncorrectPassword()
    {
        // Arrange
        var correctPassword = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        PasswordHasher.CreatePasswordHash(correctPassword, out byte[] hash, out byte[] salt);

        // Act
        var result = PasswordHasher.VerifyPasswordHash(wrongPassword, hash, salt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPasswordHash_ReturnsFalseForWrongSalt()
    {
        // Arrange
        var password = "TestPassword123!";
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Генерируем другой salt
        byte[] wrongSalt;
        using (var hmac = new HMACSHA512())
        {
            wrongSalt = hmac.Key;
        }

        // Act
        var result = PasswordHasher.VerifyPasswordHash(password, hash, wrongSalt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPasswordHash_ReturnsFalseForWrongHash()
    {
        // Arrange
        var password = "TestPassword123!";
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Создаем неправильный hash
        byte[] wrongHash = new byte[hash.Length];
        hash.CopyTo(wrongHash, 0);
        wrongHash[0] = (byte)(wrongHash[0] + 1); // Меняем первый байт

        // Act
        var result = PasswordHasher.VerifyPasswordHash(password, wrongHash, salt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPasswordHash_ReturnsFalseForEmptyPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        var emptyPassword = "";
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act
        var result = PasswordHasher.VerifyPasswordHash(emptyPassword, hash, salt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPasswordHash_WithNullHash_ThrowsArgumentNullException()
    {
        // Arrange
        var password = "TestPassword123!";
        PasswordHasher.CreatePasswordHash(password, out byte[] _, out byte[] salt);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PasswordHasher.VerifyPasswordHash(password, null, salt));
    }

    [Fact]
    public void VerifyPasswordHash_WithNullSalt_ThrowsArgumentNullException()
    {
        // Arrange
        var password = "TestPassword123!";
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] _);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PasswordHasher.VerifyPasswordHash(password, hash, null));
    }

    [Fact]
    public void VerifyPasswordHash_ReturnsFalseForEmptyHash()
    {
        // Arrange
        var password = "TestPassword123!";
        PasswordHasher.CreatePasswordHash(password, out byte[] _, out byte[] salt);

        // Act
        var result = PasswordHasher.VerifyPasswordHash(password, new byte[0], salt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPasswordHash_ReturnsFalseForEmptySalt()
    {
        // Arrange
        var password = "TestPassword123!";
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] _);

        // Act
        var result = PasswordHasher.VerifyPasswordHash(password, hash, new byte[0]);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("verylongpasswordverylongpasswordverylongpasswordverylongpassword")]
    [InlineData("Password123!")]
    [InlineData("test@example.com")]
    [InlineData("1234567890")]
    [InlineData("!@#$%^&*()")]
    public void VerifyPasswordHash_WorksWithVariousPasswordFormats(string password)
    {
        // Arrange
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act
        var result = PasswordHasher.VerifyPasswordHash(password, hash, salt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CreatePasswordHash_WorksWithUnicodeCharacters()
    {
        // Arrange
        var password = "Пароль123!测试密码🎉";

        // Act
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);
        var result = PasswordHasher.VerifyPasswordHash(password, hash, salt);

        // Assert
        Assert.True(result);
        Assert.NotNull(hash);
        Assert.NotNull(salt);
    }

    [Fact]
    public void VerifyPasswordHash_WithModifiedHash_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Модифицируем хеш
        hash[hash.Length / 2] = (byte)(hash[hash.Length / 2] + 1);

        // Act
        var result = PasswordHasher.VerifyPasswordHash(password, hash, salt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPasswordHash_WithModifiedSalt_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        PasswordHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Модифицируем salt
        salt[salt.Length / 2] = (byte)(salt[salt.Length / 2] + 1);

        // Act
        var result = PasswordHasher.VerifyPasswordHash(password, hash, salt);

        // Assert
        Assert.False(result);
    }
}