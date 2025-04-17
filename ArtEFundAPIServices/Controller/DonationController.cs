using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.Creator;
using ArtEFundAPIServices.DataAccess.Donation;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO.Donation;
using Microsoft.AspNetCore.Mvc;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/[controller]")]
public class DonationController : ControllerBase
{
    private readonly IDonationInterface _donationRepository;
    private readonly ICreatorInterface _creatorRepository;
    private readonly IUserInterface _userRepository;

    public DonationController(IDonationInterface donationRepository, ICreatorInterface creatorRepository,
        IUserInterface userRepository)
    {
        _donationRepository = donationRepository;
        _creatorRepository = creatorRepository;
        _userRepository = userRepository;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DonationViewDto>> GetDonationById(int id)
    {
        var donation = await _donationRepository.GetDonationById(id);
        if (donation == null)
        {
            return NotFound();
        }


        var donationViewDto = new DonationViewDto
        {
            DonationId = donation.DonationId,
            DonationDate = donation.DonationDate,
            DonationAmount = donation.DonationAmount,
            DonationMessage = donation.DonationMessage,
            CreatorId = donation.CreatorId,
            UserId = donation.UserId
        };

        if (donation.UserId != null && donation.UserId != 0)
        {
            var user = await _userRepository.GetUserById((int)donation.UserId);

            if (user != null)
            {
                donationViewDto.userName = user.UserName;
            }
        }

        return Ok(donationViewDto);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<DonationViewDto>>> GetDonationsByUserId(int userId)
    {
        var donations = await _donationRepository.GetDonationsByUserId(userId);
        var donationViewDtos = donations.Select(donation => new DonationViewDto
        {
            DonationId = donation.DonationId,
            DonationDate = donation.DonationDate,
            DonationAmount = donation.DonationAmount,
            DonationMessage = donation.DonationMessage,
            CreatorId = donation.CreatorId,
            UserId = donation.UserId,
            userName = donation.Creator.UserModel.UserName
        }).ToList();

        return Ok(donationViewDtos);
    }

    [HttpGet("creator/{creatorId}")]
    public async Task<ActionResult<List<DonationViewDto>>> GetDonationsByCreatorId(int creatorId)
    {
        var donations = await _donationRepository.GetDonationsByCreatorId(creatorId);

        List<DonationViewDto> donationViewDtos = new List<DonationViewDto>();

        foreach (var donation in donations)
        {
            var donationDto = new DonationViewDto
            {
                DonationId = donation.DonationId,
                DonationDate = donation.DonationDate,
                DonationAmount = donation.DonationAmount,
                DonationMessage = donation.DonationMessage,
                CreatorId = donation.CreatorId,
                UserId = donation.UserId
            };
            if (donation.UserId != null && donation.UserId != 0)
            {
                var user = await _userRepository.GetUserById((int)donation.UserId);

                if (user != null)
                {
                    donationDto.userName = user.UserName;
                }
            }

            donationViewDtos.Add(donationDto);
        }


        return Ok(donationViewDtos);
    }

    [HttpGet("user/userName/{userName}")]
    // Gets creators donations by userName
    public async Task<ActionResult<List<DonationViewDto>>> GetDonationsByUserName(string userName)
    {
        var donations = await _donationRepository.GetDonationsByUserName(userName);
        var donationViewDtos = donations.Select(donation => new DonationViewDto
        {
            DonationId = donation.DonationId,
            DonationDate = donation.DonationDate,
            DonationAmount = donation.DonationAmount,
            DonationMessage = donation.DonationMessage,
            CreatorId = donation.CreatorId,
            UserId = donation.UserId
        }).ToList();

        return Ok(donationViewDtos);
    }

    [HttpPost]
    public async Task<ActionResult<DonationViewDto>> CreateDonation(DonationCreateDto donationCreateDto)
    {
        var donation = new DonationModel
        {
            DonationAmount = donationCreateDto.DonationAmount,
            DonationMessage = donationCreateDto.DonationMessage,
            CreatorId = donationCreateDto.CreatorId,
            UserId = donationCreateDto.UserId ?? 0
        };

        var createdDonation = await _donationRepository.CreateDonation(donation);

        var donationViewDto = new DonationViewDto
        {
            DonationId = createdDonation.DonationId,
            DonationDate = createdDonation.DonationDate,
            DonationAmount = createdDonation.DonationAmount,
            DonationMessage = createdDonation.DonationMessage,
            CreatorId = createdDonation.CreatorId,
            UserId = createdDonation.UserId
        };

        var goals = await _donationRepository.GetGoalsByCreatorId(donation.CreatorId);

        var activeGoal = goals.FirstOrDefault(g => g.IsGoalActive && !g.IsGoalReached);

        if (activeGoal != null)
        {
            activeGoal.GoalProgress += donation.DonationAmount;
            if (activeGoal.GoalProgress >= activeGoal.GoalAmount)
            {
                activeGoal.IsGoalReached = true;
            }

            await _donationRepository.UpdateDonationGoal(activeGoal);
        }

        return CreatedAtAction(nameof(GetDonationById), new { id = donationViewDto.DonationId }, donationViewDto);
    }

    [HttpPost("goal")]
    public async Task<ActionResult<GoalModel>> CreateDonationGoal([FromBody] GoalModel donationGoal)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var creator = await _creatorRepository.GetCreatorById(donationGoal.CreatorId);

        if (creator == null)
        {
            return NotFound(new { message = "Creator not found" });
        }

        var goals = await _donationRepository.GetGoalsByCreatorId(donationGoal.CreatorId);

        if (goals.Any(g => g.IsGoalActive && !g.IsGoalReached))
        {
            return BadRequest(new { message = "A goal is already active for this creator." });
        }

        donationGoal.IsGoalActive = true;
        donationGoal.IsGoalReached = false;

        var createdGoal = await _donationRepository.CreateDonationGoal(donationGoal);
        return CreatedAtAction(nameof(GetDonationById), new { id = createdGoal.GoalId }, createdGoal);
    }

    [HttpGet("goal/{goalId}")]
    public async Task<ActionResult<GoalModel>> GetDonationGoalById(int goalId)
    {
        var goal = await _donationRepository.GetDonationGoalById(goalId);
        if (goal == null)
        {
            return NotFound();
        }

        return Ok(goal);
    }

    [HttpGet("goal/creator/{creatorId}")]
    public async Task<ActionResult<List<GoalModel>>> GetGoalsByCreatorId(int creatorId)
    {
        var goals = await _donationRepository.GetGoalsByCreatorId(creatorId);
        return Ok(goals);
    }

    [HttpPut("goal")]
    public async Task<ActionResult<GoalModel>> UpdateDonationGoal(GoalModel donationGoal)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingGoal = await _donationRepository.GetDonationGoalById(donationGoal.GoalId);

        if (existingGoal == null)
        {
            return NotFound(new { message = "Goal not found" });
        }

        var creator = await _creatorRepository.GetCreatorById(donationGoal.CreatorId);

        if (creator == null)
        {
            return NotFound(new { message = "Creator not found" });
        }

        if (donationGoal.CreatorId != creator.CreatorId)
        {
            return BadRequest(new { message = "Creator id not match" });
        }

        if (!donationGoal.IsGoalActive)
        {
            return BadRequest(new { message = "Goal is not active" });
        }
        existingGoal.GoalDescription = donationGoal.GoalDescription;
        existingGoal.GoalAmount = donationGoal.GoalAmount;
        existingGoal.GoalTitle = donationGoal.GoalTitle;
        
        var updatedGoal = await _donationRepository.UpdateDonationGoal(existingGoal);
        if (updatedGoal == null)
        {
            return NotFound();
        }

        return Ok(updatedGoal);
    }

