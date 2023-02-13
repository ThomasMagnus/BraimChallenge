using BraimChallenge.Models;

namespace BraimChallenge.IServices
{
    public interface IDetecter
    {
        public string[] HeaderData(string header);
        public int DetectUserAuth(string Authorize);
        public int DetectId(long? accountId);
        public int DetectAccount(long? accountId, string Authorize, List<Account> accountList);

    }
}
