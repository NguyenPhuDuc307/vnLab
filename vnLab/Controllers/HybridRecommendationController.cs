using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnLab.Data;
using vnLab.Data.Entities;

namespace vnLab.Controllers
{
    public class HybridRecommendationController : Controller
    {
        private readonly vnLabDbContext _context;
        private readonly UserManager<User> _userManager;

        public HybridRecommendationController(vnLabDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var collaborativeFilteringRecommendations = await RecommendArticlesCollaborative(50);
            var contentBasedRecommendations = await RecommendArticlesContentBased(50);

            // Kết hợp kết quả từ cả hai thuật toán
            var combinedRecommendations = collaborativeFilteringRecommendations
                .Union(contentBasedRecommendations)
                .Distinct()
                .ToList();

            return View(combinedRecommendations);
        }

        public async Task<List<Recommendation>> RecommendArticlesCollaborative(int numRecommendations)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Thực hiện thuật toán Collaborative Filtering để đề xuất bài viết
            var currentUserRatings = await _context.Interactions
                .Where(r => r.UserId == currentUser.Id)
                .Select(r => r.PostId)
                .ToListAsync();

            if (currentUserRatings.Count == 0)
            {
                throw new InvalidOperationException("User has no ratings in the database.");
            }

            var recommendations = new List<Recommendation>();

            var allPosts = await _context.Posts.ToListAsync();
            foreach (var post in allPosts)
            {
                double ratingScore = currentUserRatings.Contains(post.Id) ? 1 : 0;
                int viewCount = post.Viewed;
                DateTime lastModified = post.Modified;
                int userInteractions = await _context.Interactions
                    .Where(r => r.PostId == post.Id && r.UserId == currentUser.Id)
                    .CountAsync();

                double recommendationScore = CalculateRecommendationScore(ratingScore, viewCount, lastModified, userInteractions);

                recommendations.Add(new Recommendation
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    Asked = post.Asked,
                    Modified = post.Modified,
                    Viewed = post.Viewed,
                    Tags = post.Tags,
                    Score = recommendationScore
                });
            }

            recommendations = recommendations.OrderByDescending(r => r.Score).ToList();

            return recommendations.Take(numRecommendations).ToList();
        }

        public async Task<List<Recommendation>> RecommendArticlesContentBased(int numRecommendations)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Thực hiện thuật toán ContentBased để đề xuất bài viết
            var userTags = await GetUserTags(currentUser.Id, 10);

            var posts = await _context.Posts
                .Where(p => p.Tags != null && p.Tags != "")
                .ToListAsync();

            var recommendedPosts = new List<Recommendation>();

            foreach (var post in posts)
            {
                var postTags = await GetPostTags(post.Id);
                var similarity = ContentBased_CosineSimilarity(userTags!, postTags!);

                // Tính toán điểm đề xuất sử dụng các yếu tố
                double recommendationScore = CalculateRecommendationScore(similarity, post.Viewed, post.Modified);

                // Thêm vào danh sách đề xuất
                recommendedPosts.Add(new Recommendation
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    Asked = post.Asked,
                    Modified = post.Modified,
                    Viewed = post.Viewed,
                    Tags = post.Tags,
                    Score = recommendationScore
                });
            }

            // Sắp xếp danh sách các bài viết theo điểm số giảm dần
            recommendedPosts = recommendedPosts.OrderByDescending(r => r.Score).ToList();

            // Trả về danh sách các bài viết từ các cặp (bài viết, điểm tương đồng)
            return recommendedPosts.Take(numRecommendations).ToList();
        }
        private double CalculateRecommendationScore(double ratingScore, int viewCount, DateTime lastModified, int userInteractions)
        {
            double recommendationScore = (ratingScore * 0.4) + (viewCount * 0.3) + (userInteractions * 0.2) + ((DateTime.UtcNow - lastModified).TotalDays * 0.1);
            return recommendationScore;
        }
        private async Task<List<string>> GetUserTags(string userId, int topCount)
        {
            var topTags = await _context.UserTags
                .Where(ut => ut.UserId == userId)
                .OrderByDescending(ut => ut.Number)
                .Select(ut => ut.TagId)
                .Take(topCount)
                .ToListAsync();

            return topTags!;
        }

        private async Task<List<string?>> GetPostTags(int postId)
        {
            return await _context.PostTags
                .Where(pt => pt.PostId == postId)
                .Select(pt => pt.Tag!.Name)
                .ToListAsync();
        }

        private double ContentBased_CosineSimilarity(List<string> currentUserTags, List<string> postTags)
        {
            // Tính toán độ tương đồng cosine giữa vector tag của người dùng và bài viết
            var allTags = currentUserTags.Union(postTags).ToList();
            var vector1 = new double[allTags.Count];
            var vector2 = new double[allTags.Count];

            for (int i = 0; i < allTags.Count; i++)
            {
                string tag = allTags[i];
                vector1[i] = currentUserTags.Count(t => t == tag);
                vector2[i] = postTags.Count(t => t == tag);
            }

            // Tính toán dot product giữa hai vector
            double dotProduct = 0;
            for (int i = 0; i < allTags.Count; i++)
            {
                dotProduct += vector1[i] * vector2[i];
            }

            // Tính độ dài (norm) của mỗi vector
            double norm1 = Math.Sqrt(vector1.Sum(x => x * x));
            double norm2 = Math.Sqrt(vector2.Sum(x => x * x));

            // Tính độ tương đồng cosine
            if (norm1 == 0 || norm2 == 0)
            {
                return 0;
            }
            else
            {
                return dotProduct / (norm1 * norm2);
            }
        }
        private double CalculateRecommendationScore(double similarity, int viewCount, DateTime lastModified)
        {
            // Đặt các trọng số cho các yếu tố
            double similarityWeight = 0.6;
            double viewCountWeight = 0.3;
            double lastModifiedWeight = 0.1;

            // Tính toán điểm đề xuất sử dụng các yếu tố và trọng số
            double recommendationScore = (similarity * similarityWeight) + (viewCount * viewCountWeight) + (CalculateLastModifiedScore(lastModified) * lastModifiedWeight);

            return recommendationScore;
        }

        private double CalculateLastModifiedScore(DateTime lastModified)
        {
            // Giả sử càng gần đây bài viết được cập nhật, điểm số càng cao
            TimeSpan timeSinceLastModified = DateTime.Now - lastModified;
            double daysSinceLastModified = timeSinceLastModified.TotalDays;

            // Điểm số tăng dần theo thời gian cập nhật gần đây
            double lastModifiedScore = Math.Exp(-daysSinceLastModified / 30); // Sử dụng một hàm mũ để đảm bảo điểm số giảm dần theo thời gian

            return lastModifiedScore;
        }
    }
}
