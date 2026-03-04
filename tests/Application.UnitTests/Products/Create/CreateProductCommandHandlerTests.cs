using Application.Products.Create;
using Application.UnitTests.Fakes;
using Domain.Products;
using FluentAssertions;
using SharedKernel;
using Xunit;

namespace Application.UnitTests.Products.Create;

public sealed class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsCategoryNotFound()
    {
        await using var context = FakeApplicationDbContext.Create();
        var userContext = new FakeUserContext { UserId = Guid.NewGuid() };
        var dateTimeProvider = new FakeDateTimeProvider { UtcNow = DateTime.UtcNow };

        var handler = new CreateProductCommandHandler(
            context,
            userContext,
            dateTimeProvider);

        var command = new CreateProductCommand
        {
            Sku = "SKU-001",
            Name = "Test Product",
            CategoryId = Guid.NewGuid(),
            UnitOfMeasureId = Guid.NewGuid(),
            TaxCategoryId = Guid.NewGuid(),
            CostPrice = 10,
            SellingPrice = 15,
            ReorderLevel = 5
        };

        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.CategoryNotFound");
    }
}
