namespace LambdaCrudAPI.Interfaces
{
    public interface IAuthService
    {
        string Authenticate(string email, string password);
        bool Register(string email, string password, string firstName, string lastName, string personType);
    }
}
