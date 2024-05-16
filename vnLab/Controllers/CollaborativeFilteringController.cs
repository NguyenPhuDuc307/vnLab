using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnLab.Data;
using vnLab.Data.Entities;

namespace vnLab.Controllers
{
    public class CollaborativeFilteringController : Controller
    {
        private readonly vnLabDbContext _context;
        private readonly UserManager<User> _userManager;

        public CollaborativeFilteringController(vnLabDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("collaborative-filtering")]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var recommendedPosts = await RecommendArticles(100);

            return View(recommendedPosts);
        }

        public async Task<List<Recommendation>> RecommendArticles(int numRecommendations)
        {
            // Lấy thông tin người dùng hiện tại
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                throw new InvalidOperationException("Không tìm thấy người dùng.");
            }

            // Lấy tất cả các đánh giá của người dùng hiện tại từ cơ sở dữ liệu
            var currentUserRatings = await _context.Interactions
                .Where(r => r.UserId == currentUser.Id)
                .ToDictionaryAsync(r => r.PostId, r => r.Rating);

            if (!currentUserRatings.Any())
            {
                throw new InvalidOperationException("Người dùng không có đánh giá trong cơ sở dữ liệu.");
            }

            // Lấy danh sách tất cả các bài viết và đánh giá từ cơ sở dữ liệu
            var allPosts = await _context.Posts.ToListAsync();
            var allInteractions = await _context.Interactions.ToListAsync();

            var recommendations = new List<Recommendation>();

            foreach (var post in allPosts)
            {
                // Tính điểm đề xuất cho từng bài viết dựa trên cosine similarity
                double cosineSimilarity = CalculateCosineSimilarity(currentUserRatings, post.Id, allInteractions);

                // Thêm bài viết và điểm đề xuất vào danh sách gợi ý
                recommendations.Add(new Recommendation
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    Asked = post.Asked,
                    Modified = post.Modified,
                    Viewed = post.Viewed,
                    Tags = post.Tags,
                    Score = cosineSimilarity
                });
            }

            // Sắp xếp danh sách gợi ý theo điểm đề xuất giảm dần
            recommendations = recommendations.OrderByDescending(r => r.Score).ToList();

            // Trả về số lượng gợi ý theo yêu cầu
            return recommendations.Take(numRecommendations).ToList();
        }

        private double CalculateCosineSimilarity(Dictionary<int, int> currentUserRatings, int postId, List<Interaction> allInteractions)
        {
            // Tìm tất cả các bài đánh giá của những người dùng khác với bài viết cụ thể
            var otherRatings = allInteractions
                .Where(r => r.PostId == postId)
                .ToList();

            // Nếu không có đánh giá nào khác, trả về 0
            if (!otherRatings.Any()) return 0;

            // Tạo vector cho điểm đánh giá của người dùng hiện tại và các người dùng khác
            var userRatingsVector = currentUserRatings.Values.ToArray();
            var otherRatingsVector = otherRatings.Select(r => r.Rating).ToArray();

            // Tính cosine similarity
            double dotProduct = 0, userMagnitude = 0, otherMagnitude = 0;
            for (int i = 0; i < userRatingsVector.Length; i++)
            {
                dotProduct += userRatingsVector[i] * otherRatingsVector[i];
                userMagnitude += Math.Pow(userRatingsVector[i], 2);
                otherMagnitude += Math.Pow(otherRatingsVector[i], 2);
            }

            userMagnitude = Math.Sqrt(userMagnitude);
            otherMagnitude = Math.Sqrt(otherMagnitude);

            return (userMagnitude * otherMagnitude == 0) ? 0 : dotProduct / (userMagnitude * otherMagnitude);
        }
    }
}
