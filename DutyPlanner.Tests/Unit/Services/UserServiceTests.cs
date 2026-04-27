using DutyPlanner.Domain.Repositories;
using DutyPlanner.Models;
using DutyPlanner.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DutyPlanner.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();

    private UserService CreateService(IEnumerable<User>? seed = null)
    {
        _repo.GetAll().Returns((seed ?? []).ToList().AsReadOnly());
        return new UserService(_repo);
    }

    [Fact]
    public void GetAll_ReturnsAllUsersFromRepository()
    {
        var users = new[] { new User(Guid.NewGuid(), "Alice", 8) };
        var sut = CreateService(users);

        sut.GetAll().Should().BeEquivalentTo(users);
    }

    [Fact]
    public void Add_CreatesUserWithCorrectNameAndHours()
    {
        var sut = CreateService();

        var user = sut.Add("Bob", 12);

        user.Name.Should().Be("Bob");
        user.Hours.Should().Be(12);
    }

    [Fact]
    public void Add_AssignsNonEmptyGuid()
    {
        var sut = CreateService();

        var user = sut.Add("Alice", 8);

        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Add_CallsSaveOnRepository()
    {
        var sut = CreateService();

        sut.Add("Alice", 8);

        _repo.Received(1).Save(Arg.Any<IEnumerable<User>>());
    }

    [Fact]
    public void Remove_SavesListWithoutRemovedUser()
    {
        var id = Guid.NewGuid();
        var users = new[]
        {
            new User(id, "Alice", 8),
            new User(Guid.NewGuid(), "Bob", 4)
        };
        var sut = CreateService(users);

        sut.Remove(id);

        _repo.Received().Save(Arg.Is<IEnumerable<User>>(u => u.All(x => x.Id != id)));
    }

    [Fact]
    public void Update_ReplacesExistingUserById()
    {
        var id = Guid.NewGuid();
        var sut = CreateService([new User(id, "Alice", 8)]);

        sut.Update(new User(id, "Alice Updated", 12));

        sut.GetAll().Should().ContainSingle(u => u.Name == "Alice Updated" && u.Hours == 12);
    }
}
