using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace LudaFit.Core.Features.Common.Parameters;

internal readonly record struct PaginationParams(
    [FromQuery, Range(1, int.MaxValue)] int Page = 1,
    [FromQuery, Range(1, 100)] int PageSize = 30
);
