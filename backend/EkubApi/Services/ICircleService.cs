using EkubApi.DTOs;

namespace EkubApi.Services;

public interface ICircleService
{
    Task<CircleDetailDto> CreateCircleAsync(CreateCircleDto dto, int organizerId);
    Task<List<CircleSummaryDto>> GetMyCirclesAsync(int userId);
    Task<CircleDetailDto?> GetCircleByIdAsync(int circleId, int userId);
    Task<MemberDto> AddMemberAsync(int circleId, AddMemberDto dto, int organizerId);
    Task RemoveMemberAsync(int circleId, int userId, int organizerId);
    Task<CircleDetailDto> StartCircleAsync(int circleId, int organizerId);
    Task<MemberHomeDto?> GetMemberHomeAsync(int circleId, int userId);
    Task<List<PublicCircleSummaryDto>> GetPublicCirclesAsync(int userId, int? categoryId);
    Task<JoinRequestDto> SubmitJoinRequestAsync(int circleId, int userId, SubmitJoinRequestDto dto);
    Task<List<JoinRequestDto>> GetJoinRequestsAsync(int circleId, int organizerId);
    Task<JoinRequestDto> ReviewJoinRequestAsync(int circleId, int requestId, int organizerId, ReviewJoinRequestDto dto);
}
