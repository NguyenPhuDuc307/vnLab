using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using vnLab.Data;
using vnLab.Data.Entities;
using vnLab.Helpers;
using vnLab.Models;
using vnLab.Services;

namespace vnLab.Controllers
{
    public class PostsController : Controller
    {
        private readonly vnLabDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IPostsService _postsService;
        private readonly IMapper _mapper;

        public PostsController(vnLabDbContext context, UserManager<User> userManager, IMapper mapper, IPostsService postsService)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _postsService = postsService;
        }

        // GET: Posts
        public async Task<IActionResult> Index(int? pageNumber)
        {
            return View(await _postsService.GetAllPaging(pageNumber, 20));
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.PostTags!)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Tăng số lần xem của bài viết
            post.Viewed += 1;
            await _context.SaveChangesAsync();

            // Lấy danh sách các thẻ của bài viết
            var postTags = post!.PostTags!.Select(pt => pt.Tag!.Name).ToList();

            // Lấy người dùng hiện tại
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Lấy danh sách các thẻ của người dùng
            var userTags = await _context.UserTags
                .Where(ut => ut.UserId == currentUser.Id)
                .Include(ut => ut.Tag)
                .ToListAsync();

            // Thêm hoặc cập nhật các thẻ của người dùng
            foreach (var postTag in post.PostTags!)
            {
                var userTag = userTags.FirstOrDefault(ut => ut.TagId == postTag.TagId);
                if (userTag != null)
                {
                    userTag.Number += 1; // Cập nhật số lần xuất hiện của thẻ
                }
                else
                {
                    // Thêm mới thẻ vào danh sách thẻ của người dùng
                    userTag = new UserTag
                    {
                        UserId = currentUser.Id,
                        TagId = postTag.TagId,
                        Number = 1
                    };
                    _context.UserTags.Add(userTag);
                }
            }
            await _context.SaveChangesAsync();

            return View(post);
        }
        private async Task<List<string?>> GetUserTags(string userId)
        {
            return await _context.UserTags
                .Where(ut => ut.UserId == userId)
                .Select(ut => ut.TagId)
                .ToListAsync();
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Asked,Modified,Viewed")] Post post)
        {
            if (ModelState.IsValid)
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Import(PostImportRequest request)
        {
            if (request.File != null)
            {
                List<PostCSV> dataList = new List<PostCSV>();
                using (TextFieldParser parser = new TextFieldParser(request.File!.OpenReadStream()))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    // skip the header line
                    parser.ReadLine();
                    int idCounter = 1;

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields()!;
                        PostCSV data = new PostCSV();
                        data.Id = int.Parse(fields[0]);
                        data.Title = fields[1];
                        data.Body = fields[2];
                        data.Tags = fields[3].Split(new char[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);
                        data.CreationDate = DateTime.ParseExact(fields[4], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        data.Y = fields[5];
                        dataList.Add(data);
                        idCounter++;
                    }
                }

                List<PostCreateRequest> listPostRQ = _mapper.Map<List<PostCSV>, List<PostCreateRequest>>(dataList);

                foreach (var postRQ in listPostRQ)
                {
                    Post post = _mapper.Map<PostCreateRequest, Post>(postRQ);
                    _context.Posts.Add(post);
                    await _context.SaveChangesAsync();
                    //Process label
                    if (postRQ.Tags?.Length > 0)
                    {
                        await ProcessLabel(postRQ, post);
                    }

                    var result = await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(request);
        }

        private async Task ProcessLabel(PostCreateRequest request, Post post)
        {
            if (request.Tags != null)
            {
                foreach (var labelText in request.Tags)
                {
                    if (labelText == null) continue;
                    var tagId = TextHelper.ToUnsignedString(labelText.ToString()); var existingLabel = await _context.Tags.FindAsync(tagId);
                    if (existingLabel == null)
                    {
                        var labelEntity = new Tag()
                        {
                            Id = tagId,
                            Name = labelText.ToString()
                        };
                        _context.Tags.Add(labelEntity);
                    }
                    if (await _context.PostTags.FindAsync(tagId, post.Id) == null)
                    {
                        _context.PostTags.Add(new PostTag()
                        {
                            PostId = post.Id,
                            TagId = tagId
                        });
                    }
                }
            }
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Asked,Modified,Viewed")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> Rating(int id, int point)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var existingRating = await _context.Interactions.FirstOrDefaultAsync(x => x.PostId == id && x.UserId == user.Id);
                if (existingRating != null)
                {
                    // Đánh giá đã tồn tại, sửa đổi nó và lưu thay đổi
                    existingRating.Rating = point;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Đánh giá chưa tồn tại, thêm mới đánh giá
                    var post = await _context.Posts.FindAsync(id);
                    if (post != null)
                    {
                        post.Viewed -= 1; // Giảm số lần xem của bài viết

                        Interaction interaction = new Interaction
                        {
                            PostId = id,
                            Rating = point,
                            UserId = user.Id,
                            TimeStamp = DateTime.Now
                        };
                        _context.Add(interaction);
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }


        public async Task<IActionResult> Favorite()
        {
            return View(await _context.UserTags.Include(x => x.User).OrderByDescending(x => x.Number)
                .ToListAsync());
        }

    }
}
