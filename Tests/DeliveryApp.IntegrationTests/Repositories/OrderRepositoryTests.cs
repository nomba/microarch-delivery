using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class OrderRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _databaseFixture;

    // ReSharper disable once ConvertToPrimaryConstructor
    public OrderRepositoryTests()
    {
        _databaseFixture = new DatabaseFixture();
        Substitute.For<IMediator>();
    }

    [Fact]
    public async Task CanAddOrder()
    {
        // Arrange
        
        var order = Order.Create(Guid.NewGuid(), Location.Min).Value;

        // Act

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var orderRepository = new OrderRepository(dbContext);
            var unitOfWork = new UnitOfWork(dbContext);

            await orderRepository.Add(order);
            await unitOfWork.SaveEntities();
        }
        
        // Assert

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var orderRepository = new OrderRepository(dbContext);
            var orderFromDb = await orderRepository.GetById(order.Id);
            
            order.Should().BeEquivalentTo(orderFromDb);
        }
    }

    [Fact]
    public async Task CanUpdateOrder()
    {
        // Arrange
        
        var order = Order.Create(Guid.NewGuid(), Location.Min).Value;
        var courier = Courier.Create("Test Pedestrian", Transport.Pedestrian, Location.Min).Value;
        
        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var orderRepository = new OrderRepository(dbContext);
            var courierRepository = new CourierRepository(dbContext);
            var unitOfWork = new UnitOfWork(dbContext);

            await courierRepository.Add(courier);
            await orderRepository.Add(order);
            await unitOfWork.SaveEntities();
        }
        
        // Act

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            // Update order by assigning courier
            
            var orderAssignToCourierResult = order.Assign(courier);
            orderAssignToCourierResult.IsSuccess.Should().BeTrue();
            
            // Update in database
            
            var orderRepository = new OrderRepository(dbContext);
            var unitOfWork = new UnitOfWork(dbContext);

            await orderRepository.Update(order);
            await unitOfWork.SaveEntities();
        }

        // Assert

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var orderRepository = new OrderRepository(dbContext);
            var orderFromDb = await orderRepository.GetById(order.Id);
            
            order.Should().BeEquivalentTo(orderFromDb);
        }
    }
    
    [Fact]
    public async Task CanGetById()
    {
        // Arrange
        
        var order = Order.Create(Guid.NewGuid(), Location.Min).Value;
    
        // Act

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var orderRepository = new OrderRepository(dbContext);
            await orderRepository.Add(order);
            
            var unitOfWork = new UnitOfWork(dbContext);
            await unitOfWork.SaveEntities();
        }

        // Assert

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var orderRepository = new OrderRepository(dbContext);
            var orderFromDb = await orderRepository.GetById(order.Id);
            
            order.Should().BeEquivalentTo(orderFromDb);
        }
    }
    
    [Fact]
    public async Task CanGetAllCreated()
    {
        // Arrange
        
        var courier = Courier.Create("Test Pedestrian", Transport.Pedestrian, Location.Create(1, 1).Value).Value;

        // Cannot use the Location.Min for both orders due to:
        // 
        // > Instances of owned entity types cannot be shared by multiple owners
        // > (this is a well-known scenario for value objects that cannot be implemented using owned entity types).
        // 
        // https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities#by-design-restrictions
        
        var location1 = Location.Create(1, 1).Value;
        var location2 = Location.Create(1, 1).Value;
        
        var assignedOrder = Order.Create(Guid.NewGuid(), location1).Value;
        var orderAssignToCourierResult = assignedOrder.Assign(courier);
        orderAssignToCourierResult.IsSuccess.Should().BeTrue();
        
        var freeOrder = Order.Create(Guid.NewGuid(), location2).Value;

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            // Add two orders
            
            var orderRepository = new OrderRepository(dbContext);
            
            await orderRepository.Add(assignedOrder);
            await orderRepository.Add(freeOrder);
            
            // Add a courier
            
            var courierRepository = new CourierRepository(dbContext);
            await courierRepository.Add(courier);

            var unitOfWork = new UnitOfWork(dbContext);
            await unitOfWork.SaveEntities();
        }

        // Act

        IReadOnlyCollection<Order> ordersFromDb;
        
        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var orderRepository = new OrderRepository(dbContext);
            ordersFromDb = await orderRepository.GetAllCreated();
        }

        // Assert
        
        ordersFromDb.Should().HaveCount(1);
        ordersFromDb.First().Should().BeEquivalentTo(freeOrder);
    }
    
    [Fact]
    public async Task CanGetAllAssigned()
    {
        // Arrange
        
        var courier = Courier.Create("Test Pedestrian", Transport.Pedestrian, Location.Min).Value;
        
        var location1 = Location.Create(1, 1).Value;
        var location2 = Location.Create(1, 1).Value;
        
        var assignedOrder = Order.Create(Guid.NewGuid(), location1).Value;
        var orderAssignToCourierResult = assignedOrder.Assign(courier);
        orderAssignToCourierResult.IsSuccess.Should().BeTrue();
        
        var freeOrder = Order.Create(Guid.NewGuid(), location2).Value;

        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            // Add two orders
            
            var orderRepository = new OrderRepository(dbContext);
            
            await orderRepository.Add(assignedOrder);
            await orderRepository.Add(freeOrder);
            
            // Add a courier
            
            var courierRepository = new CourierRepository(dbContext);
            await courierRepository.Add(courier);

            var unitOfWork = new UnitOfWork(dbContext);
            await unitOfWork.SaveEntities();
        }

        // Act

        IReadOnlyCollection<Order> ordersFromDb;
        
        await using (var dbContext = _databaseFixture.InstantiateDbContext())
        {
            var orderRepository = new OrderRepository(dbContext);
            ordersFromDb = await orderRepository.GetAllAssigned();
        }

        // Assert
        
        ordersFromDb.Should().HaveCount(1);
        ordersFromDb.First().Should().BeEquivalentTo(assignedOrder);
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