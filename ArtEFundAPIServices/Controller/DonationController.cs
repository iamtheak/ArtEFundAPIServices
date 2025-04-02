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

        return CreatedAtAction(nameof(GetDonationById), new { id = donationViewDto.DonationId }, donationViewDto);
    }
}