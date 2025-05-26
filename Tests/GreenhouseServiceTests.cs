using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Services;
using Data;
using Data.Entities;
using Microsoft.Extensions.Configuration;
using Tests.Helpers;
using Xunit;

namespace Tests
{
    public class GreenhouseServiceTests
    {
        private GreenhouseService CreateService(AppDbContext ctx)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { KeyValuePair.Create<string, string?>("MalApi:BaseUrl", "http://dummy/") })
                .Build();
            return new GreenhouseService(ctx, new HttpClientFactoryStub(), config, null!);
        }

        private class HttpClientFactoryStub : IHttpClientFactory
        {
            public HttpClient CreateClient(string name) => new HttpClient();
        }

        [Fact]
        public async Task GetAllGreenhousesForUser_ThrowsOnEmptyEmail()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => svc.GetAllGreenhousesForUser(string.Empty));
        }

        [Fact]
        public async Task GetAllGreenhousesForUser_ReturnsUserOwned()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var user = new User { email = "u@x", Password = "p" };
            ctx.Users.Add(user);
            ctx.Greenhouses.Add(new Greenhouse
            {
                Name = "G1",
                MacAddress = "M1",
                UserEmail = user.email,
                LightingMethod = "L",
                WateringMethod = "W",
                FertilizationMethod = "F"
            });
            ctx.Greenhouses.Add(new Greenhouse
            {
                Name = "G2",
                MacAddress = "M2",
                UserEmail = "other",
                LightingMethod = "L",
                WateringMethod = "W",
                FertilizationMethod = "F"
            });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var list = await svc.GetAllGreenhousesForUser(user.email);
            Assert.Single(list);
            Assert.Equal("G1", list.First().Name);
        }
    }
}
