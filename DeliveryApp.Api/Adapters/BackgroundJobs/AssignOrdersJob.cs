using DeliveryApp.Core.Application.UseCases.Commands.AssignOrders;
using MediatR;
using Quartz;

namespace DeliveryApp.Api.Adapters.BackgroundJobs;

/*
 * Распределение и назначение заказов на курьеров (раз в 1 секунду)
 */

[DisallowConcurrentExecution]
public class AssignOrdersJob : IJob
{
    private readonly IMediator _mediator;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AssignOrdersJob(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var assignOrdersCommand = new AssignOrdersCommand();
        await _mediator.Send(assignOrdersCommand);
    }
}