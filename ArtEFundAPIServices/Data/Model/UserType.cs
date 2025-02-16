using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.Data.Model;

public class UserType
{
    
    public int UserTypeId { get; set; }

    public string UserTypeName { get; set; }
}