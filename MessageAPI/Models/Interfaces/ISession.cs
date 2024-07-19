namespace MessageAPI.Models.Interfaces
{
    public interface ISession : IModel
    {
        public IUser User { get; set; }
    }
}
