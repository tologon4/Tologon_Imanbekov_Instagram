namespace lesson58.Models;

public class SubAndSub
{
    public int Id { get; set; }
    
    public int? FollowToId { get; set; }
    public User? FollowTo { get; set; }
    
    public int? FollowFromId { get; set; }
    public User? FollowFrom { get; set; }
    
}