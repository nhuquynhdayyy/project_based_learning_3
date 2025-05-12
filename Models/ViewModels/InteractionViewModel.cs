// using System;
// using System.Collections.Generic;
// using TourismWeb.Models;

// namespace TourismWeb.Models.ViewModels
// {
//     public class InteractionViewModel
//     {
//         public InteractionStats Stats { get; set; }
//         public List<PostInteractionModel> TopPosts { get; set; }
//         public List<SpotInteractionModel> TopSpots { get; set; }
//         public List<UserInteractionModel> TopUsers { get; set; }
//         public Dictionary<string, List<int>> TimeSeriesData { get; set; }
//         public Dictionary<string, int> DistributionData { get; set; }
//         public Dictionary<string, Dictionary<string, int>> PostTypeData { get; set; }
//     }

//     public class InteractionStats
//     {
//         public int PostLikes { get; set; }
//         public int SpotLikes { get; set; }
//         public int PostShares { get; set; }
//         public int SpotShares { get; set; }
//         public int TotalInteractions => PostLikes + SpotLikes + PostShares + SpotShares;
        
//         public double PostLikesGrowth { get; set; }
//         public double SpotLikesGrowth { get; set; }
//         public double PostSharesGrowth { get; set; }
//         public double SpotSharesGrowth { get; set; }
//     }

//     public class PostInteractionModel
//     {
//         public int PostId { get; set; }
//         public string Title { get; set; }
//         public string PostType { get; set; }
//         public int Likes { get; set; }
//         public int Shares { get; set; }
//         public int TotalInteractions => Likes + Shares;
//     }

//     public class SpotInteractionModel
//     {
//         public int SpotId { get; set; }
//         public string Name { get; set; }
//         public string Region { get; set; }
//         public int Likes { get; set; }
//         public int Shares { get; set; }
//         public int TotalInteractions => Likes + Shares;
//     }

//     public class UserInteractionModel
//     {
//         public int UserId { get; set; }
//         public string UserName { get; set; }
//         public string Email { get; set; }
//         public string AvatarUrl { get; set; }
//         public int PostLikes { get; set; }
//         public int SpotLikes { get; set; }
//         public int PostShares { get; set; }
//         public int SpotShares { get; set; }
//         public int TotalInteractions => PostLikes + SpotLikes + PostShares + SpotShares;
//     }
// }