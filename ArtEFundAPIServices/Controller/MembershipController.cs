using System.Text;
using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.Creator;
using ArtEFundAPIServices.DataAccess.Membership;
using ArtEFundAPIServices.DataAccess.Payment;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO;
using ArtEFundAPIServices.DTO.EnrolledMembership;
using ArtEFundAPIServices.DTO.Membership;
using ArtEFundAPIServices.Mapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/[controller]")]
public class MembershipController : ControllerBase
{
    private readonly IMembershipInterface _membershipRepository;
    private readonly IUserInterface _userRepository;
    private readonly ICreatorInterface _creatorRepository;
    private readonly IPaymentInterface _paymentRepository;

    public MembershipController(IMembershipInterface membershipRepository, IUserInterface userRepository,
        ICreatorInterface creatorRepository, IPaymentInterface paymentRepository)
    {
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _creatorRepository = creatorRepository;
        _paymentRepository = paymentRepository;
    }

    // GET: api/Membership/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipViewDto>> GetMembership(int id)
    {
        var membership = await _membershipRepository.GetMembershipById(id);

        if (membership == null)
        {
            return NotFound();
        }

        return MembershipMapper.MapToViewDto(membership);
    }

    // GET: api/Membership/creator/5
    [HttpGet("creator/{creatorId}")]
    public async Task<ActionResult<IEnumerable<MembershipViewDto>>> GetMembershipsByCreator(int creatorId)
    {
        var memberships = await _membershipRepository.GetMembershipsByCreatorId(creatorId);

        if (memberships == null || memberships.Count == 0)
        {
            return NotFound();
        }

        return memberships.Select(m => MembershipMapper.MapToViewDto(m)).ToList();
    }

    [HttpGet("creator/growth/{creatorId}")]
    public async Task<ActionResult<IEnumerable<MembershipViewDto>>> GetMembershipsByCreatorGrowth(int creatorId)
    {
        var memberships = await _membershipRepository.GetEnrolledMembershipsByCreatorId(creatorId);

        if (memberships == null || memberships.Count == 0)
        {
            return NotFound();
        }

        var growth = memberships.GroupBy(em => em.EnrolledDate)
            .Select(g => new MembershipGrowthDto()
            {
                Date = g.Key,
                MemberCount = g.Count()
            }).ToList();

        return Ok(growth);
    }


    [HttpGet("creator/userName/{userName}")]
    public async Task<ActionResult<IEnumerable<MembershipViewDto>>> GetMembershipsByCreator(string userName)
    {
        var memberships = await _membershipRepository.GetMembershipsByUserName(userName);

        if (memberships == null || memberships.Count == 0)
        {
            return NotFound();
        }

        return memberships.Select(m => MembershipMapper.MapToViewDto(m)).ToList();
    }

    // POST: api/Membership
    [HttpPost]
    public async Task<ActionResult<MembershipViewDto>> CreateMembership(MembershipCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var creator = await _creatorRepository.GetCreatorById(createDto.CreatorId);

        if (creator == null)
        {
            return NotFound(new { message = "Creator not found" });
        }

        var membershipModel = MembershipMapper.MapToModel(createDto);

        var createdMembership = await _membershipRepository.CreateMembership(membershipModel);

        return CreatedAtAction(
            nameof(GetMembership),
            new { id = createdMembership.MembershipId },
            MembershipMapper.MapToViewDto(createdMembership));
    }

