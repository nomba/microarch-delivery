using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using MediatR;
using Quartz;

namespace DeliveryApp.Api.Adapters.BackgroundJobs;

/*
 * Перемещение курьеров (раз в 2 секунды)
 */

[DisallowConcurrentExecution]
public class MoveCouriersJob : IJob
{
    private readonly IMediator _mediator;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MoveCouriersJob(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var moveCourierToOrderCommand = new MoveCouriersCommand();
        await _mediator.Send(moveCourierToOrderCommand);
    }
}