using DuelApp.Modules.Users.Shared.Dto;

namespace DuelApp.Modules.Duels.Application.Models;

public record DuelCompletedDto(
    UserInfo PlayerOneDetails,
    int PlayerOneScore,
    UserInfo PlayerTwoDetails,
    int PlayerTwoScore,
    bool IsDraw,
    Guid? WinnerId
);