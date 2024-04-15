using AutoMapper;
using Microsoft.EntityFrameworkCore;
using vnLab.Data;
using vnLab.Data.Entities;
using vnLab.Models;

namespace vnLab.Services;

public class PostsService : IPostsService
{
    private readonly vnLabDbContext _context;
    private readonly IMapper _mapper;

    public PostsService(vnLabDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<Post>> GetAllPaging(int? pageNumber, int pageSize)
    {
        var posts = from m in _context.Posts select m;
        return PaginatedList<Post>.Create(await posts.ToListAsync(), pageNumber ?? 1, pageSize);
    }
}