namespace Discussly.Server.DTO.Responses
{
    public class PagedCommentsResponse
    {
        public Comment[] Comments { get; set; } = [];

        public int TotalComments { get; set; }

        public int CurrentPage { get; set; }
    }
}