namespace KuroFinance.Web.Services;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmationUrl);
}
