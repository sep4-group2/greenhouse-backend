using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Services;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tests.Helpers;
using Xunit;
using System.Text.Json;
using System.Threading;
using System.Collections.Generic;

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

        [Fact]
        public async Task SetPresetAsync_NullArguments_ThrowsArgumentNullException()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            await Assert.ThrowsAsync<ArgumentNullException>(() => svc.SetPresetAsync(null!, new Preset()));
            await Assert.ThrowsAsync<ArgumentNullException>(() => svc.SetPresetAsync(new Greenhouse(), null!));
        }

        [Fact]
        public async Task SetPresetAsync_AssignsActivePreset()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            var gh = new Greenhouse { Name = "GH", MacAddress = "M1", UserEmail = "u@u.com", LightingMethod = "L", WateringMethod = "W", FertilizationMethod = "F" };
            ctx.Greenhouses.Add(gh);
            await ctx.SaveChangesAsync();
            var preset = new Preset { Name = "P", HoursOfLight = 5 };
            // Act
            var result = await svc.SetPresetAsync(gh, preset);
            // Assert
            Assert.Equal(preset, result.ActivePreset);
        }

        [Fact]
        public async Task PairGreenhouse_NewGreenhouse_AddsGreenhouse()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            // Add user for pairing
            ctx.Users.Add(new User { email = "user@x.com", Password = "p" });
            await ctx.SaveChangesAsync();
            var svc = CreateService(ctx);
            var dto = new GreenhousePairDto { Name = "GNew", MacAddress = "MACX" };
            await svc.PairGreenhouse(dto, "user@x.com");
            var saved = ctx.Greenhouses.FirstOrDefault(g => g.MacAddress == "MACX");
            Assert.NotNull(saved);
            Assert.Equal("GNew", saved.Name);
            Assert.Equal("user@x.com", saved.UserEmail);
            Assert.Equal("manual", saved.LightingMethod);
        }

        [Fact]
        public async Task PairGreenhouse_AlreadyPaired_ThrowsUnauthorizedAccessException()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            // Add user and existing greenhouse
            ctx.Users.Add(new User { email = "user@x.com", Password = "p" });
            var existing = new Greenhouse { Name = "G1", MacAddress = "MAC1", UserEmail = "u@u.com",
                LightingMethod = "L", WateringMethod = "W", FertilizationMethod = "F" };
            ctx.Greenhouses.Add(existing);
            await ctx.SaveChangesAsync();
            var svc = CreateService(ctx);
            var dto = new GreenhousePairDto { Name = "G1", MacAddress = "MAC1" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => svc.PairGreenhouse(dto, "user@x.com"));
        }

        [Fact]
        public async Task UnpairGreenhouse_NullOrEmptyEmail_ThrowsUnauthorizedAccessException()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => svc.UnpairGreenhouse(1, string.Empty));
        }

        [Fact]
        public async Task UnpairGreenhouse_GreenhouseNotFound_ThrowsUnauthorizedAccessException()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => svc.UnpairGreenhouse(99, "u@u.com"));
        }

        [Fact]
        public async Task UnpairGreenhouse_Succeeds_SetsUserEmailNull()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var gh = new Greenhouse
            {
                Name = "G",
                MacAddress = "M",
                UserEmail = "u@u.com",
                LightingMethod = "L",
                WateringMethod = "W",
                FertilizationMethod = "F"
            };
            ctx.Greenhouses.Add(gh);
            await ctx.SaveChangesAsync();
            var svc = CreateService(ctx);
            // Act
            await svc.UnpairGreenhouse(gh.Id, "u@u.com");
            // Assert: tracked entity UserEmail should be null
            Assert.Null(gh.UserEmail);
        }

        [Fact]
        public async Task RenameGreenhouse_NullOrEmptyEmail_ThrowsUnauthorizedAccessException()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            var dto = new GreenhouseRenameDto { Id = 1, Name = "X" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => svc.RenameGreenhouse(dto, string.Empty));
        }

        [Fact]
        public async Task RenameGreenhouse_NotFoundOrNotOwned_ThrowsUnauthorizedAccessException()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var gh = new Greenhouse { Name = "G", MacAddress = "M", UserEmail = "owner@x.com", LightingMethod = "L", WateringMethod = "W", FertilizationMethod = "F" };
            ctx.Greenhouses.Add(gh);
            await ctx.SaveChangesAsync();
            var svc = CreateService(ctx);
            var dto = new GreenhouseRenameDto { Id = gh.Id, Name = "New" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => svc.RenameGreenhouse(dto, "other@x.com"));
        }

        [Fact]
        public async Task RenameGreenhouse_Succeeds_UpdatesName()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var gh = new Greenhouse { Name = "Old", MacAddress = "M", UserEmail = "u@u.com",
                LightingMethod = "L", WateringMethod = "W", FertilizationMethod = "F" };
            ctx.Greenhouses.Add(gh);
            await ctx.SaveChangesAsync();
            var svc = CreateService(ctx);
            var dto = new GreenhouseRenameDto { Id = gh.Id, Name = "New" };
            await svc.RenameGreenhouse(dto, "u@u.com");
            var updated = await ctx.Greenhouses.FindAsync(gh.Id);
            Assert.NotNull(updated);
            Assert.Equal("New", updated!.Name);
        }

        [Fact]
        public async Task GetPredictionFromLatestValuesAsync_MissingReadings_ThrowsException()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            await Assert.ThrowsAsync<Exception>(() => svc.GetPredictionFromLatestValuesAsync(1));
        }

        private class FakeHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public FakeHandler(HttpResponseMessage response) => _response = response;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_response);
        }

        private class HttpClientFactoryStubWithClient : IHttpClientFactory
        {
            private readonly HttpClient _client;
            public HttpClientFactoryStubWithClient(HttpClient client) => _client = client;
            public HttpClient CreateClient(string name) => _client;
        }

        // Updated sensor reading types to match service queries
        [Fact]
        public async Task GetPredictionFromLatestValuesAsync_Success_ReturnsForecast()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var ghId = 1;
            var now = DateTime.UtcNow;
            ctx.SensorReadings.AddRange(
                new SensorReading { GreenhouseId = ghId, Type = "SoilHumidity", Value = 1.1, Unit = "%", Timestamp = now.AddMinutes(-3) },
                new SensorReading { GreenhouseId = ghId, Type = "Temperature", Value = 2.2, Unit = "°C", Timestamp = now.AddMinutes(-2) },
                new SensorReading { GreenhouseId = ghId, Type = "Humidity", Value = 3.3, Unit = "%", Timestamp = now.AddMinutes(-1) }
            );
            await ctx.SaveChangesAsync();

            var forecast = new List<double> { 1.0, 2.0, 3.0 };
            var predDto = new PredictionResultDto { Forecast = forecast };
            var json = JsonSerializer.Serialize(predDto);
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
            var client = new HttpClient(new FakeHandler(response));
            var svc = new GreenhouseService(
                ctx,
                new HttpClientFactoryStubWithClient(client),
                new ConfigurationBuilder().AddInMemoryCollection(new[] { KeyValuePair.Create<string, string?>("MalApi:BaseUrl", "http://dummy/") }).Build(),
                null!
            );
            var result = await svc.GetPredictionFromLatestValuesAsync(ghId);

            Assert.NotNull(result);
            Assert.NotNull(result.Forecast);
            Assert.Equal(3, result.Forecast.Count);
            Assert.Equal(forecast, result.Forecast);
        }
    }
}
