using MimeKit;

namespace Yippy.Emailing;

public static class EmailUtils
{
    public static MailboxAddress ToMailboxAddress(this EmailDetail.EmailName @this) => new(@this.Name, @this.Email); 
}