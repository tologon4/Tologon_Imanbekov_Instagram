namespace lesson58.Models;

public class SubAndSub
{
    public int Id { get; set; }
    
    public int? FollowingId { get; set; }
    public User? Following { get; set; }
    
    public int? FollowerId { get; set; }
    public User? Follower { get; set; }
    
    
}