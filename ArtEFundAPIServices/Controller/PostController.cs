using System.Security.Claims;
using ArtEFundAPIServices.Attributes;
using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.Membership;
using ArtEFundAPIServices.DataAccess.Post;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO.Post;
using ArtEFundAPIServices.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("/api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostInterface _postRepository;
    private readonly IUserInterface _userRepository;
    private readonly IMembershipInterface _membershipRepository;

    public PostController(IPostInterface postRepository, IUserInterface userRepository,
        IMembershipInterface membershipRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
    }

    // GET: api/Post/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostViewDto>> GetPost(int id)
    {
        var post = await _postRepository.GetPost(id);
        if (post == null)
        {
            return NotFound();
        }

        var likes = await _postRepository.GetPostLikesByPostId(id);
        var comments = await _postRepository.GetPostCommentsByPostId(id);
        var postDto = PostMapper.ToViewDto(post, likes.Count, comments.Count);

        return Ok(postDto);
    }

    [HttpGet("top")]
    public async Task<ActionResult<List<PostViewDto>>> GetTopPosts(int count = 10)
    {
        var posts = await _postRepository.GetTopPosts(count);
        if (posts == null || posts.Count == 0)
        {
            return NotFound();
        }

        var postDtos = new List<PostViewDto>();
        foreach (var post in posts)
        {
            var likes = await _postRepository.GetPostLikesByPostId(post.PostId);
            var comments = await _postRepository.GetPostCommentsByPostId(post.PostId);
            postDtos.Add(PostMapper.ToViewDto(post, likes.Count, comments.Count));
        }

        return Ok(postDtos);
    }

    // GET: api/Post/slug/{slug}
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<PostViewDto>> GetPostBySlug(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return BadRequest("Slug cannot be empty");
        }

        var post = await _postRepository.GetPostBySlug(slug);
        if (post == null)
        {
            return NotFound();
        }

        var likes = await _postRepository.GetPostLikesByPostId(post.PostId);
        var comments = await _postRepository.GetPostCommentsByPostId(post.PostId);
        var postDto = PostMapper.ToViewDto(post, likes.Count, comments.Count);

        return Ok(postDto);
    }

    // GET: api/Post/creator/{creatorId}
    [HttpGet("creator/{creatorId:int}")]
    [Authorize]
    public async Task<ActionResult<List<PostViewDto>>> GetPostsByCreatorId(int creatorId)
    {
        var posts = await _postRepository.GetPostsByCreatorId(creatorId);
        var postDtos = new List<PostViewDto>();

        foreach (var post in posts)
        {
            var likes = await _postRepository.GetPostLikesByPostId(post.PostId);
            var comments = await _postRepository.GetPostCommentsByPostId(post.PostId);
            postDtos.Add(PostMapper.ToViewDto(post, likes.Count, comments.Count));
        }

        return Ok(postDtos);
    }

    // POST: api/Post
    [HttpPost]
    [Authorize]
    [RoleCheck("creator")]
    public async Task<ActionResult<PostViewDto>> CreatePost([FromBody] PostCreateDto postDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }


        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var postModel = PostMapper.ToModel(postDto, userId);
        var createdPost = await _postRepository.CreatePost(postModel);
        var createdPostDto = PostMapper.ToViewDto(createdPost);

        return CreatedAtAction(nameof(GetPost), new { id = createdPost.PostId }, createdPostDto);
    }

    // PUT: api/Post/{id}
    [HttpPut("{id:int}")]
    [Authorize]
    [RoleCheck("creator")]
    public async Task<ActionResult<PostViewDto>> UpdatePost(int id, [FromBody] PostUpdateDto postDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var existingPost = await _postRepository.GetPost(id);
        if (existingPost == null)
        {
            return NotFound();
        }

        if (existingPost.CreatorId != userId)
        {
            return new JsonResult(new { message = "You can only update your own posts" })
            {
                StatusCode = 403
            };
        }

        PostMapper.UpdateModelFromDto(existingPost, postDto);
        var updatedPost = await _postRepository.UpdatePost(existingPost);

        if (updatedPost == null)
        {
            return NotFound();
        }

        var likes = await _postRepository.GetPostLikesByPostId(id);
        var comments = await _postRepository.GetPostCommentsByPostId(id);
        var updatedPostDto = PostMapper.ToViewDto(updatedPost, likes.Count, comments.Count);

        return Ok(updatedPostDto);
    }

    [HttpPost("view")]
    public async Task<ActionResult<PostViewDto>> ViewPost(int postId)
    {
        var post = await _postRepository.GetPost(postId);
        if (post == null)
        {
            return NotFound();
        }

        post.Views += 1;
        var updatedPost = await _postRepository.UpdatePost(post);
        if (updatedPost == null)
        {
            return NotFound();
        }

        return Ok(true);
    }

    // DELETE: api/Post/{id}
    [HttpDelete("{id:int}")]
    [Authorize]
    [RoleCheck("creator")]
    public async Task<ActionResult> DeletePost(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var existingPost = await _postRepository.GetPost(id);
        if (existingPost == null)
        {
            return NotFound();
        }

        if (existingPost.CreatorId != userId)
        {
            return new JsonResult(new { message = "You can only delete your own posts" })
            {
                StatusCode = 403
            };
        }

        var result = await _postRepository.DeletePost(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{postId:int}/view/{userId:int}")]
    public async Task<ActionResult<bool>> CanUserViewPost(int postId, int userId)
    {
        var post = await _postRepository.GetPost(postId);
        if (post == null)
        {
            return Ok(false);
        }

        if (!post.IsMembersOnly)
        {
            return Ok(true);
        }

        var user = await _userRepository.GetUserById(userId);
        if (user == null)
        {
            return Ok(false);
        }

        if (post.Creator.UserModel.UserId == user.UserId)
        {
            return Ok(true);
        }

        if (user.RoleModel.RoleName == "admin")
        {
            return Ok(true);
        }

        var enrolledMembershipModels =
            await _membershipRepository.GetEnrolledMembershipsByUserIdAndCreatorId(userId, post.CreatorId);


        var currentEnrollment = enrolledMembershipModels.FirstOrDefault(em => em.IsActive);


        if (currentEnrollment == null)
        {
            return Ok(false);
        }

        if (post.MembershipTier <= currentEnrollment.Membership.MembershipTier)
        {
            return Ok(true);
        }

        return Ok(false);
    }

    [HttpGet("user/{userName}")]
    public async Task<ActionResult<List<PostModel>>> GetPostsByUserName(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return BadRequest("User name cannot be empty");
        }

        var posts = await _postRepository.GetPostsByUserName(userName);
        if (posts == null || posts.Count == 0)
        {
            return NotFound();
        }

        var postDtos = new List<PostViewDto>();
        foreach (var post in posts)
        {
            var likes = await _postRepository.GetPostLikesByPostId(post.PostId);
            var comments = await _postRepository.GetPostCommentsByPostId(post.PostId);
            postDtos.Add(PostMapper.ToViewDto(post, likes.Count, comments.Count));
        }

        return Ok(postDtos);
    }

    // POST: api/Post/like
    [HttpPost("like")]
    [Authorize]
    public async Task<ActionResult<PostLikeDto>> CreatePostLike([FromBody] PostLikeCreateDto likeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var likeModel = PostMapper.ToModel(likeDto, userId);
        var createdLike = await _postRepository.CreatePostLike(likeModel);
        var likeViewDto = PostMapper.ToDto(createdLike);

        return Ok(likeViewDto);
    }


    // DELETE: api/Post/like/{postId}/{userId}
    [HttpDelete("like/{postId:int}/{userId:int}")]
    [Authorize]
    public async Task<ActionResult> DeletePostLike(int postId, int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int currentUserId))
        {
            return Unauthorized();
        }

        if (userId != currentUserId)
        {
            return new JsonResult(new { message = "You can only remove your own likes" })
            {
                StatusCode = 403
            };
        }

        var result = await _postRepository.DeletePostLike(postId, userId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    // GET: api/Post/likes/post/{postId}
    [HttpGet("likes/post/{postId:int}")]
    public async Task<ActionResult<List<PostLikeDto>>> GetPostLikesByPostId(int postId)
    {
        var likes = await _postRepository.GetPostLikesByPostId(postId);
        var likeDtos = likes.Select(PostMapper.ToDto).ToList();
        return Ok(likeDtos);
    }

    // POST: api/Post/comment
    // POST: api/Post/comment
    [HttpPost("comment")]
    [Authorize]
    public async Task<ActionResult<PostCommentViewDto>> CreatePostComment([FromBody] PostCommentCreateDto commentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var commentModel = PostMapper.ToModel(commentDto, userId);
        var createdComment = await _postRepository.CreatePostComment(commentModel);

        // Fetch the complete comment with user information
        var completeComment = await _postRepository.GetPostComment(createdComment.CommentId);
        if (completeComment == null)
        {
            return StatusCode(500, "Created comment couldn't be retrieved");
        }

        var commentViewDto = PostMapper.ToDto(completeComment);

        return CreatedAtAction(nameof(GetPostComment), new { commentId = completeComment.CommentId }, commentViewDto);
    }

    // GET: api/Post/comment/{commentId}
    [HttpGet("comment/{commentId:int}")]
    public async Task<ActionResult<PostCommentViewDto>> GetPostComment(int commentId)
    {
        var comment = await _postRepository.GetPostComment(commentId);
        if (comment == null)
        {
            return NotFound();
        }

        return Ok(PostMapper.ToDto(comment));
    }

    // GET: api/Post/comments/{postId}
    [HttpGet("comments/{postId:int}")]
    public async Task<ActionResult<List<PostCommentViewDto>>> GetPostCommentsByPostId(int postId)
    {
        var comments = await _postRepository.GetPostCommentsByPostId(postId);
        var commentDtos = comments.Select(PostMapper.ToDto).ToList();
        return Ok(commentDtos);
    }

    // PUT: api/Post/comment/{id}
    [HttpPut("comment/{id:int}")]
    [Authorize]
    public async Task<ActionResult<PostCommentViewDto>> UpdatePostComment(int id,
        [FromBody] PostCommentUpdateDto commentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var existingComment = await _postRepository.GetPostComment(id);
        if (existingComment == null)
        {
            return NotFound();
        }

        if (existingComment.UserId != userId)
        {
            return new JsonResult(new { message = "You can only update your own comments" })
            {
                StatusCode = 403
            };
        }

        PostMapper.UpdateModelFromDto(existingComment, commentDto);
        var updatedComment = await _postRepository.UpdatePostComment(existingComment);

        if (updatedComment == null)
        {
            return NotFound();
        }

        return Ok(PostMapper.ToDto(updatedComment));
    }


    // DELETE: api/Post/comment/{commentId}
    [HttpDelete("comment/{commentId:int}")]
    [Authorize]
    public async Task<ActionResult> DeletePostComment(int commentId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var existingComment = await _postRepository.GetPostComment(commentId);
        if (existingComment == null)
        {
            return NotFound();
        }

        if (existingComment.UserId != userId)
        {
            return new JsonResult(new { message = "You can only delete your own comments" })
            {
                StatusCode = 403
            };
        }

        var result = await _postRepository.DeletePostComment(commentId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}