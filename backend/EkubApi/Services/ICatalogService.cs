using EkubApi.DTOs;

namespace EkubApi.Services;

public interface ICatalogService
{
    // Admin: categories
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, int adminId);
    Task<List<CategoryDto>> GetCategoriesAsync(int userId);

    // Admin: sub-categories
    Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto dto, int adminId);
    Task StartEkubAsync(int subCategoryId, int adminId);

    // User: browse
    Task<List<SubCategoryDto>> GetSubCategoriesAsync(int categoryId, int userId);
    Task<SubCategoryDetailDto?> GetSubCategoryByIdAsync(int subCategoryId, int userId);

    // User: join
    Task<JoinResultDto> JoinSubCategoryAsync(int subCategoryId, bool agreedToTerms, int userId);
    Task<EkubSubscriptionDto> SubmitPaymentProofAsync(int subscriptionId, SubmitPaymentProofDto dto, int userId);

    // User: my Ekubs
    Task<List<MyEkubDto>> GetMyEkubsAsync(int userId);
}
