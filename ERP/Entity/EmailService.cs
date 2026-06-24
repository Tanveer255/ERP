using ERP.Data.DTO.Auth;
using ERP.Data.Request;
using ERP.Helpers;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text.Json;

namespace ERP.Entity;

/// <summary>
/// This is the IEmailService interface which inherits all properties of ICrudService and has some methods given below.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Method declration of IEmailService to send the email.
    /// </summary>
    /// <param name="email">Parameter to send email.</param>
    /// <param name="subject">Paramter as subject of email.</param>
    /// <param name="htmlContent">Paramter to send the email.</param>
    /// <returns>Response Object after send.</returns>
    public Task<global::SendGrid.Response> SendEmail(string email, string subject, string htmlContent);

    /// <summary>
    /// Method declration of IEmailService to send signup email.
    /// </summary>
    /// <param name="signUpEmailDTO"></param>
    /// <returns>Returns SendGrid Response</returns>
    Task<global::SendGrid.Response> SendSignUpEmail(SignUpEmailDTO signUpEmailDTO);

    /// <summary>
    /// Method declration of IEmailService to send email.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cc"></param>
    /// <param name="subject"></param>
    /// <param name="replyto"></param>
    /// <param name="htmlContent"></param>
    /// <returns>Returns SendGrid Response</returns>
    public Task<global::SendGrid.Response> SendEmail(List<string> email, List<string> cc, string subject, string replyto, string htmlContent);

    /// <summary>
    /// Method declration of IEmailService to send the attchment email.
    /// </summary>
    /// <param name="email">Parameter to send attachment email.</param>
    /// <param name="subject">Paramter to send the attachment email.</param>
    /// <param name="htmlContent">Parameter to send the attachment email.</param>
    /// <param name="attachmentFile">Paramter to send the attchment email.</param>
    /// <returns>Reposne object after send.</returns>
    public Task<global::SendGrid.Response> SendAttachmentEmail(string email, string subject, string htmlContent, List<byte[]> attachmentFile);

    /// <summary>
    /// Method declration of IEmailService to validate the email.
    /// </summary>
    /// <param name="email">Parameter to validate the email.</param>
    /// <returns>True if valid otherwise false.</returns>
    public Task<bool> ValidateEmail(string email);

    /// <summary>
    /// Method declration of IEmailService to send forgot password email.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="token"></param>
    /// <param name="firstName"></param>
    /// <returns>Returns SendGrid Response</returns>
    Task<global::SendGrid.Response> SendForgotPasswordEmail(string email, string token, string firstName);

    /// <summary>
    /// Method declration of IEmailService to send forgot password email to staff.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="token"></param>
    /// <param name="firstName"></param>
    /// <returns>Returns SendGrid Response</returns>
    Task<global::SendGrid.Response> SendStaffForgotPasswordEmail(string email, string token, string firstName);

    /// <summary>
    ///  Method of Email Service to notify support is requested.
    /// </summary>
    /// <param name="userDetails"></param>
    /// <returns></returns>
    Task<global::SendGrid.Response> NotifySupportRequested(UserDetailsForEmailDTO userDetails);

    /// <summary>
    ///  Method of Email Service to notify staff user logged in.
    /// </summary>
    /// <param name="userDetails"></param>
    /// <returns></returns>
    Task<global::SendGrid.Response> StaffUserLoginNotification(string email, string ipAddress);

    /// <summary>
    ///  Method of Email Service to notify when staff user launched an account
    /// </summary>
    /// <param name="userDetails"></param>
    /// <returns></returns>
    Task<global::SendGrid.Response> StaffUserLaunchedNotification(string email, string ipAddress, string tenantId);
    /// <summary>
    ///   Get Support Signup Alert Template  
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task SupportSignupAlertTemplate(String email);
}

