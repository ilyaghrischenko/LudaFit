using LudaFit.Core.Features.Common.Dto;
using LudaFit.Core.Features.Common.Endpoints;
using LudaFit.Core.Features.Common.Extensions;
using LudaFit.Core.Features.Common.Parameters;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.Features.SocialNetwork;

internal static class GetSocialNetworks
{
    internal sealed record Response(
        int Id,
        string Name,
        string Url,
        string PhotoUrl
    );

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/social-networks", Handle)
                .Produces<Pagination<Response>>()
                .WithTags("SocialNetwork");
        }

        private static async Task<IResult> Handle(
            [AsParameters] PaginationParams paginationParams,
            [FromServices] LudaFitDbContext db,
            CancellationToken cancellationToken)
        {
            Pagination<Response> socialNetworks = await db.SocialNetworks
                .AsNoTracking()
                .OrderByDescending(entity => entity.Id)
                .ToPagedListAsync(
                    paginationParams,
                    entity => new Response(
                        entity.Id,
                        entity.Name,
                        entity.Url,
                        entity.PhotoUrl
                    ),
                    cancellationToken
                );

            return Results.Ok(socialNetworks);
        }
    }
}