    // PUT: api/Membership/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMembership(int id, MembershipUpdateDto updateDto)
    {
        if (id != updateDto.MembershipId)
        {
            return BadRequest("ID in the URL does not match ID in the request body");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingMembership = await _membershipRepository.GetMembershipById(id);
        if (existingMembership == null)
        {
            return NotFound();
        }

        // Apply updates from DTO to existing entity
        MembershipMapper.ApplyUpdates(updateDto, existingMembership);

        var updatedMembership = await _membershipRepository.UpdateMembership(existingMembership);

        if (updatedMembership == null)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/Membership/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMembership(int id)
    {
        var result = await _membershipRepository.DeleteMembership(id);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("khalti/initiate")]
    public async Task<ActionResult<KhaltiDto>> KhaltiMembershipInitiate([FromBody] EnrollMembershipDto membershipDto)
    {
        var membership = await _membershipRepository.GetMembershipById(membershipDto.MembershipId);

        if (membership == null)
        {
            return NotFound(new { message = "Membership not found" });
        }

        var user = await _userRepository.GetUserById(membershipDto.UserId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var creator = await _creatorRepository.GetCreatorById(membership.CreatorId);
        if (creator == null)
        {
            return NotFound(new { message = "Creator not found" });
        }

        if (creator.UserId == user.UserId)
        {
            return BadRequest(new { message = "Creator cannot enroll in their own membership" });
        }

        var creatorMembers = await _membershipRepository.GetEnrolledMembershipsByCreatorId(membership.CreatorId);
        var isAlreadyEnrolled = creatorMembers.Any(em => em.UserId == user.UserId && em.IsActive);
        if (isAlreadyEnrolled && membershipDto.Type == EnrollmentType.New)
        {
            return BadRequest(new { message = "User already enrolled in a membership of the creator" });
        }

        var url = "https://dev.khalti.com/api/v2/epayment/initiate/";

        var purchaseOrderId = JsonConvert.SerializeObject(new
        {
            userId = user.UserId,
            membershipId = membership.MembershipId,
            membershipType = membershipDto.Type == EnrollmentType.New ? "New" : "Upgrade",
            nanoId = Guid.NewGuid().ToString(),
        });

        // Setup default customer info
        var customerName = "Anonymous";
        var customerEmail = "anonymous@customer.com";
        var customerPhone = "9800000000";

        customerName = user.UserName;
        customerEmail = user.Email ?? "user@artefund.com";

        var amountInPaisa = (int)(membership.MembershipAmount * 100);

        var payload = new
        {
            return_url = "http://localhost:3000/membership-success",
            website_url = "http://localhost:3000/",
            amount = amountInPaisa.ToString(),
            purchase_order_id = purchaseOrderId,
            purchase_order_name = $"membership of {membership.MembershipId}",
            customer_info = new
            {
                name = customerName,
                email = customerEmail,
                phone = customerPhone
            },
            merchant_name = "ArtEFund",
        };

        var jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "key 98fed20dac3e4f549ee853503a124c0b");


        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            var response = await client.PostAsync(url, content, cts.Token);
            var responseContent = await response.Content.ReadAsStringAsync();

            var khaltidto = JsonConvert.DeserializeObject<KhaltiDto>(responseContent);

            if (khaltidto == null)
            {
                return BadRequest("Failed to deserialize Khalti response");
            }

            return Ok(khaltidto);
        }
        catch (OperationCanceledException)
        {
            return BadRequest(new { message = "Khalti payment initiation timed out" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error verifying the request", error = ex.Message });
        }
        finally
        {
            client.Dispose();
        }
    }


    [HttpPost("khalti/verify")]
    public async Task<ActionResult<EnrollMembershipDto>> KhaltiMembershipVerify(
        [FromBody] KhaltiMembershipVerifyDto membershipVerifyDto)
    {
        var url = "https://dev.khalti.com/api/v2/epayment/lookup/";
        var payload = new { pidx = membershipVerifyDto.KhaltiPaymentId };
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

        var existingPayment = await _paymentRepository.GetPaymentByKhaltiId(membershipVerifyDto.KhaltiPaymentId);

        if (existingPayment != null)
        {
            return BadRequest("Payment already verified");
        }

        var user = await _userRepository.GetUserById(membershipVerifyDto.UserId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var membership = await _membershipRepository.GetMembershipById(membershipVerifyDto.MembershipId);

        if (membership == null)
        {
            return NotFound(new { message = "Membership not found" });
        }

        if (membershipVerifyDto.Type != EnrollmentType.New)
        {
            var allEnrollments =
                await _membershipRepository.GetEnrolledMembershipsByUserIdAndCreatorId(user.UserId,
                    membership.CreatorId);
            var currentEnrollment = allEnrollments.FirstOrDefault(em => em.IsActive);


            if (currentEnrollment == null)
            {
                return NotFound(new { message = "User not enrolled in the membership cannot upgrage" });
            }

            if (currentEnrollment.Membership.MembershipTier > membership.MembershipTier)
            {
                return BadRequest(new { message = "Cannot downgrade membership while active membership" });
            }

            var endResult = await _membershipRepository.EndMembership(currentEnrollment);

            if (!endResult)
            {
                return BadRequest(new { message = "Old Membership failed to end" });
            }
        }


        var paymentModel = new PaymentModel
        {
            KhaltiPaymentId = membershipVerifyDto.KhaltiPaymentId,
            Amount = khaltiResponse.TotalAmount,
            PaymentDate = DateTime.Now,
            PaymentStatus = "Completed"
        };

        var paymentResult = await _paymentRepository.CreatePayment(paymentModel);

        var enrolledMembership = new EnrolledMembershipModel
        {
            UserId = user.UserId,
            MembershipId = membership.MembershipId,
            EnrolledDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddMonths(1),
            IsActive = true,
            PaidAmount = membership.MembershipAmount,
            PaymentId = paymentResult.PaymentId,
        };
        var result = await _membershipRepository.EnrollMembership(enrolledMembership);
        return Ok(new { message = "Enrolled" });
    }

    [HttpPost("enroll")]
    public async Task<ActionResult<EnrolledMembershipModel>> EnrollMembership(
        [FromBody] EnrollMembershipDto enrollMembership)
    {
        var userId = enrollMembership.UserId;
        var membershipId = enrollMembership.MembershipId;

        var membership = await _membershipRepository.GetMembershipById(membershipId);
        if (membership == null)
        {
            return NotFound(new { message = "Membership not found" });
        }

        var user = await _userRepository.GetUserById(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var creatorMembers = await _membershipRepository.GetEnrolledMembershipsByCreatorId(membership.CreatorId);
        var isAlreadyEnrolled = creatorMembers.Any(em => em.UserId == userId && em.IsActive);
        if (isAlreadyEnrolled)
        {
            return BadRequest(new { message = "User already enrolled in a membership of the creator" });
        }

        var enrolledMembership = new EnrolledMembershipModel
        {
            UserId = userId,
            MembershipId = membershipId,
            EnrolledDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddMonths(1),
            IsActive = true
        };
        var result = await _membershipRepository.EnrollMembership(enrolledMembership);
        return Ok(new { message = "Enrolled" });
    }

    [HttpPost("change")]
    public async Task<ActionResult<EnrolledMembershipModel>> ChangeMembership([FromBody] ChangeMembershipDto changeDto)
    {
        int newMembershipId = changeDto.MembershipId;
        int membershipId = changeDto.EnrolledMembershipId;
        int userId = changeDto.UserId;

        var membership = await _membershipRepository.GetMembershipById(newMembershipId);

        if (membership == null)
        {
            return NotFound(new { message = "New Membership not found" });
        }

        var user = await _userRepository.GetUserById(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var enrolledMembership =
            await _membershipRepository.GetEnrolledMembershipByUserIdAndMembershipId(userId, membershipId);
        if (enrolledMembership == null)
        {
            return NotFound(new { message = "User not enrolled in the membership" });
        }

        if (enrolledMembership.Membership.MembershipTier > membership.MembershipTier && enrolledMembership.IsActive)
        {
            return BadRequest(new { message = "Cannot downgrade membership while active membership" });
        }

        var result = await _membershipRepository.ChangeMembership(enrolledMembership, newMembershipId);
        return Ok(new { message = "Upgraded" });
    }


    [HttpPost("end")]
    public async Task<ActionResult> EndMembership([FromBody] int enrolledMembershipiD)
    {
        var enrolledMembership =
            await _membershipRepository.GetEnrolledMembershipById(enrolledMembershipiD);
        if (enrolledMembership == null)
        {
            return NotFound();
        }

        var result = await _membershipRepository.EndMembership(enrolledMembership);
        if (result)
        {
            return Ok(new { message = "Membership ended" });
        }

        return NotFound();
    }

    [HttpGet("enrolled/creator/{creatorId}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>> GetEnrolledMembershipsByCreatorId(
        int creatorId)
    {
        var enrolledMemberships = await _membershipRepository.GetEnrolledMembershipsByCreatorId(creatorId);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/username/{userName}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>> GetEnrolledMembershipsByUserName(
        string userName)
    {
        var enrolledMemberships = await _membershipRepository.GetEnrolledMembershipsByUserName(userName);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/{id}")]
    public async Task<ActionResult<EnrolledMembershipViewDto>> GetEnrolledMembershipById(int id)
    {
        var enrolledMembership = await _membershipRepository.GetEnrolledMembershipById(id);

        if (enrolledMembership == null)
        {
            return NotFound();
        }

        return MembershipMapper.MapToViewDto(enrolledMembership);
    }

    [HttpGet("enrolled/membership/{membershipId}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>> GetEnrolledMembershipsByMembershipId(
        int membershipId)
    {
        var enrolledMemberships = await _membershipRepository.GetEnrolledMembershipsByMembershipId(membershipId);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/membership/{membershipId}/creator/{creatorId}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>>
        GetEnrolledMembershipsByMembershipIdAndCreatorId(int membershipId, int creatorId)
    {
        var enrolledMemberships =
            await _membershipRepository.GetEnrolledMembershipsByMembershipIdAndCreatorId(membershipId, creatorId);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/user/{userId}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>> GetEnrolledMembershipsByUserId(int userId)
    {
        var enrolledMemberships = await _membershipRepository.GetEnrolledMembershipsByUserId(userId);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/user/{userId}/membership/{membershipId}")]
    public async Task<ActionResult<EnrolledMembershipViewDto>> GetEnrolledMembershipByUserIdAndMembershipId(int userId,
        int membershipId)
    {
        var enrolledMembership =
            await _membershipRepository.GetEnrolledMembershipByUserIdAndMembershipId(userId, membershipId);

        if (enrolledMembership == null)
        {
            return NotFound();
        }

        return MembershipMapper.MapToViewDto(enrolledMembership);
    }
}