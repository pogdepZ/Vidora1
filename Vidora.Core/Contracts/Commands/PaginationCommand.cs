namespace Vidora.Core.Contracts.Commands;

public record PaginationCommand(
    int Page,
    int Limit
);