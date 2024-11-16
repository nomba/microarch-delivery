using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Grpc.GeoService;

public class GeoClient : IGeoClient
{
    public Task<Result<Location, Error>> GetLocation(string street, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}