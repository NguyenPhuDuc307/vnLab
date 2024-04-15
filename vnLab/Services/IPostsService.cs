using vnLab.Data.Entities;
using vnLab.Models;

namespace vnLab.Services;

public interface IPostsService
{
    Task<PaginatedList<Post>> GetAllPaging(int? pageNumber, int pageSize);
}