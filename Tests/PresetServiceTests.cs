using System;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Services;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;
using Xunit;

namespace Tests
{
    public class PresetServiceTests
    {
        private PresetService CreateService(AppDbContext ctx)
            => new PresetService(ctx, null!);

        [Fact]
        public async Task GetPresetAsync_ReturnsNull_WhenNotExists()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);

            var result = await svc.GetPresetAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPresetAsync_ReturnsPreset_WhenExists()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var preset = new Preset { Name = "P1", MinTemperature = 10, MaxTemperature = 20, MinAirHumidity = 30, MaxAirHumidity = 40, MinSoilHumidity = 50, MaxSoilHumidity = 60, HoursOfLight = 8 };
            ctx.Presets.Add(preset);
            await ctx.SaveChangesAsync();
            var svc = CreateService(ctx);

            var result = await svc.GetPresetAsync(preset.Id);
            Assert.NotNull(result);
            Assert.Equal("P1", result.Name);
        }

        [Fact]
        public async Task CreatePresetAsync_CreatesPreset_AndUserPreset()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            var dto = new CreatePresetDTO
            {
                Name = "New",
                MinTemperature = 5,
                MaxTemperature = 15,
                MinAirHumidity = 20,
                MaxAirHumidity = 30,
                MinSoilHumidity = 35,
                MaxSoilHumidity = 45,
                HoursOfLight = 9
            };

            var result = await svc.CreatePresetAsync(dto, "user@x.com");

            Assert.NotNull(result);
            var fromDb = await ctx.Presets.FindAsync(result.Id);
            Assert.NotNull(fromDb);
            var up = await ctx.UserPresets.FindAsync(result.Id);
            Assert.NotNull(up);
            Assert.Equal("user@x.com", up.UserEmail);
        }

        [Fact]
        public async Task CreatePresetAsync_NullUserEmail_Throws()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            var dto = new CreatePresetDTO { Name = "x", MinTemperature = 1, MaxTemperature = 2, MinAirHumidity = 3, MaxAirHumidity = 4, MinSoilHumidity = 5, MaxSoilHumidity = 6, HoursOfLight = 7 };
            await Assert.ThrowsAsync<Exception>(() => svc.CreatePresetAsync(dto, null!));
        }

        [Fact]
        public async Task DeletePresetAsync_ReturnsFalse_WhenNotExists()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            var result = await svc.DeletePresetAsync(123, "u");
            Assert.False(result);
        }

        [Fact]
        public async Task DeletePresetAsync_Throws_WhenSystemPreset()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            // system preset seeded with Id = 1
            await Assert.ThrowsAsync<Exception>(() => svc.DeletePresetAsync(1, "u@u.com"));
        }

        [Fact]
        public async Task DeletePresetAsync_Throws_WhenOtherUser()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var p = new Preset { Name = "U1", MinTemperature = 1, MaxTemperature = 2, MinAirHumidity = 3, MaxAirHumidity = 4, MinSoilHumidity = 5, MaxSoilHumidity = 6, HoursOfLight = 7 };
            ctx.Presets.Add(p);
            await ctx.SaveChangesAsync();
            ctx.UserPresets.Add(new UserPreset { Id = p.Id, UserEmail = "a@a.com" });
            await ctx.SaveChangesAsync();
            var svc = CreateService(ctx);

            await Assert.ThrowsAsync<Exception>(() => svc.DeletePresetAsync(p.Id, "b@b.com"));
        }

        [Fact]
        public async Task DeletePresetAsync_Deletes_WhenValid()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var p = new Preset { Name = "V", MinTemperature = 1, MaxTemperature = 2, MinAirHumidity = 3, MaxAirHumidity = 4, MinSoilHumidity = 5, MaxSoilHumidity = 6, HoursOfLight = 7 };
            ctx.Presets.Add(p);
            await ctx.SaveChangesAsync();
            ctx.UserPresets.Add(new UserPreset { Id = p.Id, UserEmail = "u@u.com" });
            await ctx.SaveChangesAsync();
            var svc = CreateService(ctx);

            var res = await svc.DeletePresetAsync(p.Id, "u@u.com");
            Assert.True(res);
            var check = await ctx.Presets.FindAsync(p.Id);
            Assert.Null(check);
        }

        [Fact]
        public async Task GetAllPresetsAsync_ReturnsSystemAndUserPresets()
        {
            var ctx = TestDbHelper.GetInMemoryDbContext();
            var svc = CreateService(ctx);
            // create user preset
            var p = new Preset { Name = "UP", MinTemperature = 1, MaxTemperature = 2, MinAirHumidity = 3, MaxAirHumidity = 4, MinSoilHumidity = 5, MaxSoilHumidity = 6, HoursOfLight = 7 };
            ctx.Presets.Add(p);
            await ctx.SaveChangesAsync();
            ctx.UserPresets.Add(new UserPreset { Id = p.Id, UserEmail = "x@x.com" });
            await ctx.SaveChangesAsync();

            var all = await svc.GetAllPresetsAsync("x@x.com");
            // default system preset seeded + one user preset
            Assert.Contains(all, pr => pr.Id == 1);
            Assert.Contains(all, pr => pr.Id == p.Id);
        }
    }
}