    [HttpGet("goal/active/{creatorId}")]
    public async Task<ActionResult<GoalModel>> GetActiveDonationGoalByCreatorId(int creatorId)
    {
        var goals = await _donationRepository.GetGoalsByCreatorId(creatorId);   
        var activeGoal = goals.FirstOrDefault(g => g.IsGoalActive);
        if (activeGoal == null)
        {
            return NotFound();
        }

        return Ok(activeGoal);
    }

    [HttpDelete("goal/{goalId}")]
    public async Task<ActionResult<GoalModel>> DeleteDonationGoal(int goalId)
    {
        var goal = await _donationRepository.GetDonationGoalById(goalId);
        if (goal == null)
        {
            return NotFound();
        }

        var deletedGoal = await _donationRepository.DeleteDonationGoal(goal);
        if (deletedGoal == null)
        {
            return NotFound();
        }

        return Ok(deletedGoal);
    }

    [HttpPatch("goal/inactive/{goalId}")]
    public async Task<ActionResult<GoalModel>> InactivateDonationGoal(int goalId)
    {
        var goal = await _donationRepository.GetDonationGoalById(goalId);
        if (goal == null)
        {
            return NotFound( new { message = "Goal not found" });
        }

        goal.IsGoalActive = false;
        goal.IsGoalReached = false;

        var updatedGoal = await _donationRepository.UpdateDonationGoal(goal);
        if (updatedGoal == null)
        {
            return NotFound();
        }

        return Ok(updatedGoal);
    }
}