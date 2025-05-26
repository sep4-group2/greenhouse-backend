using System;
using System.Threading.Tasks;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Tests.Helpers;
using Api.Services;
using Xunit;

namespace Tests
{
    public class UserServiceTests
    {
        private readonly IPasswordHasher<User> _hasher = new PasswordHasher<User>();

        [Fact]
        public async Task DeleteUser_NullOrEmptyEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = new UserService(ctx, _hasher);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => svc.DeleteUser(null!, "any"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => svc.DeleteUser(string.Empty, "any"));
        }

        [Fact]
        public async Task DeleteUser_UserNotFound_ThrowsException()
        {
            // Arrange
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = new UserService(ctx, _hasher);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => svc.DeleteUser("nonexistent@example.com", "pwd"));
            Assert.Equal("User not found", ex.Message);
        }

        [Fact]
        public async Task DeleteUser_WrongPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var user = new User { email = "u@test.com" };
            user.Password = _hasher.HashPassword(user, "correctPwd");
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            var svc = new UserService(ctx, _hasher);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => svc.DeleteUser(user.email, "wrongPwd"));
        }

        [Fact]
        public async Task DeleteUser_ValidCredentials_RemovesUser()
        {
            // Arrange
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var user = new User { email = "u@test.com" };
            user.Password = _hasher.HashPassword(user, "correctPwd");
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            var svc = new UserService(ctx, _hasher);

            // Act
            await svc.DeleteUser(user.email, "correctPwd");

            // Assert
            var fromDb = await ctx.Users.FindAsync(user.email);
            Assert.Null(fromDb);
        }
    }
}
