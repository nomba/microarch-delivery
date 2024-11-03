using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class CourierRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _databaseFixture;

    public CourierRepositoryTests()
    {
        _databaseFixture = new DatabaseFixture();
    }
    
    [Fact]
    public async Task CanAddCourier()
    {
        // Arrange
        
        var courierCreateResult = Courier.Create("Test Courier", Transport.Pedestrian, Location.Create(1, 1).Value);
        courierCreateResult.IsSuccess.Should().BeTrue();
        var courier = courierCreateResult.Value;

        // Act

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var courierRepository = new CourierRepository(dbContext);
            var unitOfWork = new UnitOfWork(dbContext);
            
            await courierRepository.Add(courier);
            await unitOfWork.SaveEntities();
        }

        // Assert

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var courierRepository = new CourierRepository(dbContext);
            var courierFromDb = await courierRepository.GetById(courier.Id);
            
            courier.Should().BeEquivalentTo(courierFromDb);
        }
    }

    [Fact]
    public async Task CanUpdateCourier()
    {
        // Arrange
        
        var courierCreateResult = Courier.Create("Test Courier", Transport.Pedestrian, Location.Create(1, 1).Value);
        courierCreateResult.IsSuccess.Should().BeTrue();
        var courier = courierCreateResult.Value;

        var orderCreateResult = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value);
        orderCreateResult.IsSuccess.Should().BeTrue();
        var order = orderCreateResult.Value;

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var courierRepository = new CourierRepository(dbContext);
            var orderRepository = new OrderRepository(dbContext);
            
            await courierRepository.Add(courier);
            await orderRepository.Add(order);
            
            var unitOfWork = new UnitOfWork(dbContext);
            await unitOfWork.SaveEntities();
        }

        // Act
        
        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            // Update courier by taking order
            
            var courierStartWorkResult = courier.Take(order);
            courierStartWorkResult.IsSuccess.Should().BeTrue();
            
            // Update in database
            
            var courierRepository = new CourierRepository(dbContext);
            var unitOfWork = new UnitOfWork(dbContext);
            
            await courierRepository.Update(courier);
            await unitOfWork.SaveEntities();
        }

        // Assert

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var courierRepository = new CourierRepository(dbContext);
            var courierFromDb = await courierRepository.GetById(courier.Id);
            
            courier.Should().BeEquivalentTo(courierFromDb);
            courierFromDb.Status.Should().Be(CourierStatus.Busy);
        }
    }
    
    [Fact]
    public async Task CanGetById()
    {
        // Arrange
        
        var courierCreateResult = Courier.Create("Test Courier", Transport.Pedestrian, Location.Create(1, 1).Value);
        courierCreateResult.IsSuccess.Should().BeTrue();
        var courier = courierCreateResult.Value;
        
        // Act
        
        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var courierRepository = new CourierRepository(dbContext);
            var unitOfWork = new UnitOfWork(dbContext);
            
            await courierRepository.Add(courier);
            await unitOfWork.SaveEntities();
        }
        
        // Assert
        
        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var courierRepository = new CourierRepository(dbContext);
            var courierFromDb = await courierRepository.GetById(courier.Id);
            
            courier.Should().BeEquivalentTo(courierFromDb);
        }
    }
    
    [Fact]
    public async Task CanGetAllFree()
    {
        // Arrange
        
        var orderCreateResult = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value);
        orderCreateResult.IsSuccess.Should().BeTrue();
        var order = orderCreateResult.Value;
        
        var busyCourierCreateResult = Courier.Create("Test Courier 1", Transport.Pedestrian, Location.Create(1, 1).Value);
        busyCourierCreateResult.IsSuccess.Should().BeTrue();
        var busyCourier = busyCourierCreateResult.Value;
        busyCourier.Take(order);
        
        var freeCourierCreateResult = Courier.Create("Test Courier 2", Transport.Pedestrian, Location.Create(1, 1).Value);
        freeCourierCreateResult.IsSuccess.Should().BeTrue();
        var freeCourier = freeCourierCreateResult.Value;
    
        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            // Add two couriers
            
            var courierRepository = new CourierRepository(dbContext);
            
            await courierRepository.Add(busyCourier);
            await courierRepository.Add(freeCourier);
            
            // Add a order
            
            var orderRepository = new OrderRepository(dbContext);
            await orderRepository.Add(order);
            
            var unitOfWork = new UnitOfWork(dbContext);
            await unitOfWork.SaveEntities();
        }
    
        // Act

        IReadOnlyCollection<Courier> couriersFromDb;
        
        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var courierRepository = new CourierRepository(dbContext);
            couriersFromDb = await courierRepository.GetAllFree();
        }

        // Assert
        
        couriersFromDb.Should().HaveCount(1);
        couriersFromDb.First().Should().BeEquivalentTo(freeCourier);
    }

    public Task InitializeAsync()
    {
        return _databaseFixture.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        return _databaseFixture.DisposeAsync();
    }
}