using System.Diagnostics.CodeAnalysis;

namespace LudaFit.Core.Features.Common.Dto;

[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Factory method for Pagination class do not share state and require strict encapsulation.")]
internal sealed record Pagination<TDto>(IReadOnlyCollection<TDto> Items, int CurrentPage, int TotalPages)
{
    public static Pagination<TDto> Empty(int page)
        => new([], page, 0);
}
