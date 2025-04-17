using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class GoalModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GoalId { get; set; }

    [MaxLength(50)]
    [Required]
    public string? GoalTitle { get; set; }
    [ForeignKey("CreatorId")] public int CreatorId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a positive number")]
    [Required]
    public decimal GoalAmount { get; set; }
    
    public decimal GoalProgress { get; set; }

    [MaxLength(200)] public string? GoalDescription { get; set; }

    public bool IsGoalReached { get; set; }
    public bool IsGoalActive { get; set; }
}