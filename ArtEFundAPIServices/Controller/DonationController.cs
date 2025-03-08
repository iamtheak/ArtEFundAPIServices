using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.Donation;
using ArtEFundAPIServices.DTO.Donation;
using Microsoft.AspNetCore.Mvc;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/[controller]")]
public class DonationController : ControllerBase
{
    private readonly IDonationInterface _donationRepository;

    public DonationController(IDonationInterface donationRepository)
    {
        _donationRepository = donationRepository;
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

        return Ok(donationViewDto);
    }

    [HttpGet("creator/{creatorId}")]
    public async Task<ActionResult<List<DonationViewDto>>> GetDonationsByCreatorId(int creatorId)
    {
        var donations = await _donationRepository.GetDonationsByCreatorId(creatorId);
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

    [HttpGet("user/{userName}")]
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
            UserId = donationCreateDto.UserId
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