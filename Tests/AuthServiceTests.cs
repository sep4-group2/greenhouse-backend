using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Services;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
    }
}
