using ArtEFundAPIServices.Attributes;
using ArtEFundAPIServices.DataAccess.Creator;
using ArtEFundAPIServices.DataAccess.Donation;
using ArtEFundAPIServices.DataAccess.Membership;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtEFundAPIServices.Controller;

// [Authorize]
[ApiController]
[Route("/api/[controller]/[action]")]
public class HomeController : ControllerBase
{
    private readonly IUserInterface _userInterface;
    private readonly IDonationInterface _donationInterface;
    private readonly ICreatorInterface _creatorInterface;
    private readonly IMembershipInterface _membershipInterface;


    public HomeController(IUserInterface userInterface, IDonationInterface donationInterface,
        ICreatorInterface creatorInterface, IMembershipInterface membershipInterface)
    {
        _userInterface = userInterface;
        _donationInterface = donationInterface;
        _creatorInterface = creatorInterface;
        _membershipInterface = membershipInterface;
    }


    [HttpGet]
    public async Task<IActionResult> GetTotalMembers(int creatorId)
    {
        var creator = await _creatorInterface.GetCreatorById(creatorId);
        if (creator == null)
        {
            return NotFound("Creator not found.");
        }

        var memberships = await _membershipInterface.GetEnrolledMembershipsByCreatorId(creatorId);
        var totalMembers = memberships.Count;

        return Ok(totalMembers);
    }


    [HttpGet]
    public async Task<IActionResult> GetTotalDonations(int creatorId)
    {
        var creator = await _creatorInterface.GetCreatorById(creatorId);
        if (creator == null)
        {
            return NotFound("Creator not found.");
        }

        var donations = await _donationInterface.GetDonationsByCreatorId(creatorId);
        var totalDonations = donations.Sum(d => d.DonationAmount);

        return Ok(totalDonations);
    }

    [HttpGet]
    public async Task<IActionResult> GetProfileViews(int creatorId)
    {
        var creator = await _creatorInterface.GetCreatorById(creatorId);

        if (creator == null)
        {
            return NotFound("Creator not found.");
        }

        int profileViews = creator.ProfileVisits;

        return Ok(profileViews);
    }

    [HttpGet("admin/daily-donations")]
    [RoleCheck("admin")]
    public async Task<IActionResult> GetDailyDonationsAdmin()
    {
        var donations = await _donationInterface.GetDonationsAsync();

        var dailyDonations = donations
            .GroupBy(d => d.DonationDate.Date)
            .Select(g => new DailyDonationDto
            {
                Date = DateOnly.FromDateTime(g.Key),
                Donations = g.Sum(d => d.DonationAmount)
            })
            .ToList();

        return Ok(dailyDonations);
    }

    [HttpGet("admin/top-earners")]
    [RoleCheck("admin")]
    public async Task<IActionResult> GetTopEarners()
    {
        var creators = await _creatorInterface.GetCreators();

         List<TopEarnerDto> topEarners = new List<TopEarnerDto>();

        foreach (var creator in creators)
        {
            var donations = await _donationInterface.GetDonationsByCreatorId(creator.CreatorId);
            var totalDonations = donations.Sum(d => d.DonationAmount);

            topEarners.Add(new TopEarnerDto
            {
                Name = $"{creator.UserModel.FirstName} {creator.UserModel.LastName} ",
                TotalEarnings = totalDonations,
                ProfilePicture = creator.UserModel.ProfilePicture,
                CreatorId = creator.CreatorId
            });
        }

        topEarners = topEarners.OrderByDescending(te => te.TotalEarnings).Take(6).ToList();

        return Ok(topEarners);
    }

    [HttpGet]
    public async Task<IActionResult> GetDailyDonations(int creatorId)
    {
        var creator = await _creatorInterface.GetCreatorById(creatorId);
        if (creator == null)
        {
            return NotFound("Creator not found.");
        }

        var donations = await _donationInterface.GetDonationsByCreatorId(creatorId);

        var dailyDonations = donations
            .GroupBy(d => d.DonationDate.Date)
            .Select(g => new DailyDonationDto
            {
                Date = DateOnly.FromDateTime(g.Key),
                Donations = g.Sum(d => d.DonationAmount)
            })
            .ToList();

        return Ok(dailyDonations);
    }

    [HttpGet]
    public async Task<IActionResult> GetTopDonators(int creatorId)
    {
        var creator = await _creatorInterface.GetCreatorById(creatorId);
        if (creator == null)
        {
            return NotFound("Creator not found.");
        }

        var donations = await _donationInterface.GetDonationsByCreatorId(creatorId);

        // Sort the donations by ascending order of donation amount and get all the donations
        var topDonators = donations
            .Select(g => new DonatorDto
            {
                Amount = g.DonationAmount,
                Id = g.DonationId,
                UserId = g.UserId,
                Message = g.DonationMessage
            })
            .OrderByDescending(d => d.Amount)
            .ToList();

        topDonators = topDonators.Take(6).ToList();

        foreach (var topDonator in topDonators)
        {
            if (!topDonator.UserId.HasValue)
            {
                topDonator.Name = "Anonymous";
                continue;
            }

            var user = await _userInterface.GetUserById(topDonator.UserId.Value);
            if (user != null)
            {
                topDonator.Name = user.UserName;
                topDonator.AvatarUrl = user.ProfilePicture;
            }
            else
            {
                topDonator.Name = "Anonymous";
            }
        }

        return Ok(topDonators);
    }

    [HttpGet]
    public async Task<IActionResult> GetDonationSources(int creatorId)
    {
        var donations = await _donationInterface.GetDonationsByCreatorId(creatorId);


        var sourceDto = new DonationSourceDto();
        foreach (var donation in donations)
        {
            if (donation.UserId.HasValue)
            {
                sourceDto.UserTotal += donation.DonationAmount;
            }
            else
            {
                sourceDto.AnonymousTotal += donation.DonationAmount;
            }
        }

        return Ok(sourceDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetNewMembers(int creatorId)
    {
        var creator = await _creatorInterface.GetCreatorById(creatorId);
        if (creator == null)
        {
            return NotFound("Creator not found.");
        }

        var memberships = await _membershipInterface.GetEnrolledMembershipsByCreatorId(creatorId);

        var newMembers = memberships
            .Where(m => m.IsActive && m.ExpiryDate > DateTime.UtcNow.Date)
            .Select(m => new MemberDto
            {
                Id = m.UserId,
                AvatarUrl = m.User.ProfilePicture,
                MembershipTier = m.Membership.MembershipTier,
                Name = m.User.FirstName + " " + m.User.LastName,
                MembershipName = m.Membership.MembershipName,
                JoinDate = m.EnrolledDate
            })
            .ToList();

        newMembers = newMembers.Take(6).ToList();

        return Ok(newMembers);
    }
}