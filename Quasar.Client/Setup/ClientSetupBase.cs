using Everything.User;

namespace Everything.Setup
{
    public abstract class ClientSetupBase
    {
        protected UserAccount UserAccount;

        protected ClientSetupBase()
        {
            UserAccount = new UserAccount();
        }
    }
}
