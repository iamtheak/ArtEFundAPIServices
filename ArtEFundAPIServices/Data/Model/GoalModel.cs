using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class GoalModel
{
    [Key] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GoalId { get; set; }
    
    [ForeignKey("CreatorId")]
    public int CreatorId { get; set; }
    public decimal GoalAmount { get; set; }
    
    public float GoalProgress { get; set; }
    
    public string? GoalDescription { get; set; }
    
    public bool IsGoalReached { get; set; }
    public bool IsGoalActve { get; set; } = true;
}