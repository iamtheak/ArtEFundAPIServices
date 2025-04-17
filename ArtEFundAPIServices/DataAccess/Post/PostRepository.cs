using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;

namespace ArtEFundAPIServices.DataAccess.Post;

public class PostRepository : IPostInterface
{
    private readonly ApplicationDbContext _context;

    public PostRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PostModel> CreatePost(PostModel post)
    {
        string slug;
        do
        {
            slug = await Nanoid.GenerateAsync(size: 12); // customizable length
        } while (await _context.Posts.AnyAsync(p => p.PostSlug == slug));

        post.PostSlug = slug;
        post.CreatedAt = DateTime.UtcNow;

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return post;
    }

    public async Task<PostModel?> GetPost(int id)
    {
        return await _context.Posts
            .Include(p => p.Creator)
            .ThenInclude(c => c.UserModel)
            .FirstOrDefaultAsync(p => p.PostId == id);
    }

    public async Task<List<PostModel>> GetPostsByCreatorId(int creatorId)
    {
        return await _context.Posts
            .Where(p => p.CreatorId == creatorId && !p.IsDeleted)
            .Include(p => p.Creator)
            .ThenInclude(c => c.UserModel)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PostModel?> UpdatePost(PostModel post)
    {
        post.UpdatedAt = DateTime.UtcNow;
        _context.Entry(post).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(post.PostId))
            {
                return null;
            }

            throw;
        }

        return post;
    }

    public async Task<bool> DeletePost(int id)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
        {
            return false;
        }

        post.IsDeleted = true;
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PostModel?> GetPostBySlug(string slug)
    {
        return await _context.Posts
            .Include(p => p.Creator)
            .ThenInclude(c => c.UserModel)
            .FirstOrDefaultAsync(p => p.PostSlug == slug);
    }

    public async Task<PostLikeModel?> GetPostLike(int postId)
    {
        return await _context.PostLikes
            .Include(pl => pl.User)
            .FirstOrDefaultAsync(pl => pl.PostId == postId);
    }

    public async Task<PostLikeModel> CreatePostLike(PostLikeModel postLike)
    {
        postLike.LikedAt = DateTime.UtcNow;
        _context.PostLikes.Add(postLike);
        await _context.SaveChangesAsync();
        return postLike;
    }

    public async Task<bool> DeletePostLike(int postId, int userId)
    {
        var like = await _context.PostLikes
            .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);

        if (like == null)
        {
            return false;
        }

        _context.PostLikes.Remove(like);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<PostLikeModel>> GetPostLikesByUserId(int userId)
    {
        return await _context.PostLikes
            .Where(pl => pl.UserId == userId)
            .Include(pl => pl.Post)
            .ToListAsync();
    }

    public async Task<List<PostLikeModel>> GetPostLikesByPostId(int postId)
    {
        return await _context.PostLikes
            .Where(pl => pl.PostId == postId)
            .Include(pl => pl.User)
            .ToListAsync();
    }

    public async Task<PostCommentModel?> GetPostComment(int commentId)
    {
        return await _context.PostComments
            .Include(pc => pc.User)
            .FirstOrDefaultAsync(pc => pc.CommentId == commentId);
    }

    public async Task<List<PostCommentModel>> GetPostCommentsByPostId(int postId)
    {
        return await _context.PostComments
            .Where(pc => pc.PostId == postId && !pc.IsDeleted)
            .Include(pc => pc.User)
            .OrderByDescending(pc => pc.CommentedAt)
            .ToListAsync();
    }

    public async Task<PostCommentModel> CreatePostComment(PostCommentModel postComment)
    {
        postComment.CommentedAt = DateTime.UtcNow;
        _context.PostComments.Add(postComment);
        await _context.SaveChangesAsync();
        return postComment;
    }

    public async Task<PostCommentModel?> UpdatePostComment(PostCommentModel postComment)
    {
        postComment.UpdatedAt = DateTime.UtcNow;
        _context.Entry(postComment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CommentExists(postComment.CommentId))
            {
                return null;
            }

            throw;
        }

        return postComment;
    }

    public async Task<bool> DeletePostComment(int commentId)
    {
        var comment = await _context.PostComments.FindAsync(commentId);

        if (comment == null)
        {
            return false;
        }

        comment.IsDeleted = true;
        _context.PostComments.Update(comment);
        await _context.SaveChangesAsync();

        return true;
    }

    public Task<List<PostModel>> GetPostsByUserName(string userName)
    {
        return _context.Posts
            .Include(p => p.Creator)
            .ThenInclude(c => c.UserModel)
            .Where(p => p.Creator.UserModel.UserName == userName && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }


    private bool PostExists(int id)
    {
        return _context.Posts.Any(p => p.PostId == id);
    }

    private bool CommentExists(int id)
    {
        return _context.PostComments.Any(c => c.CommentId == id);
    }
}