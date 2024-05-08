namespace lesson58.Models;

public class SubAndSub
{
    public int Id { get; set; }
    
    public int? SubcribtionId { get; set; }
    public User? Subcribtion { get; set; }
    
    public int? SubcriberId { get; set; }
    public User? Subcriber { get; set; }
    
    
}