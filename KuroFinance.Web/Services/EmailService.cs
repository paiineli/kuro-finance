using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace KuroFinance.Web.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmationUrl)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(config["Email:From"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Confirme seu e-mail — KuroFinance";
        message.Body = new TextPart("html")
        {
            Text = $"""
                <!DOCTYPE html>
                <html lang="pt-BR">
                <head><meta charset="utf-8"><meta name="viewport" content="width=device-width"></head>
                <body style="margin:0;padding:0;background-color:#0a0a0a;font-family:Menlo,Monaco,Consolas,'Courier New',monospace;">
                  <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#0a0a0a;padding:48px 20px;">
                    <tr>
                      <td align="center">
                        <table width="100%" cellpadding="0" cellspacing="0" style="max-width:480px;">

                          <tr>
                            <td align="center" style="padding-bottom:28px;">
                              <span style="font-size:20px;font-weight:700;color:#e5e7eb;letter-spacing:-0.3px;">
                                kuro<span style="color:#4ade80;">finance</span>
                              </span>
                            </td>
                          </tr>

                          <tr>
                            <td style="background-color:#111111;border:1px solid rgba(255,255,255,0.08);border-radius:12px;padding:40px 36px;">

                              <h2 style="margin:0 0 8px 0;font-size:18px;font-weight:600;color:#e5e7eb;">
                                Confirme seu e-mail
                              </h2>
                              <p style="margin:0 0 32px 0;font-size:14px;color:#6b7280;line-height:1.6;">
                                Olá, {toName}! Clique no botão abaixo para ativar sua conta no KuroFinance.
                              </p>

                              <table width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                  <td align="center">
                                    <a href="{confirmationUrl}"
                                       style="display:inline-block;padding:13px 36px;background-color:#4ade80;color:#000000;font-weight:700;font-size:14px;text-decoration:none;border-radius:6px;letter-spacing:-0.2px;">
                                      Confirmar e-mail
                                    </a>
                                  </td>
                                </tr>
                              </table>

                            </td>
                          </tr>

                          <tr>
                            <td style="padding:24px 0 0 0;text-align:center;">
                              <p style="margin:0 0 4px 0;font-size:12px;color:#6b7280;">
                                Este link expira em <span style="color:#e5e7eb;font-weight:600;">24 horas</span>.
                              </p>
                              <p style="margin:0;font-size:12px;color:#6b7280;">
                                Se você não criou uma conta, ignore este e-mail.
                              </p>
                            </td>
                          </tr>

                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(config["Email:SmtpHost"], int.Parse(config["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(config["Email:SmtpUser"], config["Email:SmtpPass"]);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}
