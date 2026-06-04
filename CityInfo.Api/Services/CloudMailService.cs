using System;
using Microsoft.Extensions.Configuration;

namespace CityInfo.Api.Services;

public class CloudMailService(IConfiguration configuration): IMailService
{
  private string _mailTo = configuration["mailSettings:mailToAddress"]
    ?? throw new ArgumentException("mailSettings:mailToAddress not set");
  private string _mailFrom = configuration["mailSettings:mailFromAddress"]
    ?? throw new ArgumentException("mailSettings:mailFromAddress not set");

  public void SendMail(string subject, string message)
  {
    // Fake sending by writing to console
    Console.WriteLine($"From: {_mailFrom} To: {_mailTo} using {nameof(CloudMailService)}");
    Console.WriteLine($"Subject: {subject}");
    Console.WriteLine($"Body: {message}");
  }
}
