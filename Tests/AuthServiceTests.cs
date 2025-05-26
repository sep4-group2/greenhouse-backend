using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Services;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tests.Helpers;
using Xunit;

namespace Tests
{
    public class AuthServiceTests
    {
        private readonly IPasswordHasher<User> _hasher = new PasswordHasher<User>();
        private readonly IConfiguration _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        [Fact]
        public async Task RegisterUser_Success()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            var service = new AuthService(context, _hasher, _configuration);
            var dto = new RegisterRequestDto
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                PasswordConfirmation = "Password123!"
            };

            // Act
            var user = await service.RegisterUser(dto);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(dto.Email, user.email);
            // Password should be hashed, not equal to plain
            Assert.NotEqual(dto.Password, user.Password);
            // Persisted in DB
            var fromDb = context.Users.FirstOrDefault(u => u.email == dto.Email);
            Assert.NotNull(fromDb);
        }

        [Fact]
        public async Task RegisterUser_EmailExists_Throws()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            var existing = new User { email = "exists@example.com", Password = "hash" };
            context.Users.Add(existing);
            await context.SaveChangesAsync();
            var service = new AuthService(context, _hasher, _configuration);
            var dto = new RegisterRequestDto
            {
                Email = existing.email,
                Password = "Pwd1!",
                PasswordConfirmation = "Pwd1!"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterUser(dto));
            Assert.Equal("Email already exists", ex.Message);
        }

        [Fact]
        public async Task RegisterUser_PasswordsDoNotMatch_Throws()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            var service = new AuthService(context, _hasher, _configuration);
            var dto = new RegisterRequestDto
            {
                Email = "mismatch@example.com",
                Password = "Password1",
                PasswordConfirmation = "Password2"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterUser(dto));
            Assert.Equal("Passwords do not match", ex.Message);
        }

        [Fact]
        public async Task Login_UserNotFound_Throws()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            var service = new AuthService(context, _hasher, _configuration);
            var dto = new UserDto("nouser@example.com", "any");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.Login(dto));
            Assert.Equal("Invalid Email", ex.Message);
        }

        [Fact]
        public async Task Login_WrongPassword_Throws()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            var user = new User { email = "user@example.com" };
            // Hash known password
            user.Password = _hasher.HashPassword(user, "CorrectPassword");
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var service = new AuthService(context, _hasher, _configuration);
            var dto = new UserDto(user.email, "WrongPassword");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.Login(dto));
            Assert.Equal("Invalid Password", ex.Message);
        }

        [Fact]
        public async Task Login_Success()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            var user = new User { email = "gooduser@example.com" };
            user.Password = _hasher.HashPassword(user, "RightPass!23");
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var service = new AuthService(context, _hasher, _configuration);
            var dto = new UserDto(user.email, "RightPass!23");

            // Act
            var result = await service.Login(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.email, result.email);
        }

        [Fact]
        public void GenerateJwtToken_IncludesEmailClaim()
        {
            // Arrange
            var longKey = "abcdefghijklmnopqrstuvwxyzABCDEF012345"; // 32+ chars
            var cfg = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = longKey,
                    ["Jwt:Issuer"] = "testissuer",
                    ["Jwt:Audience"] = "testaudience",
                    ["Jwt:AccessTokenMinutes"] = "60"
                })
                .Build();
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = new AuthService(ctx, _hasher, cfg);
            var user = new User { email = "test@example.com" };

            // Act
            var token = svc.GenerateJwtToken(user);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Equal("testissuer", jwt.Issuer);
            Assert.Equal("testaudience", jwt.Audiences.First());
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.email);
        }

        [Fact]
        public void GenerateRefreshToken_IncludesRefreshClaim()
        {
            // Arrange
            var longRefreshKey = "ABCDEFGHIJKLMNOPQRSTUVWXabcdefghijklmn"; // 32+ chars
            var cfg = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:RefreshKey"] = longRefreshKey,
                    ["Jwt:Issuer"] = "testissuer",
                    ["Jwt:Audience"] = "testaudience",
                    ["Jwt:RefreshTokenDays"] = "7"
                })
                .Build();
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = new AuthService(ctx, _hasher, cfg);
            var user = new User { email = "test2@example.com" };

            // Act
            var token = svc.GenerateRefreshToken(user);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Equal("testissuer", jwt.Issuer);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.email);
            Assert.Contains(jwt.Claims, c => c.Type == "token_type" && c.Value == "refresh");
        }

        [Fact]
        public async Task RefreshToken_Success()
        {
            // Arrange
            var key = "ABCDEFGHIJKLMNOPQRSTUVWXabcdefghijklmn"; // same as test above
            var cfg = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:RefreshKey"] = key,
                    ["Jwt:Issuer"] = "testissuer",
                    ["Jwt:Audience"] = "testaudience",
                    ["Jwt:RefreshTokenDays"] = "7"
                })
                .Build();
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = new AuthService(ctx, _hasher, cfg);
            var user = new User { email = "u@t.com" };
            // Ensure required fields
            user.Password = _hasher.HashPassword(user, "dummy");
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            var token = svc.GenerateRefreshToken(user);

            // Act
            var result = await svc.RefreshToken(token);

            // Assert
            Assert.Equal(user.email, result.email);
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_Throws()
        {
            // Arrange
            var key = "ABCDEFGHIJKLMNOPQRSTUVWXabcdefghijklmn"; // 32+ chars
            var cfg2 = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:RefreshKey"] = key,
                    ["Jwt:Issuer"] = "testissuer",
                    ["Jwt:Audience"] = "testaudience",
                    ["Jwt:RefreshTokenDays"] = "7"
                })
                .Build();
            var svc = new AuthService(TestDbHelper.GetInMemoryDbContext(), _hasher, cfg2);
            var bad = "not_a_valid_token";

            // Act & Assert: underlying JWT lib throws on invalid token
            await Assert.ThrowsAnyAsync<Exception>(() => svc.RefreshToken(bad));
        }

        [Fact]
        public async Task RefreshToken_WrongTokenType_Throws()
        {
            // Arrange: generate access token and attempt to refresh
            var longKey2 = "abcdefghijklmnopqrstuvwxyzABCDEF012345"; // 32+ chars
            var cfg3 = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = longKey2,
                    ["Jwt:RefreshKey"] = longKey2,
                    ["Jwt:Issuer"] = "testissuer",
                    ["Jwt:Audience"] = "testaudience",
                    ["Jwt:AccessTokenMinutes"] = "60"
                })
                .Build();
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = new AuthService(ctx, _hasher, cfg3);
            var user = new User { email = "wrongtype@t.com" };
            // Ensure required fields
            user.Password = _hasher.HashPassword(user, "dummy");
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            var access = svc.GenerateJwtToken(user);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => svc.RefreshToken(access));
        }
    }
}
