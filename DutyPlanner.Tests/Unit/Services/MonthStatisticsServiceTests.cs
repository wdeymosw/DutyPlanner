using DutyPlanner.Domain.Repositories;
using DutyPlanner.Infrastructure.JsonFileStorage;
using DutyPlanner.Models;
using DutyPlanner.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DutyPlanner.Tests.Unit.Services;

public class MonthStatisticsServiceTests : IDisposable
{
    private readonly IJsonFileStorage _storage = Substitute.For<IJsonFileStorage>();
    private readonly IDayRepository _dayRepository = Substitute.For<IDayRepository>();
    private readonly string _tempDir;

    public MonthStatisticsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DPTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _dayRepository.GetDayFilePaths(Arg.Any<string>())
            .Returns(ci => Directory.GetFiles(ci.Arg<string>(), "*.json"));
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    private MonthStatisticsService CreateService() => new(_storage, _dayRepository);

    private string CreateDayFile(int day)
    {
        var path = Path.Combine(_tempDir, $"{day}.json");
        File.WriteAllText(path, "[]");
        return path;
    }

    [Fact]
    public void BuildMonth_EmptyFolder_ReturnsNoRows()
    {
        var result = CreateService().BuildMonth(2025, 1, _tempDir);

        result.Rows.Should().BeEmpty();
    }

    [Fact]
    public void BuildMonth_NullFromStorage_DoesNotThrow()
    {
        // Документирует баг 1.2: до фикса упадёт с NullReferenceException
        var path = CreateDayFile(5);
        _storage.Load<List<DayUserDto>>(path).Returns((List<DayUserDto>)null!);

        var act = () => CreateService().BuildMonth(2025, 1, _tempDir);

        act.Should().NotThrow();
    }

    [Fact]
    public void BuildMonth_CountsOnlyActivePlacement()
    {
        var activeId = Guid.NewGuid();
        var path = CreateDayFile(10);
        _storage.Load<List<DayUserDto>>(path).Returns(
        [
            new DayUserDto(activeId, "Alice", 8, DayUserPlacement.Active),
            new DayUserDto(Guid.NewGuid(), "Bob",   4, DayUserPlacement.Reserve),
        ]);

        var result = CreateService().BuildMonth(2025, 1, _tempDir);

        result.Rows.Should().ContainSingle(r => r.UserId == activeId);
        result.Rows.Should().NotContain(r => r.UserName == "Bob");
    }

    [Fact]
    public void BuildMonth_InvalidDayInFileName_IsIgnored()
    {
        // день 32 не существует ни в одном месяце — TryParseDay должен вернуть false
        File.WriteAllText(Path.Combine(_tempDir, "32.json"), "[]");
        _storage.Load<List<DayUserDto>>(Arg.Any<string>()).Returns(
        [
            new DayUserDto(Guid.NewGuid(), "Ghost", 8, DayUserPlacement.Active)
        ]);

        var result = CreateService().BuildMonth(2025, 1, _tempDir);

        result.Rows.Should().BeEmpty();
    }
}
