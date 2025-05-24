using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.Post;

public interface IPostInterface
{
    Task<PostModel?> GetPost(int id);
    Task<List<PostModel>> GetPostsByCreatorId(int creatorId);

    Task<List<PostModel>> GetPostsByUserName(string userName);
    Task<PostModel> CreatePost(PostModel post);
    Task<PostModel?> UpdatePost(PostModel post);
    Task<bool> DeletePost(int id);
    Task<PostModel?> GetPostBySlug(string slug);
    Task<PostLikeModel?> GetPostLike(int postId);
    Task<PostLikeModel> CreatePostLike(PostLikeModel postLike);
    Task<bool> DeletePostLike(int postId, int userId);
    Task<List<PostLikeModel>> GetPostLikesByUserId(int userId);
    Task<List<PostLikeModel>> GetPostLikesByPostId(int postId);
    Task<PostCommentModel?> GetPostComment(int commentId);
    Task<List<PostCommentModel>> GetPostCommentsByPostId(int postId);
    Task<PostCommentModel> CreatePostComment(PostCommentModel postComment);
    Task<PostCommentModel?> UpdatePostComment(PostCommentModel postComment);
    Task<bool> DeletePostComment(int commentId);
    Task<List<PostModel>> GetTopPosts(int count);
}