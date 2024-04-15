using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnLab.Data;
using vnLab.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

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

        private double CalculateRecommendationScore(double ratingScore, int viewCount, DateTime lastModified, int userInteractions)
        {
            double recommendationScore = (ratingScore * 0.4) + (viewCount * 0.3) + (userInteractions * 0.2) + ((DateTime.UtcNow - lastModified).TotalDays * 0.1);
            return recommendationScore;
        }
    }
}
