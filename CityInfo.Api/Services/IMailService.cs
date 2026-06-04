namespace CityInfo.Api.Services;

public interface IMailService
{
  void SendMail(string subject, string message);
}
