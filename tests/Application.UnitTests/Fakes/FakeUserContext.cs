using Application.Abstractions.Authentication;

namespace Application.UnitTests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IUserContext"/> for unit tests.
/// </summary>
public sealed class FakeUserContext : IUserContext
{
    public Guid UserId { get; set; }
}
