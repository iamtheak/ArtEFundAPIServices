using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DTO.EnrolledMembership;
using ArtEFundAPIServices.DTO.Membership;

namespace ArtEFundAPIServices.Mapper;

public static class MembershipMapper
{
    public static MembershipViewDto MapToViewDto(MembershipModel model)
    {
        return new MembershipViewDto
        {
            MembershipId = model.MembershipId,
            MembershipTier = model.MembershipTier,
            MembershipName = model.MembershipName,
            CreatorId = model.CreatorId,
            MembershipAmount = model.MembershipAmount,
            MembershipBenefits = model.MembershipBenefits,
            IsDeleted = model.IsDeleted
        };
    }

    // Map from create DTO to model
    public static MembershipModel MapToModel(MembershipCreateDto dto)
    {
        return new MembershipModel
        {
            MembershipTier = dto.MembershipTier,
            MembershipName = dto.MembershipName,
            CreatorId = dto.CreatorId,
            MembershipAmount = dto.MembershipAmount,
            MembershipBenefits = dto.MembershipBenefits,
            IsDeleted = false // Default value
        };
    }

    // Apply updates from update DTO to model
    public static void ApplyUpdates(MembershipUpdateDto dto, MembershipModel model)
    {
        // Only update properties that are not null
        if (dto.MembershipTier.HasValue)
            model.MembershipTier = dto.MembershipTier.Value;

        if (!String.IsNullOrEmpty(dto.MembershipName))
            model.MembershipName = dto.MembershipName;

        if (dto.MembershipAmount.HasValue)
            model.MembershipAmount = dto.MembershipAmount.Value;

        // String can be null, but we'll update it regardless
        model.MembershipBenefits = dto.MembershipBenefits;
    }

    public static EnrolledMembershipViewDto MapToViewDto(EnrolledMembershipModel model)
    {
        return new EnrolledMembershipViewDto
        {
            EnrolledMembershipId = model.EnrolledMembershipId,
            UserId = model.UserId,
            MembershipId = model.MembershipId,
            EnrolledDate = model.EnrolledDate,
            ExpiryDate = model.ExpiryDate,
            IsActive = model.IsActive,
            PaidAmount = model.PaidAmount,
            UserName = model.User.UserName,
            MembershipName = model.Membership.MembershipName,
            CreatorId = model.Membership.CreatorId,
            IsDeleted = model.Membership.IsDeleted,
            MembershipTier = model.Membership.MembershipTier,
            ProfilePicture = model.User.ProfilePicture,
            
        };
    }

    public static EnrolledMembershipModel MapToModel(EnrolledMembershipUpdateDto dto)
    {
        return new EnrolledMembershipModel
        {
            EnrolledMembershipId = dto.EnrolledMembershipId,
            ExpiryDate = dto.ExpiryDate,
            IsActive = dto.IsActive,
            PaidAmount = dto.PaidAmount
        };
    }
}