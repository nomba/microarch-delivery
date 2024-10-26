using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class CourierRepository : ICourierRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CourierRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task Add(Courier courier, CancellationToken cancellationToken = default)
    {
        if (courier.Status != null) _dbContext.Attach(courier.Status);
        if (courier.Transport != null) _dbContext.Attach(courier.Transport);

        await _dbContext.Couriers.AddAsync(courier, cancellationToken);
    }

    public Task Update(Courier courier, CancellationToken cancellationToken = default)
    {
        if (courier.Status != null) _dbContext.Attach(courier.Status);
        if (courier.Transport != null) _dbContext.Attach(courier.Transport);

        _dbContext.Couriers.Update(courier);
        return Task.CompletedTask;
    }

    public async Task<Courier> GetById(Guid courierId, CancellationToken cancellationToken = default)
    {
        var courier = await _dbContext
            .Couriers
            .Include(x => x.Status)
            .Include(x => x.Transport)
            .FirstOrDefaultAsync(o => o.Id == courierId, cancellationToken: cancellationToken);
        
        return courier;
    }

    public async Task<IReadOnlyCollection<Courier>> GetAllFree(CancellationToken cancellationToken = default)
    {
        var couriers = _dbContext
            .Couriers
            .Include(x => x.Status)
            .Include(x => x.Transport)
            .Where(o => o.Status == CourierStatus.Free);
        
        return await couriers.ToListAsync(cancellationToken: cancellationToken);
    }
}