/// <summary>
/// Initializes a new instance of the <see cref="EmailService"/> class.
/// </summary>
/// <param name="appSetting"></param>
/// <param name="applicationSettings"></param>
/// <param name="sendGridSettings"></param>
/// <param name="logger"></param>
public class EmailService(
    IOptions<AppSetting> appSetting,
    IOptions<ApplicationSettings> applicationSettings,
    IOptions<SendGridSettings> sendGridSettings,
    ILogger<EmailService> logger
    ) : IEmailService
{
    private readonly AppSetting _appSetting = appSetting.Value;
    private readonly ApplicationSettings _applicationSettings = applicationSettings.Value;
    private readonly SendGridSettings _sendGridSettings = sendGridSettings.Value;
    private readonly ILogger<EmailService> _logger = logger;

    /// <summary>
    /// Method of Email Service to send the email.
    /// </summary>
    /// <param name="email">Paramter to send the email.</param>
    /// <param name="subject">Paramter to get the subject of email.</param>
    /// <param name="htmlContent">Paramter of to send the email.</param>
    /// <returns>Response Object after send.</returns>
    public Task<global::SendGrid.Response> SendEmail(string email, string subject, string htmlContent)
    {
        try
        {
            string emailApiKey = _sendGridSettings.EmailApiKey;
            string fromemail = _sendGridSettings.FromEmail;
            var client = new SendGridClient(emailApiKey);
            var message = new SendGridMessage();
            message.SetFrom(fromemail);
            message.SetSubject(subject);
            string[] emails = email.Split(',');
            if (emails.Length > 1)
            {
                List<EmailAddress> emailAddresses = new List<EmailAddress>();
                foreach (string emailAddress in emails)
                {
                    emailAddresses.Add(new EmailAddress(emailAddress));
                }

                message.AddTos(emailAddresses);
            }
            else
            {
                message.AddTo(email);
            }

            message.HtmlContent = htmlContent;
            var response = client.SendEmailAsync(message);
            return response;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Method of Email Service to send the signup email.
    /// </summary>
    /// <param name="signUpEmailDTO"></param>
    /// <returns></returns>
    public async Task<global::SendGrid.Response> SendSignUpEmail(SignUpEmailDTO signUpEmailDTO)
    {
        try
        {
            _logger.LogInformation($"Email Service: {nameof(EmailAddress)} Action: {nameof(SendSignUpEmail)}, Start sending email");
            string confirmationLink = $"{_applicationSettings.UiUrl}verify-token?token={Uri.EscapeDataString(signUpEmailDTO.Token)}&email={Uri.EscapeDataString(signUpEmailDTO.Email)}";
            string htmlContent = $@"<!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Confirm Your Email</title>
    <link
      href=""https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600&display=swap""
      rel=""stylesheet""
    />
    <style>
      body {{
        font-family: ""Poppins"", Arial, sans-serif;
        background-color: #f4f4f7;
        margin: 0;
        padding: 0;
        color: #333;
      }}
      .container {{
        max-width: 600px;
        margin: 40px auto;
        background-color: #ffffff;
        border-radius: 8px;
        overflow: hidden;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      }}
      .header {{
        padding: 25px;
        text-align: center;
        color: white;
        font-size: 24px;
        font-weight: 600;
      }}
      .content {{
        padding: 5px 30px;
        text-align: center;
      }}
      .content p {{
        font-size: 16px;
        line-height: 1.7;
        margin-bottom: 20px;
      }}
      .greeting {{
        font-size: 22px;
        font-weight: 600;
        margin-bottom: 25px;
      }}
      .button {{
        display: inline-block;
        margin: 10px 0;
        padding: 14px 20px;
        background-color: #002060;
        color: white;
        text-decoration: none;
        border-radius: 5px;
        font-size: 18px;
        transition: background-color 0.3s;
      }}
      .button:hover {{
        background-color: rgb(0 32 96 / 90%);
      }}
      .link-section {{
        background: #f8f9fa;
        border-radius: 8px;
        padding: 20px;
        margin: 30px 0;
        text-align: left;
        font-size: 14px;
        line-height: 1.6;
      }}
      .link-section a {{
        color: #002060;
        text-decoration: none;
        word-break: break-all;
      }}
      .link-section a:hover {{
        text-decoration: underline;
      }}
      .footer {{
        margin-bottom: 50px;
        text-align: center;
        font-size: 16px;
        line-height: 1.6;
      }}
      .footer a {{
        color: #002060;
        text-decoration: none;
      }}
      .footer a:hover {{
        text-decoration: underline;
      }}
      .divider {{
        height: 1px;
        background: #e0e0e0;
      }}
      .highlight {{color: #002060;
    font-weight: 400;
    text-decoration: none;
    word-break: break-all;
    overflow-wrap: anywhere;
      }}
    </style>
  </head>
  <body>
    <div class=""container"">
      <div class=""header"">
        <img
          src=""https://www.anbeond.com/wp-content/uploads/2025/03/Anbeond_Text_logo_Side.png""
          alt=""Anbeond Logo""
          width=""350""
        />
      </div>
      <div class=""content"">
        <div class=""greeting"">Hi {signUpEmailDTO.FirstName}!</div>

        <p>Big move - you've just joined Anbeond 🚀</p>

        <p>
          We help you ditch the guesswork and actually know your duties, taxes,
          and compliance before shipping.
        </p>

        <p>
          Click below to confirm your email and get rolling!
        </p>

        <a href=""{confirmationLink}"" target='_blank' class=""button"">Confirm Email</a>

        <p>
          If the button doesn't work, copy and paste the following link into
          your browser: <span class=""highlight""> {confirmationLink} </span>
        </p>

        <p>
          If you have any questions, contact us at
          <a href=""mailto:{_applicationSettings.Support}"" class=""highlight"">{_applicationSettings.Support}</a>
        </p>
        <div class=""divider""></div>
      </div>

      <div class=""footer"">
        <p>© Gabriel Merchant Inc. {DateTime.UtcNow.Year}, All Rights Reserved.</p>
      </div>
    </div>
  </body>
</html>
";

            var client = new SendGridClient(_sendGridSettings.EmailApiKey);
            var message = new SendGridMessage();
            message.SetFrom(_sendGridSettings.FromEmail);
            message.SetSubject("You’re in! Welcome to Anbeond");
            message.AddTo(signUpEmailDTO.Email);
            //message.AddCc(_sendGridSettings.CCEmail);
            message.HtmlContent = htmlContent;
            var response = await client.SendEmailAsync(message);
            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in Service: {nameof(EmailAddress)} Action: {nameof(SendSignUpEmail)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException != null ? exception.InnerException.Message : string.Empty);
            return null;
        }
    }

    /// <summary>
    /// Method of Email Service to send the email for reset password.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<global::SendGrid.Response> SendForgotPasswordEmail(string email, string token, string firstName)
    {
        try
        {
            _logger.LogInformation($"Email Service: {nameof(EmailAddress)} Action: {nameof(SendForgotPasswordEmail)}, Start sending reset password email");

            string resetLink = $"{_applicationSettings.UiUrl}reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            string htmlContent = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Confirm Your Email</title>
    <link href='https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600&display=swap' rel='stylesheet' />
    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.7.2/css/all.min.css'
        integrity='sha512-Evv84Mr4kqVGRNSgIGL/F/aIDqQb7xQ2vcrdIwxfjThSH8CSR7PBEakCr51Ck+w+/U6swU2Im1vVX0SVk9ABhg==' crossorigin='anonymous'
        referrerpolicy='no-referrer' />
    <style>
        body {{
            font-family: 'Poppins', Arial, sans-serif;
            background-color: #f4f4f7;
            margin: 0;
            padding: 0;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }}
        .color {{
            color: #002060;
        }}
        .header {{
            padding: 25px;
            text-align: center;
            color: white;
            font-size: 24px;
            font-weight: 600;
        }}
        .content {{
            padding: 5px 30px;
            text-align: center;
        }}
        .content p {{
            font-size: 16px;
            line-height: 1.7;
            margin-bottom: 20px;
        }}
        .greeting {{
            font-size: 22px;
            font-weight: 600;
            margin-bottom: 25px;
        }}
        .button {{
            display: inline-block;
            margin: 10px 0;
            padding: 14px 20px;
            background-color: #002060;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            font-size: 18px;
            transition: background-color 0.3s;
        }}
        .button:hover {{
            background-color: rgb(0 32 96 / 90%);
        }}
        .link-section {{
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            margin: 30px 0;
            text-align: left;
            font-size: 14px;
            line-height: 1.6;
        }}
        .link-section a {{
            color: #002060;
            text-decoration: none;
            word-break: break-word;
        }}
        .link-section a:hover {{
            text-decoration: underline;
        }}
        .footer {{
            padding: 25px;
            text-align: center;
            font-size: 16px;
            line-height: 1.6;
        }}
        .footer a {{
            color: #002060;
            text-decoration: none;
        }}
        .footer a:hover {{
            text-decoration: underline;
        }}
        .divider {{
            height: 1px;
            background: #e0e0e0;
        }}
        .highlight {{color: #002060;
            font-weight: 400;
            text-decoration: none;
            word-break: break-all;
            overflow-wrap: anywhere;
         }}
        .footer-inner-div {{
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 25px;
        }}
        .footer-inner-div a {{
            width: 48px;
            height: 48px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: black;
            font-size: 26px;
            text-decoration: none;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://www.anbeond.com/wp-content/uploads/2025/03/Anbeond_Text_logo_Side.png' alt='Anbeond Logo' width='350' />
        </div>
        <div class='content'>
            <div class='greeting'>Hi {firstName}!</div>
            <p>
                Thank you for taking the time to secure your account and update your password. This link will expire in 24 hours.
            </p>
            <a href='{resetLink}' target='_blank' class='button'>Update your password</a>
            <p>
              If the button doesn't work, copy and paste the following link into
              your browser: <span class=""highlight""> {resetLink} </span>
            </p>
            <p>
                If you have any questions, contact us at
                <a href='mailto:{_applicationSettings.Support}' class='highlight'>{_applicationSettings.Support}</a>
            </p>
            <div class='divider'></div>
        </div>
        <div class='footer'>
            <p>&copy; Gabriel Merchant Inc. {DateTime.UtcNow.Year}, All Rights Reserved.</p>
        </div>
    </div>
</body>
</html>";


            var client = new SendGridClient(_sendGridSettings.EmailApiKey);
            var message = new SendGridMessage();
            message.SetFrom(_sendGridSettings.FromEmail);
            message.SetSubject("Reset Your Password - Anbeond");
            message.AddTo(email);
            //message.AddCc(_sendGridSettings.CCEmail);
            message.HtmlContent = htmlContent;

            var response = await client.SendEmailAsync(message);
            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in Service: {nameof(EmailAddress)} Action: {nameof(SendForgotPasswordEmail)}");
            _logger.LogError(exception, exception.Message, exception.InnerException?.Message);
            return null;
        }
    }

    /// <summary>
    /// Method of Email Service to send the email for reset password to staff user.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<global::SendGrid.Response> SendStaffForgotPasswordEmail(string email, string token, string firstName)
    {
        try
        {
            _logger.LogInformation($"Email Service: {nameof(EmailAddress)} Action: {nameof(SendForgotPasswordEmail)}, Start sending reset password email");

            string resetLink = $"{_applicationSettings.StaffUiUrl}reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            string htmlContent = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Confirm Your Email</title>
    <link href='https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600&display=swap' rel='stylesheet' />
    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.7.2/css/all.min.css'
        integrity='sha512-Evv84Mr4kqVGRNSgIGL/F/aIDqQb7xQ2vcrdIwxfjThSH8CSR7PBEakCr51Ck+w+/U6swU2Im1vVX0SVk9ABhg==' crossorigin='anonymous'
        referrerpolicy='no-referrer' />
    <style>
        body {{
            font-family: 'Poppins', Arial, sans-serif;
            background-color: #f4f4f7;
            margin: 0;
            padding: 0;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }}
        .color {{
            color: #002060;
        }}
        .header {{
            padding: 25px;
            text-align: center;
            color: white;
            font-size: 24px;
            font-weight: 600;
        }}
        .content {{
            padding: 5px 30px;
            text-align: center;
        }}
        .content p {{
            font-size: 16px;
            line-height: 1.7;
            margin-bottom: 20px;
        }}
        .greeting {{
            font-size: 22px;
            font-weight: 600;
            margin-bottom: 25px;
        }}
        .button {{
            display: inline-block;
            margin: 10px 0;
            padding: 14px 20px;
            background-color: #002060;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            font-size: 18px;
            transition: background-color 0.3s;
        }}
        .button:hover {{
            background-color: rgb(0 32 96 / 90%);
        }}
        .link-section {{
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            margin: 30px 0;
            text-align: left;
            font-size: 14px;
            line-height: 1.6;
        }}
        .link-section a {{
            color: #002060;
            text-decoration: none;
            word-break: break-word;
        }}
        .link-section a:hover {{
            text-decoration: underline;
        }}
        .footer {{
            padding: 25px;
            text-align: center;
            font-size: 16px;
            line-height: 1.6;
        }}
        .footer a {{
            color: #002060;
            text-decoration: none;
        }}
        .footer a:hover {{
            text-decoration: underline;
        }}
        .divider {{
            height: 1px;
            background: #e0e0e0;
        }}
        .highlight {{color: #002060;
            font-weight: 400;
            text-decoration: none;
            word-break: break-all;
            overflow-wrap: anywhere;
         }}
        .footer-inner-div {{
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 25px;
        }}
        .footer-inner-div a {{
            width: 48px;
            height: 48px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: black;
            font-size: 26px;
            text-decoration: none;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://anbeond.com/wp-content/uploads/2025/11/image-25-1.png' alt='Anbeond Logo' width='350' />
        </div>
        <div class='content'>
            <div class='greeting'>Reset your password</div>
            <p>
                Need to reset your password? No problem! Just click the button below and you’ll be on your way. If you did not make this request, please ignore this email.
            </p>
            <a href='{resetLink}' target='_blank' class='button'>Update your password</a>
            <p>
              If the button doesn't work, copy and paste the following link into
              your browser: <span class=""highlight""> {resetLink} </span>
            </p>
            <div class='divider'></div>
        </div>
        <div class='footer'>
            <p>&copy; Gabriel Merchant Inc. {DateTime.UtcNow.Year}, All Rights Reserved.</p>
        </div>
    </div>
</body>
</html>";


            var client = new SendGridClient(_sendGridSettings.EmailApiKey);
            var message = new SendGridMessage();
            message.SetFrom(_sendGridSettings.FromEmail);
            message.SetSubject("Reset Your Password - Anbeond Staff");
            message.AddTo(email);
            message.HtmlContent = htmlContent;

            var response = await client.SendEmailAsync(message);
            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in Service: {nameof(EmailAddress)} Action: {nameof(SendStaffForgotPasswordEmail)}");
            _logger.LogError(exception, exception.Message, exception.InnerException?.Message);
            return null;
        }
    }

    /// <summary>
    /// Method of Email Service to send the emai with html content.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cc"></param>
    /// <param name="subject"></param>
    /// <param name="replyto"></param>
    /// <param name="htmlContent"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public async Task<global::SendGrid.Response> SendEmail(List<string> email, List<string> cc, string subject, string replyto, string htmlContent)
    {
        try
        {
            string emailApiKey = _sendGridSettings.EmailApiKey;
            string fromemail = _sendGridSettings.FromEmail;
            var client = new SendGridClient(emailApiKey);
            var message = new SendGridMessage();

            message.SetFrom(fromemail);
            message.SetSubject(subject);

            // Add CCs
            foreach (var ccEmail in cc)
            {
                message.AddCc(new EmailAddress(ccEmail));
            }

            // Add ReplyTo address
            message.AddReplyTo(new EmailAddress(replyto));

            // Add To addresses
            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            foreach (var emailAddress in email)
            {
                emailAddresses.Add(new EmailAddress(emailAddress));
            }
            message.AddTos(emailAddresses);

            // Set HTML content
            message.HtmlContent = htmlContent;

            // Send the email
            var response = await client.SendEmailAsync(message);
            return response;
        }
        catch (Exception exception)
        {
            // Handle exception (log, rethrow, etc.)
            _logger.LogInformation($"Error sending email{email}");
            throw new ApplicationException("Error sending email", exception);
        }
    }

    /// <summary>
    /// Method of Email Service to send the email with attachment.
    /// </summary>
    /// <param name="email">Paramter to send the email.</param>
    /// <param name="subject">Paramter to get the subject of email.</param>
    /// <param name="htmlContent">Paramter of to send the email.</param>
    /// <param name="attachmentFile">Paramter to set the file for attach.</param>
    /// <returns>Response Object after send.</returns>
    public Task<global::SendGrid.Response> SendAttachmentEmail(string email, string subject, string htmlContent, List<byte[]> attachmentFile)
    {
        try
        {
            string emailApiKey = _sendGridSettings.EmailApiKey;
            string fromemail = _sendGridSettings.FromEmail;
            var client = new SendGridClient(emailApiKey);
            var message = new SendGridMessage();
            message.SetFrom(fromemail);
            message.SetSubject(subject);
            message.AddTo(email);
            message.HtmlContent = htmlContent;
            attachmentFile.ForEach(x =>
            {
                message.AddAttachment(new SendGrid.Helpers.Mail.Attachment()
                {
                    Content = Convert.ToBase64String(x),
                    ContentId = Guid.NewGuid().ToString(),
                    Disposition = "attachment",
                    Filename = "Receipt.pdf",
                    Type = "application/pdf",
                });
            });
            return client.SendEmailAsync(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Method of Email Service class use to validate the email.
    /// </summary>
    /// <param name="email">Paramter to get the email.</param>
    /// <returns>True if email is valid otherwise false.</returns>
    public async Task<bool> ValidateEmail(string email)
    {
        try
        {
            bool emailValidationEnabled = _appSetting.EmailValidationEnabled;
            string validationUrl = _sendGridSettings.ValidationUrl;
            if (emailValidationEnabled)
            {
                EmailValidation.Request request = new EmailValidation.Request();
                EmailValidation.Response response = new EmailValidation.Response();
                request = new EmailValidation.Request()
                {
                    email = email,
                    source = "signup",
                };
                using (var httpClient = new HttpClient())
                {
                    using (var httpRequest = new HttpRequestMessage(new HttpMethod("POST"), validationUrl))
                    {
                        httpRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer " + _sendGridSettings.ValidationApiKey);
                        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request));
                        httpRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var httpResponse = await httpClient.SendAsync(httpRequest);
                        var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            response = JsonSerializer.Deserialize<EmailValidation.Response>(jsonResponse);
                        }
                    }
                }

                if (response != null && response.result != null && response.result.checks != null)
                {
                    if (response.result.checks.domain != null)
                    {
                        if (!response.result.checks.domain.has_valid_address_syntax)
                        {
                            return false;
                        }
                        else if (!response.result.checks.domain.has_mx_or_a_record)
                        {
                            return false;
                        }
                        else if (response.result.checks.domain.is_suspected_disposable_address)
                        {
                            return false;
                        }
                    }

                    if (response.result.checks.additional != null)
                    {
                        if (response.result.checks.additional.has_known_bounces)
                        {
                            return false;
                        }
                        else if (response.result.checks.additional.has_suspected_bounces)
                        {
                            return false;
                        }
                    }
                }

                if (response != null && response.result != null && response.result.verdict != null && response.result.verdict != string.Empty)
                {
                    if (response.result.verdict.ToString().Trim().ToLower() == "risky")
                    {
                        return false;
                    }
                    else if (response.result.verdict.ToString().Trim().ToLower() == "invalid")
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    ///  Method of Email Service to get signup email template.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public string GetSignUpEmailTemplate(String email, String url)
    {
        String htmlBody = "<!DOCTYPE html><html><head> <title></title> <style>a{text-decoration: none}a:link, a:visited{color: blue}a:hover{color: red}html{font-family: Verdana !important;}</style></head><body> <p> Hello [EMAIL],<br><br> Welcome to Ambeond!<br><br>To activate your account and start saving your business time and money, click the link below:<br><br> <b> <a target='_blank' href='[URL]'>ACTIVATE MY AMBEOND ACCOUNT</a> </b><br><br> Or paste this link into your browser:<br><br> <a target='_blank' href='[URL]'>[URL]</a><br><br><br> Need a helping hand? We offer a FREE demonstration for all our new customers!<br> Just get in touch on +44 (0)141 202 0614 (Monday - Friday, 9am - 5pm) or email <a target='_blank' href='mailto:[SupportEmail]'>[SupportEmail]</a> for more information. <br><br>We look forward to hearing from you!<br><br>Thanks,<br>Ambeond Team</p></body></html>";
        htmlBody = htmlBody.Replace("[EMAIL]", email);
        htmlBody = htmlBody.Replace("[SupportEmail]", _appSetting.Support);
        htmlBody = htmlBody.Replace("[URL]", url);
        return htmlBody;
    }

    /// <summary>
    ///  Method of Email Service to notify support is requested.
    /// </summary>
    /// <param name="userDetails"></param>
    /// <returns></returns>
    public async Task<global::SendGrid.Response> NotifySupportRequested(UserDetailsForEmailDTO userDetails)
    {
        try
        {
            _logger.LogInformation($"Email Service: {nameof(EmailAddress)} Action: {nameof(NotifySupportRequested)}, Start sending notification of support requested by {userDetails.Email}");

            var address = string.Join(", ",
                new[]
                {
                    userDetails.AddressLine,
                    userDetails.TownLocality,
                    userDetails.CityRegion,
                    userDetails.State,
                    userDetails.PostalZipCode
                }.Where(s => !string.IsNullOrWhiteSpace(s))
            );

            string htmlContent = $@"

<p style='line-height: 2; margin-bottom: 5px;'>
    Hi Anbeond Support Team,<br>
    A customer has enabled Support Access in their Anbeond account and is requesting assistance.<br>
    You are now authorized to access the customer's Anbeond account in order to provide support.<br>
    We recommend contacting the customer first to understand their request before accessing the account.
</p>

<p style='line-height: 1.6; margin-bottom: 8px;'>
<strong>Customer details:</strong> {userDetails.FullName}<br>
<strong>Company:</strong> {userDetails.Company}<br>
<strong>Address:</strong> {address}<br>
<strong>Country:</strong> {CountryTools.GetCountryName(userDetails.Country)}<br>
<strong>Phone:</strong> {userDetails.Phone}<br>
<strong>Email:</strong> {userDetails.Email}
</p>

<p>
Kind regards,<br>
Anbeond Support Team
</p>
";


            var client = new SendGridClient(_sendGridSettings.EmailApiKey);
            var message = new SendGridMessage();
            message.SetFrom(_sendGridSettings.FromEmail);
            message.SetSubject("Support Access Enabled - Customer Assistance Requested");
            message.AddTo(_applicationSettings.Support);
            message.HtmlContent = htmlContent;

            var response = await client.SendEmailAsync(message);
            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in Service: {nameof(EmailAddress)} Action: {nameof(NotifySupportRequested)}");
            _logger.LogError(exception, exception.Message, exception.InnerException?.Message);
            return null;
        }
    }

    /// <summary>
    ///  Method of Email Service to notify staff user logged in
    /// </summary>
    /// <param name="userDetails"></param>
    /// <returns></returns>
    public async Task<global::SendGrid.Response> StaffUserLoginNotification(string email, string ipAddress)
    {
        try
        {
            _logger.LogInformation($"Email Service: {nameof(EmailAddress)} Action: {nameof(StaffUserLoginNotification)}, Start sending notification of support requested by {email}");

            string htmlContent = $@"
<p style='margin-bottom: 15px;'>
    <strong>UserId:</strong> <a href='mailto:{email}'>{email}</a>
</p>
<p style='margin-bottom: 15px;'>
    <strong>Date and Time:</strong> {DateTime.UtcNow:yyyy/MM/dd HH:mm:ss tt}
</p>
<p style='margin-bottom: 15px;'>
    <strong>IP Address:</strong> {ipAddress}
</p>
<p>
    <strong>URL:</strong> <a href='{_applicationSettings.StaffUiUrl}'>{_applicationSettings.StaffUiUrl}</a>
</p>
";

            var client = new SendGridClient(_sendGridSettings.EmailApiKey);
            var message = new SendGridMessage();
            message.SetFrom(_sendGridSettings.FromEmail);
            message.SetSubject("Staff User Login Notification");
            message.AddTo(_applicationSettings.Support);
            message.HtmlContent = htmlContent;

            var response = await client.SendEmailAsync(message);
            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in Service: {nameof(EmailAddress)} Action: {nameof(StaffUserLoginNotification)}");
            _logger.LogError(exception, exception.Message, exception.InnerException?.Message);
            return null;
        }
    }

    /// <summary>
    ///  Method of Email Service to notify when staff user launched an account
    /// </summary>
    /// <param name="userDetails"></param>
    /// <returns></returns>
    public async Task<global::SendGrid.Response> StaffUserLaunchedNotification(string email, string ipAddress, string tenantId)
    {
        try
        {
            _logger.LogInformation($"Email Service: {nameof(EmailAddress)} Action: {nameof(StaffUserLaunchedNotification)}, Start sending notification of support requested by {email}");

            string htmlContent = $@"
<p>
    <strong>Launched Trader TenantId:</strong> {tenantId}
</p>
<p style='margin-bottom: 15px;'>
    <strong>Staff UserId:</strong> <a href='mailto:{email}'>{email}</a>
</p>
<p style='margin-bottom: 15px;'>
    <strong>Date and Time:</strong> {DateTime.UtcNow:yyyy/MM/dd HH:mm:ss tt}
</p>
<p style='margin-bottom: 15px;'>
    <strong>IP Address:</strong> {ipAddress}
</p>
";

            var client = new SendGridClient(_sendGridSettings.EmailApiKey);
            var message = new SendGridMessage();
            message.SetFrom(_sendGridSettings.FromEmail);
            message.SetSubject("Staff User Launched");
            message.AddTo(_applicationSettings.Support);
            message.HtmlContent = htmlContent;

            var response = await client.SendEmailAsync(message);
            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error in Service: {nameof(EmailAddress)} Action: {nameof(StaffUserLaunchedNotification)}");
            _logger.LogError(exception, exception.Message, exception.InnerException?.Message);
            return null;
        }
    }
    /// <summary>
    ///   Get Support Signup Alert Template  
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task SupportSignupAlertTemplate(String email)
    {
        string subjectIT = $"Anbeond New Sigup Alert";
        String htmlBody = "<!DOCTYPE html><html><head><title></title>" +
        "<style>a{text-decoration:none}a:link,a:visited{color:blue}a:hover{color:red}html{font-family:Verdana !important;}</style>" +
        "</head><body>" +
        "<p>Hello Support Team,<br><br>" +
        "A new user has successfully signed up on Anbeond.<br><br>" +

        "<b>User Email:</b> ##EMAIL##<br>" +
        "<b>Signup Date:</b> ##DATE##<br><br>" +

        "Please review the account and take any necessary actions if required.<br><br>" +

        "If you need more information, please check the Anbeond Staff.<br><br>" +

        "Thanks,<br>" +
        "Anbeond System" +
        "</p>" +
        "</body></html>";

        htmlBody = htmlBody.Replace("##EMAIL##", email);
        htmlBody = htmlBody.Replace("##DATE##", DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));

        await SendEmail(_sendGridSettings.ToSupportEmail, subjectIT, htmlBody);
    }
}
