using FluentValidation;
using FluentValidation.Results;
using LudaFit.Core.Features.Common.Endpoints;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecialistEntity = LudaFit.Domain.Entities.Specialist;

namespace LudaFit.Core.Features.Specialist;

internal static class GetSpecialist
{
    internal sealed record Response(
        int Id,
        string Name,
        string PhotoUrl,
        string Description,
        TimeOnly WorkTimeStart,
        TimeOnly WorkTimeEnd,
        IEnumerable<SocialNetworkDto> SocialNetworks
    );

    internal sealed record SocialNetworkDto(
        int Id,
        string Name,
        string Url,
        string PhotoUrl
    );

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/specialist", Handle)
                .Produces<Response>()
                .ProducesProblem(404)
                .WithTags("Specialist");
        }

        private static async Task<IResult> Handle(
            [FromServices] LudaFitDbContext db,
            CancellationToken cancellationToken)
        {
            Response? response = await db.Specialists
                .AsNoTracking()
                .Include("_socialNetworks")
                .Select(entity => new Response(
                    entity.Id,
                    entity.Name,
                    entity.PhotoUrl,
                    entity.Description,
                    entity.WorkTime.Start,
                    entity.WorkTime.End,
                    EF.Property<List<SocialNetwork>>(entity, "_socialNetworks")
                        .Select(sn => new SocialNetworkDto(
                            sn.Id,
                            sn.Name,
                            sn.Url,
                            sn.PhotoUrl
                        ))
                    )
                )
                .FirstOrDefaultAsync(cancellationToken);

            if (response is null)
            {
                return Results.NotFound();
            }
            
            return Results.Ok(response);
        }
    }
}
