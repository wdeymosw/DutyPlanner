using DutyPlanner.Models;
using DutyPlanner.Models.Dto;
using DutyPlanner.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DutyPlanner.Tests.Unit.Services;

public class YearStatisticsServiceTests
{
    private readonly IUserService _users = Substitute.For<IUserService>();
    private readonly IMonthStatisticsService _monthStats = Substitute.For<IMonthStatisticsService>();
    private readonly IMonthManagementService _monthMgmt = Substitute.For<IMonthManagementService>();

    private YearStatisticsService CreateService() =>
        new(_users, _monthStats, _monthMgmt);

    private static MonthDescriptor Descriptor(int year, int month) =>
        new() { Year = year, Month = month };

    private static MonthStatisticsDto EmptyMonthDto(int year, int month) =>
        new() { Year = year, Month = month, FolderPath = "", Days = [], Rows = [] };

    [Fact]
    public void BuildPeriod_CallsBuildMonthOnlyForExistingMonths()
    {
        _users.GetAll().Returns([]);
        _monthMgmt.LoadExistingMonths().Returns([Descriptor(2025, 3)]);
        _monthMgmt.GetFolderPath(Arg.Any<int>(), Arg.Any<int>()).Returns(string.Empty);
        _monthStats.BuildMonth(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
                   .Returns(ci => EmptyMonthDto(ci.ArgAt<int>(0), ci.ArgAt<int>(1)));

        CreateService().BuildPeriod(new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

        _monthStats.Received(1).BuildMonth(2025, 3, Arg.Any<string>());
        _monthStats.DidNotReceive().BuildMonth(2025, 1, Arg.Any<string>());
    }

    [Fact]
    public void BuildPeriod_SkipsMonthsOutsideDateRange()
    {
        _users.GetAll().Returns([]);
        // 2024-12 существует, но лежит за пределами запрошенного диапазона
        _monthMgmt.LoadExistingMonths().Returns([Descriptor(2024, 12), Descriptor(2025, 1)]);
        _monthMgmt.GetFolderPath(Arg.Any<int>(), Arg.Any<int>()).Returns(string.Empty);
        _monthStats.BuildMonth(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
                   .Returns(ci => EmptyMonthDto(ci.ArgAt<int>(0), ci.ArgAt<int>(1)));

        var result = CreateService().BuildPeriod(new DateTime(2025, 1, 1), new DateTime(2025, 6, 30));

        result.Months.Should().NotContain(new YearMonth(2024, 12));
        _monthStats.DidNotReceive().BuildMonth(2024, 12, Arg.Any<string>());
    }

    [Fact]
    public void BuildPeriod_LoadExistingMonths_CalledOnce()
    {
        // Документирует N+1 из фазы 5: до фикса будет вызван N раз (по числу месяцев)
        _users.GetAll().Returns([]);
        _monthMgmt.LoadExistingMonths().Returns([]);

        CreateService().BuildPeriod(new DateTime(2025, 1, 1), new DateTime(2025, 3, 31));

        _monthMgmt.Received(1).LoadExistingMonths();
    }

    [Fact]
    public void BuildPeriod_AggregatesAllUsersIntoRows()
    {
        var userId = Guid.NewGuid();
        _users.GetAll().Returns([new User(userId, "Alice", 8)]);
        _monthMgmt.LoadExistingMonths().Returns([]);

        var result = CreateService().BuildPeriod(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));

        result.Rows.Should().ContainSingle(r => r.UserId == userId && r.UserName == "Alice");
    }
}
