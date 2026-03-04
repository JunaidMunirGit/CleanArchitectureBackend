using SharedKernel;

namespace Application.UnitTests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IDateTimeProvider"/> for unit tests.
/// </summary>
public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;
}
