namespace ArtEFundAPIServices.Data.Model;

public class BaseModel
{
    public DateTime CreatedDate { get; set; }
    
    public DateTime ModifiedDate { get; set; }
    
    public DateTime DeletedDate { get; set; }
    
    public int CreatedBy { get; set; }
    
    public int ModifiedBy { get; set; }
    
    public int DeletedBy { get; set; }    
}