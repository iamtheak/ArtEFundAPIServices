using System.Net;
using System.Text;
using ArtEFundAPIServices.Attributes;
using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.Creator;
using ArtEFundAPIServices.DataAccess.Donation;
using ArtEFundAPIServices.DataAccess.Payment;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO;
using ArtEFundAPIServices.DTO.Donation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/[controller]")]
public class DonationController : ControllerBase
{
    private readonly IDonationInterface _donationRepository;
    private readonly ICreatorInterface _creatorRepository;
    private readonly IUserInterface _userRepository;
    private readonly IPaymentInterface _paymentRepository;
    private readonly IEncryptionService _encryptionService;

    public DonationController(IDonationInterface donationRepository, ICreatorInterface creatorRepository,
        IUserInterface userRepository, IPaymentInterface paymentRepository, IEncryptionService encryptionService)
    {
        _donationRepository = donationRepository;
        _creatorRepository = creatorRepository;
        _userRepository = userRepository;
        _paymentRepository = paymentRepository;
        _encryptionService = encryptionService;
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


    [HttpGet("total")]
    [Authorize]
    [RoleCheck("admin")]
    public async Task<ActionResult<int>> GetTotalDonations()
    {
        var donations = await _donationRepository.GetDonationsAsync();
        var total = donations.Sum(d => d.DonationAmount);

        return Ok(total);
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

    [HttpPost("khalti/initiate")]
    public async Task<ActionResult<KhaltiDto>> KhaltiDonationInitiate(KhaltiDonationInitiateDto donationDto)
    {
        
        
        if(donationDto.DonationAmount > 100000)
        {
            return BadRequest("Donation amount cannot exceed 1,00,000");
        }
        var url = "https://dev.khalti.com/api/v2/epayment/initiate/";

        // Create purchase order ID with metadata
        var purchaseOrderId = JsonConvert.SerializeObject(new
        {
            userId = donationDto.UserId ?? 0,
            creatorId = donationDto.CreatorId,
            donationType = "donation",
            nanoId = Guid.NewGuid().ToString(),
            amount = donationDto.DonationAmount
        });


        var creator = await _creatorRepository.GetCreatorById(donationDto.CreatorId);

        if (creator == null)
        {
            return BadRequest("This creator does not exist");
        }

        // Setup default customer info
        var customerName = "Anonymous";
        var customerEmail = "anonymous@customer.com";
        var customerPhone = "9800000000";

        // If user is not anonymous, fetch their details
        if (donationDto.UserId.HasValue && donationDto.UserId.Value > 0)
        {
            var user = await _userRepository.GetUserById(donationDto.UserId.Value);
            if (user != null)
            {
                customerName = user.UserName;
                customerEmail = user.Email ?? "user@artefund.com";
            }
        }

        // Calculate amount in paisa (Khalti uses paisa, not rupees)
        var amountInPaisa = (int)(donationDto.DonationAmount * 100);

        var payload = new
        {
            return_url = "http://localhost:3000/donation-success",
            website_url = "http://localhost:3000/",
            amount = amountInPaisa.ToString(),
            purchase_order_id = purchaseOrderId,
            purchase_order_name = donationDto.DonationMessage ?? "No message", // Store message here
            customer_info = new
            {
                name = customerName,
                email = customerEmail,
                phone = customerPhone
            },
            merchant_name = "ArtEFund",
            merchant_extra = $"Donation to Creator #{donationDto.CreatorId}"
        };

        var jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var client = new HttpClient();
        var creatorApiKey = _encryptionService.Decrypt(creator.ApiKey.EncryptedApiKey);
        client.DefaultRequestHeaders.Add("Authorization", $"key {creatorApiKey}");

        try
        {
            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();


            if (response.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest($"Error with khalti please try again later");
            }

            var khaltidto = JsonConvert.DeserializeObject<KhaltiDto>(responseContent);


            if (khaltidto == null)
            {
                return BadRequest("Failed to deserialize Khalti response");
            }

            return Ok(khaltidto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("khalti/verify")]
    public async Task<ActionResult<DonationViewDto>> VerifyKhaltiPayment([FromBody] KhaltiDonationVerifyDto donationDto)
    {
        // Verify the payment with Khalti
        var url = "https://dev.khalti.com/api/v2/epayment/lookup/";
        var payload = new { pidx = donationDto.KhaltiPaymentId };
        var jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "key 98fed20dac3e4f549ee853503a124c0b");

        var response = await client.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            return BadRequest("Payment verification failed");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var khaltiResponse = JsonConvert.DeserializeObject<KhaltiLookUpDto>(responseContent);

        if (khaltiResponse == null)
        {
            return BadRequest("Failed to deserialize Khalti response");
        }

        // Check if payment is completed
        if (khaltiResponse.Status.ToString() != "Completed")
        {
            return BadRequest($"Payment not completed. Status: {khaltiResponse.Status}");
        }

        var existingPayment = await _paymentRepository.GetPaymentByKhaltiId(donationDto.KhaltiPaymentId);

        if (existingPayment != null)
        {
            return BadRequest("Payment already verified");
        }

        var creator = await _creatorRepository.GetCreatorById(donationDto.CreatorId);


        if (creator == null)
        {
            BadRequest("This creator does not exist");
        }


        var user = await _userRepository.GetUserById(donationDto.UserId ?? 0);


        if (user == null && donationDto.UserId != null && donationDto.UserId.Value > 0)
        {
            BadRequest("This user does not exist");
        }


        // Create payment record
        var payment = new PaymentModel
        {
            KhaltiPaymentId = donationDto.KhaltiPaymentId,
            PaymentStatus = khaltiResponse.Status.ToString(),
            PaymentDate = DateTime.Now,
            Amount = donationDto.Amount * 100 // Store in paisa for accuracy
        };

        var createdPayment = await _paymentRepository.CreatePayment(payment);


        if (donationDto.Message == "No message")
        {
            donationDto.Message = null;
        }

        // Create donation with payment info
        var donation = new DonationModel
        {
            DonationAmount = donationDto.Amount,
            DonationMessage = donationDto.Message,
            CreatorId = donationDto.CreatorId,
            UserId = donationDto.UserId > 0 ? donationDto.UserId : null,
            PaymentId = createdPayment.PaymentId
        };

        var createdDonation = await _donationRepository.CreateDonation(donation);

        // Update goals
        var goals = await _donationRepository.GetGoalsByCreatorId(donationDto.CreatorId);
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

        var donationViewDto = new DonationViewDto
        {
            DonationId = createdDonation.DonationId,
            DonationDate = createdDonation.DonationDate,
            DonationAmount = createdDonation.DonationAmount,
            DonationMessage = createdDonation.DonationMessage,
            CreatorId = createdDonation.CreatorId,
            UserId = createdDonation.UserId,
        };

        return CreatedAtAction(nameof(GetDonationById), new { id = donationViewDto.DonationId }, donationViewDto);
    }

    [HttpPost]
    public async Task<ActionResult<DonationViewDto>> CreateDonation(DonationCreateDto donationCreateDto)
    {
        var donation = new DonationModel
        {
            DonationAmount = donationCreateDto.DonationAmount,
            DonationMessage = donationCreateDto.DonationMessage,
            CreatorId = donationCreateDto.CreatorId,
            UserId = donationCreateDto.UserId ?? 0,
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
            return NotFound(new { message = "Goal not found" });
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