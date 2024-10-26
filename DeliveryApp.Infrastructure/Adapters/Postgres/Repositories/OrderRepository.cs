using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task Add(Order order, CancellationToken cancellationToken = default)
    {
        if (order.Status != null) _dbContext.Attach(order.Status);
        
        await _dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public Task Update(Order order, CancellationToken cancellationToken = default)
    {
        if (order.Status != null) _dbContext.Attach(order.Status);

        _dbContext.Orders.Update(order);
        return Task.CompletedTask;
    }

    public async Task<Order> GetById(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext
            .Orders
            .Include(x => x.Status)
            .SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken: cancellationToken);
        
        return order;
    }

    public async Task<IReadOnlyCollection<Order>> GetAllCreated(CancellationToken cancellationToken = default)
    {
        var orders = _dbContext
            .Orders
            .Include(x => x.Status)
            .Where(o => o.Status == OrderStatus.Created);
        
        return await orders.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> GetAllAssigned(CancellationToken cancellationToken = default)
    {
        var orders = _dbContext
            .Orders
            .Include(x => x.Status)
            .Where(o => o.Status == OrderStatus.Assigned);
        
        return await orders.ToListAsync(cancellationToken);
    }
}